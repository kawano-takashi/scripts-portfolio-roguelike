# C# ユニットテストパターン (xUnit)

## 目次

1. [プロジェクト構成と構造](#プロジェクト構成と構造)
2. [テスト命名規約](#テスト命名規約)
3. [AAA パターン (Arrange-Act-Assert)](#aaa-パターン-arrange-act-assert)
4. [xUnit 属性](#xunit-属性)
5. [Moq による Test Double](#moq-による-test-double)
6. [テストライフサイクル](#テストライフサイクル)
7. [アサーションパターン](#アサーションパターン)
8. [アンチパターンと例](#アンチパターンと例)
9. [リファクタリングレシピ](#リファクタリングレシピ)

---

## プロジェクト構成と構造

テストプロジェクトはソースプロジェクトの構造をミラーする。ソースプロジェクトごとに1つのテストプロジェクト。

### ソリューションレイアウト

```
solution/
├── src/
│   └── MyApp.Domain/
│       ├── Models/
│       │   └── Order.cs
│       └── Services/
│           └── PriceCalculator.cs
└── test/
    └── MyApp.Domain.Tests/
        ├── Models/
        │   └── OrderTests.cs
        └── Services/
            └── PriceCalculatorTests.cs
```

### テストプロジェクト (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Moq" Version="4.*" />
    <PackageReference Include="coverlet.collector" Version="6.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\MyApp.Domain\MyApp.Domain.csproj" />
  </ItemGroup>
</Project>
```

### コードカバレッジ

```bash
# カバレッジ収集
dotnet test --collect:"XPlat Code Coverage"

# HTMLレポート生成（dotnet-reportgenerator-globaltool が必要）
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

設計ルール:
- テストプロジェクト名: `{SourceProject}.Tests`
- テストクラス名: `{ClassUnderTest}Tests`
- テストクラスはソースプロジェクトのフォルダ構成をミラーする
- `IsPackable` を `false` に設定する
- カバレッジ率だけでは品質指標にならない -- 意味のある振る舞いのテストに注力する

---

## テスト命名規約

Microsoft Learn の規約: `[MethodName]_[Scenario]_[ExpectedBehavior]`

アンダースコアで区切られた3つの部分で、何がテストされ、どのような条件下で、何が起こるべきかを伝える。

```csharp
// パターン: [MethodName]_[Scenario]_[ExpectedBehavior]

// PriceCalculator.CalculateDiscount() のテスト
[Fact]
public void CalculateDiscount_GoldTier_Returns10PercentOff() { ... }

[Fact]
public void CalculateDiscount_NegativePrice_ThrowsArgumentException() { ... }

[Fact]
public void CalculateDiscount_ZeroPrice_ReturnsZero() { ... }

[Fact]
public void CalculateDiscount_NullCustomer_ThrowsArgumentNullException() { ... }

// Order.AddLine() のテスト
[Fact]
public void AddLine_ValidProduct_IncreasesLineCount() { ... }

[Fact]
public void AddLine_DuplicateProduct_ThrowsDomainException() { ... }

[Fact]
public void AddLine_MaxLinesReached_ThrowsInvalidOperationException() { ... }
```

設計ルール:
- メソッド名はテスト対象のパブリックメソッドと正確に一致する
- シナリオは実装詳細ではなく条件や入力を説明する
- 期待される振る舞いは動詞で始まる: `Returns`、`Throws`、`Sets`、`Creates`、`DoesNotCall`
- 汎用的な名前を避ける: `Test1`、`WorksCorrectly`、`ShouldPass`
- プロパティテストの場合: `PropertyName_Condition_ExpectedBehavior`（例: `Total_AfterAddingTwoLines_ReturnsSumOfLineTotals`）

---

## AAA パターン (Arrange-Act-Assert)

すべてのテストはコメントと空行で区切られた3つの明確なフェーズに従う。

### 標準 AAA

```csharp
[Fact]
public void Add_TwoPositiveNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(2, 3);

    // Assert
    Assert.Equal(5, result);
}
```

### Test Double を使った AAA

```csharp
[Fact]
public async Task PlaceOrder_ValidOrder_ReturnsSuccessResult()
{
    // Arrange
    var stubRepository = new Mock<IOrderRepository>();
    stubRepository
        .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    var sut = new PlaceOrderUseCase(stubRepository.Object);
    var command = new PlaceOrderCommand(CustomerId: Guid.NewGuid());

    // Act
    var result = await sut.ExecuteAsync(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
}
```

### 例外テストの AAA

```csharp
[Fact]
public void Withdraw_InsufficientFunds_ThrowsDomainException()
{
    // Arrange
    var account = new BankAccount(balance: 100m);

    // Act
    Action act = () => account.Withdraw(200m);

    // Assert
    var exception = Assert.Throws<DomainException>(act);
    Assert.Equal("Insufficient funds.", exception.Message);
}
```

設計ルール:
- 各フェーズは空行とコメント（`// Arrange`、`// Act`、`// Assert`）で区切る
- Act セクションはちょうど1つの文（メソッド呼び出しまたはラムダキャプチャ）を含む
- Assert セクションは1つの論理的概念をテストする（1つの概念に対して複数の `Assert` 呼び出しを使用してもよい）
- Arrange フェーズにアサーションを置かない。Assert フェーズにアレンジを置かない
- 例外の場合、Act で `Action`/`Func` をキャプチャし、Assert でアサートする

---

## xUnit 属性

### [Fact] -- 単一ケーステスト

テストがパラメータなしのちょうど1つのシナリオを持つ場合に `[Fact]` を使用する。

```csharp
[Fact]
public void Validate_EmptyName_ReturnsFalse()
{
    // Arrange
    var validator = new CustomerValidator();

    // Act
    var result = validator.Validate(new Customer { Name = "" });

    // Assert
    Assert.False(result.IsValid);
}
```

### [Theory] + [InlineData] -- シンプルなパラメータ化

プリミティブ（string、int、bool 等）の2〜5個のパラメータセットには `[InlineData]` を使用する。

```csharp
[Theory]
[InlineData("", false)]
[InlineData("A", true)]
[InlineData("Valid Name", true)]
[InlineData(null, false)]
public void Validate_Name_ReturnsExpectedResult(string? name, bool expected)
{
    // Arrange
    var validator = new CustomerValidator();

    // Act
    var result = validator.Validate(new Customer { Name = name });

    // Assert
    Assert.Equal(expected, result.IsValid);
}
```

### [Theory] + [MemberData] -- 再利用可能なデータ

テストデータがテスト間で共有される場合や `[InlineData]` には複雑すぎる場合に `[MemberData]` を使用する。

```csharp
public static IEnumerable<object[]> InvalidEmails => new List<object[]>
{
    new object[] { "" },
    new object[] { "no-at-sign" },
    new object[] { "@no-local-part.com" },
    new object[] { "spaces in@address.com" },
};

[Theory]
[MemberData(nameof(InvalidEmails))]
public void Validate_InvalidEmail_ReturnsFalse(string email)
{
    // Arrange
    var validator = new EmailValidator();

    // Act
    var result = validator.Validate(email);

    // Assert
    Assert.False(result);
}
```

### [Theory] + [ClassData] -- 複雑なデータ

テストデータがオブジェクト構築を必要とする場合や複数のテストクラスで使用される場合に `[ClassData]` を使用する。

```csharp
public class DiscountCalculationTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 100m, LoyaltyTier.Gold, 90m };
        yield return new object[] { 100m, LoyaltyTier.Silver, 95m };
        yield return new object[] { 100m, LoyaltyTier.Bronze, 98m };
        yield return new object[] { 0m, LoyaltyTier.Gold, 0m };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(DiscountCalculationTestData))]
public void CalculateDiscount_VariousTiers_ReturnsExpectedPrice(
    decimal price, LoyaltyTier tier, decimal expected)
{
    // Arrange
    var calculator = new DiscountCalculator();

    // Act
    var result = calculator.Calculate(price, tier);

    // Assert
    Assert.Equal(expected, result);
}
```

### 属性選択ガイド

| シナリオ | 属性 |
|---------|------|
| 単一シナリオ、パラメータなし | `[Fact]` |
| 2〜5個のシンプルなパラメータセット（プリミティブ） | `[Theory]` + `[InlineData]` |
| テストメソッド間で共有する再利用可能なデータ | `[Theory]` + `[MemberData]` |
| テストデータに複雑なオブジェクト構築が必要 | `[Theory]` + `[ClassData]` |

---

## Moq による Test Double

Microsoft Learn では3種類の Test Double を定義している:
- **Fake**: あらゆる Test Double の総称（使い方により Stub にも Mock にもなる）
- **Stub**: 制御された戻り値を提供する。アサート対象にならない。SUT の出力を通じて検証される。
- **Mock**: インタラクションを検証する。Assert フェーズで `Verify()` によりアサートされる。

### Stub（データを提供、アサートしない）

```csharp
[Fact]
public void GetDiscountedPrice_OnTuesday_ReturnsHalfPrice()
{
    // Arrange
    var stubDateTimeProvider = new Mock<IDateTimeProvider>();
    stubDateTimeProvider
        .Setup(d => d.DayOfWeek())
        .Returns(DayOfWeek.Tuesday);
    var calculator = new PriceCalculator(stubDateTimeProvider.Object);

    // Act
    var result = calculator.GetDiscountedPrice(100);

    // Assert
    Assert.Equal(50, result);  // Stub ではなく SUT の戻り値をアサートする
}
```

### Mock（インタラクションを検証）

```csharp
[Fact]
public async Task PlaceOrder_ValidOrder_SavesOrderToRepository()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    var sut = new PlaceOrderUseCase(mockRepository.Object);
    var command = CreateValidPlaceOrderCommand();

    // Act
    await sut.ExecuteAsync(command, CancellationToken.None);

    // Assert
    mockRepository.Verify(
        r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
        Times.Once);  // Mock のインタラクションをアサートする
}
```

### 静的依存関係の Seam パターン

静的呼び出し（`DateTime.Now`、`Guid.NewGuid()`）をインターフェースで包んでテスト可能にする。

```csharp
// インターフェース Seam
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DayOfWeek DayOfWeek();
}

// 本番実装
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DayOfWeek DayOfWeek() => DateTime.UtcNow.DayOfWeek;
}

// テスト内: 制御された値を提供する
var stub = new Mock<IDateTimeProvider>();
stub.Setup(d => d.UtcNow).Returns(new DateTime(2025, 1, 14, 10, 0, 0));
stub.Setup(d => d.DayOfWeek()).Returns(DayOfWeek.Tuesday);
```

### Moq の一般的なパターン

```csharp
// 値を返す
mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Order());

// 入力によって異なる値を返す
mock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
    .ReturnsAsync(existingOrder);
mock.Setup(x => x.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
    .ReturnsAsync((Order?)null);

// 例外をスローする
mock.Setup(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .ThrowsAsync(new DbUpdateException());

// 引数マッチングで検証する
mock.Verify(x => x.AddAsync(
    It.Is<Order>(o => o.CustomerId == expectedCustomerId),
    It.IsAny<CancellationToken>()), Times.Once);

// 呼び出されていないことを検証する
mock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
```

設計ルール:
- 変数名は意図を反映させる: `stubRepository`、`mockNotifier`
- Stub: `Setup(...).Returns(...)` -- `Verify()` 呼び出しなし
- Mock: Assert フェーズで `Verify(...)` -- インタラクションの発生を確認する
- Value Object、DTO、副作用のないシンプルなクラスをモックしない
- テストに関係のない引数には `It.IsAny<T>()` を使用する
- 引数の値が重要な場合は `It.Is<T>(predicate)` を使用する
- Mock より Stub を優先する -- 可能な限り SUT の出力をアサートする

---

## テストライフサイクル

xUnit は `[SetUp]`/`[TearDown]` の代わりにコンストラクタ/`IDisposable` を使用する。共有フィクスチャにより高コストなセットアップの繰り返しを避ける。

### コンストラクタ（テストごとのセットアップ）

コンストラクタは各テストメソッドの前に実行される。軽量で共通のセットアップに使用する。

```csharp
public class PriceCalculatorTests
{
    private readonly PriceCalculator _sut;

    public PriceCalculatorTests()
    {
        _sut = new PriceCalculator();
    }

    [Fact]
    public void Calculate_PositivePrice_ReturnsExpectedResult()
    {
        // Act
        var result = _sut.Calculate(100m);

        // Assert
        Assert.Equal(100m, result);
    }
}
```

### IDisposable（テストごとのクリーンアップ）

テストがクリーンアップを必要とするリソースを作成する場合に `IDisposable` を実装する。

```csharp
public class FileProcessorTests : IDisposable
{
    private readonly string _tempFile;

    public FileProcessorTests()
    {
        _tempFile = Path.GetTempFileName();
    }

    [Fact]
    public void Process_ValidFile_ReturnsContent()
    {
        // Arrange
        File.WriteAllText(_tempFile, "test content");
        var processor = new FileProcessor();

        // Act
        var result = processor.Process(_tempFile);

        // Assert
        Assert.Equal("test content", result);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }
}
```

### IClassFixture\<T\>（クラス単位で共有）

単一クラス内のすべてのテストで高コストなフィクスチャインスタンスを共有する。フィクスチャは1回作成され、すべてのテスト完了後に破棄される。

```csharp
public class DatabaseFixture : IDisposable
{
    public DatabaseFixture()
    {
        Connection = new SqlConnection("...");
        Connection.Open();
    }

    public SqlConnection Connection { get; }

    public void Dispose()
    {
        Connection.Dispose();
    }
}

public class OrderRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetById_ExistingOrder_ReturnsOrder()
    {
        // _fixture.Connection を使用する ...
    }
}
```

### ICollectionFixture\<T\>（クラス横断で共有）

複数のテストクラスでフィクスチャインスタンスを共有する。`[CollectionDefinition]` マーカークラスが必要。

```csharp
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database")]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}

[Collection("Database")]
public class CustomerRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public CustomerRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### ライフサイクル選択ガイド

| 要件 | メカニズム |
|------|-----------|
| テストごとに新鮮な状態 | コンストラクタ |
| テストごとのクリーンアップ | `IDisposable` |
| 1つのテストクラス内での高コストなセットアップ共有 | `IClassFixture<T>` |
| 複数のテストクラス間での高コストなセットアップ共有 | `ICollectionFixture<T>` |

設計ルール:
- ユニットテストにはコンストラクタベースのセットアップを優先する
- 共有フィクスチャは主にインテグレーションテスト（データベース、外部サービス）向け
- `static` フィールドでテスト間のミュータブル状態を共有しない
- `ICollectionFixture<T>` はコレクション内の並列実行を無効にする -- 控えめに使用する
- xUnit 2.x は `[SetUp]` と `[TearDown]` を廃止した -- これらの属性は使用しない

---

## アサーションパターン

### 値アサーション

```csharp
Assert.Equal(expected, actual);
Assert.NotEqual(unexpected, actual);
Assert.True(condition, "optional message");
Assert.False(condition, "optional message");
Assert.Null(value);
Assert.NotNull(value);
Assert.InRange(actual, low, high);
Assert.NotInRange(actual, low, high);
```

### 文字列アサーション

```csharp
Assert.Equal("expected", actual);
Assert.StartsWith("prefix", actual);
Assert.EndsWith("suffix", actual);
Assert.Contains("substring", actual);
Assert.DoesNotContain("substring", actual);
Assert.Matches("regex-pattern", actual);
```

### コレクションアサーション

```csharp
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);
Assert.Equal(expectedCount, collection.Count);
Assert.Contains(expectedItem, collection);
Assert.DoesNotContain(unexpectedItem, collection);
Assert.All(collection, item => Assert.True(item.IsActive));
```

### 例外アサーション

```csharp
// 同期
var exception = Assert.Throws<ArgumentException>(
    () => sut.Process(invalidInput));
Assert.Equal("Expected message.", exception.Message);

// 非同期
var exception = await Assert.ThrowsAsync<NotFoundException>(
    () => sut.GetByIdAsync(unknownId, CancellationToken.None));
Assert.Equal(unknownId, exception.EntityId);
```

### 型アサーション

```csharp
Assert.IsType<OrderPlacedEvent>(domainEvent);
Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
Assert.IsNotType<OrderCancelledEvent>(domainEvent);
```

---

## アンチパターンと例

### 8.1 テスト内のロジック

`if`、`for`、`while`、`switch` を含むテストは脆弱で、失敗を隠す可能性がある。

```csharp
// BAD: テスト内のループ -- 1つのケースが失敗すると後続のケースがテストされない
[Fact]
public void Add_MultipleInputs_ReturnsCorrectResults()
{
    var calculator = new Calculator();
    var testCases = new[] { (1, 2, 3), (0, 0, 0), (-1, 1, 0) };

    foreach (var (a, b, expected) in testCases)
    {
        var result = calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
}

// GOOD: パラメータ化 Theory -- 各ケースが独立して実行される
[Theory]
[InlineData(1, 2, 3)]
[InlineData(0, 0, 0)]
[InlineData(-1, 1, 0)]
public void Add_TwoNumbers_ReturnsSum(int a, int b, int expected)
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(a, b);

    // Assert
    Assert.Equal(expected, result);
}
```

### 8.2 複数の Act

テストごとに複数のアクションがあると、どの振る舞いが失敗を引き起こしたか特定できなくなる。

```csharp
// BAD: 1つのテストで2つの振る舞いをテストしている
[Fact]
public void StringCalculator_AddAndSubtract_ReturnCorrectResults()
{
    var calc = new StringCalculator();

    var addResult = calc.Add("1,2");
    var subResult = calc.Subtract("5,3");

    Assert.Equal(3, addResult);
    Assert.Equal(2, subResult);
}

// GOOD: 振る舞いごとに個別のテスト
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    // Arrange
    var calc = new StringCalculator();

    // Act
    var result = calc.Add("1,2");

    // Assert
    Assert.Equal(3, result);
}

[Fact]
public void Subtract_TwoNumbers_ReturnsDifference()
{
    // Arrange
    var calc = new StringCalculator();

    // Act
    var result = calc.Subtract("5,3");

    // Assert
    Assert.Equal(2, result);
}
```

### 8.3 マジック文字列/数値

説明のないハードコード値はテストの意図を曖昧にする。

```csharp
// BAD: "1001" がなぜ重要なのかの説明がない
[Fact]
public void Process_Input_ThrowsException()
{
    var processor = new InputProcessor();

    Action act = () => processor.Process("1001");

    Assert.Throws<OverflowException>(act);
}

// GOOD: 名前付き定数で境界を説明する
[Fact]
public void Process_ExceedsMaximumValue_ThrowsOverflowException()
{
    // Arrange
    var processor = new InputProcessor();
    const string InputExceedingMaximum = "1001";

    // Act
    Action act = () => processor.Process(InputExceedingMaximum);

    // Assert
    Assert.Throws<OverflowException>(act);
}
```

### 8.4 プライベートメソッドのテスト

リフレクションでプライベートメソッドをテストすると、テストが実装詳細に結合される。

```csharp
// BAD: リフレクションでプライベートメソッドを呼び出している
[Fact]
public void TrimInput_WithSpaces_ReturnsTrimmedString()
{
    var parser = new LogParser();
    var method = typeof(LogParser)
        .GetMethod("TrimInput", BindingFlags.NonPublic | BindingFlags.Instance);

    var result = method!.Invoke(parser, new object[] { " data " });

    Assert.Equal("data", result);
}

// GOOD: プライベートメソッドを使用するパブリック API 経由でテストする
[Fact]
public void ParseLogLine_InputWithLeadingAndTrailingSpaces_ReturnsTrimmedResult()
{
    // Arrange
    var parser = new LogParser();

    // Act
    var result = parser.ParseLogLine(" data ");

    // Assert
    Assert.Equal("data", result);
}
```

### 8.5 ユニットテスト内のインフラ

ユニットテスト内のデータベース接続、ファイルI/O、HTTP呼び出しはテストを低速、脆弱、順序依存にする。

```csharp
// BAD: ユニットテストで実データベースを使用している
[Fact]
public async Task GetCustomer_ExistingId_ReturnsCustomer()
{
    using var context = new AppDbContext(GetRealConnectionString());
    var repository = new CustomerRepository(context);

    var customer = await repository.GetByIdAsync(
        Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

    Assert.NotNull(customer);
}

// GOOD: Test Double でインフラを置き換える
[Fact]
public async Task GetCustomer_ExistingId_ReturnsCustomer()
{
    // Arrange
    var expectedCustomer = new CustomerDto { Name = "Test Customer" };
    var stubRepo = new Mock<ICustomerRepository>();
    stubRepo
        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedCustomer);
    var sut = new GetCustomerUseCase(stubRepo.Object);

    // Act
    var result = await sut.ExecuteAsync(
        new GetCustomerQuery(Guid.NewGuid()), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Test Customer", result.Value.Name);
}
```

### 8.6 セットアップ/ティアダウンの乱用

すべてのテストで必要としない状態を構成する共有セットアップは、隠れた依存関係を作りテストを理解しにくくする。

```csharp
// BAD: コンストラクタがほとんどのテストに無関係なオブジェクトを設定している
public class OrderTests
{
    private readonly Order _order;
    private readonly Customer _customer;
    private readonly List<Product> _products;
    private readonly Mock<IPaymentGateway> _paymentGateway;
    private readonly Mock<INotificationService> _notificationService;

    public OrderTests()
    {
        _customer = new Customer("John", "john@test.com");
        _products = new List<Product> { new("Widget", 9.99m) };
        _paymentGateway = new Mock<IPaymentGateway>();
        _notificationService = new Mock<INotificationService>();
        _order = Order.Create(_customer);
        _order.AddLine(_products[0].Id, new Quantity(1), _products[0].Price);
    }

    [Fact]
    public void Create_ValidCustomer_SetsStatusToDraft()
    {
        // _products、_paymentGateway、_notificationService はここでは無関係
        Assert.Equal(OrderStatus.Draft, _order.Status);
    }
}

// GOOD: 各テストが必要なものだけをアレンジする
public class OrderTests
{
    [Fact]
    public void Create_ValidCustomer_SetsStatusToDraft()
    {
        // Arrange
        var customer = CreateTestCustomer();

        // Act
        var order = Order.Create(customer);

        // Assert
        Assert.Equal(OrderStatus.Draft, order.Status);
    }

    private static Customer CreateTestCustomer()
        => new("John", "john@test.com");
}
```

### 8.7 過度なモッキング

Value Object やシンプルなクラスを含むすべての依存関係をモックすると、テスト価値を向上させずに複雑さが増す。

```csharp
// BAD: Value Object をモックしている
[Fact]
public void CalculateTotal_WithMoney_ReturnsCorrectAmount()
{
    var mockMoney = new Mock<Money>();
    mockMoney.Setup(m => m.Amount).Returns(100m);
    var mockQuantity = new Mock<Quantity>();
    mockQuantity.Setup(q => q.Value).Returns(2);

    var result = CalculateTotal(mockMoney.Object, mockQuantity.Object);

    Assert.Equal(200m, result);
}

// GOOD: 実際の Value Object を使用する -- インフラ依存関係のみモックする
[Fact]
public void CalculateTotal_TwoLineItems_ReturnsSumOfLineTotals()
{
    // Arrange
    var unitPrice = new Money(50m, Currency.USD);
    var order = CreateOrderWithLine(unitPrice, quantity: 2);

    // Act
    var total = order.Total;

    // Assert
    Assert.Equal(new Money(100m, Currency.USD), total);
}
```

---

## リファクタリングレシピ

### レシピ1: ロジックを Theory に置き換える

テストメソッドが複数ケースのテストに `foreach`、`for`、`if`、`switch` を含む場合。

1. ループまたは条件ロジックを含むテストを特定する
2. ループ/条件から変化する入力と期待される出力を抽出する
3. `[Fact]` を `[Theory]` に変更する
4. 各ケースに `[InlineData(...)]` を追加する（複雑なデータの場合は `[MemberData]`/`[ClassData]`）
5. データに一致するパラメータをテストメソッドに追加する
6. ループ/条件を削除する -- テストランナーが反復処理を行う
7. 各データ行がテストエクスプローラで独立したテストとして実行されることを確認する

### レシピ2: 複数 Act テストを分割する

テストメソッドに複数の Act フェーズ（テスト対象への複数メソッド呼び出し）がある場合。

1. 複数の Act フェーズ（SUT への複数メソッド呼び出し）を持つテストを特定する
2. 各 Act について個別のテストメソッドを作成する
3. 共有 Arrange を各新規テストにコピーする（またはヘルパーメソッドを抽出する）
4. 各新規テストはちょうど1つの Act と対応する `Assert` 呼び出しを持つ
5. 各テストメソッドに特定の振る舞いを反映した名前を付ける
6. 元の複数 Act テストを削除する
7. すべてのテストを実行して振る舞いが保持されていることを確認する

### レシピ3: 実装ではなく振る舞いをテストする

テストが観測可能な振る舞いではなく、内部状態、呼び出し回数、メソッド呼び出し順序をアサートしている場合。

1. プライベートフィールド、呼び出し回数、内部メソッド呼び出しをアサートしているテストを特定する
2. 観測可能な振る舞いを特定する: 戻り値、状態変化、例外、副作用
3. 可能な限り Mock の `Verify()` 呼び出しを SUT の出力/状態に対するアサーションに置き換える
4. 真の副作用（通知送信、イベント発行、データ永続化）にのみ `Verify()` を残す
5. 実装詳細ではなく観測可能な振る舞いを説明するようにテスト名を変更する
6. リファクタリング後のテストが SUT の振る舞い変更時にリグレッションを検出することを確認する

### レシピ4: テストヘルパーメソッドを抽出する

複数のテストの Arrange フェーズに重複したオブジェクト構築がある場合。

1. 複数テスト間で繰り返されるオブジェクト構築（特に Arrange 内）を特定する
2. `private static` ヘルパーメソッドを作成する: `CreateValidOrder()`、`CreateTestCustomer()`
3. テスト間で変化する値にはメソッドパラメータを使用し、残りには適切なデフォルト値を使用する
4. 重複した構築をヘルパーメソッド呼び出しに置き換える
5. ヘルパーは同じテストクラスに置く（複数クラスで使用する場合は共有 `TestHelpers` クラス）

### レシピ5: テスト定数を抽出する

テストに説明のないハードコードされたリテラル値（マジック文字列、マジック数値）がある場合。

1. テスト内のハードコードされたリテラル値を特定する
2. その値がドメイン的に意味のある値か任意の値かを判断する
3. ドメイン的に意味のある値の場合、テストクラスの先頭に名前付き `const` または `static readonly` フィールドを作成する
4. 任意の値の場合、「任意の有効な値」であることを伝えるビルダーまたはファクトリメソッドを使用する
5. 元の名前がリテラル値に依存してコンテキストを提供していた場合、テスト名を変更する

### レシピ6: 静的依存関係にインターフェース Seam を導入する

本番コードがテストで制御できない静的メソッド（`DateTime.Now`、`Guid.NewGuid()`、`File.Exists()`）を呼び出している場合。

1. 静的メソッドを呼び出す本番コードを特定する
2. 静的呼び出しをラップするインターフェースを定義する（例: `IDateTimeProvider`、`IGuidGenerator`）
3. 実際の静的メソッドに委譲する本番実装を作成する
4. コンストラクタ経由でテスト対象クラスにインターフェースを注入する
5. テストでは `Mock<IInterface>` を使用して制御された戻り値を提供する
6. DI コンテナに本番実装を登録する
