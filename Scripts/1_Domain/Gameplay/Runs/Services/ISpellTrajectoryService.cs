using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// 呪文の射線・到達座標を計算するドメインサービスです。
    /// </summary>
    public interface ISpellTrajectoryService
    {
        IReadOnlyList<Position> BuildLinearTrajectory(
            Map map,
            Position origin,
            Direction direction,
            int range);
    }
}


