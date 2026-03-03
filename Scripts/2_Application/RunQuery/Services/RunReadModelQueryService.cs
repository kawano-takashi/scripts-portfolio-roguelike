using System;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// RunSession から表示向けReadModelを取得するクエリサービスです。
    /// </summary>
    public sealed class RunReadModelQueryService
    {
        private readonly IRunReadStore _runRepository;

        public RunReadModelQueryService(IRunReadStore runRepository)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
        }

        public bool TryGetCurrentRunReadModel(out RunReadModelDto readModel)
        {
            if (!_runRepository.TryGetCurrent(out var run) ||
                run?.Player == null ||
                run.Map == null)
            {
                readModel = RunReadModelDto.Empty;
                return false;
            }

            readModel = RunReadModelAssembler.BuildReadModel(run, _runRepository.HasRun);
            return readModel.HasRun;
        }
    }
}
