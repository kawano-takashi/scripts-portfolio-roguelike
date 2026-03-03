using TMPro;
using UnityEngine;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.Hud.DisplayModels;
using Roguelike.Presentation.Gameplay.Map.DisplayModels;
using Roguelike.Presentation.Gameplay.RunResult.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Hud.Views
{
    /// <summary>
    /// 入力説明表示の受動ビューです。
    /// </summary>
    public sealed class RunInputDescriptionView : MonoBehaviour, IRunInputDescriptionView
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _bodyText;

        public void Init()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            if (_bodyText != null)
            {
                _bodyText.text = string.Empty;
            }
        }

        public void Render(RunInputDescriptionDisplayModel model)
        {
            if (_panel != null)
            {
                _panel.SetActive(model != null && model.IsVisible);
            }

            if (_bodyText != null)
            {
                _bodyText.text = model?.DescriptionText ?? string.Empty;
            }
        }
    }
}




