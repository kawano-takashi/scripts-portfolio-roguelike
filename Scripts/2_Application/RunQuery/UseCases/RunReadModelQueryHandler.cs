using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// 現在ラン状態を表示向けReadModelへ投影する参照ユースケースです。
    /// </summary>
    public sealed class RunReadModelQueryHandler : IUseCase<Unit, RunReadModelDto>
    {
        private readonly RunReadModelQueryService _runReadModelQueryService;

        public RunReadModelQueryHandler(RunReadModelQueryService runReadModelQueryService)
        {
            _runReadModelQueryService = runReadModelQueryService
                ?? throw new ArgumentNullException(nameof(runReadModelQueryService));
        }

        public Result<RunReadModelDto> Handle(Unit _)
        {
            if (!_runReadModelQueryService.TryGetCurrentRunReadModel(out var readModel))
            {
                return Result<RunReadModelDto>.Failure("Active run was not found.");
            }

            return Result<RunReadModelDto>.Success(readModel);
        }

        public Result<RunReadModelDto> Handle()
        {
            return Handle(Unit.Value);
        }
    }
}
