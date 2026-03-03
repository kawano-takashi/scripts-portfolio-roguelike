using System;
using Roguelike.Application.Commands;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Services;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// Application入力をDomain初期化サービスへ接続するアダプターです。
    /// </summary>
    public sealed class PlayerInitializationPolicy
    {
        private readonly IPlayerInitializationService _playerInitializationService;

        public PlayerInitializationPolicy(IPlayerInitializationService playerInitializationService)
        {
            _playerInitializationService = playerInitializationService
                ?? throw new ArgumentNullException(nameof(playerInitializationService));
        }

        public Actor CreateInitialPlayer(StartRunCommand command, Position startPosition)
        {
            return _playerInitializationService.CreateInitialPlayer(
                command.PlayerName,
                command.PlayerMaxHp,
                command.PlayerAttack,
                command.PlayerDefense,
                command.PlayerIntelligence,
                command.PlayerSightRadius,
                command.PlayerMaxHunger,
                startPosition);
        }

        public void PreparePlayerForNextFloor(RunSession session, Position startPosition)
        {
            _playerInitializationService.PreparePlayerForNextFloor(session, startPosition);
        }
    }
}
