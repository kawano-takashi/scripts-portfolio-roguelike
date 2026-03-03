using System;
using System.Collections.Generic;
using Roguelike.Application.Commands;
using Roguelike.Application.Ports;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Tests.Application.TestSupport
{
    internal static class ApplicationTestFactory
    {
        public static StartRunCommand CreateStartRunCommand(
            int floor = 1,
            int clearFloor = 10,
            int? seed = 1234,
            int? width = null,
            int? height = null,
            bool startImmediately = true,
            string playerName = "tester")
        {
            return new StartRunCommand(
                PlayerName: playerName,
                Floor: floor,
                ClearFloor: clearFloor,
                Seed: seed,
                Width: width,
                Height: height,
                StartImmediately: startImmediately,
                PlayerMaxHp: 20,
                PlayerAttack: 3,
                PlayerDefense: 1,
                PlayerIntelligence: 14,
                PlayerSightRadius: 8,
                PlayerMaxHunger: 100f);
        }

        public static Actor CreateActor(
            string name = "Player",
            Faction faction = Faction.Player,
            Position? position = null,
            ActorStats? stats = null,
            EnemyArchetype? enemyArchetype = null,
            Direction facing = Direction.Down)
        {
            return new Actor(
                ActorId.NewId(),
                name,
                faction,
                position ?? new Position(1, 1),
                stats ?? new ActorStats(maxHp: 20, attack: 3, defense: 1, intelligence: 14, sightRadius: 8, maxHunger: 100f),
                enemyArchetype,
                facing);
        }

        public static Map CreateMap(
            int width = 8,
            int height = 8,
            IEnumerable<Position> floors = null,
            IEnumerable<MapRect> rooms = null,
            Position? start = null,
            Position? stairs = null)
        {
            var map = new Map(width, height);

            if (floors == null)
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        map.SetTileType(new Position(x, y), TileType.Floor);
                    }
                }
            }
            else
            {
                foreach (var floor in floors)
                {
                    map.SetTileType(floor, TileType.Floor);
                }
            }

            if (rooms != null)
            {
                map.SetRooms(rooms);
            }

            if (start.HasValue)
            {
                map.SetStartPosition(start.Value);
            }

            if (stairs.HasValue)
            {
                map.SetStairsDownPosition(stairs.Value);
            }

            return map;
        }

        public static RunSession CreateRunSession(
            Map map = null,
            Actor player = null,
            IEnumerable<Actor> enemies = null,
            IEnumerable<MapItem> items = null,
            int seed = 1234,
            int floor = 1,
            int clearFloor = 10,
            RunPhase phase = RunPhase.InRun)
        {
            map ??= CreateMap(start: new Position(1, 1));
            player ??= CreateActor(position: map.StartPosition ?? new Position(1, 1));

            var run = new RunSession(seed, floor, map, player, clearFloor, enemies);
            ApplyPhase(run, phase);

            if (items != null)
            {
                foreach (var item in items)
                {
                    run.AddItem(item);
                }
            }

            return run;
        }

        public static InventoryItem AddInventoryItem(Actor actor, ItemId itemType, Guid? itemId = null)
        {
            var item = new InventoryItem(new ItemInstanceId(itemId ?? Guid.NewGuid()), itemType);
            if (!actor.AddToInventory(item))
            {
                throw new InvalidOperationException("Failed to add item to actor inventory.");
            }

            return item;
        }

        private static void ApplyPhase(RunSession session, RunPhase phase)
        {
            switch (phase)
            {
                case RunPhase.RunStart:
                    return;
                case RunPhase.InRun:
                    session.StartRun();
                    return;
                case RunPhase.Pause:
                    session.StartRun();
                    session.PauseRun();
                    return;
                case RunPhase.GameOver:
                    session.StartRun();
                    session.MarkGameOver();
                    return;
                case RunPhase.Clear:
                    session.StartRun();
                    session.MarkCleared();
                    return;
                default:
                    throw new InvalidOperationException($"Unsupported test phase: {phase}");
            }
        }

        internal sealed class SpyRunStore : IRunWriteStore
        {
            private RunSession _current;

            public SpyRunStore(RunSession current = null)
            {
                _current = current;
            }

            public int SaveCount { get; private set; }
            public int ClearCount { get; private set; }
            public bool HasRun => _current != null;
            public RunSession Current => _current;
            public RunSession LastSaved { get; private set; }

            public void Save(RunSession session)
            {
                SaveCount++;
                LastSaved = session;
                _current = session;
            }

            public bool TryGetCurrent(out RunSession run)
            {
                run = _current;
                return run != null;
            }

            public void Clear()
            {
                ClearCount++;
                _current = null;
            }
        }

        internal sealed class PersistentRunStoreFake : IRunWriteStore
        {
            private readonly List<RunSession> _savedSessions = new List<RunSession>();
            private RunSession _current;

            public PersistentRunStoreFake(RunSession current = null)
            {
                _current = current;
            }

            public bool HasRun => _current != null;
            public RunSession Current => _current;
            public RunSession LastSaved => _savedSessions.Count == 0 ? null : _savedSessions[_savedSessions.Count - 1];
            public IReadOnlyList<RunSession> SavedSessions => _savedSessions;

            public void Save(RunSession session)
            {
                _savedSessions.Add(session);
                _current = session;
            }

            public bool TryGetCurrent(out RunSession run)
            {
                run = _current;
                return run != null;
            }

            public void Clear()
            {
                _current = null;
            }
        }
    }
}

