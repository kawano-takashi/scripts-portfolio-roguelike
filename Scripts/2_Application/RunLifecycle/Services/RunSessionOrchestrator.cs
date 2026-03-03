using System;
using Roguelike.Application.Commands;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ラン開始/遷移時の共通初期化フローを提供します。
    /// </summary>
    public sealed class RunSessionOrchestrator
    {
        private readonly IMapGenerationService _mapGenerationService;
        private readonly IRunPopulationService _populationService;
        private readonly PlayerInitializationPolicy _playerInitializationPolicy;
        private readonly RunBootstrapService _runBootstrapService;

        public RunSessionOrchestrator(
            IMapGenerationService mapGenerationService,
            IRunPopulationService populationService,
            PlayerInitializationPolicy playerInitializationPolicy,
            RunBootstrapService runBootstrapService)
        {
            _mapGenerationService = mapGenerationService ?? throw new ArgumentNullException(nameof(mapGenerationService));
            _populationService = populationService ?? throw new ArgumentNullException(nameof(populationService));
            _playerInitializationPolicy = playerInitializationPolicy ?? throw new ArgumentNullException(nameof(playerInitializationPolicy));
            _runBootstrapService = runBootstrapService ?? throw new ArgumentNullException(nameof(runBootstrapService));
        }

        internal RunSession CreateInitialSession(StartRunCommand command)
        {
            var floor = command.Floor;
            var clearFloor = command.ClearFloor;
            var seed = command.Seed ?? Environment.TickCount;

            var map = command.Width.HasValue && command.Height.HasValue
                ? _mapGenerationService.Generate(command.Width.Value, command.Height.Value, seed)
                : _mapGenerationService.Generate(seed);

            var startPosition = _runBootstrapService.ResolveStartPosition(map);
            var player = _playerInitializationPolicy.CreateInitialPlayer(command, startPosition);

            var session = new RunSession(seed, floor, map, player, clearFloor);
            if (command.StartImmediately)
            {
                session.StartRun();
            }

            _populationService.Populate(session);
            _runBootstrapService.ApplyInitialVisibility(session);
            return session;
        }

        internal RunSession BuildNextFloorSession(RunSession current)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (current.Map == null || current.Player == null)
            {
                return current;
            }

            if (current.Floor >= current.ClearFloor)
            {
                current.MarkCleared();
                return current;
            }

            var seed = current.Random?.Next() ?? Environment.TickCount;
            var size = current.Map.Size;
            var map = _mapGenerationService.Generate(size.Width, size.Height, seed);
            var startPosition = _runBootstrapService.ResolveStartPosition(map);

            var player = current.Player;
            var session = new RunSession(seed, current.Floor + 1, map, player, current.ClearFloor);
            _playerInitializationPolicy.PreparePlayerForNextFloor(session, startPosition);
            session.StartRun();

            _populationService.Populate(session);
            _runBootstrapService.ApplyInitialVisibility(session);
            return session;
        }
    }
}

