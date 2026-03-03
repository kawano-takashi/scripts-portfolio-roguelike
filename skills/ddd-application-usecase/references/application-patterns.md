# Application Layer パターンリファレンス

## 目次

1. [コア Use Case Interface](#1-コア-use-case-interface)
2. [Command Use Case パターン](#2-command-use-case-パターン)
3. [Query Use Case パターン](#3-query-use-case-パターン)
4. [DTO 設計（Request と Response）](#4-dto-設計request-と-response)
5. [Port Interface 設計](#5-port-interface-設計)
6. [Result パターン](#6-result-パターン)
7. [入力バリデーションパターン](#7-入力バリデーションパターン)
8. [トランザクション境界と Unit of Work](#8-トランザクション境界と-unit-of-work)
9. [Domain Event ディスパッチ](#9-domain-event-ディスパッチ)
10. [アンチパターンと例](#10-アンチパターンと例)
11. [リファクタリングレシピ](#11-リファクタリングレシピ)

---

## 1. コア Use Case Interface

Application Layer は Use Case Interface のファミリーを定義する。基本的な契約は `IUseCase<TRequest, TResponse>` である。Command（変更操作）と Query（読み取り操作）の特化バリアントが存在する。

```csharp
// 基本的な Use Case 契約
public interface IUseCase<in TRequest, TResponse>
{
    Task<Result<TResponse>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}

// Command Use Case - 状態を変更する。データを返す場合と返さない場合がある
public interface ICommandUseCase<in TCommand> : IUseCase<TCommand, Unit>
    where TCommand : ICommand
{
}

public interface ICommandUseCase<in TCommand, TResult> : IUseCase<TCommand, TResult>
    where TCommand : ICommand
{
}

// Query Use Case - データを読み取る。状態を変更しない
public interface IQueryUseCase<in TQuery, TResult> : IUseCase<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}

// Request 用マーカーインターフェース
public interface ICommand { }
public interface ICommand<TResult> { }
public interface IQuery<TResult> { }

// 戻り値のない Command 用の Unit 型
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
```

設計ルール:
- すべての Use Case は `Task<Result<TResponse>>` を返す（非同期 + 明示的なエラーハンドリング）
- Command は `ICommand` または `ICommand<TResult>` マーカーを実装する
- Query は `IQuery<TResult>` マーカーを実装する
- `CancellationToken` は常に最後のパラメータ
- Use Case はステートレス -- ミュータブルなインスタンスフィールドを持たない

---

## 2. Command Use Case パターン

Command Use Case は書き込み操作を表す。入力をバリデーションし、Port を通じてドメイン Aggregate をロードし、ドメインの振る舞いを呼び出し、変更を永続化する。5ステップ構造に従う: Load -> Domain Logic -> Persist -> Events -> Commit。

```csharp
// Command Request DTO
public sealed record PlaceOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequest> Items,
    string? ShippingNotes) : ICommand<Guid>;

public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);

// Command Use Case 実装
public sealed class PlaceOrderUseCase
    : ICommandUseCase<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductCatalog _productCatalog;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public PlaceOrderUseCase(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductCatalog productCatalog,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productCatalog = productCatalog;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Guid>> ExecuteAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Port を通じてドメインオブジェクトをロードする
        var customer = await _customerRepository.GetByIdAsync(
            command.CustomerId, cancellationToken);
        if (customer is null)
            return Result<Guid>.Failure("Customer not found.");

        var productIds = command.Items.Select(i => i.ProductId).ToList();
        var products = await _productCatalog.GetByIdsAsync(
            productIds, cancellationToken);

        // 2. ビジネスロジックをドメインに委譲する
        var orderItems = command.Items.Select(item =>
        {
            var product = products[item.ProductId];
            return new OrderLineItem(product.Id, product.Price, item.Quantity);
        }).ToList();

        var orderResult = Order.Place(customer, orderItems, command.ShippingNotes);
        if (orderResult.IsFailure)
            return Result<Guid>.Failure(orderResult.Error);

        var order = orderResult.Value;

        // 3. Port を通じて永続化する
        await _orderRepository.AddAsync(order, cancellationToken);

        // 4. Domain Event をディスパッチする
        await _eventDispatcher.DispatchAsync(
            order.DomainEvents, cancellationToken);

        // 5. トランザクションをコミットする
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }
}
```

Command Use Case 構造ルール:
- 番号付きステップに従う: Load -> Domain Logic -> Persist -> Events -> Commit
- すべてのビジネスルールをドメイン Aggregate メソッド（例: `Order.Place()`）に委譲する
- Use Case は「未検出」とオーケストレーションレベルの条件のみをチェックする
- Domain Event は Aggregate が収集し、Use Case がディスパッチする
- 最後に1回のトランザクションでコミットする

---

## 3. Query Use Case パターン

Query Use Case は読み取り操作を表す。読み取り専用の Port を通じてデータをロードし、Response DTO を返す。Query は状態を変更しない。

```csharp
// Query Request DTO
public sealed record GetOrderDetailQuery(Guid OrderId)
    : IQuery<OrderDetailResponse>;

// Query Response DTO
public sealed record OrderDetailResponse(
    Guid Id,
    string CustomerName,
    DateTime PlacedAt,
    string Status,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderLineResponse> Lines,
    bool CanCancel,
    bool CanShip);

public sealed record OrderLineResponse(
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

// Query Use Case 実装
public sealed class GetOrderDetailUseCase
    : IQueryUseCase<GetOrderDetailQuery, OrderDetailResponse>
{
    private readonly IOrderReadRepository _orderReader;

    public GetOrderDetailUseCase(IOrderReadRepository orderReader)
    {
        _orderReader = orderReader;
    }

    public async Task<Result<OrderDetailResponse>> ExecuteAsync(
        GetOrderDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderReader.GetDetailByIdAsync(
            query.OrderId, cancellationToken);

        if (order is null)
            return Result<OrderDetailResponse>.Failure("Order not found.");

        return Result<OrderDetailResponse>.Success(order);
    }
}
```

Query Use Case ルール:
- 読み取り専用 Port（`IOrderReadRepository`）を注入する。Command Repository は使わない
- Response DTO はフラット -- ドメイン Entity が漏れ出さない
- 算出された UI 状態（`CanCancel`、`CanShip`）はリードリポジトリのプロジェクションまたはマッピングステップで解決される
- Query には `IUnitOfWork` やイベントディスパッチがない
- Query は自然にべき等である

---

## 4. DTO 設計（Request と Response）

DTO は Application Layer とその呼び出し元の間の契約を形成する。Request DTO は入力データを運び、Response DTO は出力データを運ぶ。どちらも振る舞いを含まない。

```csharp
// Request DTO - 不変性のための sealed record
public sealed record PlaceOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequest> Items,
    string? ShippingNotes) : ICommand<Guid>;

// 複雑な入力用のネストされたサブ DTO
public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);

// Response DTO
public sealed record OrderSummaryResponse(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Currency,
    string Status,
    DateTime PlacedAt);

// ページングレスポンスラッパー
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
```

DTO ガイドライン:
- 不変性と値の等価性のための sealed record
- Request DTO はマーカーインターフェース（`ICommand`、`IQuery<T>`）を実装する
- Response DTO はプリミティブ型、文字列、日時、ネストされた Response DTO のみを含む
- ドメイン型（Entity、Value Object、ドメイン列挙型）を参照しない
- コレクションには `List<T>` や `IEnumerable<T>` ではなく `IReadOnlyList<T>` を使用する
- オプションフィールドには Nullable 参照型（`string?`）を使用する
- 構造的に類似していても Request と Response は別の DTO にする

---

## 5. Port Interface 設計

Port は Application Layer が定義し Infrastructure が実装する Interface である。Application Layer がこれらの Interface を所有し、Infrastructure の実装を参照しない。

```csharp
// Repository Port - 永続化抽象
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(
        Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(
        Order order, CancellationToken cancellationToken = default);
}

// 読み取り専用 Repository Port（Entity ではなく DTO を返す）
public interface IOrderReadRepository
{
    Task<OrderDetailResponse?> GetDetailByIdAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderSummaryResponse>> GetPagedAsync(
        OrderListFilter filter, CancellationToken cancellationToken = default);
}

// 外部サービス Port
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(
        decimal amount,
        string currency,
        string paymentMethodToken,
        CancellationToken cancellationToken = default);
}

// 通知 Port
public interface INotificationSender
{
    Task SendOrderConfirmationAsync(
        Guid orderId,
        string customerEmail,
        CancellationToken cancellationToken = default);
}

// Unit of Work Port
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}

// Domain Event Dispatcher Port
public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}
```

Port 設計ルール:
- Port は Application Layer プロジェクトに定義する。Infrastructure には定義しない
- Command Repository はドメイン Entity を受け取り/返す。Read Repository は DTO を返す
- すべてのメソッドが最後のパラメータとして `CancellationToken` を受け取る
- 外部サービス Port はアプリケーションが必要とするものをモデル化する。外部 API の提供するものではない
- Port 名は技術ではなく機能を表す: `ISmtpClient` ではなく `INotificationSender`

---

## 6. Result パターン

Result パターンは予期されたエラーケースで例外を置き換える。成功/失敗の契約を型システムで明示的にする。

```csharp
// Result<T> 実装
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(string error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            "Cannot access Value on a failed Result.");

    public string Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException(
            "Cannot access Error on a successful Result.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);

    public TOut Match<TOut>(
        Func<T, TOut> onSuccess,
        Func<string, TOut> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}

// 値なしの Result（何も返さない Command 用）
public sealed class Result
{
    private readonly string? _error;

    private Result() { IsSuccess = true; _error = null; }
    private Result(string error) { IsSuccess = false; _error = error; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException(
            "Cannot access Error on a successful Result.");

    public static Result Success() => new();
    public static Result Failure(string error) => new(error);
}
```

Use Case での使用例:

```csharp
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command,
    CancellationToken cancellationToken = default)
{
    // バリデーションエラー
    if (command.Items.Count == 0)
        return Result<Guid>.Failure("Order must contain at least one item.");

    // 未検出
    var customer = await _customerRepository.GetByIdAsync(
        command.CustomerId, cancellationToken);
    if (customer is null)
        return Result<Guid>.Failure("Customer not found.");

    // ドメインルール違反（Aggregate から伝播）
    var orderResult = Order.Place(customer, orderItems);
    if (orderResult.IsFailure)
        return Result<Guid>.Failure(orderResult.Error);

    // 成功
    return Result<Guid>.Success(order.Id);
}
```

Result パターンルール:
- 予期されたエラー（バリデーション、未検出、ドメインルール違反）には `Result<T>` を使用する
- 予期しないエラー（インフラ障害、プログラミングエラー）にのみ例外を使用する
- ドメイン Aggregate は `Result<T>` を返し、Use Case がそれを伝播する
- 呼び出し元は `Match()` を使用するか、`IsSuccess`/`IsFailure` をチェックする
- `IsSuccess` をチェックせずに `Value` にアクセスしない

---

## 7. 入力バリデーションパターン

入力バリデーションはドメイン操作の前に Application 境界で行われる。Request の構造的な妥当性をチェックする。ドメインバリデーションは Aggregate 内部でビジネス不変条件を強制する。

```csharp
// バリデーションインターフェース
public interface IValidator<in T>
{
    ValidationResult Validate(T request);
}

// バリデーション結果
public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;

    public void AddError(string field, string message)
        => _errors.Add(new ValidationError(field, message));

    public static ValidationResult Valid() => new();
}

public sealed record ValidationError(string Field, string Message);

// Validator 実装
public sealed class PlaceOrderCommandValidator
    : IValidator<PlaceOrderCommand>
{
    public ValidationResult Validate(PlaceOrderCommand command)
    {
        var result = new ValidationResult();

        if (command.CustomerId == Guid.Empty)
            result.AddError(nameof(command.CustomerId),
                "Customer ID is required.");

        if (command.Items is null || command.Items.Count == 0)
            result.AddError(nameof(command.Items),
                "At least one order item is required.");

        if (command.Items is not null)
        {
            for (int i = 0; i < command.Items.Count; i++)
            {
                var item = command.Items[i];
                if (item.ProductId == Guid.Empty)
                    result.AddError($"Items[{i}].ProductId",
                        "Product ID is required.");
                if (item.Quantity <= 0)
                    result.AddError($"Items[{i}].Quantity",
                        "Quantity must be greater than zero.");
            }
        }

        if (command.ShippingNotes?.Length > 500)
            result.AddError(nameof(command.ShippingNotes),
                "Shipping notes cannot exceed 500 characters.");

        return result;
    }
}

// Use Case にバリデーションを統合する
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command,
    CancellationToken cancellationToken = default)
{
    var validation = _validator.Validate(command);
    if (!validation.IsValid)
    {
        var errors = string.Join("; ",
            validation.Errors.Select(e => $"{e.Field}: {e.Message}"));
        return Result<Guid>.Failure($"Validation failed: {errors}");
    }

    // ドメイン操作に進む...
}
```

バリデーション境界:

| 関心事 | 場所 | 例 |
|--------|------|-----|
| 入力バリデーション | Application Layer（`IValidator`） | 必須フィールド、文字列長、フォーマット、範囲 |
| ビジネス不変条件 | Domain Layer（Aggregate メソッド） | 「出荷済みの注文はキャンセルできない」「注文合計は正でなければならない」 |

---

## 8. トランザクション境界と Unit of Work

各 Command Use Case は1つのトランザクション境界を表す。Unit of Work はすべての変更を収集し、最後にアトミックにコミットする。

```csharp
// Unit of Work Port（Application Layer に定義）
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}

// 明示的なトランザクション境界を持つ Use Case
public sealed class TransferOrderUseCase
    : ICommandUseCase<TransferOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferOrderUseCase(
        IOrderRepository orderRepository,
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> ExecuteAsync(
        TransferOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(
            command.OrderId, cancellationToken);
        if (order is null)
            return Result<Unit>.Failure("Order not found.");

        var warehouse = await _warehouseRepository.GetByIdAsync(
            command.TargetWarehouseId, cancellationToken);
        if (warehouse is null)
            return Result<Unit>.Failure("Target warehouse not found.");

        // ドメイン操作（複数の Aggregate を変更する場合がある）
        var transferResult = order.TransferTo(warehouse);
        if (transferResult.IsFailure)
            return Result<Unit>.Failure(transferResult.Error);

        // 変更を永続化する
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);

        // すべての変更を1回でコミットする -- アトミックなトランザクション
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
```

トランザクション境界ルール:
- 1つの Command Use Case = 1つのトランザクション
- `IUnitOfWork.CommitAsync()` は Use Case の最後に1回だけ呼び出す
- ループや条件分岐の中で `CommitAsync()` を呼ばない
- Query Use Case は `IUnitOfWork` を使用しない
- 境界付きコンテキスト間の調整には分散トランザクションではなく Domain Event や Saga を使用する

---

## 9. Domain Event ディスパッチ

Domain Event はドメイン操作中に Aggregate が発行し、Application Layer がディスパッチする。

```csharp
// Domain Event インターフェース（Domain Layer に定義）
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

// Domain Event の例
public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTime OccurredAt) : IDomainEvent;

// Domain Event Dispatcher Port（Application Layer に定義）
public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyList<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}

// Domain Event Handler インターフェース
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);
}

// Aggregate 基底クラスがイベントを発行する
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Use Case がイベントをディスパッチする
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command,
    CancellationToken cancellationToken = default)
{
    // ... バリデーション、Aggregate のロード、ドメインロジック ...

    var order = orderResult.Value;
    await _orderRepository.AddAsync(order, cancellationToken);

    // 整合性の要件に応じてコミット前またはコミット後にイベントをディスパッチする
    await _eventDispatcher.DispatchAsync(
        order.DomainEvents, cancellationToken);
    order.ClearDomainEvents();

    await _unitOfWork.CommitAsync(cancellationToken);

    return Result<Guid>.Success(order.Id);
}

// Event Handler（Application Layer）
public sealed class SendOrderConfirmationOnOrderPlaced
    : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly INotificationSender _notificationSender;
    private readonly ICustomerReadRepository _customerReader;

    public SendOrderConfirmationOnOrderPlaced(
        INotificationSender notificationSender,
        ICustomerReadRepository customerReader)
    {
        _notificationSender = notificationSender;
        _customerReader = customerReader;
    }

    public async Task HandleAsync(
        OrderPlacedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerReader.GetByIdAsync(
            @event.CustomerId, cancellationToken);
        if (customer is null) return;

        await _notificationSender.SendOrderConfirmationAsync(
            @event.OrderId, customer.Email, cancellationToken);
    }
}
```

Domain Event ルール:
- Aggregate がイベントを発行し、Application Layer がディスパッチする
- 各 Handler は1つのことを行う（通知の送信、リードモデルの更新など）
- ディスパッチのタイミング: コミット前（強整合性）またはコミット後（結果整合性）
- Event Handler はイベントを発行した Aggregate を変更してはならない

---

## 10. アンチパターンと例

### 10.1 肥大化した Use Case

Use Case に Domain Layer に属するビジネスルールが含まれている。

```csharp
// BAD: ビジネスルールが Use Case にある
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command, CancellationToken ct)
{
    var customer = await _customerRepo.GetByIdAsync(command.CustomerId, ct);

    // これらのルールはドメイン Aggregate に属する
    if (customer.CreditLimit < command.Items.Sum(i => i.Price * i.Quantity))
        return Result<Guid>.Failure("Insufficient credit.");

    if (customer.IsBlacklisted)
        return Result<Guid>.Failure("Customer is blacklisted.");

    var order = new Order();
    order.CustomerId = customer.Id;
    order.Status = OrderStatus.Placed;
    order.TotalAmount = command.Items.Sum(i => i.Price * i.Quantity);

    // 手動の割引計算 -- ドメインロジック
    if (order.TotalAmount > 1000m)
        order.TotalAmount *= 0.9m;

    await _orderRepo.AddAsync(order, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result<Guid>.Success(order.Id);
}

// GOOD: Use Case がオーケストレーションし、ドメイン Aggregate がルールを所有する
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command, CancellationToken ct)
{
    var customer = await _customerRepo.GetByIdAsync(command.CustomerId, ct);
    if (customer is null)
        return Result<Guid>.Failure("Customer not found.");

    var products = await _productCatalog.GetByIdsAsync(
        command.Items.Select(i => i.ProductId).ToList(), ct);

    var lineItems = command.Items.Select(i =>
        new OrderLineItem(i.ProductId, products[i.ProductId].Price, i.Quantity))
        .ToList();

    // ドメイン Aggregate がすべてのビジネスルールを強制する
    var orderResult = Order.Place(customer, lineItems);
    if (orderResult.IsFailure)
        return Result<Guid>.Failure(orderResult.Error);

    await _orderRepo.AddAsync(orderResult.Value, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result<Guid>.Success(orderResult.Value.Id);
}
```

### 10.2 ドメインロジックの漏洩

状態遷移ロジックが Aggregate ではなく Use Case で実装されている。

```csharp
// BAD: 状態遷移ロジックが Use Case にある
public async Task<Result> ExecuteAsync(
    CancelOrderCommand command, CancellationToken ct)
{
    var order = await _orderRepo.GetByIdAsync(command.OrderId, ct);

    // これらのルールは Order.Cancel() に属する
    if (order.Status == OrderStatus.Shipped)
        return Result.Failure("Cannot cancel shipped order.");
    if (order.Status == OrderStatus.Delivered)
        return Result.Failure("Cannot cancel delivered order.");
    if (DateTime.UtcNow - order.PlacedAt > TimeSpan.FromDays(30))
        return Result.Failure("Cancellation window has passed.");

    order.Status = OrderStatus.Cancelled;
    order.CancelledAt = DateTime.UtcNow;
    order.CancellationReason = command.Reason;

    await _orderRepo.UpdateAsync(order, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result.Success();
}

// GOOD: ドメイン Aggregate がルールを強制する
public async Task<Result> ExecuteAsync(
    CancelOrderCommand command, CancellationToken ct)
{
    var order = await _orderRepo.GetByIdAsync(command.OrderId, ct);
    if (order is null)
        return Result.Failure("Order not found.");

    var result = order.Cancel(command.Reason);
    if (result.IsFailure)
        return Result.Failure(result.Error);

    await _orderRepo.UpdateAsync(order, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result.Success();
}
```

### 10.3 インフラ結合

Port Interface ではなくインフラに直接依存している。

```csharp
// BAD: インフラへの直接依存
public sealed class ExportOrdersUseCase
{
    private readonly ApplicationDbContext _dbContext;  // EF Core!
    private readonly HttpClient _httpClient;           // 生の HTTP!

    public async Task<Result<byte[]>> ExecuteAsync(
        ExportOrdersQuery query, CancellationToken ct)
    {
        var orders = await _dbContext.Orders
            .Where(o => o.PlacedAt >= query.From)
            .ToListAsync(ct);

        var response = await _httpClient.PostAsJsonAsync(
            "https://export-service/api/export", orders, ct);

        return Result<byte[]>.Success(
            await response.Content.ReadAsByteArrayAsync(ct));
    }
}

// GOOD: Port Interface を使用する
public sealed class ExportOrdersUseCase
{
    private readonly IOrderReadRepository _orderReader;
    private readonly IExportService _exportService;

    public async Task<Result<byte[]>> ExecuteAsync(
        ExportOrdersQuery query, CancellationToken ct)
    {
        var orders = await _orderReader.GetOrdersForExportAsync(
            query.From, ct);
        var exportData = await _exportService.GenerateExportAsync(
            orders, ct);
        return Result<byte[]>.Success(exportData);
    }
}
```

### 10.4 貧血 Use Case（CRUD パススルー）

Repository に単に委譲するだけで付加価値のない Use Case。

```csharp
// BAD: 付加価値なし -- Repository に委譲するだけ
public sealed class GetCustomerByIdUseCase
{
    private readonly ICustomerRepository _repo;

    public async Task<Result<CustomerResponse>> ExecuteAsync(
        GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await _repo.GetByIdAsync(query.Id, ct);
        if (customer is null)
            return Result<CustomerResponse>.Failure("Not found.");
        return Result<CustomerResponse>.Success(MapToDto(customer));
    }
}

// 評価: Use Case は必要か？
// バリデーション、認可、キャッシュ、エンリッチメントがなければ:
//   -> Presenter/Controller がリードリポジトリを直接呼び出してもよい
// 横断的ロジックがあれば:
//   -> Use Case には存在意義がある

// GOOD（Use Case が正当化される場合）: 付加価値を明示する
public sealed class GetCustomerByIdUseCase
{
    private readonly ICustomerReadRepository _customerReader;
    private readonly IAuthorizationService _auth;

    public async Task<Result<CustomerDetailResponse>> ExecuteAsync(
        GetCustomerByIdQuery query, CancellationToken ct)
    {
        if (!await _auth.CanViewCustomerAsync(
            query.RequestedByUserId, query.Id, ct))
            return Result<CustomerDetailResponse>.Failure("Access denied.");

        var customer = await _customerReader.GetDetailByIdAsync(query.Id, ct);
        if (customer is null)
            return Result<CustomerDetailResponse>.Failure("Customer not found.");

        return Result<CustomerDetailResponse>.Success(customer);
    }
}
```

### 10.5 バリデーション不足

入力バリデーションがなく、データベース制約やドメイン例外に依存している。

```csharp
// BAD: 入力バリデーションがない
public async Task<Result<Guid>> ExecuteAsync(
    CreateProductCommand command, CancellationToken ct)
{
    // 入力チェックなしでドメインに直行する
    var product = Product.Create(command.Name, command.Price);
    await _productRepo.AddAsync(product, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result<Guid>.Success(product.Id);
}
// null の名前で SqlException、負の価格でドメイン ArgumentException
// -- クリーンなバリデーションメッセージがない

// GOOD: Application 境界で入力をバリデーションする
public async Task<Result<Guid>> ExecuteAsync(
    CreateProductCommand command, CancellationToken ct)
{
    var validation = _validator.Validate(command);
    if (!validation.IsValid)
        return Result<Guid>.Failure(FormatErrors(validation));

    var product = Product.Create(command.Name, command.Price);

    await _productRepo.AddAsync(product, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result<Guid>.Success(product.Id);
}
```

### 10.6 横断的関心事の汚染

ロギング、認可、メトリクスが Use Case 本体に混在している。

```csharp
// BAD: 横断的関心事が Use Case 本体にある
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command, CancellationToken ct)
{
    _logger.LogInformation("PlaceOrder started for {Id}", command.CustomerId);
    var stopwatch = Stopwatch.StartNew();

    if (!await _authService.IsAuthorizedAsync("PlaceOrder"))
        return Result<Guid>.Failure("Unauthorized.");

    try
    {
        var result = await PlaceOrderInternal(command, ct);
        _logger.LogInformation("PlaceOrder done in {Ms}ms",
            stopwatch.ElapsedMilliseconds);
        _metrics.RecordDuration("PlaceOrder", stopwatch.ElapsedMilliseconds);
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "PlaceOrder failed");
        _metrics.IncrementFailure("PlaceOrder");
        throw;
    }
}

// GOOD: 横断的関心事に Decorator/Pipeline を使用する
public async Task<Result<Guid>> ExecuteAsync(
    PlaceOrderCommand command, CancellationToken ct)
{
    // 純粋なオーケストレーションロジックのみ
    var customer = await _customerRepo.GetByIdAsync(command.CustomerId, ct);
    if (customer is null)
        return Result<Guid>.Failure("Customer not found.");

    // ... ドメインロジック、永続化、イベント ...

    return Result<Guid>.Success(order.Id);
}

// 横断的関心事は Decorator で処理する
public sealed class LoggingDecorator<TRequest, TResponse>
    : IUseCase<TRequest, TResponse>
{
    private readonly IUseCase<TRequest, TResponse> _inner;
    private readonly ILogger _logger;

    public LoggingDecorator(
        IUseCase<TRequest, TResponse> inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse>> ExecuteAsync(
        TRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Executing {UseCase}", typeof(TRequest).Name);
        var result = await _inner.ExecuteAsync(request, ct);
        _logger.LogInformation("Completed {UseCase}: {Success}",
            typeof(TRequest).Name, result.IsSuccess);
        return result;
    }
}
```

### 10.7 共有ミュータブル状態

static フィールドや Singleton を通じて Use Case 間で状態を共有している。

```csharp
// BAD: 呼び出し間で static/共有状態を持つ
public sealed class PlaceOrderUseCase
{
    private static int _orderCounter = 0;
    private static readonly List<Order> _cache = new();

    public async Task<Result<Guid>> ExecuteAsync(
        PlaceOrderCommand command, CancellationToken ct)
    {
        _orderCounter++;
        // ... _orderCounter を使用し、_cache に追加する ...
        // スレッドアンセーフでリクエスト間で漏洩する
    }
}

// GOOD: ステートレスな Use Case、状態はインフラに持たせる
public sealed class PlaceOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNumberGenerator _numberGenerator;  // Port!

    public async Task<Result<Guid>> ExecuteAsync(
        PlaceOrderCommand command, CancellationToken ct)
    {
        var orderNumber = await _numberGenerator.NextAsync(ct);
        // ステートレス -- 各呼び出しが独立している
    }
}
```

---

## 11. リファクタリングレシピ

### レシピ 1: ドメインロジックを Domain Layer に押し下げる

1. Use Case 内のビジネスルールを特定する（不変条件チェック、状態遷移、計算）
2. ルールを該当するドメイン Aggregate メソッドまたは Domain Service に移動する
3. Aggregate メソッドがルール違反時に `Result<T>` を返すようにする
4. Use Case は Aggregate メソッドを呼び出し、Result を伝播する
5. Use Case にドメイン状態に対する `if` チェックがないことを確認する（null/未検出チェックのみ）

### レシピ 2: Port Interface を抽出する

1. Use Case 内の具象インフラ参照を特定する（`DbContext`、`HttpClient`、`SmtpClient`）
2. Use Case が必要とするものを記述する Interface を Application Layer に定義する
3. Interface を機能で命名する: `IOrderRepository`、`IPaymentGateway`、`INotificationSender`
4. インフラコードを Infrastructure Layer の実装クラスに移動する
5. DI に Interface から実装へのバインディングを登録する
6. Use Case プロジェクトが Infrastructure プロジェクトを参照していないことを確認する

### レシピ 3: 責務ごとに Use Case を分割する

1. 複数の無関係な操作を行う Use Case を特定する
2. 各操作に対して別々の Command/Query 型を定義する
3. 操作ごとに別の Use Case クラスを作成する
4. 元の Use Case から各新しい Use Case に該当するロジックを移動する
5. 共有のセットアップ/ロードは共有プライベートメソッドまたは別のサービスにする
6. 各新しい Use Case の注入される依存は最大5〜6個にする

### レシピ 4: Use Case の必要性を評価する（貧血 Use Case）

1. Repository からロードして DTO を返すだけの Use Case を特定する
2. Use Case が付加価値を持つか確認する: 認可、バリデーション、キャッシュ、エンリッチメント、監査
3. 付加価値がなければ: 呼び出し元がリードリポジトリを直接使用できるか検討する
4. 価値が隠れているなら: それを明示する（認可チェック、バリデーションなどを追加する）
5. 目的が明白でない場合、Use Case の存在理由をドキュメント化する

### レシピ 5: Request バリデーションを導入する

1. 入力バリデーションのない Use Case を特定する
2. `IValidator<TRequest>` を実装する Validator クラスを作成する
3. チェック項目: 必須フィールド、文字列長、数値範囲、フォーマット制約
4. Validator を Use Case に注入する
5. ドメイン操作の前に `_validator.Validate(request)` を呼び出す
6. フォーマットされたバリデーションエラーとともに `Result.Failure()` を返す
7. 不正な入力形状に対して `SqlException` やドメイン `ArgumentException` が発生しないことを確認する

### レシピ 6: Decorator/Pipeline で横断的関心事を抽出する

1. Use Case 本体に混在しているロギング、認可、メトリクス、キャッシュコードを特定する
2. 内部の Use Case をラップする `IUseCase<TRequest, TResponse>` を実装する Decorator クラスを作成する
3. 横断的コードを Decorator の `ExecuteAsync` に移動する
4. Decorator を DI に登録する（手動または Scrutor 経由）
5. Use Case 本体がオーケストレーションロジックのみを含むことを確認する
6. よくある Decorator: `LoggingDecorator`、`ValidationDecorator`、`AuthorizationDecorator`

### レシピ 7: Result パターンを導入する

1. 予期されたエラーに例外をスローする Use Case を特定する
2. 戻り値の型を `Task<TResponse>` から `Task<Result<TResponse>>` に変更する
3. 予期されたエラーの `throw new` を `return Result<T>.Failure(...)` に置き換える
4. 例外スローは予期しない/インフラ障害にのみ残す
5. 呼び出し元を `Result.IsSuccess`/`IsFailure` をハンドリングするように更新する
6. ビジネスロジック例外が Use Case からエスケープしないことを確認する

### レシピ 8: Unit of Work を導入する

1. 複数の Repository で `SaveChangesAsync()` を呼び出している、またはループ内で呼び出している Use Case を特定する
2. Application Layer に単一の `CommitAsync()` メソッドを持つ `IUnitOfWork` を定義する
3. `IUnitOfWork` を Use Case に注入する
4. 個々の Repository メソッドから `SaveChangesAsync` 呼び出しを削除する
5. Use Case の最後に `_unitOfWork.CommitAsync()` を1回だけ呼び出す
6. 確認: Use Case の実行ごとに1回のコミット、ループや条件分岐内にコミットがない
