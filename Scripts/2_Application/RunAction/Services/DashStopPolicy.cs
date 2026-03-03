using Roguelike.Application.Enums;
using Roguelike.Application.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Services;
using DomainDashDecision = Roguelike.Domain.Gameplay.Runs.ValueObjects.DashContinuationDecision;
using DomainDashStopReason = Roguelike.Domain.Gameplay.Runs.Enums.DashContinuationStopReason;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ダッシュ継続可否を判定するポリシーです。
    /// Domain が公開する汎用クエリを組み合わせて停止条件を決めます。
    /// </summary>
    public sealed class DashStopPolicy
    {
        private readonly IDashContinuationPolicy _policy;

        public DashStopPolicy(IDashContinuationPolicy policy)
        {
            _policy = policy ?? throw new System.ArgumentNullException(nameof(policy));
        }

        public DashStepDecision EvaluateBeforeStep(RunSession run, Actor actor, Direction direction)
        {
            return ToApplicationDecision(_policy.EvaluateBeforeStep(run, actor, direction));
        }

        public DashStepDecision EvaluateAfterStep(
            RunSession run,
            Actor actor,
            Position previousPosition,
            Direction currentDirection)
        {
            return ToApplicationDecision(
                _policy.EvaluateAfterStep(run, actor, previousPosition, currentDirection));
        }

        private static DashStepDecision ToApplicationDecision(DomainDashDecision decision)
        {
            return decision.ShouldContinue
                ? DashStepDecision.Continue((int)decision.NextDirection)
                : DashStepDecision.Stop(ToApplicationStopReason(decision.StopReason), (int)decision.NextDirection);
        }

        private static DashStopReason ToApplicationStopReason(DomainDashStopReason reason)
        {
            return reason switch
            {
                DomainDashStopReason.None => DashStopReason.None,
                DomainDashStopReason.InvalidState => DashStopReason.InvalidState,
                DomainDashStopReason.BlockedAhead => DashStopReason.BlockedAhead,
                DomainDashStopReason.OccupiedAhead => DashStopReason.OccupiedAhead,
                DomainDashStopReason.EnemyAhead => DashStopReason.EnemyAhead,
                DomainDashStopReason.OnItem => DashStopReason.OnItem,
                DomainDashStopReason.OnStairs => DashStopReason.OnStairs,
                DomainDashStopReason.EnemySighted => DashStopReason.EnemySighted,
                DomainDashStopReason.EnemyNearby => DashStopReason.EnemyNearby,
                DomainDashStopReason.RoomBoundary => DashStopReason.RoomBoundary,
                DomainDashStopReason.Junction => DashStopReason.Junction,
                DomainDashStopReason.DeadEnd => DashStopReason.DeadEnd,
                DomainDashStopReason.ActionFailed => DashStopReason.ActionFailed,
                _ => DashStopReason.BlockedAhead
            };
        }
    }
}


