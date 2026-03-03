# Application Layer リファクタリングチェックリスト

## 1. リファクタリング前分析

リファクタリング前にコードベース分析を実施する:

1. **Application Layer を特定する**: Use Case を含むプロジェクト/フォルダを見つける（よくある名前: `Application/`、`UseCases/`、`Commands/`、`Queries/`、`Services/`、`Handlers/`）
2. **Use Case を棚卸しする**: すべての Command および Query Use Case をその Request/Response 型とともに一覧にする
3. **ドメイン依存をマッピングする**: 各 Use Case について、どの Aggregate、Repository、Domain Service に触れているかを一覧にする
4. **インフラ依存をマッピングする**: 各 Use Case について、どの Port Interface に依存しているかを一覧にする
5. **問題点を特定する**: 下記のチェックリストを適用して違反を見つける

---

## 2. Use Case 健全性チェックリスト

- [ ] 各 Use Case がちょうど1つのアプリケーション操作を処理している
- [ ] Use Case クラスが動詞句で命名されている（`PlaceOrderUseCase`、`GetOrderDetailQuery`）
- [ ] `IUseCase<TRequest, TResponse>` または特化バリアントを実装している
- [ ] ステートレスである（ミュータブルなインスタンスフィールドがない、static な状態がない）
- [ ] オーケストレーションロジックのみを含む（ロード、ドメインへの委譲、永続化、イベントディスパッチ）
- [ ] ビジネスルールが Domain Aggregate/Service に委譲されている
- [ ] ドメイン操作の前に入力をバリデーションしている
- [ ] 予期されたエラーに対して生の値や例外ではなく `Result<T>` を返している
- [ ] `CancellationToken` を受け取り、すべての非同期呼び出しに転送している
- [ ] 注入される依存が最大5〜6個である
- [ ] 横断的関心事（ロギング、認可、メトリクス）が Use Case 本体に混在していない

---

## 3. DTO 健全性チェックリスト

- [ ] Request DTO が sealed record である（不変）
- [ ] Request DTO がマーカーインターフェース（`ICommand`、`IQuery<T>`）を実装している
- [ ] Response DTO が sealed record である（不変）
- [ ] プリミティブ型、文字列、日時、ネストされた DTO、DTO のコレクションのみを含む
- [ ] ドメイン型（Entity、Value Object、ドメイン列挙型）を参照していない
- [ ] インフラ型を参照していない
- [ ] Request と Response が構造的に類似していても別の型である
- [ ] コレクションに `List<T>` や `IEnumerable<T>` ではなく `IReadOnlyList<T>` を使用している
- [ ] オプションフィールドに Nullable 参照型（`string?`）を使用している

---

## 4. Port/Interface 健全性チェックリスト

- [ ] すべてのインフラ依存が Port Interface で抽象化されている
- [ ] Port Interface が Infrastructure ではなく Application Layer プロジェクトに定義されている
- [ ] Port 名が技術ではなく機能を表している（`ISmtpClient` ではなく `INotificationSender`）
- [ ] Repository Port が読み取り（DTO を返す）と書き込み（ドメイン Entity を受け取る）を分離している
- [ ] すべての Port メソッドが最後のパラメータとして `CancellationToken` を受け取っている
- [ ] Port メソッドシグネチャがインフラ型（`DbConnection`、`HttpResponseMessage`）を公開していない
- [ ] 外部サービス Port が外部 API の提供するものではなく、アプリケーションが必要とするものをモデル化している
- [ ] `IUnitOfWork` がトランザクション管理の Port として定義されている

---

## 5. 依存関係 健全性チェックリスト

- [ ] Use Case が依存するのは: Port Interface、ドメイン型、Application Layer の抽象のみ
- [ ] Infrastructure への直接参照がない（`DbContext`、`HttpClient`、`SmtpClient`）
- [ ] Presentation Layer への直接参照がない（Controller、ViewModel、Presenter）
- [ ] Application Layer プロジェクトが参照するのは: Domain Layer プロジェクトのみ
- [ ] Infrastructure プロジェクトが参照するのは: Application Layer プロジェクト（Port の実装のため）
- [ ] Application Layer と他レイヤー間に循環依存がない
- [ ] Use Case が他の Use Case に依存していない（Domain Event で連携する）

---

## 6. バリデーション健全性チェックリスト

- [ ] すべての Command Use Case がドメイン操作の前に入力をバリデーションしている
- [ ] すべての Command Request DTO に Validator が存在する
- [ ] 入力バリデーションがチェックするもの: 必須フィールド、文字列長、数値範囲、フォーマット制約
- [ ] 入力バリデーションがビジネスルール（ドメインバリデーション）をチェックしていない
- [ ] バリデーションエラーがフィールドレベルのエラーメッセージとともに `Result.Failure` を返している
- [ ] 基本的な入力バリデーションにデータベース制約やドメイン例外に依存していない
- [ ] Query Use Case が ID フォーマットとページネーションパラメータをバリデーションしている

---

## 7. エラーハンドリング健全性チェックリスト

- [ ] Use Case がすべての予期されたエラーに対して `Result<T>` を返している
- [ ] 予期されたエラー: バリデーションエラー、未検出、ドメインルール違反
- [ ] 例外は予期しないエラー（インフラ障害、プログラミングエラー）にのみ使用されている
- [ ] ビジネスロジック例外が Use Case から漏れていない
- [ ] Domain Aggregate の `Result` エラーが伝播されている（握りつぶしや再スローされていない）
- [ ] エラーメッセージが説明的だが内部詳細を漏らしていない
- [ ] 呼び出し元が `Result.IsSuccess` / `IsFailure` を明示的にハンドリングしている

---

## 8. 診断テンプレート

```
## Application Layer 診断

### 概要
- Application プロジェクト: [パス]
- Command Use Case 数: [件数]
- Query Use Case 数: [件数]
- Port Interface 数: [件数]
- DTO 数（Request + Response）: [件数]
- Validator 数: [件数]

### 検出されたコードスメル

#### [スメル名]
- 場所: [ファイル:行]
- 説明: [何が問題か]
- 影響: [なぜ重要か]
- 推奨修正: [適用すべきリファクタリングレシピ]

### Use Case 分析
- 肥大化した Use Case（Use Case 内のドメインロジック）: [リスト]
- インフラ結合: [リスト]
- 貧血 Use Case（CRUD パススルー）: [リスト]
- 入力バリデーション不足: [リスト]
- 横断的関心事の混在: [リスト]
- 6個以上の依存: [リスト]
- 共有ミュータブル状態: [リスト]

### Port 分析
- Port Interface の欠如（インフラへの直接アクセス）: [リスト]
- 誤ったレイヤーに定義された Port: [リスト]
- インフラ型を公開する Port: [リスト]

### DTO 分析
- ドメイン型を参照する DTO: [リスト]
- ミュータブルな DTO（record ではなく class）: [リスト]
- DTO の欠如（ドメイン型を直接使用）: [リスト]

### 依存関係分析
- 循環依存: [リスト]
- Application から Infrastructure への直接参照: [リスト]
- Application から Presentation への直接参照: [リスト]
- 他の Use Case に依存する Use Case: [リスト]

### エラーハンドリング分析
- 予期されたエラーに例外をスローする Use Case: [リスト]
- Result パターンの欠如: [リスト]
- 握りつぶされたドメインエラー: [リスト]

### 優先度
1. [最も影響の大きいリファクタリング]
2. [2番目の優先事項]
3. [3番目の優先事項]
```
