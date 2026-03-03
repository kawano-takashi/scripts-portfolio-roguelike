using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// ダッシュ継続可否判定ポリシーの抽象です。
    /// </summary>
    public interface IDashContinuationPolicy
    {
        DashContinuationDecision EvaluateBeforeStep(RunSession run, Actor actor, Direction direction);

        DashContinuationDecision EvaluateAfterStep(
            RunSession run,
            Actor actor,
            Position previousPosition,
            Direction currentDirection);
    }
}


