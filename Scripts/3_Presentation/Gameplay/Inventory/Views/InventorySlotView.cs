using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Roguelike.Presentation.Gameplay.Inventory.Views
{
    /// <summary>
    /// インベントリスロット1つ分の表示を担当します。
    /// </summary>
    public sealed class InventorySlotView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private Image _backgroundImage;

        /// <summary>
        /// アイテムを表示します。
        /// </summary>
        /// <param name="displayName">表示名。</param>
        /// <param name="isEquipped">装備中かどうか。</param>
        public void SetItem(string displayName)
        {
            gameObject.SetActive(true);

            if (_itemNameText != null)
            {
                _itemNameText.text = displayName;
            }
        }

        /// <summary>
        /// 空スロットとして非表示にします。
        /// </summary>
        public void SetEmpty()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 選択状態を設定します。
        /// </summary>
        /// <param name="selected">選択されているかどうか。</param>
        public void SetSelected(bool selected)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = selected ? Color.yellow : Color.clear;
                _itemNameText.color = selected ? Color.black : Color.white;
            }
        }
    }
}



