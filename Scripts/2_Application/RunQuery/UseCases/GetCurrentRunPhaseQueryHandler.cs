using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetCurrentRunPhaseQuery() : IQuery<RunPhaseDto>;

    /// <summary>
    /// 現在ランのフェーズを取得するクエリです。
    /// </summary>
    public sealed class GetCurrentRunPhaseQueryHandler : IUseCase<GetCurrentRunPhaseQuery, RunPhaseDto>
    {
        private readonly RunAccessQueryService _runAccessQueryService;

        public GetCurrentRunPhaseQueryHandler(RunAccessQueryService runAccessQueryService)
        {
            _runAccessQueryService = runAccessQueryService
                ?? throw new ArgumentNullException(nameof(runAccessQueryService));
        }

        public Result<RunPhaseDto> Handle(GetCurrentRunPhaseQuery query)
        {
            if (query == null)
            {
                return Result<RunPhaseDto>.Failure("Query is required.");
            }

            if (!_runAccessQueryService.TryGetCurrentRunPhase(out var phase))
            {
                return Result<RunPhaseDto>.Failure("Active run was not found.");
            }

            return Result<RunPhaseDto>.Success(phase);
        }
    }
}
