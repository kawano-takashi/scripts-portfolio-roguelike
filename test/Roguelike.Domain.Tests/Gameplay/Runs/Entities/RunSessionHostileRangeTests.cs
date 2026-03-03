using System;
using System.Collections.Generic;
using Xunit;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Entities
{
    /// <summary>
    /// RunSessionHostileRange の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunSessionHostileRangeTests
    {
        // 観点: HasHostileWithinRange_ReturnsTrue_WhenHostileEnemyIsWithinDistance の期待挙動を検証する。
        [Fact]
        public void HasHostileWithinRange_ReturnsTrue_WhenHostileEnemyIsWithinDistance()
        {
            // 敵対勢力が指定距離内にいる場合に true を返すことを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(4, 4)
                });
            var session = CreateSession(map, new Position(2, 2));
            session.AddEnemy(CreateActor("Enemy", Faction.Enemy, new Position(4, 4)));

            var result = session.HasHostileWithinRange(new Position(2, 2), Faction.Player, 2);

            Assert.True(result);
        }

        // 観点: HasHostileWithinRange_ReturnsFalse_WhenHostileEnemyIsOutsideDistance の期待挙動を検証する。
        [Fact]
        public void HasHostileWithinRange_ReturnsFalse_WhenHostileEnemyIsOutsideDistance()
        {
            // 敵対勢力が指定距離外なら false を返すことを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(5, 5)
                });
            var session = CreateSession(map, new Position(2, 2));
            session.AddEnemy(CreateActor("Enemy", Faction.Enemy, new Position(5, 5)));

            var result = session.HasHostileWithinRange(new Position(2, 2), Faction.Player, 2);

            Assert.False(result);
        }

        // 観点: HasHostileWithinRange_IgnoresDeadAndAlliedActors の期待挙動を検証する。
        [Fact]
        public void HasHostileWithinRange_IgnoresDeadAndAlliedActors()
        {
            // 死亡済みプレイヤーと同陣営（Enemy）のアクターは検知対象から除外されることを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(3, 2),
                    new Position(3, 3)
                });
            var session = CreateSession(map, new Position(2, 2));
            session.AddEnemy(CreateActor("AllyEnemy", Faction.Enemy, new Position(3, 3)));

            session.Player.ApplyDamage(session.Player.CurrentHp);

            var result = session.HasHostileWithinRange(new Position(2, 2), Faction.Enemy, 2);

            Assert.False(result);
        }

        // 観点: HasHostileWithinRange_DetectsPlayerWhenSelfFactionIsEnemy の期待挙動を検証する。
        [Fact]
        public void HasHostileWithinRange_DetectsPlayerWhenSelfFactionIsEnemy()
        {
            // 自分が敵陣営として判定する場合はプレイヤーを敵対対象として検知できることを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(1, 1),
                    new Position(3, 3)
                });
            var session = CreateSession(map, new Position(3, 3));

            var result = session.HasHostileWithinRange(new Position(1, 1), Faction.Enemy, 2);

            Assert.True(result);
        }

        // 観点: HasHostileWithinRange_Throws_WhenDistanceIsNotPositive の期待挙動を検証する。
        [Fact]
        public void HasHostileWithinRange_Throws_WhenDistanceIsNotPositive()
        {
            // 距離が 1 未満の場合は例外を送出して不正入力を拒否することを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 2)
                });
            var session = CreateSession(map, new Position(2, 2));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => session.HasHostileWithinRange(new Position(1, 1), Faction.Player, 0));
        }

        private static RunSession CreateSession(Map map, Position playerPosition)
        {
            var player = CreateActor("Player", Faction.Player, playerPosition);
            var session = new RunSession(1234, 1, map, player, clearFloor: 10);
            session.StartRun();
            return session;
        }

        private static Actor CreateActor(string name, Faction faction, Position position)
        {
            return new Actor(
                ActorId.NewId(),
                name,
                faction,
                position,
                new ActorStats(10, 4, 2, 10, 5, 100f));
        }

        private static Map CreateMap(int width, int height, IEnumerable<Position> floorTiles)
        {
            var map = new Map(width, height);

            foreach (var floor in floorTiles)
            {
                map.SetTileType(floor, TileType.Floor);
            }

            return map;
        }
    }
}






