using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Enums;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// ダッシュ1ステップを実行するユースケースです。
    /// </summary>
    public sealed class ExecuteDashStepCommandHandler : IUseCase<DashStepCommand, DashStepExecutionResultDto>
    {
        private readonly IRunWriteStore _runRepository;
        private readonly DashStepExecutionService _dashStepExecutionService;
        private readonly IValidator<DashStepCommand> _validator;

        public ExecuteDashStepCommandHandler(
            IRunWriteStore runRepository,
            DashStepExecutionService dashStepExecutionService,
            IValidator<DashStepCommand> validator)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
            _dashStepExecutionService = dashStepExecutionService
                ?? throw new ArgumentNullException(nameof(dashStepExecutionService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public Result<DashStepExecutionResultDto> Handle(DashStepCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsValid)
            {
                return Result<DashStepExecutionResultDto>.ValidationFailure(validation.Errors);
            }

            if (!_runRepository.TryGetCurrent(out var run) || run == null)
            {
                return Result<DashStepExecutionResultDto>.Failure("Active run was not found.");
            }

            var requestedDirection = DirectionMapper.ToDomain(command.RequestedDirection);
            var execution = _dashStepExecutionService.ExecuteStep(run, requestedDirection);
            _runRepository.Save(run);
            return Result<DashStepExecutionResultDto>.Success(execution);
        }
    }
}
