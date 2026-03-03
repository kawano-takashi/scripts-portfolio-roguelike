using System;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ダッシュ1ステップ実行時の判定ロジックを提供します。
    /// </summary>
    public sealed class DashStepExecutionService
    {
        private readonly ITurnEngine _turnEngine;
        private readonly DashStopPolicy _dashStopPolicy;
        private readonly RunExecutionResultAssembler _resultAssembler;

        public DashStepExecutionService(
            ITurnEngine turnEngine,
            DashStopPolicy dashStopPolicy,
            RunExecutionResultAssembler resultAssembler)
        {
            _turnEngine = turnEngine ?? throw new ArgumentNullException(nameof(turnEngine));
            _dashStopPolicy = dashStopPolicy ?? throw new ArgumentNullException(nameof(dashStopPolicy));
            _resultAssembler = resultAssembler ?? throw new ArgumentNullException(nameof(resultAssembler));
        }

        public DashStepExecutionResultDto ExecuteStep(RunSession run, Direction requestedDirection)
        {
            if (run?.Player == null)
            {
                return BuildInvalidStateResult((int)requestedDirection);
            }

            var player = run.Player;
            var beforeDecision = _dashStopPolicy.EvaluateBeforeStep(run, player, requestedDirection);
            if (!beforeDecision.ShouldContinue)
            {
                return new DashStepExecutionResultDto(
                    dashStepResult: new DashStepResultDto(
                        resolution: _resultAssembler.CreateNoopTurnResult(run.TurnCount),
                        shouldContinue: false,
                        nextDirectionValue: beforeDecision.NextDirectionValue,
                        stopReason: beforeDecision.StopReason),
                    snapshot: _resultAssembler.BuildSnapshot(run, hasRun: true),
                    lifecycleEvents: Array.Empty<RunLifecycleEventDto>());
            }

            var from = player.Position;
            var moveAction = new MoveAction(player.Id, requestedDirection);
            var domainResolution = _turnEngine.Resolve(run, moveAction);
            var turnResult = _resultAssembler.ToTurnResult(domainResolution);

            var dashResult = BuildDashStepResult(
                run,
                player,
                from,
                requestedDirection,
                domainResolution,
                turnResult);

            return new DashStepExecutionResultDto(
                dashStepResult: dashResult,
                snapshot: _resultAssembler.BuildSnapshot(run, hasRun: true),
                lifecycleEvents: _resultAssembler.DrainLifecycleEvents(run));
        }

        public DashStepExecutionResultDto BuildInvalidStateResult(int requestedDirectionValue)
        {
            return new DashStepExecutionResultDto(
                dashStepResult: new DashStepResultDto(
                    resolution: RunTurnResultDto.Empty,
                    shouldContinue: false,
                    nextDirectionValue: requestedDirectionValue,
                    stopReason: DashStopReason.InvalidState),
                snapshot: RunSnapshotDto.Empty,
                lifecycleEvents: Array.Empty<RunLifecycleEventDto>());
        }

        private DashStepResultDto BuildDashStepResult(
            RunSession run,
            Roguelike.Domain.Gameplay.Actors.Entities.Actor player,
            Position from,
            Direction requestedDirection,
            TurnResolution domainResolution,
            RunTurnResultDto turnResult)
        {
            var moveOutcome = domainResolution.PlayerMoveOutcome;
            if (!moveOutcome.HasValue || !moveOutcome.Success || moveOutcome.To == from)
            {
                return new DashStepResultDto(
                    resolution: turnResult,
                    shouldContinue: false,
                    nextDirectionValue: (int)requestedDirection,
                    stopReason: DashStopReason.ActionFailed);
            }

            var afterDecision = _dashStopPolicy.EvaluateAfterStep(run, player, from, requestedDirection);
            return new DashStepResultDto(
                resolution: turnResult,
                shouldContinue: afterDecision.ShouldContinue,
                nextDirectionValue: afterDecision.NextDirectionValue,
                stopReason: afterDecision.StopReason);
        }
    }
}
