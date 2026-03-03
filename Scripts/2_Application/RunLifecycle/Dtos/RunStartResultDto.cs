using System;
using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 新規ラン開始の結果DTOです。
    /// </summary>
    public readonly struct RunStartResultDto
    {
        private static readonly IReadOnlyList<RunLifecycleEventDto> EmptyLifecycleEvents = Array.Empty<RunLifecycleEventDto>();

        public static RunStartResultDto Empty => new RunStartResultDto(
            started: false,
            snapshot: RunSnapshotDto.Empty,
            lifecycleEvents: EmptyLifecycleEvents);

        public bool Started { get; }
        public RunSnapshotDto Snapshot { get; }
        public IReadOnlyList<RunLifecycleEventDto> LifecycleEvents { get; }

        public RunStartResultDto(
            bool started,
            RunSnapshotDto snapshot,
            IReadOnlyList<RunLifecycleEventDto> lifecycleEvents)
        {
            Started = started;
            Snapshot = snapshot;
            LifecycleEvents = lifecycleEvents ?? EmptyLifecycleEvents;
        }
    }
}
