using System;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ラン内アクターの識別情報と座標を取得するクエリサービスです。
    /// </summary>
    public sealed class RunActorLocatorQueryService
    {
        private readonly IRunReadStore _runRepository;

        public RunActorLocatorQueryService(IRunReadStore runRepository)
        {
            _runRepository = runRepository ?? throw new ArgumentNullException(nameof(runRepository));
        }

        public bool TryGetPlayerId(out Guid playerId)
        {
            if (_runRepository.TryGetCurrent(out var run) &&
                run?.Player != null)
            {
                playerId = run.Player.Id.Value;
                return true;
            }

            playerId = Guid.Empty;
            return false;
        }

        public bool IsPlayerActor(Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return false;
            }

            return _runRepository.TryGetCurrent(out var run) &&
                run?.Player != null &&
                run.Player.Id.Value == actorId;
        }

        public bool TryGetStairsDownPosition(out GridPositionDto position)
        {
            if (!_runRepository.TryGetCurrent(out var run) ||
                run?.Map == null ||
                !run.Map.StairsDownPosition.HasValue)
            {
                position = default;
                return false;
            }

            var stairs = run.Map.StairsDownPosition.Value;
            position = new GridPositionDto(stairs.X, stairs.Y);
            return true;
        }

        public bool TryGetActorPosition(Guid actorId, out GridPositionDto position)
        {
            position = default;
            if (actorId == Guid.Empty)
            {
                return false;
            }

            if (!_runRepository.TryGetCurrent(out var run) ||
                run?.Player == null)
            {
                return false;
            }

            if (run.Player.Id.Value == actorId)
            {
                position = new GridPositionDto(run.Player.Position.X, run.Player.Position.Y);
                return true;
            }

            for (var i = 0; i < run.Enemies.Count; i++)
            {
                var enemy = run.Enemies[i];
                if (enemy?.Id.Value != actorId)
                {
                    continue;
                }

                position = new GridPositionDto(enemy.Position.X, enemy.Position.Y);
                return true;
            }

            return false;
        }
    }
}
