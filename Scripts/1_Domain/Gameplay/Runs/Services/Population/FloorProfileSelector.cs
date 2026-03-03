using System;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// フロアの種類を決定するサービスです。
    /// トルネコ1ベースの確率でモンスターハウスの有無を決定します。
    /// </summary>
    public sealed class FloorProfileSelector
    {
        /// <summary>
        /// 階数に応じてフロアの種類を決定します。
        /// </summary>
        public FloorProfile Select(int floorNumber, Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var monsterHouseProbability = GetMonsterHouseProbability(floorNumber);
            var roll = random.NextDouble();
            var type = roll < monsterHouseProbability
                ? FloorProfileType.MonsterHouse
                : FloorProfileType.Normal;

            return new FloorProfile(floorNumber, type);
        }

        /// <summary>
        /// 階数に応じたモンスターハウス出現確率を返します。
        /// トルネコ1ベース: 1-2階=0%, 3-5階=10%, 6-8階=20%, 9-10階=30%
        /// </summary>
        private static double GetMonsterHouseProbability(int floorNumber)
        {
            if (floorNumber <= 2)
            {
                return 0.0;
            }

            if (floorNumber <= 5)
            {
                return 0.10;
            }

            if (floorNumber <= 8)
            {
                return 0.20;
            }

            return 0.30;
        }
    }
}


