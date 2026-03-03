using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
namespace Roguelike.Domain.Gameplay.Actors.Enums
{
    /// <summary>
    /// 状態異常の種類です。
    /// </summary>
    public enum StatusEffectType
    {
        /// <summary>
        /// 沈黙（魔法が使えない状態）。
        /// </summary>
        Silence,
        /// <summary>
        /// 睡眠（行動不能）。
        /// </summary>
        Sleep
    }
}


