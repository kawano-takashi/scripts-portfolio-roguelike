using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Application.Services
{
    internal static class RunReadModelAssembler
    {
        public static RunSnapshotDto BuildSnapshot(RunSession run, bool hasRun)
        {
            if (run?.Player == null)
            {
                return RunSnapshotDto.Empty;
            }

            return new RunSnapshotDto(
                hasRun: hasRun,
                isActiveRun: run.Phase == Roguelike.Domain.Gameplay.Runs.Enums.RunPhase.InRun,
                floor: run.Floor,
                clearFloor: run.ClearFloor,
                turnCount: run.TurnCount,
                phase: RunPhaseMapper.ToDto(run.Phase),
                playerId: run.Player.Id.Value,
                playerLevel: run.Player.LevelProgress.Level,
                playerCurrentHp: run.Player.CurrentHp,
                playerMaxHp: run.Player.GetEffectiveMaxHp(),
                playerCurrentHunger: run.Player.CurrentHunger,
                playerMaxHunger: run.Player.Stats.MaxHunger,
                playerCurrentExp: run.Player.LevelProgress.CurrentExp,
                playerExpToNextLevel: run.Player.LevelProgress.ExpToNextLevel,
                playerPosition: new GridPositionDto(run.Player.Position.X, run.Player.Position.Y),
                playerFacingValue: (int)run.Player.Facing);
        }

        public static RunReadModelDto BuildReadModel(RunSession run, bool hasRun)
        {
            if (run?.Player == null || run.Map == null)
            {
                return RunReadModelDto.Empty;
            }

            var snapshot = BuildSnapshot(run, hasRun);
            var map = BuildMapSnapshot(run.Map);
            var player = new PlayerSnapshotDto(
                actorId: run.Player.Id.Value,
                name: run.Player.Name,
                position: new GridPositionDto(run.Player.Position.X, run.Player.Position.Y),
                facingValue: (int)run.Player.Facing,
                level: run.Player.LevelProgress.Level);

            var enemies = new List<EnemySnapshotDto>(run.Enemies.Count);
            for (var i = 0; i < run.Enemies.Count; i++)
            {
                var enemy = run.Enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                enemies.Add(new EnemySnapshotDto(
                    actorId: enemy.Id.Value,
                    name: enemy.Name,
                    position: new GridPositionDto(enemy.Position.X, enemy.Position.Y),
                    facingValue: (int)enemy.Facing,
                    enemyArchetypeValue: enemy.EnemyArchetype.HasValue ? (int?)enemy.EnemyArchetype.Value : null));
            }

            var groundItems = new List<GroundItemSnapshotDto>(run.Items.Count);
            for (var i = 0; i < run.Items.Count; i++)
            {
                var item = run.Items[i];
                if (item == null)
                {
                    continue;
                }

                groundItems.Add(new GroundItemSnapshotDto(
                    itemId: item.Id.Value,
                    itemTypeValue: (int)item.ItemType,
                    position: new GridPositionDto(item.Position.X, item.Position.Y)));
            }

            return new RunReadModelDto(
                hasRun: hasRun,
                snapshot: snapshot,
                map: map,
                player: player,
                enemies: enemies,
                groundItems: groundItems);
        }

        private static MapSnapshotDto BuildMapSnapshot(Roguelike.Domain.Gameplay.Maps.Entities.Map map)
        {
            if (map == null)
            {
                return MapSnapshotDto.Empty;
            }

            var tileCapacity = map.Size.Width * map.Size.Height;
            var tiles = new List<MapTileDto>(Math.Max(0, tileCapacity));
            foreach (var position in map.AllPositions())
            {
                var tile = map.GetTile(position);
                tiles.Add(new MapTileDto(
                    position: new GridPositionDto(position.X, position.Y),
                    tileTypeValue: (int)tile.Type,
                    isExplored: tile.IsExplored,
                    isVisible: tile.IsVisible));
            }

            var start = map.StartPosition.HasValue
                ? new GridPositionDto(map.StartPosition.Value.X, map.StartPosition.Value.Y)
                : (GridPositionDto?)null;
            var stairs = map.StairsDownPosition.HasValue
                ? new GridPositionDto(map.StairsDownPosition.Value.X, map.StairsDownPosition.Value.Y)
                : (GridPositionDto?)null;

            return new MapSnapshotDto(
                width: map.Size.Width,
                height: map.Size.Height,
                startPosition: start,
                stairsDownPosition: stairs,
                tiles: tiles);
        }
    }
}


