using System;
using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// フロア遷移実行の結果DTOです。
    /// </summary>
    public readonly struct FloorAdvanceResultDto
    {
        private static readonly IReadOnlyList<RunLifecycleEventDto> EmptyLifecycleEvents = Array.Empty<RunLifecycleEventDto>();

        public static FloorAdvanceResultDto Empty => new FloorAdvanceResultDto(
            advanced: false,
            snapshot: RunSnapshotDto.Empty,
            lifecycleEvents: EmptyLifecycleEvents);

        public bool Advanced { get; }
        public RunSnapshotDto Snapshot { get; }
        public IReadOnlyList<RunLifecycleEventDto> LifecycleEvents { get; }

        public FloorAdvanceResultDto(
            bool advanced,
            RunSnapshotDto snapshot,
            IReadOnlyList<RunLifecycleEventDto> lifecycleEvents)
        {
            Advanced = advanced;
            Snapshot = snapshot;
            LifecycleEvents = lifecycleEvents ?? EmptyLifecycleEvents;
        }
    }
}
