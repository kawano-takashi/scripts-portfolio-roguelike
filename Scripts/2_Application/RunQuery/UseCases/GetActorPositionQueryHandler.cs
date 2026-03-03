using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetActorPositionQuery(Guid ActorId) : IQuery<GridPositionDto>;

    /// <summary>
    /// 指定Actorの現在座標を取得するクエリです。
    /// </summary>
    public sealed class GetActorPositionQueryHandler : IUseCase<GetActorPositionQuery, GridPositionDto>
    {
        private readonly RunActorLocatorQueryService _runActorLocatorQueryService;

        public GetActorPositionQueryHandler(RunActorLocatorQueryService runActorLocatorQueryService)
        {
            _runActorLocatorQueryService = runActorLocatorQueryService
                ?? throw new ArgumentNullException(nameof(runActorLocatorQueryService));
        }

        public Result<GridPositionDto> Handle(GetActorPositionQuery query)
        {
            if (query == null)
            {
                return Result<GridPositionDto>.Failure("Query is required.");
            }

            if (query.ActorId == Guid.Empty)
            {
                return Result<GridPositionDto>.Failure("ActorId is required.");
            }

            if (!_runActorLocatorQueryService.TryGetActorPosition(query.ActorId, out var position))
            {
                return Result<GridPositionDto>.Failure("Actor position was not found.");
            }

            return Result<GridPositionDto>.Success(position);
        }
    }
}
