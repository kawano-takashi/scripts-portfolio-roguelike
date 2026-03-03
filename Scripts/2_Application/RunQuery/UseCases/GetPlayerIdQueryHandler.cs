using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetPlayerIdQuery() : IQuery<Guid>;

    /// <summary>
    /// プレイヤーActorIdを取得するクエリです。
    /// </summary>
    public sealed class GetPlayerIdQueryHandler : IUseCase<GetPlayerIdQuery, Guid>
    {
        private readonly RunActorLocatorQueryService _runActorLocatorQueryService;

        public GetPlayerIdQueryHandler(RunActorLocatorQueryService runActorLocatorQueryService)
        {
            _runActorLocatorQueryService = runActorLocatorQueryService
                ?? throw new ArgumentNullException(nameof(runActorLocatorQueryService));
        }

        public Result<Guid> Handle(GetPlayerIdQuery query)
        {
            if (query == null)
            {
                return Result<Guid>.Failure("Query is required.");
            }

            if (!_runActorLocatorQueryService.TryGetPlayerId(out var playerId))
            {
                return Result<Guid>.Failure("Player was not found.");
            }

            return Result<Guid>.Success(playerId);
        }
    }
}
