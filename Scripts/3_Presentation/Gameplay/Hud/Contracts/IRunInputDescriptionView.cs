using Roguelike.Presentation.Gameplay.Hud.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Hud.Contracts
{
    /// <summary>
    /// 入力説明表示の受動ビュー契約です。
    /// </summary>
    public interface IRunInputDescriptionView
    {
        void Render(RunInputDescriptionDisplayModel model);
    }
}
