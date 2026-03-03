using System;
using System.Collections.Generic;
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

namespace Roguelike.Tests.Domain.TestSupport
{
    internal static class DomainTestFactory
    {
        public static Actor CreateActor(
            string name = "Actor",
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
                position ?? Position.Zero,
                stats ?? new ActorStats(maxHp: 20, attack: 4, defense: 2, intelligence: 12, sightRadius: 8, maxHunger: 100f),
                enemyArchetype,
                facing);
        }

        public static Map CreateMap(
            int width = 8,
            int height = 8,
            IEnumerable<Position> floorTiles = null,
            IEnumerable<MapRect> rooms = null,
            Position? start = null,
            Position? stairs = null)
        {
            var map = new Map(width, height);

            if (floorTiles == null)
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
                foreach (var floor in floorTiles)
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
            map ??= CreateMap();
            player ??= CreateActor(name: "Player", faction: Faction.Player, position: map.StartPosition ?? new Position(1, 1));

            var session = new RunSession(seed, floor, map, player, clearFloor, enemies);
            ApplyPhase(session, phase);

            if (items != null)
            {
                foreach (var item in items)
                {
                    session.AddItem(item);
                }
            }

            return session;
        }

        public static InventoryItem CreateInventoryItem(ItemId itemType, Guid? itemId = null)
        {
            return new InventoryItem(new ItemInstanceId(itemId ?? Guid.NewGuid()), itemType);
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
    }
}

