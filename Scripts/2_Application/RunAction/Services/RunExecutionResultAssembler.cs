using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// 実行結果DTOを組み立てる実装です。
    /// </summary>
    public sealed class RunExecutionResultAssembler
    {
        private readonly RunEventProjector _eventProjector;

        public RunExecutionResultAssembler(RunEventProjector eventProjector)
        {
            _eventProjector = eventProjector ?? throw new ArgumentNullException(nameof(eventProjector));
        }

        public RunTurnResultDto ToTurnResult(TurnResolution resolution)
        {
            var projectedEvents = _eventProjector.ProjectDomainEvents(resolution.Events, resolution.TurnNumber);
            return new RunTurnResultDto(
                turnConsumed: resolution.TurnConsumed,
                turnNumber: resolution.TurnNumber,
                actionResolved: resolution.ActionResolved,
                events: projectedEvents);
        }

        public RunTurnResultDto CreateNoopTurnResult(int turnNumber)
        {
            return new RunTurnResultDto(
                turnConsumed: false,
                turnNumber: turnNumber,
                actionResolved: false,
                events: Array.Empty<IRunEventDto>());
        }

        public RunCommandExecutionResultDto CreateNoopCommandExecutionResult(RunSession run)
        {
            if (run == null)
            {
                return RunCommandExecutionResultDto.Empty;
            }

            return new RunCommandExecutionResultDto(
                turnResult: CreateNoopTurnResult(run.TurnCount),
                snapshot: BuildSnapshot(run, hasRun: true),
                lifecycleEvents: Array.Empty<RunLifecycleEventDto>());
        }

        public RunSnapshotDto BuildSnapshot(RunSession run, bool hasRun)
        {
            return RunReadModelAssembler.BuildSnapshot(run, hasRun);
        }

        public IReadOnlyList<RunLifecycleEventDto> DrainLifecycleEvents(RunSession run)
        {
            if (run == null)
            {
                return Array.Empty<RunLifecycleEventDto>();
            }

            var lifecycleEvents = run.DrainLifecycleEvents();
            return _eventProjector.ProjectDomainLifecycleEvents(lifecycleEvents);
        }

        public RunStartResultDto BuildRunStartResult(RunSession run, bool started)
        {
            return new RunStartResultDto(
                started: started,
                snapshot: BuildSnapshot(run, hasRun: started),
                lifecycleEvents: DrainLifecycleEvents(run));
        }

        public FloorAdvanceResultDto BuildFloorAdvanceResult(RunSession run, bool advanced)
        {
            return new FloorAdvanceResultDto(
                advanced: advanced,
                snapshot: BuildSnapshot(run, hasRun: advanced),
                lifecycleEvents: DrainLifecycleEvents(run));
        }
    }
}



