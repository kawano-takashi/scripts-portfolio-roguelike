#!/usr/bin/env python3
"""
AGENTS.md 用のディレクトリツリー表現を生成する。

使用方法:
    python explore_structure.py [root_path] [--depth N] [--output FORMAT]

引数:
    root_path   リポジトリルートへのパス（デフォルト: カレントディレクトリ）
    --depth N   探索する最大深度（デフォルト: 3）
    --output    出力形式: tree（デフォルト）または json
"""

import os
import sys
import argparse
import json

# 常にスキップするディレクトリ
SKIP_DIRS = {
    ".git",
    "node_modules",
    "bin",
    "obj",
    ".vs",
    ".idea",
    "__pycache__",
    ".next",
    "dist",
    "build",
    "out",
    "target",
    ".gradle",
    "vendor",
    ".venv",
    "env",
    "venv",
    ".mypy_cache",
    ".pytest_cache",
    ".tox",
    ".eggs",
    "coverage",
    ".nyc_output",
    ".cache",
    ".parcel-cache",
    ".turbo",
    ".nuxt",
    ".output",
    "Pods",
    "DerivedData",
    ".dart_tool",
    ".pub-cache",
}

# プロジェクトタイプを示すファイル
PROJECT_INDICATORS = {
    ".sln": ".NET Solution",
    "package.json": "Node.js / JavaScript",
    "pyproject.toml": "Python",
    "setup.py": "Python",
    "go.mod": "Go",
    "Cargo.toml": "Rust",
    "build.gradle": "Java / Kotlin (Gradle)",
    "pom.xml": "Java / Kotlin (Maven)",
    "Gemfile": "Ruby",
    "composer.json": "PHP",
    "pubspec.yaml": "Dart / Flutter",
}


def should_skip(name):
    """ディレクトリをスキップすべきかどうかを判定する。"""
    return name in SKIP_DIRS or name.startswith(".")


def build_tree(root, max_depth=3, current_depth=0):
    """ディレクトリツリーを表すネストされた辞書を構築する。"""
    if current_depth >= max_depth:
        return None

    result = {}
    try:
        entries = sorted(os.listdir(root))
    except PermissionError:
        return result

    for entry in entries:
        full_path = os.path.join(root, entry)
        if os.path.isdir(full_path) and not should_skip(entry):
            subtree = build_tree(full_path, max_depth, current_depth + 1)
            result[entry + "/"] = subtree

    return result


def render_tree(tree, prefix="", root_name=None):
    """ツリー辞書をASCIIツリー文字列としてレンダリングする。"""
    lines = []

    if root_name:
        lines.append(f"{root_name}/")

    if tree is None:
        lines.append(f"{prefix}└── ...")
        return lines

    items = list(tree.items())
    for i, (name, subtree) in enumerate(items):
        is_last_item = i == len(items) - 1
        connector = "└── " if is_last_item else "├── "
        lines.append(f"{prefix}{connector}{name}")

        if subtree:
            extension = "    " if is_last_item else "│   "
            sub_lines = render_tree(subtree, prefix + extension)
            lines.extend(sub_lines)

    return lines


def detect_project_type(root):
    """インジケーターファイルに基づいてプロジェクトタイプを検出する。"""
    detected = []
    try:
        entries = set(os.listdir(root))
    except PermissionError:
        return detected

    for indicator, project_type in PROJECT_INDICATORS.items():
        if indicator in entries:
            detected.append((indicator, project_type))

    return detected


def main():
    parser = argparse.ArgumentParser(
        description="AGENTS.md 用のディレクトリツリーを生成する"
    )
    parser.add_argument(
        "root",
        nargs="?",
        default=".",
        help="リポジトリルートパス（デフォルト: カレントディレクトリ）",
    )
    parser.add_argument(
        "--depth",
        type=int,
        default=3,
        help="探索する最大深度（デフォルト: 3）",
    )
    parser.add_argument(
        "--output",
        choices=["tree", "json"],
        default="tree",
        help="出力形式（デフォルト: tree）",
    )

    args = parser.parse_args()
    root = os.path.abspath(args.root)

    if not os.path.isdir(root):
        print(f"エラー: {root} はディレクトリではありません", file=sys.stderr)
        sys.exit(1)

    # プロジェクトタイプを検出
    project_types = detect_project_type(root)
    if project_types:
        print("# 検出されたプロジェクトタイプ:")
        for indicator, ptype in project_types:
            print(f"#   {ptype} ({indicator} を検出)")
        print()

    # ツリーを構築してレンダリング
    tree = build_tree(root, max_depth=args.depth)
    root_name = os.path.basename(root)

    if args.output == "json":
        print(json.dumps(tree, indent=2))
    else:
        lines = render_tree(tree, root_name=root_name)
        print("\n".join(lines))


if __name__ == "__main__":
    main()
