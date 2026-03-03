using Roguelike.Presentation.Gameplay.Map.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Map.Contracts
{
    /// <summary>
    /// ダンジョンマップ描画の受け口です。
    /// </summary>
    public interface IDungeonMapView
    {
        void Render(DungeonMapDisplayModel model);
    }
}
