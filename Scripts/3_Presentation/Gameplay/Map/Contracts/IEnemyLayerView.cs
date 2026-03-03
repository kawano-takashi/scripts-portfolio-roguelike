using Roguelike.Presentation.Gameplay.Map.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Map.Contracts
{
    /// <summary>
    /// 敵レイヤー描画の受け口です。
    /// </summary>
    public interface IEnemyLayerView
    {
        void Render(EnemyLayerDisplayModel model);
    }
}
