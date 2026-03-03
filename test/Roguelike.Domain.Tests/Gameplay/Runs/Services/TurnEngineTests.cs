using System;
using System.Linq;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// TurnEngine の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class TurnEngineTests
    {
        [Fact]
        public void Constructor_Throws_WhenEnemyDecisionPolicyIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnEngine(null, new FieldOfViewService()));
        }

        [Fact]
        public void Constructor_Throws_WhenFieldOfViewServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnEngine(new NullEnemyDecisionPolicy(), null));
        }

        // 観点: Resolve_Throws_WhenSessionIsNull の期待挙動を検証する。
        [Fact]
        public void Resolve_Throws_WhenSessionIsNull()
        {
            var sut = CreateSut();

            Assert.Throws<ArgumentNullException>(() => sut.Resolve(null, new WaitAction(ActorId.NewId())));
        }

        // 観点: Resolve_ResolvesWaitAction_AndConsumesTurn の期待挙動を検証する。
        [Fact]
        public void Resolve_ResolvesWaitAction_AndConsumesTurn()
        {
            var session = DomainTestFactory.CreateRunSession();
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new WaitAction(session.Player.Id));

            Assert.True(resolution.ActionResolved);
            Assert.True(resolution.TurnConsumed);
            Assert.Equal(1, resolution.TurnNumber);
            Assert.Contains(resolution.Events, evt => evt is TurnEndedEvent);
        }

        // 観点: Resolve_ReturnsInvalidPlayerActionLog_WhenActorIdMismatch の期待挙動を検証する。
        [Fact]
        public void Resolve_ReturnsInvalidPlayerActionLog_WhenActorIdMismatch()
        {
            var session = DomainTestFactory.CreateRunSession();
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new WaitAction(ActorId.NewId()));

            Assert.False(resolution.ActionResolved);
            Assert.False(resolution.TurnConsumed);
            var log = Assert.IsType<LogEvent>(Assert.Single(resolution.Events));
            Assert.Equal(RunLogCode.InvalidPlayerAction, log.Code);
        }

        // 観点: Resolve_ReturnsRunIsNotActiveLog_WhenPhaseIsNotInRun の期待挙動を検証する。
        [Fact]
        public void Resolve_ReturnsRunIsNotActiveLog_WhenPhaseIsNotInRun()
        {
            var session = DomainTestFactory.CreateRunSession(phase: RunPhase.RunStart);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new WaitAction(session.Player.Id));

            Assert.False(resolution.TurnConsumed);
            var log = Assert.IsType<LogEvent>(Assert.Single(resolution.Events));
            Assert.Equal(RunLogCode.RunIsNotActive, log.Code);
        }

        // 観点: Resolve_MoveIntoWall_ProducesFailedMoveEvent_AndConsumesTurn の期待挙動を検証する。
        [Fact]
        public void Resolve_MoveIntoWall_ProducesFailedMoveEvent_AndConsumesTurn()
        {
            var map = DomainTestFactory.CreateMap(
                width: 5,
                height: 5,
                floorTiles: new[] { new Position(1, 1) });
            var player = DomainTestFactory.CreateActor(position: new Position(1, 1));
            var session = DomainTestFactory.CreateRunSession(map: map, player: player);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new MoveAction(player.Id, Direction.Right));

            var moved = Assert.IsType<ActorMovedEvent>(resolution.Events.First(evt => evt is ActorMovedEvent));
            Assert.False(moved.Success);
            Assert.True(resolution.TurnConsumed);
            Assert.Equal(1, resolution.TurnNumber);
            Assert.True(resolution.PlayerMoveOutcome.HasValue);
            Assert.False(resolution.PlayerMoveOutcome.Success);
            Assert.Equal(player.Position, resolution.PlayerMoveOutcome.From);
            Assert.Equal(player.Position, resolution.PlayerMoveOutcome.To);
        }

        // 観点: Resolve_ToggleEquip_DoesNotConsumeTurn の期待挙動を検証する。
        [Fact]
        public void Resolve_ToggleEquip_DoesNotConsumeTurn()
        {
            var session = DomainTestFactory.CreateRunSession();
            var armor = DomainTestFactory.CreateInventoryItem(ItemId.Armor);
            session.Player.AddToInventory(armor);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new ToggleEquipItemAction(session.Player.Id, armor.Id));

            Assert.True(resolution.ActionResolved);
            Assert.False(resolution.TurnConsumed);
            Assert.DoesNotContain(resolution.Events, evt => evt is TurnEndedEvent);
        }

        // 観点: Resolve_PlayerAsleep_EmitsAsleepLog の期待挙動を検証する。
        [Fact]
        public void Resolve_PlayerAsleep_EmitsAsleepLog()
        {
            var session = DomainTestFactory.CreateRunSession();
            session.Player.AddStatus(StatusEffectType.Sleep, turns: 2);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new WaitAction(session.Player.Id));

            Assert.True(resolution.ActionResolved);
            Assert.Contains(resolution.Events, evt => evt is LogEvent log && log.Code == RunLogCode.ActorAsleep);
        }

        [Fact]
        public void Resolve_UnknownAction_ReturnsUnresolved_AndDoesNotConsumeTurn()
        {
            var session = DomainTestFactory.CreateRunSession();
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new UnknownAction(session.Player.Id));

            Assert.False(resolution.ActionResolved);
            Assert.False(resolution.TurnConsumed);
            Assert.Empty(resolution.Events);
            Assert.Equal(0, session.TurnCount);
        }

        [Fact]
        public void Resolve_SleepingEnemy_DoesNotEmitActorAsleepLog()
        {
            var enemy = DomainTestFactory.CreateActor(
                name: "Sleeping Enemy",
                faction: Faction.Enemy,
                position: new Position(2, 1));
            enemy.AddStatus(StatusEffectType.Sleep, turns: 2);

            var session = DomainTestFactory.CreateRunSession(
                player: DomainTestFactory.CreateActor(position: new Position(1, 1)),
                enemies: new[] { enemy });
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new WaitAction(session.Player.Id));

            Assert.True(resolution.ActionResolved);
            Assert.DoesNotContain(resolution.Events, evt => evt is LogEvent log && log.Code == RunLogCode.ActorAsleep);
        }

        [Fact]
        public void Resolve_Search_WhenTooHungry_EmitsTooHungryToSearchLog()
        {
            var session = DomainTestFactory.CreateRunSession();
            session.Player.SpendHunger(session.Player.CurrentHunger);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new SearchAction(session.Player.Id));

            Assert.True(resolution.ActionResolved);
            Assert.True(resolution.TurnConsumed);
            Assert.Contains(resolution.Events, evt => evt is LogEvent log && log.Code == RunLogCode.TooHungryToSearch);
        }

        [Fact]
        public void Resolve_UseFoodRation_RestoresHunger_AndEmitsItemUsedEvent()
        {
            var session = DomainTestFactory.CreateRunSession();
            session.Player.SpendHunger(40f);
            var food = DomainTestFactory.CreateInventoryItem(ItemId.FoodRation);
            session.Player.AddToInventory(food);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new UseItemAction(session.Player.Id, food.Id));

            Assert.True(resolution.ActionResolved);
            Assert.True(resolution.TurnConsumed);
            Assert.Contains(resolution.Events, evt => evt is HungerChangedEvent hunger && hunger.ActorId == session.Player.Id && hunger.Delta > 0);
            Assert.Contains(resolution.Events, evt => evt is ItemUsedEvent used && used.ItemId == food.Id);
        }

        [Fact]
        public void Resolve_CastSpell_WhenSilenced_EmitsSilencedLog_AndDoesNotEmitSpellCastEvent()
        {
            var session = DomainTestFactory.CreateRunSession();
            session.Player.AddStatus(StatusEffectType.Silence, turns: 2);
            var sut = CreateSut();

            var resolution = sut.Resolve(session, new CastSpellAction(session.Player.Id, ItemId.SpellbookForceBolt));

            Assert.True(resolution.ActionResolved);
            Assert.True(resolution.TurnConsumed);
            Assert.Contains(resolution.Events, evt => evt is LogEvent log && log.Code == RunLogCode.Silenced);
            Assert.DoesNotContain(resolution.Events, evt => evt is SpellCastEvent);
        }

        [Fact]
        public void Resolve_EnemyPhase_StopsProcessing_WhenSessionBecomesGameOver()
        {
            var player = DomainTestFactory.CreateActor(
                name: "Player",
                faction: Faction.Player,
                position: new Position(1, 1),
                stats: new ActorStats(maxHp: 1, attack: 4, defense: 0, intelligence: 12, sightRadius: 8, maxHunger: 100f));
            var firstEnemy = DomainTestFactory.CreateActor(
                name: "First Enemy",
                faction: Faction.Enemy,
                position: new Position(1, 2));
            var secondEnemy = DomainTestFactory.CreateActor(
                name: "Second Enemy",
                faction: Faction.Enemy,
                position: new Position(2, 2));
            var policy = new AttackPlayerEnemyDecisionPolicy();
            var session = DomainTestFactory.CreateRunSession(
                player: player,
                enemies: new[] { firstEnemy, secondEnemy });
            var sut = CreateSut(policy);

            var resolution = sut.Resolve(session, new WaitAction(session.Player.Id));

            Assert.Equal(RunPhase.GameOver, session.Phase);
            var performed = Assert.IsType<AttackPerformedEvent>(Assert.Single(resolution.Events.OfType<AttackPerformedEvent>()));
            Assert.Equal(firstEnemy.Id, performed.AttackerId);
            Assert.Equal(session.Player.Id, performed.TargetId);
            Assert.DoesNotContain(
                resolution.Events,
                evt => evt is AttackPerformedEvent attacked && attacked.AttackerId == secondEnemy.Id);
            Assert.Contains(resolution.Events, evt => evt is ActorDiedEvent died && died.ActorId == session.Player.Id);
        }

        private static TurnEngine CreateSut(IEnemyDecisionPolicy enemyDecisionPolicy = null)
        {
            return new TurnEngine(enemyDecisionPolicy ?? new NullEnemyDecisionPolicy(), new FieldOfViewService());
        }

        private sealed class UnknownAction : RoguelikeAction
        {
            public UnknownAction(ActorId actorId) : base(actorId)
            {
            }
        }

        private sealed class AttackPlayerEnemyDecisionPolicy : IEnemyDecisionPolicy
        {
            public RoguelikeAction Decide(Actor enemy, RunSession session)
            {
                return new AttackAction(enemy.Id, session.Player.Id, AttackKind.Melee, range: 1);
            }

            public int GetActionCount(Actor enemy, int turnNumber)
            {
                return 1;
            }

            public void ResetMemory(int seed)
            {
            }
        }
    }
}
