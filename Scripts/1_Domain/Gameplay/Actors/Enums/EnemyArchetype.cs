using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
namespace Roguelike.Domain.Gameplay.Actors.Enums
{
    /// <summary>
    /// 敵のタイプです。
    /// どうやって戦う敵かを分けるために使います。
    /// </summary>
    public enum EnemyArchetype
    {
        /// <summary>
        /// 近づいて殴るタイプ。
        /// </summary>
        Melee,
        /// <summary>
        /// 遠くから攻撃するタイプ。
        /// </summary>
        Ranged,
        /// <summary>
        /// じゃまする効果（例：沈黙）を使うタイプ。
        /// </summary>
        Disruptor
    }
}


