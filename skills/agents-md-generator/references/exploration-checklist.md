# コードベース探索チェックリスト

AGENTS.md 生成のためにリポジトリを探索し情報を収集する体系的な手順。
ステップを順番に実行し、各ステップの結果を記録する。

---

## ステップ1: プロジェクトルートと種類の特定

1. リポジトリルートを確認する（`.git/` の存在、またはユーザーがコマンドを実行したディレクトリ）。
2. ソリューション/ワークスペースファイルを確認する:
   - `.sln` (C# / .NET)
   - `package.json` (Node.js / JavaScript / TypeScript)
   - `pyproject.toml` または `setup.py` または `requirements.txt` (Python)
   - `go.mod` (Go)
   - `Cargo.toml` (Rust)
   - `build.gradle` または `pom.xml` (Java / Kotlin)
   - `Gemfile` (Ruby)
   - `composer.json` (PHP)
   - `pubspec.yaml` (Dart / Flutter)
3. 判定する: 単一プロジェクト、モノレポ、マルチプロジェクトソリューションのいずれか。

**記録**: プロジェクトタイプ、主要言語、ルートパス。

---

## ステップ2: ディレクトリ構成のマッピング

1. トップレベルディレクトリを一覧表示する（除外: `.git`、`node_modules`、`bin`、`obj`、`.vs`、`.idea`、`__pycache__`、`.next`、`dist`、`build`、`out`、`target`、`.gradle`、`vendor`、`.venv`、`env`、`venv`）。
2. 各トップレベルディレクトリについて、一階層分のサブディレクトリを一覧表示する。
3. モノレポの場合、すべてのパッケージ/プロジェクトを特定し、サブディレクトリを一覧表示する。
4. 注釈付きのツリー表現を生成する。

**使用するコマンド:**
```bash
# オプションA: Pythonスクリプト（利用可能な場合）
python ~/.claude/skills/agents-md-generator/scripts/explore_structure.py [repo-root]

# オプションB: Glob/Read ツールによる手動探索
# トップレベルディレクトリには "*/" パターンで Glob ツールを使用
# サブディレクトリの一覧には ls を使用
```

**記録**: 注釈付きディレクトリツリー。

---

## ステップ3: 技術スタックの特定

1. ステップ1で特定した主要設定ファイルから以下を読み取る:
   - 言語バージョン
   - フレームワーク名とバージョン
   - 主要な依存関係
2. 追加の設定ファイルを確認する:
   - `tsconfig.json` (TypeScript設定)
   - `Dockerfile` / `docker-compose.yml` (コンテナ化)
   - `.github/workflows/*.yml` または `.gitlab-ci.yml` (CI/CD)
   - `Makefile` / `Taskfile.yml` / `justfile` (タスクランナー)
3. データベース関連ファイルを確認する:
   - マイグレーションフォルダ
   - スキーマファイル
   - ORM設定

**記録**: 技術スタックテーブル（カテゴリ | 技術 | バージョン）。

---

## ステップ4: 主要ファイルの特定

1. エントリポイントを探す:
   - `Program.cs`、`Startup.cs` (C#)
   - `index.ts`、`index.js`、`main.ts`、`main.js`、`app.ts`、`app.js` (JS/TS)
   - `main.py`、`app.py`、`__main__.py`、`manage.py`、`wsgi.py`、`asgi.py` (Python)
   - `main.go`、`cmd/*/main.go` (Go)
   - `main.rs`、`lib.rs` (Rust)
   - `Main.java`、`Application.java` (Java)
2. DI / サービス登録を探す:
   - `*Module*`、`*Registration*`、`*Extensions*`、`*Container*`、`*Provider*`、`*Config*` という名前のファイル
3. ルート/エンドポイント定義を探す:
   - `routes.*`、`*Router*`、`*Controller*` ファイル
4. 共有型 / 定数を探す:
   - `shared/`、`common/`、`types/`、`constants/` 内のファイル
5. データベース/マイグレーション設定を探す。

**記録**: 非自明な役割の説明付き主要ファイルテーブル。

---

## ステップ5: アーキテクチャパターンの検出

1. フォルダ構成から既知のパターンを確認する:
   - **Clean Architecture / Onion**: `Domain/`、`Application/`、`Infrastructure/`、`Presentation/`（または `WebApi/`、`UI/`）
   - **Layered**: `Models/`、`Services/`、`Controllers/`、`Repositories/`
   - **Hexagonal**: `ports/`、`adapters/`、`core/`
   - **CQRS**: `Commands/`、`Queries/`、`Handlers/`
   - **MVC**: `Models/`、`Views/`、`Controllers/`
   - **Modular Monolith**: トップレベルの機能フォルダがそれぞれ独自のレイヤーを持つ
   - **Microservices**: 個別にデプロイ可能なサービスディレクトリ
2. プロジェクト参照（`.csproj`、`tsconfig.json` のパス、インポート文）を確認し、依存関係の方向を特定する。
3. アーキテクチャ決定記録（`docs/adr/`、`decisions/`）を探す。

**記録**: パターン名、依存関係フロー図、主要なアーキテクチャルール。

---

## ステップ6: 規約の検出

1. **命名規則**: ソースディレクトリから5〜10個のファイル名をサンプリングする。大文字小文字のパターンを記録する（PascalCase、camelCase、kebab-case、snake_case）。
2. **ファイル構成**: 1ファイル1クラスが守られているか、ファイル名がクラス/コンポーネント名と一致しているかを確認する。
3. **テスト規約**: テストディレクトリ構造、テストファイルの命名パターン、使用されているテストフレームワークを確認する。
4. **インポートスタイル**: 2〜3個のソースファイルでインポートパターンを確認する（相対 vs 絶対、バレルエクスポート、エイリアス）。
5. **リンター/フォーマッター設定**: `.eslintrc*`、`.prettierrc*`、`.editorconfig`、`stylecop.json`、`ruff.toml`、`rustfmt.toml` が存在すれば読み取る。
6. **エラーハンドリング**: Result パターン型、カスタム例外階層、エラーハンドリングミドルウェアを確認する。

**記録**: 規約の箇条書きリスト。

---

## ステップ7: 結果の集約

記録したすべての結果を AGENTS.md テンプレートのセクションに組み立てる。含める前に、すべてのファイルパスとディレクトリ参照を実際のコードベースに対して検証する。
