using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// DashContinuationPolicy の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class DashContinuationPolicyTests
    {
        private readonly DashContinuationPolicy _sut = new DashContinuationPolicy();

        // 観点: EvaluateBeforeStep_StopsWithInvalidState_WhenInputsAreMissing の期待挙動を検証する。
        [Fact]
        public void EvaluateBeforeStep_StopsWithInvalidState_WhenInputsAreMissing()
        {
            var decision = _sut.EvaluateBeforeStep(run: null, actor: null, direction: Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.InvalidState, decision.StopReason);
        }

        // 観点: EvaluateBeforeStep_StopsWithEnemyAhead_WhenHostileBlocksForwardTile の期待挙動を検証する。
        [Fact]
        public void EvaluateBeforeStep_StopsWithEnemyAhead_WhenHostileBlocksForwardTile()
        {
            var session = CreateLineSession(withHostileAhead: true);

            var decision = _sut.EvaluateBeforeStep(session, session.Player, Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.EnemyAhead, decision.StopReason);
        }

        // 観点: EvaluateBeforeStep_StopsWithOccupiedAhead_WhenAllyBlocksForwardTile の期待挙動を検証する。
        [Fact]
        public void EvaluateBeforeStep_StopsWithOccupiedAhead_WhenAllyBlocksForwardTile()
        {
            var map = DomainTestFactory.CreateMap(
                width: 6,
                height: 3,
                floorTiles: new[]
                {
                    new Position(1, 1),
                    new Position(2, 1),
                    new Position(4, 1)
                });
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(4, 1));
            var actor = DomainTestFactory.CreateActor(name: "Runner", faction: Faction.Enemy, position: new Position(1, 1));
            var blocker = DomainTestFactory.CreateActor(name: "Ally", faction: Faction.Enemy, position: new Position(2, 1));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player, enemies: new[] { actor, blocker });

            var decision = _sut.EvaluateBeforeStep(session, actor, Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.OccupiedAhead, decision.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsWithActionFailed_WhenPositionDidNotChange の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsWithActionFailed_WhenPositionDidNotChange()
        {
            var session = CreateLineSession();
            var previous = session.Player.Position;

            var decision = _sut.EvaluateAfterStep(session, session.Player, previous, Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.ActionFailed, decision.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsWithEnemySighted_WhenHostileIsInSameRoom の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsWithEnemySighted_WhenHostileIsInSameRoom()
        {
            var room = new MapRect(1, 1, 3, 3);
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: new[]
                {
                    new Position(1, 1), new Position(2, 1), new Position(3, 1),
                    new Position(1, 2), new Position(2, 2), new Position(3, 2),
                    new Position(1, 3), new Position(2, 3), new Position(3, 3)
                },
                rooms: new[] { room });

            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 2));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(3, 2));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player, enemies: new[] { enemy });

            var decision = _sut.EvaluateAfterStep(session, player, previousPosition: new Position(1, 2), currentDirection: Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.EnemySighted, decision.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsWithEnemyNearby_WhenInCorridorAndHostileWithinRange の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsWithEnemyNearby_WhenInCorridorAndHostileWithinRange()
        {
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: new[]
                {
                    new Position(1, 1), new Position(2, 1), new Position(3, 1),
                    new Position(4, 2)
                });
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 1));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(4, 2));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player, enemies: new[] { enemy });

            var decision = _sut.EvaluateAfterStep(session, player, previousPosition: new Position(1, 1), currentDirection: Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.EnemyNearby, decision.StopReason);
        }

        // 観点: EvaluateAfterStep_StopsWithRoomBoundary_WhenCrossingBetweenRoomAndCorridor の期待挙動を検証する。
        [Fact]
        public void EvaluateAfterStep_StopsWithRoomBoundary_WhenCrossingBetweenRoomAndCorridor()
        {
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: new[] { new Position(1, 1), new Position(2, 1), new Position(3, 1) },
                rooms: new[] { new MapRect(1, 1, 2, 1) });
            var player = DomainTestFactory.CreateActor(position: new Position(3, 1));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player);

            var decision = _sut.EvaluateAfterStep(session, player, previousPosition: new Position(2, 1), currentDirection: Direction.Right);

            Assert.False(decision.ShouldContinue);
            Assert.Equal(DashContinuationStopReason.RoomBoundary, decision.StopReason);
        }

        private static Roguelike.Domain.Gameplay.Runs.Entities.RunSession CreateLineSession(bool withHostileAhead = false)
        {
            var map = DomainTestFactory.CreateMap(
                width: 6,
                height: 3,
                floorTiles: new[] { new Position(1, 1), new Position(2, 1), new Position(3, 1) });
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(1, 1));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player);

            if (withHostileAhead)
            {
                session.AddEnemy(DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 1)));
            }

            return session;
        }
    }
}
