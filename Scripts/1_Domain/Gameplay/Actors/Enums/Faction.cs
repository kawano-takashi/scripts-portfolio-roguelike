using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
namespace Roguelike.Domain.Gameplay.Actors.Enums
{
    /// <summary>
    /// だれの味方かを表すラベルです。
    /// 味方か敵かを、わかりやすく分けるために使います。
    /// </summary>
    public enum Faction
    {
        /// <summary>
        /// プレイヤー側。
        /// </summary>
        Player,
        /// <summary>
        /// 敵側。
        /// </summary>
        Enemy
    }
}


