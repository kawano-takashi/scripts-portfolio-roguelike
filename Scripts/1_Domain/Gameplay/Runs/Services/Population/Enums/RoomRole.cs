namespace Roguelike.Domain.Gameplay.Runs.Services.Population.Enums
{
    /// <summary>
    /// 部屋の役割を表します。
    /// </summary>
    public enum RoomRole
    {
        /// <summary>
        /// プレイヤーのスタート地点。敵は配置されません。
        /// </summary>
        Start,

        /// <summary>
        /// 階段がある部屋。敵は少なめに配置されます。
        /// </summary>
        Stairs,

        /// <summary>
        /// モンスターハウス。敵とアイテムが大量に配置され、入室時に敵が一斉に起床します。
        /// </summary>
        MonsterHouse,

        /// <summary>
        /// 通常の部屋。標準的な配置が行われます。
        /// </summary>
        Normal
    }
}


