---
name: ddd-application-usecase
description: Refactor and maintain C# application layers following Use Case-centric design in a DDD architecture. Covers use case orchestration, application services, DTOs, port interfaces, Result pattern, input validation, transaction boundaries, and domain event dispatching. Use when working with application layer, use case, application service, command handler, query handler, IUseCase, DTO mapping, port interface, IRepository, Result pattern, input validation, Unit of Work, domain event dispatching, or application layer refactoring.
---

# DDD Application Layer - Use Case 中心設計 (C#)

Use Case オーケストレーション、クリーンな Port 境界、Domain Layer および Infrastructure Layer との厳格な分離を徹底し、C# の Application Layer をリファクタリング・保守する。

## 基本コンセプト

- **Use Case（単一のアプリケーション操作をオーケストレーションする）**: Request DTO を受け取り、Port Interface を通じてドメインオブジェクトを調整し、Result を返す。ビジネスロジックは含まない。
- **Ports（インフラの境界を定義する）**: Application Layer が所有する Interface（`IRepository`、`IPaymentGateway`）。Infrastructure がそれを実装する。依存性逆転: Application は抽象に依存し、Infrastructure には依存しない。

## ワークフロー

タスクの種類を判断する:
- 既存コードの分析? -> 分析ワークフロー
- 既存コードのリファクタリング? -> リファクタリングワークフロー
- 新規 Use Case の作成? -> 作成ワークフロー
- アーキテクチャのレビュー? -> レビューワークフロー

### 分析ワークフロー

1. Application Layer のプロジェクト/フォルダを特定する（検索対象: `Application/`、`UseCases/`、`Commands/`、`Queries/`、`Services/`、`Handlers/`）
2. すべての Use Case とその Request/Response 型を棚卸しする
3. Use Case からドメインへの依存をマッピングする（Aggregate、Repository、Domain Service）
4. Use Case からインフラへの依存をマッピングする（Port Interface）
5. `references/refactoring-checklist.md` のチェックリストを適用する
6. 診断テンプレートを使用して結果を報告する

### リファクタリングワークフロー

1. まず分析ワークフローを実行する
2. 影響度で優先順位を付ける: ドメインロジックの漏洩 > インフラ結合 > 肥大化した Use Case > 構造的問題
3. 適切なリファクタリングレシピを `references/application-patterns.md` から選択して適用する:
   - **Use Case 内のドメインロジック** -> 「ドメインロジックを Domain Layer に押し下げる」
   - **インフラ結合** -> 「Port Interface を抽出する」
   - **肥大化した Use Case** -> 「責務ごとに Use Case を分割する」
   - **貧血 Use Case** -> 「Use Case の必要性を評価する」
   - **バリデーション不足** -> 「Request バリデーションを導入する」
   - **横断的関心事の混在** -> 「Decorator/Pipeline で横断的関心事を抽出する」
   - **Result パターンの欠如** -> 「Result パターンを導入する」
   - **トランザクション境界の欠如** -> 「Unit of Work を導入する」
4. リファクタリングは一度に1つずつ適用し、各リファクタリング間でテストがパスすることを確認する
5. `references/refactoring-checklist.md` のチェックリストを再実行する

### 作成ワークフロー

1. Request DTO を定義する（`ICommand` または `IQuery<T>` を実装する sealed record）
2. Response DTO を定義する（表示に適したデータを持つ sealed record）
3. 必要なインフラ用の Port Interface を定義または再利用する
4. `IValidator<TRequest>` を実装する Validator を作成する
5. 5ステップ構造に従って Use Case クラスを実装する: Load -> Domain Logic -> Persist -> Events -> Commit
6. `references/application-patterns.md` のパターンに従う
7. `references/refactoring-checklist.md` の健全性チェックリストを実行する

### レビューワークフロー

1. `references/refactoring-checklist.md` のすべてのチェックリストを実行する
2. 診断テンプレートを使用して診断レポートを作成する
3. 優先度付きの改善提案を行う

## 基本原則

- **1つの Use Case、1つの操作**: 各 Use Case クラスはちょうど1つのアプリケーション操作を処理する。動詞句で命名する: `PlaceOrderUseCase`、`GetOrderDetailQueryHandler`、`CancelOrderCommandHandler`
- **オーケストレーション、ロジックではない**: Use Case はドメインオブジェクトと Port を調整する。ビジネスルールは Domain Aggregate と Domain Service に存在し、Use Case には決して含まない
- **Port の所有権**: Application Layer が Port Interface を所有する。Infrastructure がそれを実装する。依存性逆転
- **境界での DTO**: Use Case は Request DTO を受け取り、Response DTO を返す。ドメイン Entity を呼び出し元に公開しない
- **例外よりも Result**: 予期されたエラー（バリデーション、未検出、ビジネスルール違反）には `Result<T>` を使用する。例外は予期しないインフラ障害にのみ使用する
- **入口でのバリデーション**: ドメイン操作の前に Application 境界で入力バリデーションを行う。ドメインバリデーションは Aggregate 内部で不変条件を強制する
- **Use Case ごとのトランザクション**: 各 Command Use Case = 1つのトランザクション境界。最後に1回 Commit する

## アンチパターン

- **肥大化した Use Case**: ビジネスルール、複雑な条件分岐、計算が Domain Layer ではなく Use Case に存在する
- **ドメインロジックの漏洩**: 状態遷移ルールや不変条件が Domain Aggregate/Service ではなく Use Case で実装されている
- **インフラ結合**: Port Interface ではなく `DbContext`、`HttpClient`、`SmtpClient` に直接依存している
- **貧血 Use Case**: 調整、バリデーション、エンリッチメントなしに Repository に単に委譲するだけの CRUD パススルー
- **バリデーション不足**: Application 境界で入力バリデーションがなく、データベース制約やドメイン例外に依存している
- **横断的関心事の汚染**: ロギング、認可、キャッシュ、メトリクスが Decorator/Pipeline ではなく Use Case 本体に混在している
- **共有ミュータブル状態**: static フィールドや Singleton サービスが Use Case の呼び出し間で状態を共有している

## 参考資料

- **パターン、コード例、リファクタリングレシピ**: [references/application-patterns.md](references/application-patterns.md)
- **健全性チェックリストと診断テンプレート**: [references/refactoring-checklist.md](references/refactoring-checklist.md)
