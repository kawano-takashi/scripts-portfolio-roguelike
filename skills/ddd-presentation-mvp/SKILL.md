---
name: ddd-presentation-mvp
description: Refactor and maintain C# presentation layers following the Passive View MVP pattern in a DDD architecture. Framework-agnostic (not tied to WPF, Blazor, MAUI, or any specific UI framework). Use when working with presenters, views, presentation state, display models, IView/IPresenter contracts, or MVP pattern implementation. Triggers on presentation layer, MVP, presenter, passive view, view model, display model, IView, IPresenter, presentation refactoring, UI state management, and separation of presentation concerns.
compatibility: vscode
metadata:
  triggers:
    - presentation layer
    - MVP
    - presenter
    - passive view
    - display model
    - IView
    - IPresenter
    - presentation refactoring
    - UI state management
---

# DDD Presentation Layer - Passive View MVP (C#)

Passive View MVP とApplication Layer からのクリーンな分離を徹底し、C# の Presentation Layer をリファクタリング・保守する。

## 基本コンセプト

Presentation Layer には2つの明確な責務がある:
- **Presenter（見た目の状態を管理する）**: すべてのプレゼンテーションロジックと状態を保持し、View と Application Layer の Use Case を仲介する
- **View（見た目を描画する）**: 受動的で、Presenter が指示する内容を描画する責任のみを持つ

## ワークフロー

タスクの種類を判断する:

**既存コードの分析?** --> 下記「分析ワークフロー」に従う
**既存コードのリファクタリング?** --> 下記「リファクタリングワークフロー」に従う
**新規 Presenter/View の作成?** --> 下記「作成ワークフロー」に従う
**アーキテクチャのレビュー?** --> 下記「レビューワークフロー」に従う

## 分析ワークフロー

1. Presentation Layer のプロジェクト/フォルダを特定する
   - 検索対象: `Presentation/`、`Presenters/`、`Views/`、`ViewModels/`、`DisplayModels/`、`UI/`
   - IView/IPresenter インターフェースとその実装を特定する
2. すべての Presenter とその View 契約を棚卸しする
3. Presenter から Application Layer への依存をマッピングする（各 Presenter がどの Use Case を呼び出すか）
4. フレームワーク固有のコードが Presenter に漏洩していないかチェックする
5. [references/refactoring-checklist.md](references/refactoring-checklist.md) の診断テンプレートを適用する
6. 診断テンプレートを使用して結果を報告する

## リファクタリングワークフロー

1. まず分析ワークフローを実行する
2. 影響度で問題に優先順位を付ける（View 内のロジック > Presenter 内のドメインロジック > 構造的問題）
3. 各問題について、適切なリファクタリングレシピを選択して適用する:
   - **View 内のロジック** --> [references/mvp-patterns.md](references/mvp-patterns.md) の「View からロジックを Presenter に抽出する」
   - **Presenter 内のドメインロジック** --> [references/mvp-patterns.md](references/mvp-patterns.md) の「ドメインロジックを Application Layer に押し下げる」
   - **IView/IPresenter 契約の欠如** --> [references/mvp-patterns.md](references/mvp-patterns.md) の「Interface 契約を導入する」
   - **Presenter が生のドメインオブジェクトを管理** --> [references/mvp-patterns.md](references/mvp-patterns.md) の「Display Model を導入する」
   - **関心事が混在した肥大化 Presenter** --> [references/mvp-patterns.md](references/mvp-patterns.md) の「責務ごとに Presenter を分割する」
   - **ナビゲーション/ダイアログとの密結合** --> [references/mvp-patterns.md](references/mvp-patterns.md) の「Navigation Service を抽出する」
4. リファクタリングは一度に1つずつ適用し、各リファクタリング間でテストがパスすることを確認する
5. リファクタリング後、[references/refactoring-checklist.md](references/refactoring-checklist.md) のチェックリストを再実行する

## 作成ワークフロー

新規 Presenter と View を作成する場合:

1. まず IView インターフェースを定義する（View が表示すべきものとユーザーアクションの公開）
2. IPresenter インターフェースを定義する（View が Presenter に対して呼び出せるもの）
3. View が描画するデータ用の Display Model DTO を作成する
4. Presenter を実装する: Use Case 呼び出しの接続、状態管理、IView を通じた View の更新
5. [references/mvp-patterns.md](references/mvp-patterns.md) のパターンに従う
6. プロジェクトの既存の命名規約とフォルダ構成に合わせる
7. 新しいコードに対して [references/refactoring-checklist.md](references/refactoring-checklist.md) の健全性チェックリストを実行する

## レビューワークフロー

1. [references/refactoring-checklist.md](references/refactoring-checklist.md) のすべてのチェックリストを実行する:
   - View 健全性チェックリスト
   - Presenter 健全性チェックリスト
   - 契約健全性チェックリスト
   - 依存関係健全性チェックリスト
   - Display Model 健全性チェックリスト
2. 診断テンプレートを使用して診断レポートを作成する
3. 優先度付きの改善提案を行う

## 基本原則

- **Passive View**: View はロジックを一切持たない。状態を描画し、ユーザーアクションを転送する。すべての判断は Presenter にある。
- **Presenter = UI 状態のオーケストレーション**: Application Layer の Use Case を通じてデータをロードし、Display Model に変換し、IView を更新する。ドメインロジックは含まない。
- **テスタビリティのための Interface 契約**: IView と IPresenter の Interface により、UI フレームワークなしで Presenter をテストできる。
- **Display Model はドメインモデルではない**: Application DTO を View 固有の Display Model に変換する。ドメイン Entity を View に公開しない。
- **一方向の依存**: Presenter は IView（Interface）に依存する。View は IPresenter（Interface）に依存する。どちらも具象実装に依存しない。
- **Application Layer 境界**: Presenter は Application Layer の Use Case（Command/Query）を呼び出す。Repository や Domain Aggregate に直接アクセスしない。
- **フレームワーク非依存の Presenter**: Presenter コードに UI フレームワーク型（コントロール、コンポーネント、マークアップ）を含めない。フレームワーク固有のものはすべて View の実装に留める。

## アンチパターン

- **Smart View**: View に条件分岐、フォーマット、データ変換、Application Layer への呼び出しが含まれている
- **Presenter がドメインロジックを保持**: ビジネスルール、不変条件チェック、Aggregate 操作が Presenter にある
- **ドメイン型の View への漏洩**: Display Model ではなくドメイン Entity や Value Object を View に直接渡している
- **Presenter が UI フレームワークに依存**: Presenter で WPF Dispatcher、Blazor JSRuntime、MAUI コントロールなどを参照している
- **God Presenter**: 1つの Presenter が無関係な UI 領域を管理している。凝集性のある機能ごとに分割すべき
- **双方向の具象依存**: View が具象 Presenter を保持し、かつ Presenter が具象 View を保持している（Interface を使用する）

## 参考資料

- **MVP パターン、コード例、リファクタリングレシピ**: [references/mvp-patterns.md](references/mvp-patterns.md)
- **チェックリストと診断テンプレート**: [references/refactoring-checklist.md](references/refactoring-checklist.md)
