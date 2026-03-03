using System;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// RunSession からランのスナップショット情報を取得するクエリサービスです。
    /// </summary>
    public sealed class RunSnapshotQueryService
    {
        private readonly IRunReadStore _runRepository;

        public RunSnapshotQueryService(IRunReadStore runRepository)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
        }

        public bool TryGetCurrentRunSnapshot(out RunSnapshotDto snapshot)
        {
            if (!_runRepository.TryGetCurrent(out var run) ||
                run?.Player == null)
            {
                snapshot = RunSnapshotDto.Empty;
                return false;
            }

            snapshot = RunReadModelAssembler.BuildSnapshot(run, _runRepository.HasRun);
            return true;
        }
    }
}
