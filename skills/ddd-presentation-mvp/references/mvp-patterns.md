# C# Presentation Layer MVP パターン（DDD コンテキスト）

## 目次

1. [コア Interface 契約](#コア-interface-契約)
2. [Display Model パターン](#display-model-パターン)
3. [Presenter 構造](#presenter-構造)
4. [Passive View 実装](#passive-view-実装)
5. [Presenter と Application Layer の統合](#presenter-と-application-layer-の統合)
6. [ナビゲーションパターン](#ナビゲーションパターン)
7. [Presentation のエラーハンドリング](#presentation-のエラーハンドリング)
8. [Presenter ユニットテスト](#presenter-ユニットテスト)
9. [アンチパターンと例](#アンチパターンと例)
10. [リファクタリングレシピ](#リファクタリングレシピ)

## コア Interface 契約

IView は Presenter が View に対して何を表示させるかと、View が公開するユーザーアクションを定義する。IPresenter はユーザーインタラクションに応じて View が呼び出せるものを定義する。

```csharp
// IView: 描画契約。Presenter がこれを呼び出して表示を更新する。
public interface IOrderListView
{
    // 表示メソッド -- Presenter がこれを呼び出して状態を View にプッシュする
    void ShowOrders(IReadOnlyList<OrderDisplayModel> orders);
    void ShowLoading(bool isLoading);
    void ShowError(string message);
    void ShowEmpty(string message);

    // ユーザーアクションイベント -- ユーザーが操作したとき View がこれを発行する
    event Func<Task>? OnViewLoaded;
    event Func<Guid, Task>? OnOrderSelected;
    event Func<string, Task>? OnSearchRequested;
    event Func<Task>? OnRefreshRequested;
}
```

```csharp
// IPresenter: テスタビリティとデカップリングのための Presenter 契約を定義する。
public interface IOrderListPresenter : IDisposable
{
    Task InitializeAsync();
    Task SelectOrderAsync(Guid orderId);
    Task SearchAsync(string query);
    Task RefreshAsync();
}
```

IView 設計ルール:
- メソッドは表示更新に `Show{What}` または `Set{What}` と命名する
- イベントはユーザーインタラクションに `On{UserAction}` と命名する
- 非同期イベントシグネチャには `Func<..., Task>` を使用する
- UI フレームワーク型（`Control`、`Component` など）を公開しない
- IView メソッドはドメイン型ではなく Display Model をパラメータとして受け取る

IPresenter 設計ルール:
- メソッドはアクションとして命名する: `InitializeAsync`、`SelectAsync`、`SearchAsync`
- イベント購読解除とクリーンアップのために `IDisposable` を実装する
- すべてのメソッドは `Task` を返す（Use Case 呼び出しのためデフォルトで非同期）

## Display Model パターン

Display Model は Application Layer の DTO を表示用データに変換する。描画に特化してデータをフラット化、フォーマット、結合する。

```csharp
// Application Layer がこの DTO を返す（Query Use Case から）
public sealed record OrderSummaryDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    DateTime PlacedAt);

// Presenter がこれを View 用の Display Model に変換する
public sealed record OrderDisplayModel(
    Guid Id,
    string CustomerName,
    string FormattedTotal,     // "$1,234.56" -- 表示用にフォーマット済み
    string StatusText,         // "Placed" / "Shipped" / "Cancelled"
    string StatusCssClass,     // "status-placed" -- フレームワーク非依存の UI ヒント
    string PlacedDate,         // "Feb 24, 2026" -- ローカライズされた日付文字列
    bool CanCancel);           // 派生したプレゼンテーションロジック
```

```csharp
// Display Model マッパー -- 変換ロジックをテスト可能に保つ
public static class OrderDisplayModelMapper
{
    public static OrderDisplayModel ToDisplayModel(
        OrderSummaryDto dto, CultureInfo culture)
    {
        return new OrderDisplayModel(
            Id: dto.Id,
            CustomerName: dto.CustomerName,
            FormattedTotal: dto.TotalAmount.ToString("C", culture),
            StatusText: dto.Status.ToDisplayString(),
            StatusCssClass: $"status-{dto.Status.ToString().ToLowerInvariant()}",
            PlacedDate: dto.PlacedAt.ToString("MMM dd, yyyy", culture),
            CanCancel: dto.Status is OrderStatus.Placed or OrderStatus.Confirmed);
    }

    public static IReadOnlyList<OrderDisplayModel> ToDisplayModels(
        IEnumerable<OrderSummaryDto> dtos, CultureInfo culture)
        => dtos.Select(d => ToDisplayModel(d, culture)).ToList();
}
```

ガイドライン:
- すべての文字列フォーマットは View ではなくマッパーで行う
- Display Model は不変の record
- 表示コンテキストごとに1つの Display Model（リストアイテム、詳細ビュー、フォームなど）
- 派生した UI 状態（表示フラグ、CSS クラス、有効/無効ヒント）を含める
- Display Model にドメインの振る舞いやバリデーションロジックを含めない

## Presenter 構造

Presenter はすべてのプレゼンテーション状態を管理し、View イベントを購読し、Application Layer の Use Case を呼び出し、IView を通じて View を更新する。

```csharp
public sealed class OrderListPresenter : IOrderListPresenter
{
    private readonly IOrderListView _view;
    private readonly GetOrderListUseCase _getOrderList;
    private readonly CancelOrderUseCase _cancelOrder;
    private readonly INavigationService _navigation;
    private readonly CultureInfo _culture;

    // プレゼンテーション状態は Presenter が保持する
    private IReadOnlyList<OrderSummaryDto> _currentOrders = Array.Empty<OrderSummaryDto>();
    private string _currentSearchQuery = string.Empty;

    public OrderListPresenter(
        IOrderListView view,
        GetOrderListUseCase getOrderList,
        CancelOrderUseCase cancelOrder,
        INavigationService navigation,
        CultureInfo culture)
    {
        _view = view;
        _getOrderList = getOrderList;
        _cancelOrder = cancelOrder;
        _navigation = navigation;
        _culture = culture;

        // コンストラクタで View イベントを購読する
        _view.OnViewLoaded += HandleViewLoaded;
        _view.OnOrderSelected += HandleOrderSelected;
        _view.OnSearchRequested += HandleSearchRequested;
        _view.OnRefreshRequested += HandleRefreshRequested;
    }

    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    public async Task SelectOrderAsync(Guid orderId)
    {
        await _navigation.NavigateToAsync($"orders/{orderId}");
    }

    public async Task SearchAsync(string query)
    {
        _currentSearchQuery = query;
        await LoadOrdersAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        _view.ShowLoading(true);
        try
        {
            var query = new GetOrderListQuery(_currentSearchQuery);
            _currentOrders = await _getOrderList.ExecuteAsync(query);

            if (_currentOrders.Count == 0)
            {
                _view.ShowEmpty("No orders found.");
            }
            else
            {
                var displayModels = OrderDisplayModelMapper
                    .ToDisplayModels(_currentOrders, _culture);
                _view.ShowOrders(displayModels);
            }
        }
        catch (Exception ex)
        {
            _view.ShowError($"Failed to load orders: {ex.Message}");
        }
        finally
        {
            _view.ShowLoading(false);
        }
    }

    // イベントハンドラは public メソッドに委譲する
    private Task HandleViewLoaded() => InitializeAsync();
    private Task HandleOrderSelected(Guid orderId) => SelectOrderAsync(orderId);
    private Task HandleSearchRequested(string query) => SearchAsync(query);
    private Task HandleRefreshRequested() => RefreshAsync();

    public void Dispose()
    {
        _view.OnViewLoaded -= HandleViewLoaded;
        _view.OnOrderSelected -= HandleOrderSelected;
        _view.OnSearchRequested -= HandleSearchRequested;
        _view.OnRefreshRequested -= HandleRefreshRequested;
    }
}
```

Presenter の責務:
- コンストラクタで IView イベントを購読し、Dispose で購読解除する
- すべての状態は Presenter（private フィールド）に持ち、View には持たない
- Application DTO を View にプッシュする前に Display Model に変換する
- エラーを処理し、エラー状態を View にプッシュする
- 抽象化（INavigationService）を通じてナビゲーションを調整する
- データと変更のために Application Layer の Use Case を呼び出す

## Passive View 実装

View は IView を実装する。指示された内容を描画し、ユーザーが操作したときにイベントを発行する。条件ロジック、データ変換、状態を持たない。

```csharp
// フレームワーク非依存の View スケルトン。
// 実際には WPF Window、Blazor コンポーネント、MAUI ページなどになる。
// フレームワーク固有の描画コードのみが異なる。
public class OrderListView : IOrderListView
{
    private readonly IOrderListPresenter _presenter;

    // IView イベント
    public event Func<Task>? OnViewLoaded;
    public event Func<Guid, Task>? OnOrderSelected;
    public event Func<string, Task>? OnSearchRequested;
    public event Func<Task>? OnRefreshRequested;

    public OrderListView(IOrderListPresenter presenter)
    {
        _presenter = presenter;
    }

    // IView 表示メソッド -- データを UI 要素に割り当てるだけ
    public void ShowOrders(IReadOnlyList<OrderDisplayModel> orders)
    {
        // フレームワーク固有: リスト/グリッド/テーブルにバインド
    }

    public void ShowLoading(bool isLoading)
    {
        // フレームワーク固有: ローディングインジケータの表示/非表示
    }

    public void ShowError(string message)
    {
        // フレームワーク固有: エラー通知を表示
    }

    public void ShowEmpty(string message)
    {
        // フレームワーク固有: 空状態を表示
    }

    // UI イベントハンドラ -- イベントを転送するだけ、ロジックなし
    protected async Task HandleSearchButtonClicked(string searchText)
    {
        if (OnSearchRequested is not null)
            await OnSearchRequested.Invoke(searchText);
    }

    protected async Task HandleOrderRowClicked(Guid orderId)
    {
        if (OnOrderSelected is not null)
            await OnOrderSelected.Invoke(orderId);
    }

    protected async Task HandleViewInitialized()
    {
        if (OnViewLoaded is not null)
            await OnViewLoaded.Invoke();
    }

    protected async Task HandleRefreshClicked()
    {
        if (OnRefreshRequested is not null)
            await OnRefreshRequested.Invoke();
    }
}
```

View ルール:
- `Show*` メソッドはデータを UI 要素に割り当てるだけ -- 変換しない
- UI イベントハンドラはイベント発行の呼び出しのみを含む -- 条件分岐やロジックなし
- View は Use Case や Application Layer を直接呼び出さない
- すべての状態（ローディング、エラー、データ）は Presenter から View にプッシュされる
- View は具象 Presenter クラスではなく IPresenter インターフェースに依存する

## Presenter と Application Layer の統合

Presenter は Presentation Layer と Application Layer（Use Case/Command/Query）を橋渡しする。Use Case を呼び出すが、Repository や Domain Aggregate に直接アクセスしない。

```csharp
public sealed class OrderDetailPresenter : IOrderDetailPresenter
{
    private readonly IOrderDetailView _view;
    private readonly GetOrderDetailUseCase _getDetail;
    private readonly CancelOrderUseCase _cancelOrder;

    private OrderDetailDisplayModel? _currentOrder;

    public OrderDetailPresenter(
        IOrderDetailView view,
        GetOrderDetailUseCase getDetail,
        CancelOrderUseCase cancelOrder)
    {
        _view = view;
        _getDetail = getDetail;
        _cancelOrder = cancelOrder;

        _view.OnCancelRequested += HandleCancelRequested;
    }

    public async Task LoadAsync(Guid orderId)
    {
        _view.ShowLoading(true);
        try
        {
            var dto = await _getDetail.ExecuteAsync(
                new GetOrderDetailQuery(orderId));

            if (dto is null)
            {
                _view.ShowError("Order not found.");
                return;
            }

            _currentOrder = OrderDetailDisplayModelMapper.ToDisplayModel(dto);
            _view.ShowOrderDetail(_currentOrder);
        }
        catch (Exception ex)
        {
            _view.ShowError($"Failed to load order: {ex.Message}");
        }
        finally
        {
            _view.ShowLoading(false);
        }
    }

    private async Task HandleCancelRequested()
    {
        if (_currentOrder is null || !_currentOrder.CanCancel) return;

        _view.ShowLoading(true);
        try
        {
            // Command Use Case を呼び出す -- Application Layer を通じて状態を変更する
            await _cancelOrder.ExecuteAsync(
                new CancelOrderCommand(_currentOrder.Id, "User requested"));

            // 変更を反映するためリロードする
            await LoadAsync(_currentOrder.Id);
        }
        catch (Exception ex)
        {
            _view.ShowError($"Failed to cancel order: {ex.Message}");
        }
        finally
        {
            _view.ShowLoading(false);
        }
    }

    public void Dispose()
    {
        _view.OnCancelRequested -= HandleCancelRequested;
    }
}
```

統合ルール:
- Presenter は `IRepository` ではなく Use Case クラスまたはインターフェースに依存する
- 読み取りには Query Use Case、変更には Command Use Case を使用する
- Application Layer DTO は View に届く前に Display Model に変換する
- エラーハンドリングはアプリケーション例外をユーザーフレンドリーなメッセージに変換する

## ナビゲーションパターン

Presenter をフレームワーク非依存に保つためナビゲーションを抽象化する。

```csharp
public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
}

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message);
    Task AlertAsync(string title, string message);
}
```

```csharp
// Presenter での使用例
private async Task HandleDeleteRequested()
{
    var confirmed = await _dialog.ConfirmAsync(
        "Delete Order", "Are you sure you want to delete this order?");

    if (!confirmed) return;

    await _deleteOrder.ExecuteAsync(new DeleteOrderCommand(_currentOrder!.Id));
    await _navigation.GoBackAsync();
}
```

## Presentation のエラーハンドリング

```csharp
// Application Layer が Result<T> パターンを使用する場合
public async Task LoadAsync(Guid orderId)
{
    _view.ShowLoading(true);
    try
    {
        var result = await _getDetail.ExecuteAsync(new GetOrderDetailQuery(orderId));

        result.Match(
            onSuccess: dto =>
            {
                _currentOrder = OrderDetailDisplayModelMapper.ToDisplayModel(dto);
                _view.ShowOrderDetail(_currentOrder);
            },
            onFailure: error => _view.ShowError(error));
    }
    catch (Exception ex)
    {
        // 予期しないエラー -- ログに記録し汎用メッセージを表示する
        _view.ShowError("An unexpected error occurred. Please try again.");
    }
    finally
    {
        _view.ShowLoading(false);
    }
}
```

エラーハンドリングガイドライン:
- 予期されたエラー（バリデーション、未検出）は Result パターンで処理する -- 具体的なメッセージを表示する
- 予期しないエラーは Presenter レベルでキャッチする -- 汎用的なユーザーメッセージを表示する
- 例外がハンドリングされずに View に伝播しないようにする
- ローディング状態は常に try/finally で切り替える

## Presenter ユニットテスト

Presenter は IView と Application Layer Use Case にのみ依存するため、UI フレームワークなしで完全にテスト可能である。

```csharp
public class OrderListPresenterTests
{
    private readonly Mock<IOrderListView> _viewMock;
    private readonly Mock<GetOrderListUseCase> _getOrderListMock;
    private readonly Mock<INavigationService> _navigationMock;
    private readonly OrderListPresenter _sut;

    public OrderListPresenterTests()
    {
        _viewMock = new Mock<IOrderListView>();
        _getOrderListMock = new Mock<GetOrderListUseCase>();
        _navigationMock = new Mock<INavigationService>();

        _sut = new OrderListPresenter(
            _viewMock.Object,
            _getOrderListMock.Object,
            Mock.Of<CancelOrderUseCase>(),
            _navigationMock.Object,
            CultureInfo.InvariantCulture);
    }

    [Fact]
    public async Task InitializeAsync_WithOrders_ShowsOrderDisplayModels()
    {
        // Arrange
        var orders = new List<OrderSummaryDto>
        {
            new(Guid.NewGuid(), "Alice", 100m, "USD",
                OrderStatus.Placed, DateTime.UtcNow)
        };
        _getOrderListMock
            .Setup(x => x.ExecuteAsync(It.IsAny<GetOrderListQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        await _sut.InitializeAsync();

        // Assert
        _viewMock.Verify(v => v.ShowLoading(true), Times.Once);
        _viewMock.Verify(v => v.ShowOrders(
            It.Is<IReadOnlyList<OrderDisplayModel>>(
                list => list.Count == 1 && list[0].CustomerName == "Alice")),
            Times.Once);
        _viewMock.Verify(v => v.ShowLoading(false), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenEmpty_ShowsEmptyState()
    {
        _getOrderListMock
            .Setup(x => x.ExecuteAsync(It.IsAny<GetOrderListQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderSummaryDto>());

        await _sut.InitializeAsync();

        _viewMock.Verify(v => v.ShowEmpty("No orders found."), Times.Once);
        _viewMock.Verify(v => v.ShowOrders(It.IsAny<IReadOnlyList<OrderDisplayModel>>()),
            Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WhenUseCaseFails_ShowsError()
    {
        _getOrderListMock
            .Setup(x => x.ExecuteAsync(It.IsAny<GetOrderListQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB connection failed"));

        await _sut.InitializeAsync();

        _viewMock.Verify(v => v.ShowError(It.Is<string>(
            s => s.Contains("Failed to load orders"))), Times.Once);
        _viewMock.Verify(v => v.ShowLoading(false), Times.Once);
    }

    [Fact]
    public async Task SelectOrderAsync_NavigatesToOrderDetail()
    {
        var orderId = Guid.NewGuid();

        await _sut.SelectOrderAsync(orderId);

        _navigationMock.Verify(
            n => n.NavigateToAsync($"orders/{orderId}"), Times.Once);
    }
}
```

テスト戦略:
- IView をモックして Presenter が View に何を表示させるかを検証する
- Use Case をモックして Application Layer のレスポンスを制御する
- ローディング、空、エラーの各状態を別のテストケースとしてテストする
- ユーザーアクション（選択、検索、リフレッシュ）が正しい振る舞いをトリガーすることをテストする
- Dispose がイベントの購読解除を行うことをテストする
- UI フレームワーク不要 -- テストは純粋な C# ユニットテスト

## アンチパターンと例

### 1. Smart View（View 内のロジック）

```csharp
// BAD: View がデータに基づいて何を表示するか判断している
public void ShowOrders(IReadOnlyList<OrderSummaryDto> orders)
{
    foreach (var order in orders)
    {
        var color = order.Status == OrderStatus.Cancelled ? "red" : "green";
        var formatted = $"${order.TotalAmount:N2}";
        RenderRow(order.CustomerName, formatted, color);
    }
}

// GOOD: View は事前計算された Display Model を描画するだけ
public void ShowOrders(IReadOnlyList<OrderDisplayModel> orders)
{
    foreach (var order in orders)
    {
        RenderRow(order.CustomerName, order.FormattedTotal, order.StatusCssClass);
    }
}
```

### 2. Presenter 内のドメインロジック

```csharp
// BAD: Presenter にビジネスルールが含まれている
public async Task CancelOrderAsync()
{
    if (_currentOrder.Status == OrderStatus.Shipped)
        throw new InvalidOperationException("Cannot cancel shipped orders");
    if (DateTime.UtcNow - _currentOrder.PlacedAt > TimeSpan.FromDays(30))
        throw new InvalidOperationException("Too late to cancel");
    await _cancelOrder.ExecuteAsync(new CancelOrderCommand(_currentOrder.Id));
}

// GOOD: Presenter は Application Layer に委譲し、表示レベルのフラグを使用する
public async Task CancelOrderAsync()
{
    if (!_currentOrder.CanCancel)
    {
        _view.ShowError("This order cannot be cancelled.");
        return;
    }
    await _cancelOrder.ExecuteAsync(
        new CancelOrderCommand(_currentOrder.Id, "User requested"));
}
```

### 3. Presenter が Repository に直接アクセスする

```csharp
// BAD: Presenter が Application Layer をバイパスしている
public class OrderListPresenter
{
    private readonly IOrderRepository _repo;

    public async Task LoadAsync()
    {
        var orders = await _repo.GetAllAsync();
        _view.ShowOrders(orders.Select(o => MapToDisplay(o)).ToList());
    }
}

// GOOD: Presenter は Use Case を呼び出す
public class OrderListPresenter
{
    private readonly GetOrderListUseCase _getOrderList;

    public async Task LoadAsync()
    {
        var dtos = await _getOrderList.ExecuteAsync(new GetOrderListQuery());
        _view.ShowOrders(OrderDisplayModelMapper.ToDisplayModels(dtos));
    }
}
```

### 4. フレームワーク結合した Presenter

```csharp
// BAD: Presenter に WPF 型がある
public class OrderPresenter
{
    private readonly Dispatcher _dispatcher;

    public async Task LoadAsync()
    {
        var orders = await _getOrderList.ExecuteAsync(query);
        _dispatcher.Invoke(() => _view.ShowOrders(orders));
    }
}

// GOOD: スレッドマーシャリングは View の責務
public class OrderPresenter
{
    public async Task LoadAsync()
    {
        var orders = await _getOrderList.ExecuteAsync(query);
        _view.ShowOrders(orders);
    }
}
```

## リファクタリングレシピ

### レシピ: View からロジックを Presenter に抽出する

1. View 内の条件分岐、フォーマット、データ変換を特定する
2. 事前計算された値を保持する Display Model を作成または拡張する
3. ロジックを Presenter（または Display Model マッパー）に移動する
4. View メソッドを Display Model のフィールドを割り当て/描画するだけに簡素化する
5. View の `Show*` メソッドに `if`、`switch`、文字列フォーマット、計算がないことを確認する

### レシピ: ドメインロジックを Application Layer に押し下げる

1. Presenter 内のビジネスルール（不変条件チェック、状態遷移、ドメイン状態に基づく計算）を特定する
2. チェックを Aggregate メソッドに移動するか、新しい Use Case を作成する
3. Presenter は表示レベルのフラグ（例: DTO データから Display Model マッパーが計算する `CanCancel`）のみをチェックすべき
4. Presenter にドメイン列挙型、Domain Value Object、ドメインルールへの参照がないことを確認する

### レシピ: Interface 契約を導入する

1. 具象 View クラスから IView インターフェースを抽出する: すべての public `Show*` メソッドとイベント
2. 具象 Presenter クラスから IPresenter インターフェースを抽出する: すべての public アクションメソッド
3. Presenter のコンストラクタを IView（具象 View ではなく）を受け取るように更新する
4. View のコンストラクタを IPresenter（具象 Presenter ではなく）を受け取るように更新する
5. DI 登録を更新して Interface を実装にバインドする
6. Presenter がモックされた IView で完全にテスト可能であることを確認する

### レシピ: Display Model を導入する

1. Presenter が Application DTO やドメイン型を View に直接渡している箇所を特定する
2. View が必要とするすべてのフィールドを事前フォーマット済みで持つ Display Model record を作成する
3. DTO から Display Model への変換用の static マッパークラスを作成する
4. Presenter を `_view.Show*()` 呼び出しの前にマッパーを使用するように更新する
5. IView のシグネチャを DTO ではなく Display Model を受け取るように更新する
6. View が Application やドメインの名前空間をインポートする必要がなくなったことを確認する

### レシピ: 責務ごとに Presenter を分割する

1. 単一の Presenter 内の異なる機能領域（例: 注文リスト + 注文検索 + 注文エクスポート）を特定する
2. IView イベントと `Show*` メソッドを機能領域ごとにグループ化する
3. 機能ごとに別々の IView + IPresenter ペアに分割する
4. 共有状態（あれば）を共有サービスまたは状態ホルダーに抽出する
5. 各 Presenter の注入される依存は最大4〜5個にする

### レシピ: Navigation Service を抽出する

1. Presenter 内のナビゲーション呼び出し（ページ遷移、ダイアログ表示、メッセージボックス）を特定する
2. INavigationService および/または IDialogService インターフェースを作成する
3. ナビゲーションロジックをサービス実装に移動する
4. サービスを Presenter に注入する
5. フレームワーク固有のナビゲーションコードはサービス実装にのみ存在させる
