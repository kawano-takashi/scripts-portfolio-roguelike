using System;
using Roguelike.Application.Enums;
using Roguelike.Application.Ports;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ランのアクセス可否判定に必要な状態を取得するクエリサービスです。
    /// </summary>
    public sealed class RunAccessQueryService
    {
        private readonly IRunReadStore _runRepository;

        public RunAccessQueryService(IRunReadStore runRepository)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
        }

        public bool HasActiveRun()
        {
            return _runRepository.HasRun;
        }

        public bool TryGetCurrentRunPhase(out RunPhaseDto phase)
        {
            if (!_runRepository.TryGetCurrent(out var run) || run == null)
            {
                phase = RunPhaseDto.None;
                return false;
            }

            phase = RunPhaseMapper.ToDto(run.Phase);
            return true;
        }
    }
}


