using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Runs.Services;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// 探索中アクションコマンドを処理するユースケースです。
    /// </summary>
    public sealed class RunActionCommandHandler : IUseCase<RunActionCommand, RunCommandExecutionResultDto>
    {
        private readonly IRunWriteStore _runRepository;
        private readonly ITurnEngine _turnEngine;
        private readonly RunExecutionResultAssembler _resultAssembler;
        private readonly RunActionFactory _actionFactory;
        private readonly IValidator<RunActionCommand> _validator;

        public RunActionCommandHandler(
            IRunWriteStore runRepository,
            ITurnEngine turnEngine,
            RunExecutionResultAssembler resultAssembler,
            RunActionFactory actionFactory,
            IValidator<RunActionCommand> validator)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
            _turnEngine = turnEngine ?? throw new ArgumentNullException(nameof(turnEngine));
            _resultAssembler = resultAssembler ?? throw new ArgumentNullException(nameof(resultAssembler));
            _actionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public Result<RunCommandExecutionResultDto> Handle(RunActionCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsValid)
            {
                return Result<RunCommandExecutionResultDto>.ValidationFailure(validation.Errors);
            }

            if (!_runRepository.TryGetCurrent(out var run) || run == null)
            {
                return Result<RunCommandExecutionResultDto>.Failure("Active run was not found.");
            }

            if (!_actionFactory.TryCreateForCurrentPlayer(run, command, out var action))
            {
                return Result<RunCommandExecutionResultDto>.Failure("Action could not be created.");
            }

            var domainResolution = _turnEngine.Resolve(run, action);
            _runRepository.Save(run);

            var result = new RunCommandExecutionResultDto(
                turnResult: _resultAssembler.ToTurnResult(domainResolution),
                snapshot: _resultAssembler.BuildSnapshot(run, hasRun: true),
                lifecycleEvents: _resultAssembler.DrainLifecycleEvents(run));
            return Result<RunCommandExecutionResultDto>.Success(result);
        }
    }
}
