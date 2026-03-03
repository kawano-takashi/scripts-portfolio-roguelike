using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;
namespace Roguelike.Domain.Gameplay.Maps.Services
{
    /// <summary>
    /// マップを作るための約束（インターフェース）です。
    /// </summary>
    public interface IMapGenerationService
    {
        /// <summary>
        /// シードだけ指定してマップを作ります。
        /// </summary>
        Map Generate(int seed);
        /// <summary>
        /// 大きさとシードを指定してマップを作ります。
        /// </summary>
        Map Generate(int width, int height, int seed);
    }
}


