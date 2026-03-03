namespace Roguelike.Application.Enums
{
    /// <summary>
    /// Application層で扱う攻撃種別です。
    /// Domain.AttackKind と同じ並びを維持します。
    /// </summary>
    public enum AttackKindDto
    {
        Melee = 0,
        Ranged = 1,
        Disruptor = 2
    }
}
