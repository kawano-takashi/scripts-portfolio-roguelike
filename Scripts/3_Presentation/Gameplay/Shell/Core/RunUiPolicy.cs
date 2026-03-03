using System;

namespace Roguelike.Presentation.Gameplay.Shell.Core
{
    /// <summary>
    /// UIモーダルスタックの開閉ルールを定義します。
    /// 「どこから何を開けるか」「閉じたらどこへ戻るか」を一元化します。
    /// </summary>
    public sealed class RunUiPolicy
    {
        /// <summary>
        /// メニューを開きます。既にメニュー配下ならMenuまで戻します。
        /// </summary>
        public bool TryOpenMenu(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            // フロア確認中は他UIを受け付けない。
            if (stack.Top == RunUiState.FloorConfirm)
            {
                return false;
            }

            // 既にメニュー配下にいる場合はMenuまで巻き戻す。
            if (stack.Contains(RunUiState.Menu))
            {
                return stack.PopTo(RunUiState.Menu);
            }

            // メニューは通常探索状態（None）からのみ開く。
            if (stack.Top != RunUiState.None)
            {
                return false;
            }

            return stack.Push(RunUiState.Menu);
        }

        public bool TryCloseMenu(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));
            // メニュー配下も含めてすべて閉じる。
            return stack.Clear();
        }

        /// <summary>
        /// インベントリを開きます。既に開いている場合はInventoryまで戻します。
        /// </summary>
        public bool TryOpenInventory(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            // インベントリはメニュー配下でのみ有効。
            if (!stack.Contains(RunUiState.Menu))
            {
                return false;
            }

            if (stack.Contains(RunUiState.Inventory))
            {
                return stack.PopTo(RunUiState.Inventory);
            }

            if (stack.Top != RunUiState.Menu)
            {
                return false;
            }

            return stack.Push(RunUiState.Inventory);
        }

        /// <summary>インベントリ配下を閉じてMenuへ戻します。</summary>
        public bool TryCloseInventory(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (!stack.Contains(RunUiState.Inventory))
            {
                return false;
            }

            return stack.PopTo(RunUiState.Menu);
        }

        /// <summary>詳細メニューを開きます。</summary>
        public bool TryOpenDetailMenu(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (stack.Contains(RunUiState.InventoryDetail))
            {
                return stack.PopTo(RunUiState.InventoryDetail);
            }

            if (stack.Top != RunUiState.Inventory)
            {
                return false;
            }

            return stack.Push(RunUiState.InventoryDetail);
        }

        /// <summary>詳細メニュー配下を閉じてInventoryへ戻します。</summary>
        public bool TryCloseDetailMenu(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (!stack.Contains(RunUiState.InventoryDetail))
            {
                return false;
            }

            return stack.PopTo(RunUiState.Inventory);
        }

        /// <summary>説明ビューを開きます。</summary>
        public bool TryOpenDescriptionView(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (stack.Contains(RunUiState.InventoryDescription))
            {
                return stack.PopTo(RunUiState.InventoryDescription);
            }

            if (stack.Top != RunUiState.InventoryDetail)
            {
                return false;
            }

            return stack.Push(RunUiState.InventoryDescription);
        }

        /// <summary>説明ビューを閉じて詳細メニューへ戻します。</summary>
        public bool TryCloseDescriptionView(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (!stack.Contains(RunUiState.InventoryDescription))
            {
                return false;
            }

            return stack.PopTo(RunUiState.InventoryDetail);
        }

        /// <summary>呪文プレビューを開きます。</summary>
        public bool TryOpenSpellPreview(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (stack.Contains(RunUiState.SpellPreview))
            {
                return stack.PopTo(RunUiState.SpellPreview);
            }

            // 詳細メニューもしくは探索起点以外は受け付けない。
            if (stack.Top != RunUiState.InventoryDetail && stack.Top != RunUiState.None)
            {
                return false;
            }

            return stack.Push(RunUiState.SpellPreview);
        }

        /// <summary>呪文プレビューを閉じて詳細メニューへ戻します。</summary>
        public bool TryCloseSpellPreview(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (!stack.Contains(RunUiState.SpellPreview))
            {
                return false;
            }

            // 詳細メニュー配下から開いた場合は詳細メニューへ戻す。
            if (stack.Contains(RunUiState.InventoryDetail))
            {
                return stack.PopTo(RunUiState.InventoryDetail);
            }

            // 探索起点から開いた場合はプレビューだけ閉じて通常状態へ戻す。
            return stack.Pop();
        }

        /// <summary>操作説明を開きます。</summary>
        public bool TryOpenGuide(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (stack.Contains(RunUiState.Guide))
            {
                return stack.PopTo(RunUiState.Guide);
            }

            if (stack.Top != RunUiState.Menu)
            {
                return false;
            }

            return stack.Push(RunUiState.Guide);
        }

        /// <summary>操作説明を閉じてMenuへ戻します。</summary>
        public bool TryCloseGuide(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (!stack.Contains(RunUiState.Guide))
            {
                return false;
            }

            return stack.PopTo(RunUiState.Menu);
        }

        /// <summary>
        /// フロア遷移確認を開きます。専有UIのため、他状態は先にすべて閉じます。
        /// </summary>
        public bool TryOpenFloorConfirm(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (stack.Top == RunUiState.FloorConfirm)
            {
                return false;
            }

            stack.Clear();
            return stack.Push(RunUiState.FloorConfirm);
        }

        /// <summary>フロア遷移確認を閉じます。</summary>
        public bool TryCloseFloorConfirm(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (!stack.Contains(RunUiState.FloorConfirm))
            {
                return false;
            }

            return stack.Clear();
        }

        /// <summary>全UIを強制的に閉じます。</summary>
        public bool TryClearAll(RunUiModalStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));
            return stack.Clear();
        }
    }
}



