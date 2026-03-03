using System;
using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 通常コマンド実行の結果DTOです。
    /// </summary>
    public readonly struct RunCommandExecutionResultDto
    {
        private static readonly IReadOnlyList<RunLifecycleEventDto> EmptyLifecycleEvents = Array.Empty<RunLifecycleEventDto>();

        public static RunCommandExecutionResultDto Empty => new RunCommandExecutionResultDto(
            turnResult: RunTurnResultDto.Empty,
            snapshot: RunSnapshotDto.Empty,
            lifecycleEvents: EmptyLifecycleEvents);

        public RunTurnResultDto TurnResult { get; }
        public RunSnapshotDto Snapshot { get; }
        public IReadOnlyList<RunLifecycleEventDto> LifecycleEvents { get; }

        public RunCommandExecutionResultDto(
            RunTurnResultDto turnResult,
            RunSnapshotDto snapshot,
            IReadOnlyList<RunLifecycleEventDto> lifecycleEvents)
        {
            TurnResult = turnResult;
            Snapshot = snapshot;
            LifecycleEvents = lifecycleEvents ?? EmptyLifecycleEvents;
        }
    }
}
