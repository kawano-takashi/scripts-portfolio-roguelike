using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record IsPlayerActorQuery(Guid ActorId) : IQuery<bool>;

    /// <summary>
    /// 指定したActorIdがプレイヤーかどうかを判定するクエリです。
    /// </summary>
    public sealed class IsPlayerActorQueryHandler : IUseCase<IsPlayerActorQuery, bool>
    {
        private readonly RunActorLocatorQueryService _runActorLocatorQueryService;

        public IsPlayerActorQueryHandler(RunActorLocatorQueryService runActorLocatorQueryService)
        {
            _runActorLocatorQueryService = runActorLocatorQueryService
                ?? throw new ArgumentNullException(nameof(runActorLocatorQueryService));
        }

        public Result<bool> Handle(IsPlayerActorQuery query)
        {
            if (query == null)
            {
                return Result<bool>.Failure("Query is required.");
            }

            if (query.ActorId == Guid.Empty)
            {
                return Result<bool>.Failure("ActorId is required.");
            }

            return Result<bool>.Success(_runActorLocatorQueryService.IsPlayerActor(query.ActorId));
        }
    }
}
