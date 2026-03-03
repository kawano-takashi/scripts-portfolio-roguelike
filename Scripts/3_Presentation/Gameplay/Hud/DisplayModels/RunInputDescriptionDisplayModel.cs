namespace Roguelike.Presentation.Gameplay.Hud.DisplayModels
{
    /// <summary>
    /// 入力説明表示向けの表示モデルです。
    /// </summary>
    public sealed class RunInputDescriptionDisplayModel
    {
        public string DescriptionText { get; }
        public bool IsVisible { get; }

        public RunInputDescriptionDisplayModel(string descriptionText, bool isVisible)
        {
            DescriptionText = descriptionText ?? string.Empty;
            IsVisible = isVisible;
        }
    }
}



