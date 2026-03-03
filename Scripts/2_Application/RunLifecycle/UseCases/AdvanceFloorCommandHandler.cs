using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// 次フロア遷移コマンドを処理するユースケースです。
    /// </summary>
    public sealed class AdvanceFloorCommandHandler : IUseCase<AdvanceFloorCommand, FloorAdvanceResultDto>
    {
        private readonly IRunWriteStore _runRepository;
        private readonly RunSessionOrchestrator _sessionOrchestrator;
        private readonly RunExecutionResultAssembler _resultAssembler;

        public AdvanceFloorCommandHandler(
            IRunWriteStore runRepository,
            RunSessionOrchestrator sessionOrchestrator,
            RunExecutionResultAssembler resultAssembler)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
            _sessionOrchestrator = sessionOrchestrator ?? throw new ArgumentNullException(nameof(sessionOrchestrator));
            _resultAssembler = resultAssembler ?? throw new ArgumentNullException(nameof(resultAssembler));
        }

        public Result<FloorAdvanceResultDto> Handle(AdvanceFloorCommand command)
        {
            if (!_runRepository.TryGetCurrent(out var current) ||
                current?.Map == null ||
                current.Player == null)
            {
                return Result<FloorAdvanceResultDto>.Failure("Active run was not found.");
            }

            var nextSession = _sessionOrchestrator.BuildNextFloorSession(current);
            _runRepository.Save(nextSession);
            return Result<FloorAdvanceResultDto>.Success(
                _resultAssembler.BuildFloorAdvanceResult(nextSession, advanced: true));
        }

        public Result<FloorAdvanceResultDto> Handle()
        {
            return Handle(new AdvanceFloorCommand());
        }
    }
}
