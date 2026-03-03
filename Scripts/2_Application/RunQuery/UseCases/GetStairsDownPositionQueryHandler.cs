using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetStairsDownPositionQuery() : IQuery<GridPositionDto>;

    /// <summary>
    /// 階段座標を取得するクエリです。
    /// </summary>
    public sealed class GetStairsDownPositionQueryHandler : IUseCase<GetStairsDownPositionQuery, GridPositionDto>
    {
        private readonly RunActorLocatorQueryService _runActorLocatorQueryService;

        public GetStairsDownPositionQueryHandler(RunActorLocatorQueryService runActorLocatorQueryService)
        {
            _runActorLocatorQueryService = runActorLocatorQueryService
                ?? throw new ArgumentNullException(nameof(runActorLocatorQueryService));
        }

        public Result<GridPositionDto> Handle(GetStairsDownPositionQuery query)
        {
            if (query == null)
            {
                return Result<GridPositionDto>.Failure("Query is required.");
            }

            if (!_runActorLocatorQueryService.TryGetStairsDownPosition(out var position))
            {
                return Result<GridPositionDto>.Failure("Stairs-down position was not found.");
            }

            return Result<GridPositionDto>.Success(position);
        }
    }
}
