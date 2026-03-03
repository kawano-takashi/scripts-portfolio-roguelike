using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// ダッシュ継続可否をドメイン規則で判定します。
    /// </summary>
    public sealed class DashContinuationPolicy : IDashContinuationPolicy
    {
        private const int CorridorHostileDetectionRange = 2;

        public DashContinuationDecision EvaluateBeforeStep(RunSession run, Actor actor, Direction direction)
        {
            if (run?.Map == null || actor == null)
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.InvalidState, direction);
            }

            if (!run.CanActorStepFrom(actor, actor.Position, direction, out var blocker))
            {
                return DashContinuationDecision.Stop(ResolveBlockedReason(actor, blocker), direction);
            }

            return DashContinuationDecision.Continue(direction);
        }

        public DashContinuationDecision EvaluateAfterStep(
            RunSession run,
            Actor actor,
            Position previousPosition,
            Direction currentDirection)
        {
            if (run?.Map == null || actor == null)
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.InvalidState, currentDirection);
            }

            var currentPosition = actor.Position;
            if (currentPosition == previousPosition)
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.ActionFailed, currentDirection);
            }

            if (run.HasItemAt(currentPosition))
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.OnItem, currentDirection);
            }

            if (run.Map.StairsDownPosition.HasValue && run.Map.StairsDownPosition.Value == currentPosition)
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.OnStairs, currentDirection);
            }

            var isInRoom = run.Map.TryGetRoomAt(currentPosition, out var room);
            if (isInRoom)
            {
                if (run.ContainsHostileInRoom(room, actor.Faction))
                {
                    return DashContinuationDecision.Stop(DashContinuationStopReason.EnemySighted, currentDirection);
                }
            }
            else if (run.HasHostileWithinRange(currentPosition, actor.Faction, CorridorHostileDetectionRange))
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.EnemyNearby, currentDirection);
            }

            if (run.IsRoomBoundaryTransition(previousPosition, currentPosition))
            {
                return DashContinuationDecision.Stop(DashContinuationStopReason.RoomBoundary, currentDirection);
            }

            if (isInRoom || IsDiagonal(currentDirection))
            {
                if (run.CanActorStepFrom(actor, currentPosition, currentDirection, out _))
                {
                    return DashContinuationDecision.Continue(currentDirection);
                }

                return DashContinuationDecision.Stop(DashContinuationStopReason.BlockedAhead, currentDirection);
            }

            if (run.TryGetSingleCorridorForwardDirection(
                actor,
                currentPosition,
                previousPosition,
                out var nextDirection,
                out var pathState))
            {
                return DashContinuationDecision.Continue(nextDirection);
            }

            var stopReason = pathState switch
            {
                CorridorPathState.Junction => DashContinuationStopReason.Junction,
                CorridorPathState.DeadEnd => DashContinuationStopReason.DeadEnd,
                _ => DashContinuationStopReason.BlockedAhead
            };
            return DashContinuationDecision.Stop(stopReason, currentDirection);
        }

        private static DashContinuationStopReason ResolveBlockedReason(Actor actor, Actor blocker)
        {
            if (actor == null || blocker == null || blocker == actor)
            {
                return DashContinuationStopReason.BlockedAhead;
            }

            return blocker.Faction == actor.Faction
                ? DashContinuationStopReason.OccupiedAhead
                : DashContinuationStopReason.EnemyAhead;
        }

        private static bool IsDiagonal(Direction direction)
        {
            return direction == Direction.UpRight
                || direction == Direction.DownRight
                || direction == Direction.DownLeft
                || direction == Direction.UpLeft;
        }
    }
}


