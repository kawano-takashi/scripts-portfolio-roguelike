---
name: agents-md-generator
description: Generate or update AGENTS.md files in repository roots. Use when the user asks to "create AGENTS.md", "update AGENTS.md", "generate AGENTS.md", "refresh AGENTS.md", or mentions "AGENTS.md" in the context of documenting project structure for AI agents. Covers project structure, directory layout, key file roles, architecture patterns,tech stack, and conventions.
---

# AGENTS.md ジェネレーター

リポジトリルートに AGENTS.md ファイルを生成・更新し、AIコーディングエージェントに構造化されたプロジェクト知識（ディレクトリ構成、主要ファイル、アーキテクチャパターン、技術スタック、規約）を提供する。

## 基本コンセプト

AGENTS.md はAI向けのプロジェクト概要ドキュメントである。CLAUDE.md（Claude Code 固有のコマンド、注意点、ワークフローのヒントを対象とする）とは異なり、AGENTS.md はあらゆるAIコーディングエージェントがコードベースをナビゲートし理解するために必要な構造的・アーキテクチャ的な知識に焦点を当てる。

## ワークフロー

タスクの種類を判断する:

- **AGENTS.md が存在しない** --> 「新規作成ワークフロー」に従う
- **AGENTS.md が既に存在する** --> 「更新ワークフロー」に従う

### 新規作成ワークフロー

1. **リポジトリルートを特定する**: git ルートまたはプロジェクトルートディレクトリを特定する。
2. **コードベース探索を実行する**: [references/exploration-checklist.md](references/exploration-checklist.md) に従う。
   - Python が利用可能な場合は `scripts/explore_structure.py` でディレクトリツリーを生成する。
   - 設定ファイル、エントリポイント、フレームワークを特定する。
   - アーキテクチャパターンと規約を検出する。
3. **AGENTS.md を生成する**: [references/agents-md-template.md](references/agents-md-template.md) のテンプレートを使用する。
   - 探索結果に基づいて該当するすべてのセクションを記入する。
   - プロジェクトに該当しないセクションは省略する。
   - 自動生成セクションにマーカーを付与する（下記「セクションマーカー」参照）。
4. **ドラフトをユーザーに提示する**: ファイル書き込み前に、生成した AGENTS.md の全内容を表示する。
5. **ファイルを書き込む**: ユーザーの承認後、リポジトリルートに AGENTS.md を書き込む。

### 更新ワークフロー

1. **既存の AGENTS.md を読み込む**: 現在のファイルをロードし、存在するセクションを特定する。
2. **セクションの種類を識別する**: 自動生成セクション（`<!-- auto-generated -->` マーカー付き）と手動編集セクション（`<!-- manual -->` マーカー付きまたはマーカーなし）を区別する。
3. **コードベース探索を実行する**: [references/exploration-checklist.md](references/exploration-checklist.md) に従い、前回の生成以降の変更を検出することに焦点を当てる。
4. **自動生成セクションのみを更新する**: `<!-- auto-generated -->` マーカーが付いたセクションの内容を再生成する。手動編集セクションはそのまま保持する。
5. **陳腐化した手動セクションにフラグを立てる**: 手動セクションが存在しないファイルやディレクトリを参照している場合、警告コメントを追加するが内容は変更しない。
6. **差分をユーザーに提示する**: ファイル書き込み前に、旧バージョンと新バージョンの差分を表示する。
7. **更新されたファイルを書き込む**: ユーザーの承認後、更新された AGENTS.md を書き込む。

## セクションマーカー

自動生成コンテンツと手動編集コンテンツを区別するために、HTMLコメントを使用する:

```markdown
<!-- auto-generated: section-name -->
## セクションタイトル

[自動生成された内容]

<!-- end: section-name -->
```

マーカーなし、または `<!-- manual -->` マーカー付きのセクションはユーザーが管理するものとして扱われ、更新時に上書きされることはない。

新規に AGENTS.md を作成する場合、すべてのセクションにデフォルトで `<!-- auto-generated -->` マーカーが付与される。ユーザーはこれらのマーカーを削除するか `<!-- manual -->` に変更することで、セクションの所有権を取得できる。

## 基本原則

- **構造に焦点を当てる**: プロジェクトの物理的・論理的構造をドキュメント化する。運用コマンドやワークフローは対象外（それらは CLAUDE.md や README.md に属する）。
- **AIエージェントが対象読者**: 物がどこにあり、どう関連しているかを素早く理解する必要があるAIコーディングエージェント向けに書く。パスと名前は正確に記述する。
- **簡潔でスキャンしやすい**: テーブル、ツリー図、箇条書きを使用する。散文的な段落は避ける。
- **正確なパス**: すべてのファイルパスとディレクトリ参照は実際のコードベースに対して検証済みでなければならない。
- **不要なセクションは省略する**: すべてのプロジェクトにすべてのセクションが必要なわけではない。価値を追加しないセクションは省略する。
- **ユーザーの編集を保持する**: 更新時、手動管理されているセクションを上書きしない。

## アンチパターン

- **すべてのファイルを列挙する**: 重要な役割を持つファイルのみを記載する。`.gitignore` や `LICENSE` のような自明なファイルは、非自明な内容を含まない限り列挙しない。
- **README.md の焼き直し**: AGENTS.md は README.md のコピーではない。ユーザー向けドキュメントではなく、エージェントが必要とする構造的知識に焦点を当てる。
- **汎用的な説明**: ファイル名から明らかな場合に「このファイルはXを処理する」と書くのは避ける。非自明な役割と関係をドキュメント化する。
- **陳腐化した内容**: 古くなった AGENTS.md は AGENTS.md がないよりも悪い。常に実際のコードベースに対して検証する。
- **過度なドキュメント化**: すべてのディレクトリや規約をドキュメント化する必要はない。AIエージェントがコードだけでは推測しにくい部分に焦点を当てる。

## 参考資料

- **AGENTS.md テンプレートとセクションガイド**: [references/agents-md-template.md](references/agents-md-template.md)
- **コードベース探索手順**: [references/exploration-checklist.md](references/exploration-checklist.md)
