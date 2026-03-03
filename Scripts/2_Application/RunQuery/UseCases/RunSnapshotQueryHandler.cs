using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// 現在ランのスナップショットを参照するクエリです。
    /// </summary>
    public sealed class RunSnapshotQueryHandler : IUseCase<Unit, RunSnapshotDto>
    {
        private readonly RunSnapshotQueryService _runSnapshotQueryService;

        public RunSnapshotQueryHandler(RunSnapshotQueryService runSnapshotQueryService)
        {
            _runSnapshotQueryService = runSnapshotQueryService
                ?? throw new ArgumentNullException(nameof(runSnapshotQueryService));
        }

        public Result<RunSnapshotDto> Handle(Unit _)
        {
            if (!_runSnapshotQueryService.TryGetCurrentRunSnapshot(out var snapshot))
            {
                return Result<RunSnapshotDto>.Failure("Active run was not found.");
            }

            return Result<RunSnapshotDto>.Success(snapshot);
        }

        public Result<RunSnapshotDto> Handle()
        {
            return Handle(Unit.Value);
        }
    }
}
