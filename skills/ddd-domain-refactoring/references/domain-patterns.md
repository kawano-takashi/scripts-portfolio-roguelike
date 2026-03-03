# C# ドメインレイヤーパターン (DDD 戦術的パターン)

## 目次

1. [Entity パターン](#entity-パターン)
2. [Value Object パターン](#value-object-パターン)
3. [Aggregate パターン](#aggregate-パターン)
4. [Domain Service パターン](#domain-service-パターン)
5. [Repository Interface パターン](#repository-interface-パターン)
6. [Domain Event（オプショナルパターン）](#domain-eventオプショナルパターン)
7. [アンチパターンと例](#アンチパターンと例)
8. [リファクタリングレシピ](#リファクタリングレシピ)

## Entity パターン

Entity は永続的な識別子を持つオブジェクトである。同じIDを持つ2つの Entity は属性が異なっていても同じ Entity である。Entity はライフサイクルを持ち、状態を変更でき、ビジネスルールを強制する振る舞いをカプセル化する。

### 基底クラス

```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other)
        => other is not null && Id.Equals(other.Id);

    public override int GetHashCode()
        => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !Equals(left, right);
}
```

### 振る舞いを持つ具象 Entity

```csharp
public sealed class OrderLine : Entity<OrderLineId>
{
    public ProductId ProductId { get; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; }

    internal OrderLine(OrderLineId id, ProductId productId, Quantity quantity, Money unitPrice)
        : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void AdjustQuantity(Quantity newQuantity)
    {
        if (newQuantity.Value <= 0)
            throw new DomainException("Order line quantity must be positive.");
        Quantity = newQuantity;
    }

    public Money LineTotal => UnitPrice * Quantity.Value;
}
```

設計ルール:
- 等価性はIDのみに基づく（属性値ではない）
- Aggregate 内の子 Entity のコンストラクタは `internal`（Root のみが作成する）
- 状態変更メソッドは不変条件を強制し、違反時に `DomainException` をスローする
- public setter なし -- すべての変更は名前付き振る舞いメソッドを通じて行う
- 生の `Guid` や `int` の代わりに強い型付けのID（Value Object）を使用する

## Value Object パターン

Value Object は識別子を持たない。同じ属性を持つ2つの Value Object は等しい。不変であり、自己検証し、プリミティブの代わりにドメイン概念を表現する。

### シンプルな Value Object

```csharp
public sealed record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email address cannot be empty.");
        if (!value.Contains('@'))
            throw new DomainException($"'{value}' is not a valid email address.");
        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}
```

### 複数フィールドの Value Object

```csharp
public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot add {Currency} to {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot subtract {other.Currency} from {Currency}.");
        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainException("Resulting money amount cannot be negative.");
        return new Money(result, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator *(Money money, int multiplier)
        => new(money.Amount * multiplier, money.Currency);

    public override string ToString() => $"{Amount} {Currency}";
}
```

### 強い型付けのID

```csharp
public sealed record OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public sealed record CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public sealed record ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
```

### 制約付き Value Object

```csharp
public sealed record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be positive.");
        Value = value;
    }

    public Quantity Add(int amount)
    {
        return new Quantity(Value + amount);
    }

    public override string ToString() => Value.ToString();
}
```

設計ルール:
- 自動的な構造的等価性のために C# `record` を使用する
- コンストラクタで検証する -- 不正な Value Object は存在できない
- 不変: 構築後のプロパティに `set` や `init` を使用しない（コンストラクタがすべての状態を設定する）
- よくある生成シナリオにファクトリメソッドを提供する（`Zero`、`New`、`From`）
- ドメイン的に意味のある操作をメソッドとして提供する（`Add`、`Subtract`、演算子オーバーロード）
- 等価性を壊す継承を防ぐために `sealed` を使用する
- 意味のある表示のために `ToString` をオーバーライドする

## Aggregate パターン

Aggregate はデータ変更の単位として扱われる Entity と Value Object のクラスタである。Aggregate Root が唯一のエントリポイントとなる。Aggregate にまたがるすべての不変条件は Root が強制する。外部参照は Root のみを指す。

### 子 Entity を持つ Aggregate Root

```csharp
public sealed class Order : Entity<OrderId>
{
    private readonly List<OrderLine> _lines = new();

    public CustomerId CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public Money Total => CalculateTotal();
    public DateTime PlacedAt { get; }

    private Order(OrderId id, CustomerId customerId, DateTime placedAt)
        : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        PlacedAt = placedAt;
    }

    public static Order Create(CustomerId customerId, DateTime placedAt)
    {
        if (customerId is null)
            throw new DomainException("Customer is required.");
        return new Order(OrderId.New(), customerId, placedAt);
    }

    public void AddLine(ProductId productId, Quantity quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Can only add lines to a draft order.");
        if (_lines.Count >= 50)
            throw new DomainException("An order cannot have more than 50 lines.");

        var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine is not null)
            throw new DomainException(
                $"Product {productId} is already in the order. Adjust quantity instead.");

        var line = new OrderLine(OrderLineId.New(), productId, quantity, unitPrice);
        _lines.Add(line);
    }

    public void AdjustLineQuantity(OrderLineId lineId, Quantity newQuantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Can only adjust lines in a draft order.");

        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new DomainException($"Order line {lineId} not found.");

        line.AdjustQuantity(newQuantity);
    }

    public void RemoveLine(OrderLineId lineId)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Can only remove lines from a draft order.");

        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new DomainException($"Order line {lineId} not found.");

        _lines.Remove(line);
    }

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be placed.");
        if (_lines.Count == 0)
            throw new DomainException("Cannot place an order with no lines.");
        Status = OrderStatus.Placed;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel a shipped order.");
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled.");
        Status = OrderStatus.Cancelled;
    }

    public void MarkShipped()
    {
        if (Status != OrderStatus.Placed)
            throw new DomainException("Only placed orders can be shipped.");
        Status = OrderStatus.Shipped;
    }

    private Money CalculateTotal()
    {
        if (_lines.Count == 0)
            return Money.Zero(Currency.USD);
        return _lines.Aggregate(
            Money.Zero(_lines[0].UnitPrice.Currency),
            (sum, line) => sum + line.LineTotal);
    }
}
```

### ドメインステータス Enum

```csharp
public enum OrderStatus
{
    Draft,
    Placed,
    Confirmed,
    Shipped,
    Cancelled
}
```

### Domain Exception

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

設計ルール:
- Root が Aggregate 内で唯一パブリックに構築可能なオブジェクトである
- 子 Entity のコンストラクタは `internal` -- Root のみが作成する
- すべての状態変更は Entity 間の不変条件を強制する Root のメソッドを経由する
- 外部コードは子 Entity への参照を持たず、Root への参照のみを持つ
- Aggregate は小さく保つ: 子 Entity を減らし、個別の Aggregate を増やすことを優先する
- 外部からの変更を防ぐために private コレクションと `IReadOnlyList` パブリックアクセサを使用する
- ファクトリメソッド（`Create`）または private コンストラクタで構築時の不変条件を強制する
- 状態遷移は遷移を許可する前に現在の状態を検証する

## Domain Service パターン

Domain Service は単一の Entity や Value Object に自然に属さないドメインロジックを含む。ステートレスであり、通常は複数の Aggregate にまたがるか、複雑なドメイン計算を実行する。

### Aggregate 横断の Domain Service

```csharp
public sealed class OrderPricingService
{
    public Money CalculateDiscountedTotal(Order order, CustomerLoyaltyTier tier)
    {
        var baseTotal = order.Total;
        var discountRate = tier switch
        {
            CustomerLoyaltyTier.Gold => 0.10m,
            CustomerLoyaltyTier.Silver => 0.05m,
            CustomerLoyaltyTier.Bronze => 0.02m,
            _ => 0m
        };

        var discountAmount = baseTotal.Amount * discountRate;
        return new Money(baseTotal.Amount - discountAmount, baseTotal.Currency);
    }
}
```

### Aggregate 横断のバリデーション

```csharp
public sealed class OrderPlacementService
{
    public void PlaceOrder(Order order, Inventory inventory)
    {
        foreach (var line in order.Lines)
        {
            if (!inventory.HasSufficientStock(line.ProductId, line.Quantity))
                throw new DomainException(
                    $"Insufficient stock for product {line.ProductId}.");
        }

        order.Place();
    }
}
```

### 複雑なドメイン計算

```csharp
public sealed class ShippingCostCalculator
{
    public Money Calculate(
        Address origin,
        Address destination,
        Weight totalWeight,
        ShippingMethod method)
    {
        var baseRate = method switch
        {
            ShippingMethod.Standard => 5.00m,
            ShippingMethod.Express => 15.00m,
            ShippingMethod.Overnight => 25.00m,
            _ => throw new DomainException($"Unknown shipping method: {method}")
        };

        var weightSurcharge = totalWeight.Kilograms > 10
            ? (totalWeight.Kilograms - 10) * 1.50m
            : 0m;

        var distanceFactor = origin.Country != destination.Country ? 2.0m : 1.0m;

        var total = (baseRate + weightSurcharge) * distanceFactor;
        return new Money(total, Currency.USD);
    }
}
```

設計ルール:
- ステートレス -- ミュータブルなフィールドなし、注入された Repository なし
- ユビキタス言語で `{Concept}Service` または `{Concept}Calculator` と命名する
- ロジックが複数の Aggregate Root を含む場合に使用する
- ロジックが単一の Entity に自然に属さない場合に使用する
- 単一の Entity に属するロジックには使用しない -- Entity に配置する
- Domain Service はパラメータとして DTO やプリミティブではなくドメインオブジェクトを受け取る
- Domain Service は Application Layer ではなく Domain Layer に存在する
- Domain Service はインフラ操作を行わない（DB、HTTP、I/O なし）

## Repository Interface パターン

Repository Interface は Domain Layer で定義され、Aggregate Root へのコレクション風のアクセスを提供する。実装は Infrastructure Layer に存在する。Aggregate Root ごとにちょうど1つの Repository がある。

### 汎用 Repository Interface

```csharp
public interface IRepository<T, in TId>
    where T : Entity<TId>
    where TId : notnull
{
    Task<T?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);
}
```

### Aggregate 固有の Repository Interface

```csharp
public interface IOrderRepository : IRepository<Order, OrderId>
{
    Task<IReadOnlyList<Order>> FindByCustomerAsync(
        CustomerId customerId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        OrderId id, CancellationToken cancellationToken = default);
}
```

```csharp
public interface ICustomerRepository : IRepository<Customer, CustomerId>
{
    Task<Customer?> FindByEmailAsync(
        EmailAddress email, CancellationToken cancellationToken = default);
}
```

### Unit of Work Interface

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

設計ルール:
- Aggregate Root ごとに1つの Repository Interface -- 子 Entity や Value Object には作らない
- Interface は Domain Layer に定義し、実装は Infrastructure に配置する
- メソッドは個別の子 Entity ではなく Aggregate Root を返す
- コレクション風のセマンティクスを使用する: `FindByIdAsync`、`AddAsync`、`RemoveAsync`
- リストを返すクエリメソッドは `IReadOnlyList<T>` を返す
- すべての非同期メソッドに `CancellationToken` を含める
- Repository に複雑なクエリ/フィルタリングロジックを置かない（複雑なクエリには CQRS リードモデルを使用する）
- Interface に実装詳細を公開しない（`IQueryable`、`DbContext`）

## Domain Event（オプショナルパターン）

Domain Event はドメインで重要な出来事が起きたことを記録する。Aggregate Root から発行され、永続化後にディスパッチされる。オプショナルだが広く使われるパターンである。

### Event Interface とレコード

```csharp
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public sealed record OrderPlacedEvent(
    OrderId OrderId,
    CustomerId CustomerId,
    Money Total,
    DateTime OccurredAt) : IDomainEvent;

public sealed record OrderCancelledEvent(
    OrderId OrderId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;

public sealed record OrderLineAddedEvent(
    OrderId OrderId,
    ProductId ProductId,
    Quantity Quantity,
    DateTime OccurredAt) : IDomainEvent;
```

### Event 対応の Aggregate Root 基底クラス

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
```

### Aggregate での Event 発行

```csharp
// Order は Entity<OrderId> の代わりに AggregateRoot<OrderId> を継承する
public sealed class Order : AggregateRoot<OrderId>
{
    // ... 同じフィールドとコンストラクタ ...

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be placed.");
        if (_lines.Count == 0)
            throw new DomainException("Cannot place an order with no lines.");
        Status = OrderStatus.Placed;
        RaiseDomainEvent(new OrderPlacedEvent(Id, CustomerId, Total, DateTime.UtcNow));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel a shipped order.");
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled.");
        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason, DateTime.UtcNow));
    }
}
```

ガイドライン:
- Event は過去形の名前を持つ不変レコード（`PlaceOrder` ではなく `OrderPlaced`）
- Event は Entity の参照ではなく、ID と Value Object のみを含む
- ディスパッチは Domain Layer の外部で行われる（Infrastructure の関心事）
- 現在の Aggregate のトランザクションを壊すべきでない Aggregate 横断の副作用に使用する
- `ClearDomainEvents` はディスパッチ後に Infrastructure から呼び出される

## アンチパターンと例

### 1. 貧血ドメインモデル

```csharp
// BAD: Entity が振る舞いを持たないデータの入れ物になっている
public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public List<OrderLine> Lines { get; set; } = new();
    public decimal Total { get; set; }
}

// ビジネスロジックがドメイン外のサービスに存在する:
public class OrderService
{
    public void PlaceOrder(Order order)
    {
        if (order.Lines.Count == 0)
            throw new Exception("No lines");
        order.Status = "Placed";
        order.Total = order.Lines.Sum(l => l.Quantity * l.UnitPrice);
    }
}
```

```csharp
// GOOD: Entity が振る舞いと不変条件をカプセル化している
public sealed class Order : Entity<OrderId>
{
    private readonly List<OrderLine> _lines = new();
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public Money Total => _lines.Count == 0
        ? Money.Zero(Currency.USD)
        : _lines.Aggregate(
            Money.Zero(_lines[0].UnitPrice.Currency),
            (sum, line) => sum + line.LineTotal);

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be placed.");
        if (_lines.Count == 0)
            throw new DomainException("Cannot place an order with no lines.");
        Status = OrderStatus.Placed;
    }
}
```

### 2. プリミティブ執着

```csharp
// BAD: ドメイン概念に生のプリミティブを使用している
public class Customer : Entity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public string Currency { get; set; } = "USD";
}
```

```csharp
// GOOD: バリデーション付きの型付き Value Object
public sealed class Customer : Entity<CustomerId>
{
    public EmailAddress Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Money CreditLimit { get; private set; }

    private Customer(CustomerId id, EmailAddress email, PhoneNumber phone, Money creditLimit)
        : base(id)
    {
        Email = email;
        Phone = phone;
        CreditLimit = creditLimit;
    }

    public static Customer Register(EmailAddress email, PhoneNumber phone, Money creditLimit)
    {
        return new Customer(CustomerId.New(), email, phone, creditLimit);
    }

    public void ChangeEmail(EmailAddress newEmail)
    {
        Email = newEmail;
    }
}
```

### 3. Aggregate が大きすぎる

```csharp
// BAD: カタログ全体が1つの Aggregate Root の下にある
public class Catalog : Entity<CatalogId>
{
    public List<Category> Categories { get; } = new();
    // 各カテゴリに数百の商品が含まれる...
    // カタログをロードするとすべてがロードされる
}
```

```csharp
// GOOD: 各 Category が独自の Aggregate で、ID で Catalog を参照する
public sealed class Category : Entity<CategoryId>
{
    public CatalogId CatalogId { get; }
    public CategoryName Name { get; private set; }
    private readonly List<ProductReference> _products = new();
    public IReadOnlyList<ProductReference> Products => _products.AsReadOnly();

    // Category は独立してロード・変更可能
}
```

### 4. 子 Entity 用の Repository

```csharp
// BAD: Root でない Entity 用の Repository
public interface IOrderLineRepository
{
    Task<OrderLine?> FindByIdAsync(OrderLineId id);
    Task AddAsync(OrderLine line);
    Task RemoveAsync(OrderLine line);
}

// 使用すると Aggregate Root の不変条件チェックをバイパスする:
var line = new OrderLine(id, productId, quantity, price);
await _orderLineRepo.AddAsync(line); // Aggregate のバリデーションなし!
```

```csharp
// GOOD: Aggregate Root を通じて子にアクセスする
public interface IOrderRepository : IRepository<Order, OrderId>
{
    // OrderLine へのアクセスは Order の Aggregate メソッドを通じて行う:
    //   order.AddLine(productId, quantity, unitPrice);
    //   order.RemoveLine(lineId);
}
```

### 5. インフラ依存のある Domain Layer

```csharp
// BAD: Domain Entity が EF Core とデータアノテーションに依存している
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; } = null!;
}
```

```csharp
// GOOD: プレーンなドメインオブジェクト、ORM マッピングは Infrastructure に配置
public sealed class Order : Entity<OrderId>
{
    public CustomerId CustomerId { get; }
    public Money Total => CalculateTotal();

    // ORM 属性なし、インフラ参照なし
    // EF Core 設定は Infrastructure に配置:
    //   builder.Property(o => o.CustomerId)
    //          .HasConversion(id => id.Value, v => new CustomerId(v));
}
```

### 6. Application Service にあるビジネスロジック

```csharp
// BAD: ドメインルールが Application Service に実装されている
public class PlaceOrderHandler
{
    public async Task Handle(PlaceOrderCommand command)
    {
        var order = await _orderRepo.FindByIdAsync(command.OrderId);

        // ビジネスルールが Application Layer に散在している
        if (order.Lines.Count == 0)
            throw new Exception("No lines");
        if (order.Status != "Draft")
            throw new Exception("Not draft");

        order.Status = "Placed";
        order.Total = order.Lines.Sum(l => l.Quantity * l.UnitPrice);
        order.PlacedAt = DateTime.UtcNow;

        await _orderRepo.SaveAsync(order);
    }
}
```

```csharp
// GOOD: Application Service はオーケストレーションのみ
public sealed class PlaceOrderHandler
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Handle(
        PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.FindByIdAsync(
            new OrderId(command.OrderId), cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        order.Place(); // ドメインロジックは Aggregate に留まる

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

## リファクタリングレシピ

### レシピ: Entity の振る舞いをカプセル化する

1. public setter を持ち振る舞いメソッドのない Entity を特定する
2. 各 public setter について、プロパティを変更するすべての呼び出し箇所を見つける
3. 各変更に対して Entity に名前付きドメインメソッドを作成する（例: `ChangeEmail`、`Place`、`Cancel`）
4. 外部コードの不変条件チェックを新しいドメインメソッドに移動する
5. プロパティの setter を `private set` に変更する
6. すべての呼び出し箇所を新しいドメインメソッドの使用に更新する
7. 外部コードが Entity のプロパティを直接設定していないことを確認する

### レシピ: Value Object を抽出する

1. ドメイン概念に使用されているプリミティブを特定する（email を `string`、money を `decimal`、ID を `Guid`）
2. コンストラクタにバリデーションを持つ `sealed record` を Value Object として作成する
3. 適用可能な場合にドメイン的に意味のあるメソッドや演算子を追加する（`Add`、`Contains`、演算子オーバーロード）
4. Entity のプリミティブフィールドを Value Object 型に置き換える
5. すべての生成箇所を Value Object の構築に更新する
6. 不正な値が構築時に拒否されることを確認する

### レシピ: Aggregate の不変条件を強制する

1. Aggregate Root とその子 Entity および Value Object を特定する
2. Aggregate 内の複数オブジェクトにまたがるビジネスルールを見つける
3. これらのチェックを Aggregate Root のメソッドに移動する（例: `AddLine` が最大行数をチェック）
4. Root のみが子を作成できるように子 Entity のコンストラクタを `internal` にする
5. 外部からの変更を防ぐために子コレクションを `IReadOnlyList<T>` として公開する
6. Aggregate 内のすべての状態変更が Root のメソッドを経由することを確認する

### レシピ: ロジックを Domain に引き込む

1. Application Service にあるビジネスルール、計算、状態遷移を特定する
2. ロジックが属すべき Aggregate または Entity を判断する
3. 操作を実行するメソッドを Entity/Aggregate に作成する
4. ロジックが Aggregate をまたがる場合は Domain Service を作成する
5. Application Service がドメインメソッドを呼び出すように更新する
6. Application Service がオーケストレーションのみであることを確認する: Aggregate をロード、ドメインメソッドを呼び出し、永続化

### レシピ: Aggregate レベルの Repository アクセスを強制する

1. 子 Entity を直接返す Repository Interface やメソッドを特定する
2. 子 Entity 固有の Repository Interface とメソッドを削除する
3. Aggregate Root が子へのアクセスと変更のためのメソッドを提供することを確認する
4. アプリケーションコードを Aggregate Root をロードし、Root のメソッドを通じて子にナビゲートするように更新する
5. Aggregate Root ごとに1つの Repository Interface、子 Entity 用の Repository がないことを確認する

### レシピ: Domain Service を抽出する

1. 別の Aggregate の知識を必要とする Entity 内のドメインロジックを特定する
2. ユビキタス言語でビジネス概念にちなんだ Domain Service クラスを作成する
3. Service はパラメータとしてドメインオブジェクト（Entity、Value Object）を受け取る -- DTO やプリミティブではない
4. Aggregate 横断のロジックを Domain Service メソッドに移動する
5. Service をステートレスに保つ -- ミュータブルなフィールドなし、注入された Repository なし
6. 呼び出しコード（通常は Application Service）を Domain Service をインスタンス化または注入して呼び出すように更新する
