using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects
{
    /// <summary>
    /// フロアの特性を表す値オブジェクトです。
    /// </summary>
    public readonly struct FloorProfile
    {
        /// <summary>
        /// 階数。
        /// </summary>
        public int FloorNumber { get; }

        /// <summary>
        /// フロアの種類。
        /// </summary>
        public FloorProfileType Type { get; }

        /// <summary>
        /// モンスターハウスがあるかどうか。
        /// </summary>
        public bool HasMonsterHouse => Type == FloorProfileType.MonsterHouse;

        /// <summary>
        /// FloorProfileを作成します。
        /// </summary>
        public FloorProfile(int floorNumber, FloorProfileType type)
        {
            FloorNumber = floorNumber;
            Type = type;
        }
    }
}


