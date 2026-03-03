using System;
using System.Linq;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Services;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// PlayerInitializationService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class PlayerInitializationServiceTests
    {
        // 観点: CreateInitialPlayer_UsesFallbackName_WhenNameIsBlank の期待挙動を検証する。
        [Fact]
        public void CreateInitialPlayer_UsesFallbackName_WhenNameIsBlank()
        {
            var sut = new PlayerInitializationService();

            var player = sut.CreateInitialPlayer(
                playerName: " ",
                maxHp: 10,
                attack: 2,
                defense: 1,
                intelligence: 3,
                sightRadius: 5,
                maxHunger: 50f,
                startPosition: new Position(2, 3));

            Assert.Equal("魔術師", player.Name);
            Assert.Equal(new Position(2, 3), player.Position);
        }

        // 観点: CreateInitialPlayer_ClampsInvalidStatsToMinimum の期待挙動を検証する。
        [Fact]
        public void CreateInitialPlayer_ClampsInvalidStatsToMinimum()
        {
            var sut = new PlayerInitializationService();

            var player = sut.CreateInitialPlayer(
                playerName: "tester",
                maxHp: -1,
                attack: -2,
                defense: -3,
                intelligence: -4,
                sightRadius: -5,
                maxHunger: -6f,
                startPosition: Position.Zero);

            Assert.Equal(1, player.Stats.MaxHp);
            Assert.Equal(0, player.Stats.Attack);
            Assert.Equal(0, player.Stats.Defense);
            Assert.Equal(0, player.Stats.Intelligence);
            Assert.Equal(1, player.Stats.SightRadius);
            Assert.Equal(1f, player.Stats.MaxHunger);
        }

        // 観点: CreateInitialPlayer_AddsAndEquipsInitialSpellbook の期待挙動を検証する。
        [Fact]
        public void CreateInitialPlayer_AddsAndEquipsInitialSpellbook()
        {
            var sut = new PlayerInitializationService();

            var player = sut.CreateInitialPlayer(
                playerName: "tester",
                maxHp: 20,
                attack: 3,
                defense: 1,
                intelligence: 14,
                sightRadius: 8,
                maxHunger: 100f,
                startPosition: Position.Zero);

            var spellbook = player.Inventory.Items.Single(item => item.ItemType == ItemId.SpellbookMagicFire);

            Assert.True(player.Equipment.SpellbookItemId.HasValue);
            Assert.Equal(spellbook.Id, player.Equipment.SpellbookItemId.Value);
        }

        // 観点: PreparePlayerForNextFloor_ResetsPositionAndFacing の期待挙動を検証する。
        [Fact]
        public void PreparePlayerForNextFloor_ResetsPositionAndFacing()
        {
            var sut = new PlayerInitializationService();
            var player = CreatePlayer();
            var session = CreateSession(player);
            session.TrySetActorFacing(player, Direction.UpLeft);
            session.TrySetActorPosition(player, new Position(5, 5), out _);

            sut.PreparePlayerForNextFloor(session, new Position(1, 2));

            Assert.Equal(new Position(1, 2), player.Position);
            Assert.Equal(Direction.Down, player.Facing);
        }

        // 観点: PreparePlayerForNextFloor_Throws_WhenSessionIsNull の期待挙動を検証する。
        [Fact]
        public void PreparePlayerForNextFloor_Throws_WhenSessionIsNull()
        {
            var sut = new PlayerInitializationService();

            Assert.Throws<ArgumentNullException>(() => sut.PreparePlayerForNextFloor(null, Position.Zero));
        }

        private static Actor CreatePlayer()
        {
            return new Actor(
                ActorId.NewId(),
                "player",
                Faction.Player,
                Position.Zero,
                new ActorStats(20, 3, 1, 10, 8, 100f));
        }

        private static RunSession CreateSession(Actor player)
        {
            var map = new Map(8, 8);
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    map.SetTileType(new Position(x, y), TileType.Floor);
                }
            }

            map.SetStartPosition(Position.Zero);

            var session = new RunSession(1234, 1, map, player, clearFloor: 10);
            session.StartRun();
            return session;
        }
    }
}
