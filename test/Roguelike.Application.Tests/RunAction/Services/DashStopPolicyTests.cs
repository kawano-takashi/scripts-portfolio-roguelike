using System.Collections.Generic;
using Xunit;
using Roguelike.Application.Services;
using Roguelike.Application.Enums;
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
using Roguelike.Domain.Gameplay.Runs.Services;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// DashStopPolicy の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class DashStopPolicyTests
    {
        private readonly DashStopPolicy _policy;

        public DashStopPolicyTests()
        {
            _policy = new DashStopPolicy(new DashContinuationPolicy());
        }

        // 観点: EvaluateBeforeStep_StopsWhenEnemyAhead の期待挙動を検証する。
        [Fact]
        public void EvaluateBeforeStep_StopsWhenEnemyAhead()
        {
            // 前方に敵がいる場合はダッシュを停止し、停止理由が EnemyAhead になることを確認する。
            var map = CreateMap(
                6,
                6,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 1)
                });
            var session = CreateSession(map, new Position(1, 1), new[] { new Position(2, 1) });

            var result = _policy.EvaluateBeforeStep(session, session.Player, Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.EnemyAhead, result.StopReason);
        }

        // 観点: EvaluateBeforeStep_StopsWhenAllyAhead の期待挙動を検証する。
        [Fact]
        public void EvaluateBeforeStep_StopsWhenAllyAhead()
        {
            // 前方に同陣営アクターがいる場合は占有扱いで停止することを確認する。
            var map = CreateMap(
                6,
                6,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 1),
                    new Position(4, 1)
                });
            var session = CreateSession(map, new Position(4, 1));
            var actor = CreateActor("Runner", Faction.Enemy, new Position(1, 1));
            session.AddEnemy(actor);
            session.AddEnemy(CreateActor("Ally", Faction.Enemy, new Position(2, 1)));

            var result = _policy.EvaluateBeforeStep(session, actor, Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.OccupiedAhead, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsOnItem の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsOnItem()
        {
            // 移動後に足元へアイテムがある場合は OnItem で停止することを確認する。
            var map = CreateMap(
                6,
                6,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 1)
                });
            var session = CreateSession(map, new Position(2, 1));
            session.AddItem(MapItem.Create(ItemId.FoodRation, new Position(2, 1)));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(1, 1),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.OnItem, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsOnStairs の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsOnStairs()
        {
            // 移動後に階段マスへ乗った場合は OnStairs で停止することを確認する。
            var map = CreateMap(
                6,
                6,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 1)
                });
            map.SetStairsDownPosition(new Position(2, 1));

            var session = CreateSession(map, new Position(2, 1));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(1, 1),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.OnStairs, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsAtRoomBoundary の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsAtRoomBoundary()
        {
            // 部屋境界を越えたタイミングで RoomBoundary として停止することを確認する。
            var map = CreateMap(
                6,
                6,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 1),
                    new Position(3, 1)
                },
                new[] { new MapRect(1, 1, 2, 1) });
            var session = CreateSession(map, new Position(3, 1));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(2, 1),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.RoomBoundary, result.StopReason);
        }

        // 観点: EvaluateAfterStep_CorridorAutoTurn の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_CorridorAutoTurn()
        {
            // 通路が一方向に折れている場合は自動で向きを変えて継続することを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(3, 2),
                    new Position(3, 3)
                });
            var session = CreateSession(map, new Position(3, 2));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(2, 2),
                currentDirection: Direction.Right);

            Assert.True(result.ShouldContinue);
            Assert.Equal((int)Direction.Down, result.NextDirectionValue);
            Assert.Equal(DashStopReason.None, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsAtCorridorJunction の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsAtCorridorJunction()
        {
            // 通路の分岐点に到達した場合は Junction として停止することを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(3, 2),
                    new Position(4, 2),
                    new Position(3, 3)
                });
            var session = CreateSession(map, new Position(3, 2));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(2, 2),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.Junction, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsAtCorridorDeadEnd の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsAtCorridorDeadEnd()
        {
            // 通路の行き止まりに到達した場合は DeadEnd で停止することを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(3, 2)
                });
            var session = CreateSession(map, new Position(3, 2));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(2, 2),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.DeadEnd, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsWhenEnemyWithinTwoTilesInCorridor の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsWhenEnemyWithinTwoTilesInCorridor()
        {
            // 通路移動中に 2 マス以内へ敵が入った場合は EnemyNearby で停止することを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(3, 2),
                    new Position(4, 2),
                    new Position(5, 4)
                });
            var session = CreateSession(map, new Position(3, 2), new[] { new Position(5, 4) });

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(2, 2),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.EnemyNearby, result.StopReason);
        }

        // 観点: EvaluateAfterStep_DoesNotStopWhenEnemyIsBeyondTwoTilesInCorridor の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_DoesNotStopWhenEnemyIsBeyondTwoTilesInCorridor()
        {
            // 通路移動中に敵が 2 マスより遠い場合は停止せず進行を継続することを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(2, 2),
                    new Position(3, 2),
                    new Position(4, 2),
                    new Position(5, 2),
                    new Position(6, 5)
                });
            var session = CreateSession(map, new Position(3, 2), new[] { new Position(6, 5) });

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(2, 2),
                currentDirection: Direction.Right);

            Assert.True(result.ShouldContinue);
            Assert.Equal((int)Direction.Right, result.NextDirectionValue);
            Assert.Equal(DashStopReason.None, result.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsWhenEnemySightedInRoom の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsWhenEnemySightedInRoom()
        {
            // 部屋内で敵を視認した場合は EnemySighted で停止することを確認する。
            var floorTiles = new List<Position>();
            for (int x = 1; x <= 3; x++)
            {
                for (int y = 1; y <= 3; y++)
                {
                    floorTiles.Add(new Position(x, y));
                }
            }

            var map = CreateMap(
                8,
                8,
                floorTiles,
                new[] { new MapRect(1, 1, 3, 3) });
            var session = CreateSession(map, new Position(2, 2), new[] { new Position(3, 2) });

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(1, 2),
                currentDirection: Direction.Right);

            Assert.False(result.ShouldContinue);
            Assert.Equal(DashStopReason.EnemySighted, result.StopReason);
        }

        // 観点: EvaluateAfterStep_ContinuesDiagonalDash の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_ContinuesDiagonalDash()
        {
            // 斜めダッシュが継続条件を満たす場合は方向を維持したまま進むことを確認する。
            var map = CreateMap(
                8,
                8,
                new[]
                {
                    new Position(1, 1),
                    new Position(2, 2),
                    new Position(3, 3),
                    new Position(3, 2),
                    new Position(2, 3)
                });
            var session = CreateSession(map, new Position(2, 2));

            var result = _policy.EvaluateAfterStep(
                session,
                session.Player,
                previousPosition: new Position(1, 1),
                currentDirection: Direction.DownRight);

            Assert.True(result.ShouldContinue);
            Assert.Equal((int)Direction.DownRight, result.NextDirectionValue);
            Assert.Equal(DashStopReason.None, result.StopReason);
        }

        private static RunSession CreateSession(Map map, Position playerPosition, IEnumerable<Position> enemyPositions = null)
        {
            var player = CreateActor("Player", Faction.Player, playerPosition);
            var session = new RunSession(1234, 1, map, player, clearFloor: 10);
            session.StartRun();

            if (enemyPositions != null)
            {
                foreach (var enemyPosition in enemyPositions)
                {
                    session.AddEnemy(CreateActor("Enemy", Faction.Enemy, enemyPosition));
                }
            }

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

        private static Map CreateMap(int width, int height, IEnumerable<Position> floorTiles, IEnumerable<MapRect> rooms = null)
        {
            var map = new Map(width, height);

            foreach (var floor in floorTiles)
            {
                map.SetTileType(floor, TileType.Floor);
            }

            if (rooms != null)
            {
                map.SetRooms(rooms);
            }

            return map;
        }
    }
}








