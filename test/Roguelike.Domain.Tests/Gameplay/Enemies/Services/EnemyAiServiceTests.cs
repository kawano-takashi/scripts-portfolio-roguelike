using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// EnemyAiService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class EnemyAiServiceTests
    {
        // 観点: Decide_ReturnsWait_WhenSessionIsNull の期待挙動を検証する。
        [Fact]
        public void Decide_ReturnsWait_WhenSessionIsNull()
        {
            var sut = new EnemyAiService();
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, enemyArchetype: EnemyArchetype.Melee);

            var action = sut.Decide(enemy, session: null);

            Assert.IsType<WaitAction>(action);
        }

        // 観点: Decide_ReturnsWait_WhenRunPhaseIsNotInRun の期待挙動を検証する。
        [Fact]
        public void Decide_ReturnsWait_WhenRunPhaseIsNotInRun()
        {
            var session = DomainTestFactory.CreateRunSession(phase: RunPhase.Pause);
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, enemyArchetype: EnemyArchetype.Melee, position: new Position(2, 1));
            session.AddEnemy(enemy);
            var sut = new EnemyAiService();

            var action = sut.Decide(enemy, session);

            Assert.IsType<WaitAction>(action);
        }

        // 観点: Decide_ReturnsAttackAction_WhenMeleeEnemyIsAdjacentToPlayer の期待挙動を検証する。
        [Fact]
        public void Decide_ReturnsAttackAction_WhenMeleeEnemyIsAdjacentToPlayer()
        {
            var session = DomainTestFactory.CreateRunSession(
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 2)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, enemyArchetype: EnemyArchetype.Melee, position: new Position(3, 2));
            session.AddEnemy(enemy);
            var sut = new EnemyAiService();

            var action = sut.Decide(enemy, session);

            var attack = Assert.IsType<AttackAction>(action);
            Assert.Equal(enemy.Id, attack.ActorId);
            Assert.Equal(session.Player.Id, attack.TargetId);
        }

        // 観点: Decide_ReturnsWait_WhenEnemyIsDead の期待挙動を検証する。
        [Fact]
        public void Decide_ReturnsWait_WhenEnemyIsDead()
        {
            var session = DomainTestFactory.CreateRunSession();
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, enemyArchetype: EnemyArchetype.Melee, position: new Position(3, 3));
            enemy.ApplyDamage(999);
            var sut = new EnemyAiService();

            var action = sut.Decide(enemy, session);

            Assert.IsType<WaitAction>(action);
        }

        // 観点: GetActionCount_ReturnsOne_ForDefaultEnemyProfiles の期待挙動を検証する。
        [Fact]
        public void GetActionCount_ReturnsOne_ForDefaultEnemyProfiles()
        {
            var sut = new EnemyAiService();
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, enemyArchetype: EnemyArchetype.Ranged);

            var count = sut.GetActionCount(enemy, turnNumber: 10);

            Assert.Equal(1, count);
        }
    }
}
