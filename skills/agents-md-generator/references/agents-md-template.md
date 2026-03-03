# AGENTS.md テンプレート

プロジェクトに関連するセクションのみを使用する。各セクションには何を含めるべきか、いつ省略すべきかのガイダンスが含まれている。

---

## ヘッダー

常に含める。プロジェクト名と一行の説明。

```markdown
# [プロジェクト名]

[プロジェクトの一行説明]
```

---

## 技術スタック

プロジェクトがフレームワーク、言語、ツールを使用しており、エージェントがそれを知る必要がある場合に含める。
単一ファイルのスクリプトや自明なスタックの場合は省略する。

```markdown
<!-- auto-generated: tech-stack -->
## Tech Stack

| カテゴリ | 技術 | バージョン |
|----------|------|-----------|
| Language | [例: C# / TypeScript / Python] | [検出可能な場合のバージョン] |
| Framework | [例: ASP.NET Core / Next.js / Django] | [バージョン] |
| Build Tool | [例: MSBuild / Webpack / Vite] | [バージョン] |
| Package Manager | [例: NuGet / npm / pip] | |
| Testing | [例: xUnit / Jest / pytest] | |
| Database | [例: PostgreSQL / SQLite] | |
| Other | [例: Docker, Redis, etc.] | |

<!-- end: tech-stack -->
```

**検出のヒント:**
- `package.json`、`*.csproj`、`*.sln`、`Cargo.toml`、`pyproject.toml`、`go.mod`、`Gemfile`、`build.gradle`、`pom.xml` で言語/フレームワーク/バージョン情報を確認する。
- `Dockerfile`、`docker-compose.yml` でインフラ構成を確認する。
- CI設定（`.github/workflows/`、`.gitlab-ci.yml`、`Jenkinsfile`）でツールチェーンを確認する。

---

## ディレクトリ構成

常に含める。トップレベルのディレクトリツリーに目的の注釈を付ける。モノレポの場合は各パッケージ/モジュールに対してもう一階層を含める。

```markdown
<!-- auto-generated: directory-structure -->
## Directory Structure

[project-root]/
├── src/                    # [目的]
│   ├── Domain/             # [目的]
│   ├── Application/        # [目的]
│   ├── Infrastructure/     # [目的]
│   └── Presentation/       # [目的]
├── tests/                  # [目的]
│   ├── Unit/               # [目的]
│   └── Integration/        # [目的]
├── docs/                   # [目的]
└── scripts/                # [目的]

<!-- end: directory-structure -->
```

**ガイドライン:**
- 標準プロジェクトでは2階層、モノレポでは3階層まで表示する。
- すべてのディレクトリに目的を注釈する。
- `node_modules/`、`bin/`、`obj/`、`.git/` およびその他の生成ディレクトリは省略する。

---

## 主要ファイル

プロジェクトに名前や場所からは役割が明確でないファイル、または重要なエントリポイントとなるファイルがある場合に含める。

```markdown
<!-- auto-generated: key-files -->
## Key Files

| ファイル | 役割 |
|----------|------|
| `[パス]` | [非自明な役割や重要性] |
| `[パス]` | [エントリポイント / 設定 / オーケストレーション] |

<!-- end: key-files -->
```

**含めるべきもの:**
- アプリケーションエントリポイント（例: `Program.cs`、`main.py`、`index.ts`）
- 中央設定（例: `appsettings.json`、`.env.example`、`next.config.js`）
- DI / サービス登録ファイル
- データベースマイグレーション設定
- ルート/エンドポイント登録
- モジュール間で使用される共有型定義や定数

**省略すべきもの:**
- 名前と場所から目的が明らかなファイル
- 自動生成ファイル（特別な取り扱いが必要な場合を除く）

---

## アーキテクチャパターン

プロジェクトが認識可能なアーキテクチャパターンに従っている場合に含める。単純なスクリプトや明確なパターンがないプロジェクトの場合は省略する。

```markdown
<!-- auto-generated: architecture -->
## Architecture

**パターン**: [例: Clean Architecture / Layered / Hexagonal / Modular Monolith / Microservices / MVC / CQRS]

**レイヤー/モジュールの依存関係フロー**:

[Presentation] --> [Application] --> [Domain]
                         |
                   [Infrastructure]

**主要なアーキテクチャルール**:
- [例: Domain Layer は外部依存を持たない]
- [例: すべてのデータベースアクセスは Repository Interface を経由する]
- [例: Use Case は1クラス1操作]

<!-- end: architecture -->
```

**検出のヒント:**
- フォルダ名から: `Domain/`、`Application/`、`Infrastructure/`、`Presentation/`、`Core/`、`UseCases/`、`Handlers/`、`Controllers/`、`Services/`
- `.csproj` のプロジェクト参照やインポートパターンを確認する
- 配線を示すDI登録ファイルを探す
- CQRSパターン（`Commands/` と `Queries/` フォルダの分離）を確認する

---

## モジュールマップ

モノレポまたは複数プロジェクト/パッケージを持つソリューションの場合のみ含める。

```markdown
<!-- auto-generated: module-map -->
## Module Map

| モジュール/パッケージ | パス | 目的 | 依存関係 |
|----------------------|------|------|----------|
| [名前] | `[パス]` | [目的] | [X, Y に依存] |

<!-- end: module-map -->
```

---

## 規約

プロジェクトに命名規則、コード構成パターン、またはAIエージェントがコード生成・変更時に従うべきルールがある場合に含める。

```markdown
<!-- auto-generated: conventions -->
## Conventions

- **命名**: [例: クラスはPascalCase、変数はcamelCase、ファイルはkebab-case]
- **ファイル構成**: [例: 1ファイル1クラス、ファイル名はクラス名と一致]
- **テスト**: [例: テストは src/ の構造をミラー、*Tests.cs / *.test.ts と命名]
- **インポート**: [例: @/ エイリアスで絶対インポート、index.ts 経由のバレルエクスポート]
- **エラーハンドリング**: [例: Result パターン、ビジネスエラーで例外をスローしない]

<!-- end: conventions -->
```

**検出のヒント:**
- 既存のファイル名から命名規則を確認する
- リンター設定（`.eslintrc`、`.editorconfig`、`stylecop.json`）でスタイルルールを確認する
- テストファイルの命名と配置パターンを確認する
- 既存ファイルのインポートスタイルを確認する

---

## データフロー

プロジェクトにエージェントが理解する必要がある非自明なデータフローがある場合のみ含める。
このセクションは通常、手動で管理される。

```markdown
<!-- manual -->
## Data Flow

[Request] --> [Controller/Handler] --> [Use Case] --> [Repository] --> [Database]
                                          |
                                     [Domain Events] --> [Event Handler] --> [Side Effects]
```

---

## 構成ルール

1. 常に含める: ヘッダー、ディレクトリ構成
2. 検出可能な場合に含める: 技術スタック、主要ファイル、アーキテクチャパターン、規約
3. 該当する場合に含める: モジュールマップ（モノレポのみ）、データフロー（複雑なフローのみ）
4. セクションの順序は上記の通り
5. 新規ファイルの場合、すべてのセクションにデフォルトで `<!-- auto-generated: section-name -->` マーカーを付与する
6. 更新の場合、既存のマーカータイプを保持する
