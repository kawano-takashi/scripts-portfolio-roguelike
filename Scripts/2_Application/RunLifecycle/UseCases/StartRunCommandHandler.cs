using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// ラン開始コマンドを処理するユースケースです。
    /// </summary>
    public sealed class StartRunCommandHandler : IUseCase<StartRunCommand, RunStartResultDto>
    {
        private readonly IRunWriteStore _runRepository;
        private readonly RunSessionOrchestrator _sessionOrchestrator;
        private readonly RunExecutionResultAssembler _resultAssembler;
        private readonly IValidator<StartRunCommand> _validator;

        public StartRunCommandHandler(
            IRunWriteStore runRepository,
            RunSessionOrchestrator sessionOrchestrator,
            RunExecutionResultAssembler resultAssembler,
            IValidator<StartRunCommand> validator)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
            _sessionOrchestrator = sessionOrchestrator ?? throw new ArgumentNullException(nameof(sessionOrchestrator));
            _resultAssembler = resultAssembler ?? throw new ArgumentNullException(nameof(resultAssembler));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public Result<RunStartResultDto> Handle(StartRunCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsValid)
            {
                return Result<RunStartResultDto>.ValidationFailure(validation.Errors);
            }

            var session = _sessionOrchestrator.CreateInitialSession(command);
            _runRepository.Save(session);
            return Result<RunStartResultDto>.Success(
                _resultAssembler.BuildRunStartResult(session, started: true));
        }
    }
}
