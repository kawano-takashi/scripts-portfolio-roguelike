using System;
using System.Collections.Generic;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// ダッシュ1ステップ実行の結果DTOです。
    /// </summary>
    public readonly struct DashStepExecutionResultDto
    {
        private static readonly IReadOnlyList<RunLifecycleEventDto> EmptyLifecycleEvents = Array.Empty<RunLifecycleEventDto>();

        public static DashStepExecutionResultDto Empty => new DashStepExecutionResultDto(
            dashStepResult: new DashStepResultDto(
                    resolution: RunTurnResultDto.Empty,
                    shouldContinue: false,
                    nextDirectionValue: 0,
                    stopReason: DashStopReason.InvalidState),
            snapshot: RunSnapshotDto.Empty,
            lifecycleEvents: EmptyLifecycleEvents);

        public DashStepResultDto DashStepResult { get; }
        public RunSnapshotDto Snapshot { get; }
        public IReadOnlyList<RunLifecycleEventDto> LifecycleEvents { get; }

        public DashStepExecutionResultDto(
            DashStepResultDto dashStepResult,
            RunSnapshotDto snapshot,
            IReadOnlyList<RunLifecycleEventDto> lifecycleEvents)
        {
            DashStepResult = dashStepResult;
            Snapshot = snapshot;
            LifecycleEvents = lifecycleEvents ?? EmptyLifecycleEvents;
        }
    }
}
