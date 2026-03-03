using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Entities
{
    /// <summary>
    /// RunSession の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunSessionTests
    {
        // 観点: Constructor_InitializesRunStartPhase の期待挙動を検証する。
        [Fact]
        public void Constructor_InitializesRunStartPhase()
        {
            var map = DomainTestFactory.CreateMap();
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player);

            var session = new RunSession(seed: 1, floor: 1, map, player, clearFloor: 10);

            Assert.Equal(RunPhase.RunStart, session.Phase);
            Assert.Equal(1, session.Floor);
            Assert.Equal(10, session.ClearFloor);
        }

        // 観点: SetPhase_GameOver_AddsLifecycleEventOnlyOnce の期待挙動を検証する。
        [Fact]
        public void SetPhase_GameOver_AddsLifecycleEventOnlyOnce()
        {
            var session = CreateInRunSession();

            session.MarkGameOver();
            session.MarkGameOver();
            var events = session.DrainLifecycleEvents();

            var gameOver = Assert.Single(events);
            Assert.IsType<RunGameOverEvent>(gameOver);
            Assert.Equal(RunPhase.GameOver, session.Phase);
        }

        // 観点: SetPhase_Clear_AddsLifecycleEventOnlyOnce の期待挙動を検証する。
        [Fact]
        public void SetPhase_Clear_AddsLifecycleEventOnlyOnce()
        {
            var session = CreateInRunSession();

            session.MarkCleared();
            session.MarkCleared();
            var events = session.DrainLifecycleEvents();

            var cleared = Assert.Single(events);
            Assert.IsType<RunClearedEvent>(cleared);
            Assert.Equal(RunPhase.Clear, session.Phase);
        }

        // 観点: DrainLifecycleEvents_ClearsInternalBuffer の期待挙動を検証する。
        [Fact]
        public void DrainLifecycleEvents_ClearsInternalBuffer()
        {
            var session = CreateInRunSession();
            session.MarkGameOver();

            var first = session.DrainLifecycleEvents();
            var second = session.DrainLifecycleEvents();

            Assert.Single(first);
            Assert.Empty(second);
        }

        // 観点: CanActorStepFrom_ReturnsFalse_WhenDiagonalCornerIsBlocked の期待挙動を検証する。
        [Fact]
        public void CanActorStepFrom_ReturnsFalse_WhenDiagonalCornerIsBlocked()
        {
            var map = DomainTestFactory.CreateMap(5, 5);
            map.SetTileType(new Position(2, 1), TileType.Wall);
            map.SetTileType(new Position(1, 2), TileType.Wall);

            var player = DomainTestFactory.CreateActor(position: new Position(1, 1));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player);

            var canStep = session.CanActorStepFrom(player, new Position(1, 1), Direction.DownRight, out _);

            Assert.False(canStep);
        }

        // 観点: TryGetSingleCorridorForwardDirection_ReturnsSinglePathDirection の期待挙動を検証する。
        [Fact]
        public void TryGetSingleCorridorForwardDirection_ReturnsSinglePathDirection()
        {
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: new[]
                {
                    new Position(2, 2),
                    new Position(3, 2),
                    new Position(4, 2)
                });
            var player = DomainTestFactory.CreateActor(position: new Position(3, 2));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player);

            var result = session.TryGetSingleCorridorForwardDirection(
                player,
                current: new Position(3, 2),
                previous: new Position(2, 2),
                out var direction,
                out var state);

            Assert.True(result);
            Assert.Equal(Direction.Right, direction);
            Assert.Equal(CorridorPathState.SinglePath, state);
        }

        // 観点: TryTriggerMonsterHouse_AwakensSleepingEnemies_AndDoesNotTriggerTwice の期待挙動を検証する。
        [Fact]
        public void TryTriggerMonsterHouse_AwakensSleepingEnemies_AndDoesNotTriggerTwice()
        {
            var room = new MapRect(1, 1, 3, 3);
            var map = DomainTestFactory.CreateMap(
                width: 8,
                height: 8,
                floorTiles: Enumerable.Range(1, 3)
                    .SelectMany(x => Enumerable.Range(1, 3).Select(y => new Position(x, y))),
                rooms: new[] { room });

            var sleepingEnemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 2));
            sleepingEnemy.AddStatus(StatusEffectType.Sleep, 99);

            var session = DomainTestFactory.CreateRunSession(map: map, player: DomainTestFactory.CreateActor(position: new Position(1, 1)));
            session.AddEnemy(sleepingEnemy);
            session.SetRoomAssignments(new[] { new RoomAssignment(room, RoomRole.MonsterHouse) });

            var first = session.TryTriggerMonsterHouse(room, out var awakened);
            var second = session.TryTriggerMonsterHouse(room, out var awakenedSecond);

            Assert.True(first);
            Assert.Equal(1, awakened);
            Assert.False(sleepingEnemy.HasStatus(StatusEffectType.Sleep));
            Assert.False(second);
            Assert.Equal(0, awakenedSecond);
            Assert.True(session.IsMonsterHouseTriggered(room));
        }

        // 観点: AddEnemy_Throws_WhenFactionIsNotEnemy の期待挙動を検証する。
        [Fact]
        public void AddEnemy_Throws_WhenFactionIsNotEnemy()
        {
            var map = DomainTestFactory.CreateMap();
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(1, 1));
            var session = new RunSession(seed: 1234, floor: 1, map, player, clearFloor: 10);
            var ally = DomainTestFactory.CreateActor(name: "Ally", faction: Faction.Player, position: new Position(2, 1));

            Assert.Throws<ArgumentException>(() => session.AddEnemy(ally));
        }

        // 観点: SetRoomAssignments_CopiesInputArray の期待挙動を検証する。
        [Fact]
        public void SetRoomAssignments_CopiesInputArray()
        {
            var session = CreateInRunSession();
            var room = new MapRect(1, 1, 2, 2);
            var assignments = new[] { new RoomAssignment(room, RoomRole.Normal) };

            session.SetRoomAssignments(assignments);
            assignments[0] = new RoomAssignment(room, RoomRole.MonsterHouse);

            Assert.Single(session.RoomAssignments);
            Assert.Equal(RoomRole.Normal, session.RoomAssignments[0].Role);
        }

        // 観点: Enemies_ReturnsReadOnlyCollection の期待挙動を検証する。
        [Fact]
        public void Enemies_ReturnsReadOnlyCollection()
        {
            var session = CreateInRunSession();
            session.AddEnemy(DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 1)));
            var enemies = Assert.IsAssignableFrom<IList<Actor>>(session.Enemies);

            Assert.Throws<NotSupportedException>(() => enemies.Add(DomainTestFactory.CreateActor(name: "Other", faction: Faction.Enemy, position: new Position(3, 1))));
        }

        // 観点: Items_ReturnsReadOnlyCollection の期待挙動を検証する。
        [Fact]
        public void Items_ReturnsReadOnlyCollection()
        {
            var session = CreateInRunSession();
            session.AddItem(MapItem.Create(ItemId.FoodRation, new Position(2, 1)));
            var items = Assert.IsAssignableFrom<IList<MapItem>>(session.Items);

            Assert.Throws<NotSupportedException>(() => items.Add(MapItem.Create(ItemId.HealingPotion, new Position(3, 1))));
        }

        // 観点: TrySetActorPosition_ReturnsFalse_WhenActorIsNotManaged の期待挙動を検証する。
        [Fact]
        public void TrySetActorPosition_ReturnsFalse_WhenActorIsNotManaged()
        {
            var session = CreateInRunSession();
            var outsider = DomainTestFactory.CreateActor(name: "Outsider", faction: Faction.Player, position: new Position(1, 1));

            var result = session.TrySetActorPosition(outsider, new Position(2, 1), out var blocker);

            Assert.False(result);
            Assert.Null(blocker);
        }

        // 観点: TrySetActorPosition_ReturnsFalse_WhenTargetIsOccupied の期待挙動を検証する。
        [Fact]
        public void TrySetActorPosition_ReturnsFalse_WhenTargetIsOccupied()
        {
            var session = CreateInRunSession();
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 1));
            session.AddEnemy(enemy);

            var result = session.TrySetActorPosition(session.Player, new Position(2, 1), out var blocker);

            Assert.False(result);
            Assert.Same(enemy, blocker);
            Assert.Equal(new Position(1, 1), session.Player.Position);
        }

        // 観点: TrySetActorPosition_UpdatesActor_WhenTargetIsValid の期待挙動を検証する。
        [Fact]
        public void TrySetActorPosition_UpdatesActor_WhenTargetIsValid()
        {
            var session = CreateInRunSession();

            var result = session.TrySetActorPosition(session.Player, new Position(2, 1), out var blocker);

            Assert.True(result);
            Assert.Null(blocker);
            Assert.Equal(new Position(2, 1), session.Player.Position);
        }

        // 観点: TrySetActorFacing_ReturnsFalse_WhenActorIsNotManaged の期待挙動を検証する。
        [Fact]
        public void TrySetActorFacing_ReturnsFalse_WhenActorIsNotManaged()
        {
            var session = CreateInRunSession();
            var outsider = DomainTestFactory.CreateActor(name: "Outsider", faction: Faction.Player, position: new Position(1, 1));

            var result = session.TrySetActorFacing(outsider, Direction.Left);

            Assert.False(result);
        }

        // 観点: TrySetActorFacing_UpdatesActor_WhenManaged の期待挙動を検証する。
        [Fact]
        public void TrySetActorFacing_UpdatesActor_WhenManaged()
        {
            var session = CreateInRunSession();

            var result = session.TrySetActorFacing(session.Player, Direction.Left);

            Assert.True(result);
            Assert.Equal(Direction.Left, session.Player.Facing);
        }
        private static RunSession CreateInRunSession()
        {
            var map = DomainTestFactory.CreateMap();
            var player = DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(1, 1));
            var session = new RunSession(seed: 1234, floor: 1, map, player, clearFloor: 10);
            session.StartRun();
            return session;
        }
    }
}




