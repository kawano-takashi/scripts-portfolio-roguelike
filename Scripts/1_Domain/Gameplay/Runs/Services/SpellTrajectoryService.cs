using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// 直線型の呪文射線を計算する実装です。
    /// </summary>
    public sealed class SpellTrajectoryService : ISpellTrajectoryService
    {
        public IReadOnlyList<Position> BuildLinearTrajectory(
            Map map,
            Position origin,
            Direction direction,
            int range)
        {
            var positions = new List<Position>();
            if (map == null || range <= 0)
            {
                return positions;
            }

            var current = origin;
            for (var i = 0; i < range; i++)
            {
                var next = DirectionUtility.Apply(current, direction);
                if (!map.Contains(next))
                {
                    break;
                }

                if (map.BlocksSight(next))
                {
                    break;
                }

                positions.Add(next);
                current = next;
            }

            return positions;
        }
    }
}

