---
name: csharp-unit-testing
description: Write and maintain C# unit tests following xUnit best practices and Microsoft Learn guidelines. Use when creating, refactoring, or reviewing unit tests, test classes, test projects, or test infrastructure. Triggers on unit test, xUnit, [Fact], [Theory], test method, test class, test project, Arrange-Act-Assert, AAA pattern, test doubles, mock, stub, Moq, test naming, test refactoring, code coverage, InlineData, ClassData, MemberData, IClassFixture, ICollectionFixture, and test best practices.
---

# C# ユニットテスト - xUnit ベストプラクティス (C#)

xUnit パターン、Arrange-Act-Assert 構造、Microsoft Learn のベストプラクティスに従って C# ユニットテストを作成・保守する。

## 基本コンセプト

ユニットテストは個々のコード単位を分離してテストする。効果的なテストを構成する5つの関心事:
- **Arrange-Act-Assert** (テストの構造パターン): すべてのテストは3つのフェーズで構成される -- SUT と依存関係のセットアップ、テスト対象の振る舞いの実行、期待される結果の検証
- **テスト命名** (意図を伝える名前): `[MethodName]_[Scenario]_[ExpectedBehavior]` で何がテストされ、どのような条件下で、何が起こるべきかを伝える
- **Test Double** (依存の置き換え): Fake、Stub、Mock は SUT を分離するために実際の依存関係を置き換える。Stub はデータを提供し、Mock はインタラクションを検証する
- **テスト属性** (パラメータ化テストの制御): 単一ケースのテストには `[Fact]`、データ属性（`[InlineData]`、`[MemberData]`、`[ClassData]`）付きの `[Theory]` でパラメータ化テスト
- **テストライフサイクル** (セットアップと共有状態): テストごとのセットアップにはコンストラクタ、クラス単位の共有状態には `IClassFixture<T>`、クラス横断の共有状態には `ICollectionFixture<T>`

## ワークフロー

タスクの種類を判断する:

**新規テストの作成?** --> 下記「テスト作成ワークフロー」に従う
**既存テストのリファクタリング?** --> 下記「テストリファクタリングワークフロー」に従う
**テストインフラの構築?** --> 下記「セットアップワークフロー」に従う
**テスト品質のレビュー?** --> 下記「レビューワークフロー」に従う

## テスト作成ワークフロー

1. テスト対象のパブリックメソッドまたは振る舞いを特定する
2. テストケースを決定する: 正常系、エッジケース、エラー条件、境界値
3. 適切な属性を選択する: 単一ケースには `[Fact]`、パラメータ化には `[Theory]` + `[InlineData]`
4. テストメソッドを命名する: `[MethodName]_[Scenario]_[ExpectedBehavior]`
5. 明確なセクション分離（`// Arrange`、`// Act`、`// Assert`）で AAA パターンを実装する
6. インフラ依存関係を Test Double に置き換える（データには Stub、インタラクション検証には Mock）
7. [references/testing-patterns.md](references/testing-patterns.md) のパターンに従う

## テストリファクタリングワークフロー

1. まずレビューワークフローを実行して問題を特定する
2. 影響度で問題に優先順位を付ける（脆いテスト > テスト内ロジック > 命名 > 構造的問題）
3. 各問題について、適切なリファクタリングレシピを選択して適用する:
   - **テスト内のロジック (if/for/while)** --> [references/testing-patterns.md](references/testing-patterns.md) の「ロジックを Theory に置き換える」
   - **単一テストに複数の Act** --> [references/testing-patterns.md](references/testing-patterns.md) の「複数 Act テストを分割する」
   - **実装に結合した脆いテスト** --> [references/testing-patterns.md](references/testing-patterns.md) の「実装ではなく振る舞いをテストする」
   - **テスト間で重複するセットアップ** --> [references/testing-patterns.md](references/testing-patterns.md) の「テストヘルパーメソッドを抽出する」
   - **マジック文字列/数値** --> [references/testing-patterns.md](references/testing-patterns.md) の「テスト定数を抽出する」
   - **プライベートメソッドのテスト** --> [references/testing-patterns.md](references/testing-patterns.md) の「パブリック API 経由でテストする」
4. リファクタリングは一度に1つずつ適用し、各リファクタリング間でテストがパスすることを確認する
5. リファクタリング後、[references/testing-checklist.md](references/testing-checklist.md) のチェックリストを再実行する

## セットアップワークフロー

1. テストプロジェクトを作成する: `/test` ディレクトリに `{ProjectName}.Tests`
2. 必要なパッケージを追加する: `Microsoft.NET.Test.Sdk`、`xunit`、`xunit.runner.visualstudio`
3. 必要に応じてモッキングパッケージを追加する: `Moq`
4. テスト対象プロジェクトへの参照を追加する
5. 必要に応じて共有フィクスチャ（`IClassFixture<T>`、`ICollectionFixture<T>`）をセットアップする
6. [references/testing-patterns.md](references/testing-patterns.md) のプロジェクト構成パターンに従う

## レビューワークフロー

1. [references/testing-checklist.md](references/testing-checklist.md) のすべてのチェックリストを実行する:
   - テストメソッド健全性チェックリスト
   - テストクラス健全性チェックリスト
   - テストプロジェクト健全性チェックリスト
2. 診断テンプレートを使用して診断レポートを作成する
3. 優先度付きの改善提案を行う

## 基本原則

- **FIRST プロパティ**: Fast（高速）、Isolated（分離）、Repeatable（再現可能）、Self-Checking（自己検証）、Timely（適時）。すべてのユニットテストは5つすべてを満たす必要がある。
- **テストごとに単一の Act**: 各テストはちょうど1つの振る舞いを検証する。複数のシナリオにはループではなく `[Theory]` を使用する。
- **実装ではなく振る舞いをテストする**: 内部メソッド呼び出しやプライベート状態ではなく、観測可能な結果（戻り値、状態変化、例外）をアサートする。
- **ユニットテストにインフラを含めない**: ユニットテストはデータベース、ファイルシステム、ネットワーク、外部サービスに触れない。すべてのインフラ依存関係に Test Double を使用する。
- **最小限のテスト**: 振る舞いを検証するために可能な限りシンプルな入力を使用する。意図を曖昧にする不要なセットアップを避ける。
- **テスト内にロジックを含めない**: テストに `if`、`while`、`for`、`switch` を含めない。ロジックが必要と思われる場合は、個別のテストケースに分割する。
- **明確なテスト意図**: テスト名、定数、構造により、本番コードを読まなくてもテストの目的が明確であるべき。

## アンチパターン

- **テスト内のロジック**: テストメソッド内の `if`、`for`、`while`、`switch` 文はテスト自体のバグリスクを高める
- **複数の Act**: テストごとに複数のアクションがあると、障害の診断が曖昧になる
- **マジック値**: 名前付き定数のないハードコードされた文字列や数値はテストの意図を曖昧にする
- **プライベートメソッドのテスト**: private/internal メソッドの直接呼び出しはテストを実装詳細に結合させる
- **セットアップ/ティアダウンの乱用**: すべてのテストで必要としない状態を構成する共有セットアップは隠れた依存関係を作る
- **ユニットテストのインフラ**: ユニットテスト内のデータベース接続、ファイルI/O、HTTP呼び出しはテストを低速、脆弱、非分離にする
- **過度なモッキング**: シンプルな Value Object を含むすべての依存関係をモックすると、テスト価値を向上させずに複雑さを増す

## 参考資料

- **テストパターン、コード例、リファクタリングレシピ**: [references/testing-patterns.md](references/testing-patterns.md)
- **健全性チェックリストと診断テンプレート**: [references/testing-checklist.md](references/testing-checklist.md)
