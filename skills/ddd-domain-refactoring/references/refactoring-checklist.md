# Domain Layer リファクタリングチェックリスト (DDD 戦術的パターン)

## 目次

1. [リファクタリング前分析](#リファクタリング前分析)
2. [Entity 健全性チェックリスト](#entity-健全性チェックリスト)
3. [Value Object 健全性チェックリスト](#value-object-健全性チェックリスト)
4. [Aggregate 健全性チェックリスト](#aggregate-健全性チェックリスト)
5. [Domain Service 健全性チェックリスト](#domain-service-健全性チェックリスト)
6. [Repository Interface 健全性チェックリスト](#repository-interface-健全性チェックリスト)
7. [診断テンプレート](#診断テンプレート)

## リファクタリング前分析

リファクタリング前にコードベース分析を実施する:

1. **Domain Layer を特定する**: ドメインオブジェクトを含むプロジェクト/フォルダを見つける（よくある名前: `Domain`、`Core`、`Model`、`Entities`、`ValueObjects`、`Aggregates`）
2. **Aggregate を棚卸しする**: すべての Aggregate Root、子 Entity、Value Object を一覧にする
3. **Aggregate 境界をマッピングする**: どのオブジェクトがどの Aggregate に属するかを特定する
4. **問題点を特定する**: 下記のチェックリストに記載されたコードスメルを探す

## Entity 健全性チェックリスト

- [ ] Entity が基底クラス `Entity<TId>` を継承している、またはIDベースの等価性を実装している
- [ ] Entity が強い型付けのID（Value Object）を使用しており、生の `Guid`、`int`、`string` ではない
- [ ] Entity に public setter がない -- すべての変更は名前付き振る舞いメソッドを通じて行われる
- [ ] 各振る舞いメソッドが状態変更前に自身の不変条件を強制する
- [ ] Entity が不変条件違反時に `DomainException`（またはドメイン固有の例外）をスローする
- [ ] Entity がインフラ型（ORM属性、フレームワークアセンブリ）を参照していない
- [ ] Entity のコンストラクタが `private` または `internal` でファクトリメソッドを持つ、またはすべての入力を検証する
- [ ] 等価性がIDのみに基づいている（`Equals`、`GetHashCode`、`==`、`!=` がオーバーライドされている）
- [ ] Entity に Application Layer や Presentation Layer のロジックが含まれていない

## Value Object 健全性チェックリスト

- [ ] Value Object が `sealed record` である（C# の構造的等価性を活用）
- [ ] Value Object がコンストラクタですべての入力を検証する -- 不正な状態は存在できない
- [ ] Value Object が不変である -- ミュータブルなフィールドがなく、`set` アクセサがない
- [ ] Value Object に識別子がない（`Id` フィールドがない）
- [ ] Value Object が適用可能な場合にドメイン的に意味のあるメソッドまたは演算子を提供する
- [ ] ドメイン概念（email、money、quantity 等）のプリミティブの代わりに Value Object が使用されている
- [ ] Value Object が Infrastructure Layer や Application Layer に依存していない
- [ ] Value Object が意味のある表示のために `ToString` をオーバーライドしている

## Aggregate 健全性チェックリスト

- [ ] Aggregate Root が明確に特定されている（Aggregate ごとに1つの Root）
- [ ] 子 Entity のコンストラクタが `internal` である -- Root のみが子を作成する
- [ ] 子コレクションがミュータブルな `List<T>` ではなく `IReadOnlyList<T>` として公開されている
- [ ] 複数オブジェクトにまたがるすべての不変条件が Root のメソッドで強制されている
- [ ] 外部オブジェクトが子 Entity ではなく Aggregate Root のみを参照している
- [ ] Aggregate が小さい: 同一トランザクションで整合性が必要な Entity のみを含む
- [ ] 外部コードが Aggregate 内部に到達して子 Entity を直接変更していない
- [ ] Root のファクトリメソッドまたはコンストラクタが有効な初期状態を保証する
- [ ] Domain Event を使用する場合、Aggregate Root からのみ発行される

## Domain Service 健全性チェックリスト

- [ ] Domain Service がステートレスである（ミュータブルなインスタンスフィールドがない）
- [ ] Domain Service がドメインオブジェクト（Entity、Value Object）を操作しており、DTO やプリミティブではない
- [ ] Domain Service が真に複数の Aggregate にまたがるロジック、または単一の Entity に属さないロジックを含む
- [ ] Domain Service がインフラ操作を行わない（DB呼び出し、HTTP、ファイルI/O なし）
- [ ] Domain Service が Entity や Value Object に属すべきロジックを重複していない
- [ ] Domain Service がユビキタス言語で命名されている（`OrderHelper` ではなく `OrderPricingService`）

## Repository Interface 健全性チェックリスト

- [ ] Aggregate Root ごとに1つの Repository Interface -- 子 Entity や Value Object 用の Repository はない
- [ ] Repository Interface が Infrastructure ではなく Domain Layer プロジェクトに定義されている
- [ ] Repository メソッドが個別の子 Entity ではなく Aggregate Root を返す
- [ ] Repository がコレクション風のセマンティクスを使用する: `FindByIdAsync`、`AddAsync`、`RemoveAsync`
- [ ] Repository メソッドがドメイン型（DTO や ORM Entity ではない）を受け取り返す
- [ ] すべての非同期メソッドに `CancellationToken` パラメータが含まれている
- [ ] Repository Interface に複雑なクエリ/フィルタリングロジックがない（複雑なクエリには CQRS リードモデルを使用する）
- [ ] Repository Interface が実装詳細を公開していない（`IQueryable`、`DbContext` がない）

## 診断テンプレート

既存コードのリファクタリング機会を分析する際にこのテンプレートを使用する:

```
## Domain Layer 診断 (DDD 戦術的パターン)

### 概要
- Domain プロジェクト: [パス]
- Aggregate Root 数: [件数]
- Entity 数（合計）: [件数]
- Value Object 数: [件数]
- Domain Service 数: [件数]
- Repository Interface 数: [件数]

### 検出されたコードスメル

#### [スメル名]
- 場所: [ファイル:行]
- 説明: [何が問題か]
- 影響: [なぜ重要か]
- 推奨修正: [適用すべきリファクタリングレシピ]

### Entity 分析
- public setter を持つ Entity（貧血）: [リスト]
- プリミティブID を使用する Entity: [リスト]
- インフラ依存を持つ Entity: [リスト]
- 振る舞いメソッドのない Entity: [リスト]

### Value Object 分析
- Value Object にすべきプリミティブ型: [リスト]
- ミュータブルな状態を持つ Value Object: [リスト]
- バリデーションのない Value Object: [リスト]

### Aggregate 分析
- 不変条件の強制がない Aggregate: [リスト]
- 子コレクションが公開されている Aggregate: [リスト]
- 大きすぎる Aggregate（多くの子 Entity）: [リスト]
- 独自の Repository からアクセス可能な子 Entity: [リスト]

### Domain Service 分析
- ステートフルな Domain Service: [リスト]
- Entity メソッドであるべき Domain Service: [リスト]
- Domain に属すべき Application Service 内のビジネスロジック: [リスト]

### Repository 分析
- Aggregate Root でない Entity 用の Repository: [リスト]
- Infrastructure プロジェクト内の Repository Interface: [リスト]
- IQueryable や ORM 型を公開する Repository: [リスト]

### 優先度
1. [最も影響の大きいリファクタリング]
2. [2番目の優先事項]
3. [3番目の優先事項]
```
