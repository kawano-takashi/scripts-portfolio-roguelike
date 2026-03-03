# Presentation Layer リファクタリングチェックリスト (MVP)

## 目次

1. [リファクタリング前分析](#リファクタリング前分析)
2. [View 健全性チェックリスト](#view-健全性チェックリスト)
3. [Presenter 健全性チェックリスト](#presenter-健全性チェックリスト)
4. [契約健全性チェックリスト](#契約健全性チェックリスト)
5. [依存関係健全性チェックリスト](#依存関係健全性チェックリスト)
6. [Display Model 健全性チェックリスト](#display-model-健全性チェックリスト)
7. [診断テンプレート](#診断テンプレート)

## リファクタリング前分析

リファクタリング前にコードベース分析を実施する:

1. **Presentation Layer を特定する**: Presenter と View を含むプロジェクト/フォルダを見つける（よくある名前: `Presentation`、`UI`、`Presenters`、`Views`、`ViewModels`）
2. **Presenter-View ペアを棚卸しする**: すべての Presenter、その IView 契約、具象 View 実装を一覧にする
3. **Use Case 依存をマッピングする**: 各 Presenter について、どの Application Layer Use Case を呼び出しているかを一覧にする
4. **問題点を特定する**: 下記のチェックリストに記載されたコードスメルを探す

## View 健全性チェックリスト

- [ ] View が IView インターフェースを実装している
- [ ] View の `Show*` メソッドに条件分岐（`if`、`switch`、三項演算子）がない
- [ ] View の `Show*` メソッドに文字列フォーマットやデータ変換がない
- [ ] View が Application Layer の Use Case やサービスを直接呼び出していない
- [ ] View がプレゼンテーション状態（データコレクション、フラグ、現在の選択）を保持していない
- [ ] View のイベントハンドラがイベントの発行のみを行い、他のロジックがない
- [ ] View が Domain Layer の型（Entity、Value Object、ドメイン列挙型）を参照していない
- [ ] View が受け取るすべてのデータが Display Model 形式である（生の DTO やドメインオブジェクトではない）
- [ ] View にビジネスロジック、バリデーションロジック、ドメイン不変条件チェックがない
- [ ] スレッドマーシャリング（必要な場合）が View 内で処理され、Presenter に漏洩していない

## Presenter 健全性チェックリスト

- [ ] Presenter が IPresenter インターフェースを実装している
- [ ] Presenter が具象 View ではなく IView インターフェースに依存している
- [ ] Presenter がすべてのプレゼンテーション状態（現在のデータ、ローディングフラグ、選択）を保持している
- [ ] Presenter が Repository や Domain Service ではなく Application Layer の Use Case を呼び出している
- [ ] Presenter が Application DTO を View にプッシュする前に Display Model に変換している
- [ ] Presenter にドメインロジック（ビジネスルール、不変条件チェック、状態遷移）がない
- [ ] Presenter に UI フレームワーク型（WPF、Blazor、MAUI の参照なし）がない
- [ ] Presenter がコンストラクタで IView イベントを購読し、Dispose で購読解除している
- [ ] Presenter がエラーを処理し、`ShowError` などを通じてエラー状態を View にプッシュしている
- [ ] Presenter がローディング状態を管理している（try/finally で ShowLoading true/false）
- [ ] 各 Presenter の注入される依存が最大5〜6個である（God Presenter ではない）

## 契約健全性チェックリスト

- [ ] すべての具象 View に IView インターフェースが存在する
- [ ] すべての具象 Presenter に IPresenter インターフェースが存在する
- [ ] IView メソッドがパラメータとして DTO やドメイン型ではなく Display Model を使用している
- [ ] IView イベントが非同期シグネチャとして `Func<..., Task>` を使用している
- [ ] IView のシグネチャに UI フレームワーク固有の型を公開していない
- [ ] IPresenter メソッドがフレームワークのコールバックではなくユーザーにとって意味のあるアクションとして命名されている
- [ ] ナビゲーションがフレームワーク固有の呼び出しではなく INavigationService で抽象化されている
- [ ] ダイアログ/確認が IDialogService で抽象化されている

## 依存関係健全性チェックリスト

- [ ] Presenter が依存するのは: IView、Application Use Case、INavigationService、IDialogService のみ
- [ ] Presenter から Infrastructure Layer への直接参照がない（HTTP クライアント、DB コンテキスト、ファイルシステム）
- [ ] Presenter から Domain Layer への直接参照がない（Aggregate、Repository、Domain Service）
- [ ] View が依存するのは: IPresenter インターフェースと Display Model 型のみ
- [ ] View と Presenter 間に循環依存がない（双方が具象型ではなくインターフェースに依存）
- [ ] Presentation Layer プロジェクトが参照するのは: Application Layer プロジェクト（Use Case 型と DTO 用）のみ

## Display Model 健全性チェックリスト

- [ ] Display Model が不変の record である
- [ ] Display Model が事前フォーマット済みの表示用データのみを含む（文字列、表示用 bool、表示ヒント）
- [ ] Display Model が Application Layer や Domain Layer ではなく Presentation Layer に定義されている
- [ ] 各 Display Model 変換にマッパー/ファクトリが存在する
- [ ] コールバック（例: アイテム選択）に必要でない限り、Display Model にドメイン Entity の ID が含まれていない
- [ ] Display Model のフィールドにドメイン型が参照されていない

## 診断テンプレート

既存コードのリファクタリング機会を分析する際にこのテンプレートを使用する:

```
## Presentation Layer 診断 (MVP)

### 概要
- Presentation プロジェクト: [パス]
- Presenter 数: [件数]
- IView インターフェース数: [件数]
- 具象 View 数: [件数]
- Display Model 数: [件数]

### 検出されたコードスメル

#### [スメル名]
- 場所: [ファイル:行]
- 説明: [何が問題か]
- 影響: [なぜ重要か]
- 推奨修正: [適用すべきリファクタリングレシピ]

### View 分析
- IView インターフェースのない View: [リスト]
- Show* メソッドにロジックがある View: [リスト]
- Application Layer を直接呼び出す View: [リスト]
- ドメイン型を参照する View: [リスト]

### Presenter 分析
- IPresenter インターフェースのない Presenter: [リスト]
- ドメインロジックを持つ Presenter: [リスト]
- フレームワーク結合のある Presenter: [リスト]
- 6個以上の依存を持つ Presenter（God Presenter）: [リスト]
- 生の DTO を View に渡す Presenter（Display Model の欠如）: [リスト]

### 依存関係分析
- 循環依存: [リスト]
- Presenter からインフラへの直接参照: [リスト]
- Presenter からドメインへの直接参照: [リスト]

### 優先度
1. [最も影響の大きいリファクタリング]
2. [2番目の優先事項]
3. [3番目の優先事項]
```
