using UnityEngine;
using TMPro;
using Roguelike.Presentation.Gameplay.Menu.Types;

namespace Roguelike.Presentation.Gameplay.Menu.Views
{
    /// <summary>
    /// Displays a single menu option slot.
    /// </summary>
    public sealed class MainMenuSlotView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;

        private MainMenuOption _option;

        /// <summary>
        /// The option this slot represents.
        /// </summary>
        public MainMenuOption Option => _option;

        /// <summary>
        /// Binds this slot to a menu option.
        /// </summary>
        public void Bind(MainMenuOption option, string displayText)
        {
            _option = option;
            if (_label != null)
            {
                _label.text = displayText;
            }
        }

        /// <summary>
        /// Sets the slot color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (_label != null)
            {
                _label.color = color;
            }
        }
    }
}




