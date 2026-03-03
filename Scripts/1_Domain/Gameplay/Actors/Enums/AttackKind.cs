using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
namespace Roguelike.Domain.Gameplay.Actors.Enums
{
    /// <summary>
    /// 攻撃の種類です。敵の行動の違いを表します。
    /// </summary>
    public enum AttackKind
    {
        /// <summary>
        /// 近くで殴る攻撃。
        /// </summary>
        Melee,
        /// <summary>
        /// 遠くから撃つ攻撃。
        /// </summary>
        Ranged,
        /// <summary>
        /// じゃまする効果つき攻撃。
        /// </summary>
        Disruptor
    }
}


