---
name: ddd-domain-refactoring
description: Refactor and maintain C# domain layers following DDD tactical patterns. Use when working with entities, value objects, aggregates, domain services, or repository interfaces in a domain-driven design architecture. Triggers on domain layer, entity, value object, aggregate, aggregate root, domain service, repository interface, domain refactoring, invariant, consistency boundary, ubiquitous language, domain event, identity, structural equality, and domain model maintenance.
compatibility: vscode
metadata:
  triggers:
    - domain layer
    - entity
    - value object
    - aggregate
    - aggregate root
    - domain service
    - repository interface
    - domain refactoring
    - invariant
    - ubiquitous language
    - domain model
    - anemic domain
    - consistency boundary
    - domain event
---

# DDD Domain Layer - 戦術的パターン (C#)

DDD 戦術的パターンに従い、ビジネスルールとインフラの明確な分離を維持しながら C# の Domain Layer をリファクタリング・保守する。

## 基本コンセプト

Domain Layer はすべてのビジネスルール、不変条件、ユビキタス言語をカプセル化する。5つの戦術的パターンがこのレイヤーを構成する:
- **Entity** (IDで識別され、ライフサイクルを持つオブジェクト): 永続的な識別子を持ち、振る舞いをカプセル化し不変条件を強制するオブジェクト
- **Value Object** (値で等価比較される不変オブジェクト): 属性によって完全に定義される不変オブジェクトで、プリミティブをドメイン概念に置き換える
- **Aggregate** (整合性境界を持つ Entity の集合): 単一のルートを持つ Entity と Value Object のクラスタで、Entity 間の不変条件を強制する
- **Domain Service** (複数 Aggregate にまたがるステートレスなロジック): 複数の Aggregate にまたがるか、単一の Entity に属さないステートレスな操作
- **Repository Interface** (コレクション風の永続化契約): Aggregate レベルの永続化のために Domain Layer で定義される契約。実装は Infrastructure に配置

## ワークフロー

タスクの種類を判断する:

**既存コードの分析?** --> 下記「分析ワークフロー」に従う
**既存コードのリファクタリング?** --> 下記「リファクタリングワークフロー」に従う
**新規ドメインオブジェクトの作成?** --> 下記「作成ワークフロー」に従う
**アーキテクチャのレビュー?** --> 下記「レビューワークフロー」に従う

## 分析ワークフロー

1. Domain Layer のプロジェクト/フォルダを特定する
   - 検索対象: `Domain/`、`Core/`、`Model/`、`Entities/`、`ValueObjects/`、`Aggregates/`、`DomainServices/`
   - Aggregate Root、Entity、Value Object、Domain Service、Repository Interface を特定する
2. すべての Aggregate、その Root Entity、および含まれる Value Object と子 Entity を棚卸しする
3. Aggregate 境界をマッピングする: どの Entity と Value Object がどの Aggregate に属するか
4. Aggregate Root レベルでの不変条件の強制を確認する
5. [references/refactoring-checklist.md](references/refactoring-checklist.md) の診断テンプレートを適用する
6. 診断テンプレートを使用して結果を報告する

## リファクタリングワークフロー

1. まず分析ワークフローを実行する
2. 影響度で問題に優先順位を付ける（壊れた不変条件 > 貧血ドメインモデル > Value Object の欠如 > 構造的問題）
3. 各問題について、適切なリファクタリングレシピを選択して適用する:
   - **貧血 Entity（public setter、振る舞いなし）** --> [references/domain-patterns.md](references/domain-patterns.md) の「Entity の振る舞いをカプセル化する」
   - **プリミティブ執着（ドメイン概念に生の string/int を使用）** --> [references/domain-patterns.md](references/domain-patterns.md) の「Value Object を抽出する」
   - **不変条件の強制がない Aggregate** --> [references/domain-patterns.md](references/domain-patterns.md) の「Aggregate の不変条件を強制する」
   - **Application/Infrastructure レイヤーにあるビジネスロジック** --> [references/domain-patterns.md](references/domain-patterns.md) の「ロジックを Domain に引き込む」
   - **子 Entity を直接返す Repository** --> [references/domain-patterns.md](references/domain-patterns.md) の「Aggregate レベルの Repository アクセスを強制する」
   - **Entity メソッド内で複数 Aggregate にまたがるロジック** --> [references/domain-patterns.md](references/domain-patterns.md) の「Domain Service を抽出する」
4. リファクタリングは一度に1つずつ適用し、各リファクタリング間でテストがパスすることを確認する
5. リファクタリング後、[references/refactoring-checklist.md](references/refactoring-checklist.md) のチェックリストを再実行する

## 作成ワークフロー

新規ドメインオブジェクトを作成する場合:

1. ビジネス概念を特定し判断する: Entity、Value Object、Aggregate Root のいずれか
2. Aggregate の場合: 整合性境界を定義する（どの子 Entity と Value Object を含むか）
3. まず Value Object を定義する（Entity と Aggregate の構成要素）
4. 不変条件を強制するメソッドを持つ Aggregate Root を実装する
5. Aggregate の Repository Interface を定義する（Aggregate Root ごとに1つ）
6. [references/domain-patterns.md](references/domain-patterns.md) のパターンに従う
7. プロジェクトの既存の命名規約とフォルダ構成に合わせる
8. 新しいコードに対して [references/refactoring-checklist.md](references/refactoring-checklist.md) の健全性チェックリストを実行する

## レビューワークフロー

1. [references/refactoring-checklist.md](references/refactoring-checklist.md) のすべてのチェックリストを実行する:
   - Entity 健全性チェックリスト
   - Value Object 健全性チェックリスト
   - Aggregate 健全性チェックリスト
   - Domain Service 健全性チェックリスト
   - Repository Interface 健全性チェックリスト
2. 診断テンプレートを使用して診断レポートを作成する
3. 優先度付きの改善提案を行う

## 基本原則

- **リッチドメインモデル**: Entity と Aggregate はデータだけでなく振る舞いを含む。ビジネスルールは Application Service ではなくドメインメソッドに存在する。
- **整合性境界としての Aggregate**: Aggregate 内のすべての状態変更は Root を経由する。外部オブジェクトは Root への参照のみを保持する。
- **ドメイン概念には Value Object を使用する**: プリミティブ型（string、int、decimal）を、自己検証し意味を持つ Value Object に置き換える。
- **Root での不変条件の強制**: Aggregate Root がすべてのビジネスルールを検証し、状態変更を受け入れる前に強制する。
- **Aggregate Root ごとに1つの Repository**: Aggregate Root ごとに1つの Repository Interface を Domain Layer に定義する。子 Entity や Value Object 用の Repository は作らない。
- **Domain Layer にインフラ依存を持たない**: Domain プロジェクトは外部を参照しない -- ORM、HTTP、フレームワークアセンブリなし。
- **コードにユビキタス言語を使用する**: クラス名、メソッド名、パラメータ名は技術的な専門用語ではなくビジネスドメインの言語を反映する。

## アンチパターン

- **貧血ドメインモデル**: Entity が public getter/setter を持つデータの入れ物で振る舞いがない。すべてのロジックがドメイン外のサービスに存在する
- **プリミティブ執着**: EmailAddress に `string`、Money に `decimal`、OrderId に `Guid` を使い、型付き Value Object を使わない
- **Aggregate が大きすぎる**: Aggregate が多すぎる Entity を含み、競合やパフォーマンスの問題を引き起こす。真の整合性境界で分割する
- **カプセル化の破壊**: Entity の状態に public setter があり、不変条件チェックをバイパスできる
- **子 Entity 用の Repository**: Aggregate Root でない Entity をロード・保存する Repository メソッド
- **Domain Layer がインフラに依存する**: Domain プロジェクトが EF Core、HTTPクライアント、その他のインフラパッケージを参照している
- **Domain に属するロジックが Application Service にある**: ビジネスルール、計算、状態遷移が Domain Entity ではなく Application Layer で実装されている

## 参考資料

- **ドメインパターン、コード例、リファクタリングレシピ**: [references/domain-patterns.md](references/domain-patterns.md)
- **チェックリストと診断テンプレート**: [references/refactoring-checklist.md](references/refactoring-checklist.md)
