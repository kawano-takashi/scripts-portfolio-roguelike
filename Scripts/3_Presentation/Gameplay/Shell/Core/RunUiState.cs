namespace Roguelike.Presentation.Gameplay.Shell.Core
{
    /// <summary>
    /// UIレイヤー種別を表す列挙型です。
    /// モーダルスタックで最前面の状態を管理します。
    /// </summary>
    public enum RunUiState
    {
        /// <summary>UI無し（探索中）。</summary>
        None,

        /// <summary>メインメニュー。</summary>
        Menu,

        /// <summary>操作説明。</summary>
        Guide,

        /// <summary>インベントリ一覧。</summary>
        Inventory,

        /// <summary>アイテム詳細メニュー。</summary>
        InventoryDetail,

        /// <summary>アイテム説明表示。</summary>
        InventoryDescription,

        /// <summary>呪文プレビュー確認。</summary>
        SpellPreview,

        /// <summary>フロア移行確認。</summary>
        FloorConfirm
    }
}



