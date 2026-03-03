using Roguelike.Presentation.Gameplay.Map.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Map.Contracts
{
    /// <summary>
    /// ミニマップ描画の受け口です。
    /// </summary>
    public interface IMiniMapView
    {
        void Render(MiniMapDisplayModel model);
    }
}
