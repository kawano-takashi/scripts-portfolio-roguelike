using System;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Xunit;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Tests.Domain.Gameplay.Actors.Entities
{
    /// <summary>
    /// Actor の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class ActorTests
    {
        // 観点: Constructor_Throws_WhenNameIsEmpty の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenNameIsEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Actor(
                ActorId.NewId(),
                string.Empty,
                Faction.Player,
                Position.Zero,
                new ActorStats(10, 2, 1, 3, 5, 100f)));
        }

        // 観点: ApplyDamage_ClampsCurrentHpToZero の期待挙動を検証する。
        [Fact]
        public void ApplyDamage_ClampsCurrentHpToZero()
        {
            var sut = CreateActor();

            var actual = sut.ApplyDamage(999);

            Assert.True(actual >= 0);
            Assert.Equal(0, sut.CurrentHp);
            Assert.True(sut.IsDead);
        }

        // 観点: Heal_DoesNotExceedEffectiveMaxHp の期待挙動を検証する。
        [Fact]
        public void Heal_DoesNotExceedEffectiveMaxHp()
        {
            var sut = CreateActor();
            sut.ApplyDamage(5);

            var healed = sut.Heal(999);

            Assert.Equal(5, healed);
            Assert.Equal(sut.GetEffectiveMaxHp(), sut.CurrentHp);
        }

        // 観点: SpendAndRestoreHunger_ClampWithinRange の期待挙動を検証する。
        [Fact]
        public void SpendAndRestoreHunger_ClampWithinRange()
        {
            var sut = CreateActor();

            var spent = sut.SpendHunger(999f);
            var restored = sut.RestoreHunger(999f);

            Assert.Equal(100f, spent);
            Assert.Equal(100f, restored);
            Assert.Equal(100f, sut.CurrentHunger);
        }

        // 観点: NaturalHpRecoveryAccumulator_DrainsIntegerPartOnly の期待挙動を検証する。
        [Fact]
        public void NaturalHpRecoveryAccumulator_DrainsIntegerPartOnly()
        {
            var sut = CreateActor();
            sut.ApplyDamage(3);

            sut.AddNaturalHpRecoveryAccumulator(0.6f);
            var firstDrain = sut.DrainNaturalHpRecoveryAccumulatorAsHp();

            sut.AddNaturalHpRecoveryAccumulator(0.6f);
            var secondDrain = sut.DrainNaturalHpRecoveryAccumulatorAsHp();

            Assert.Equal(0, firstDrain);
            Assert.Equal(1, secondDrain);
        }

        // 観点: GainExperience_LevelsUp_AndFullyRecoversHp の期待挙動を検証する。
        [Fact]
        public void GainExperience_LevelsUp_AndFullyRecoversHp()
        {
            var sut = CreateActor(growth: StatGrowth.PlayerDefault);
            sut.ApplyDamage(5);

            var required = LevelProgress.CalculateExpRequired(1);
            var result = sut.GainExperience(required + 10);

            Assert.True(result.leveledUp);
            Assert.True(result.newLevel >= 2);
            Assert.Equal(sut.GetEffectiveMaxHp(), sut.CurrentHp);
        }

        // 観点: AddStatus_KeepsLongerDuration_WhenCalledMultipleTimes の期待挙動を検証する。
        [Fact]
        public void AddStatus_KeepsLongerDuration_WhenCalledMultipleTimes()
        {
            var sut = CreateActor();

            sut.AddStatus(StatusEffectType.Sleep, 2);
            sut.AddStatus(StatusEffectType.Sleep, 1);

            Assert.Equal(2, sut.GetStatusTurns(StatusEffectType.Sleep));
        }

        // 観点: TickStatusEffects_RemovesExpiredStatuses の期待挙動を検証する。
        [Fact]
        public void TickStatusEffects_RemovesExpiredStatuses()
        {
            var sut = CreateActor();
            sut.AddStatus(StatusEffectType.Sleep, 1);
            sut.AddStatus(StatusEffectType.Silence, 2);

            var expired = sut.TickStatusEffects();

            Assert.Contains(StatusEffectType.Sleep, expired);
            Assert.False(sut.HasStatus(StatusEffectType.Sleep));
            Assert.True(sut.HasStatus(StatusEffectType.Silence));
        }

        // 観点: GetEffectiveStats_IncludesGrowthAndEquipmentModifier の期待挙動を検証する。
        [Fact]
        public void GetEffectiveStats_IncludesGrowthAndEquipmentModifier()
        {
            var progress = new LevelProgress(level: 3, currentExp: 0, expToNextLevel: LevelProgress.CalculateExpRequired(3));
            var sut = CreateActor(growth: new StatGrowth(2, 1, 1), levelProgress: progress);
            var armor = new InventoryItem(ItemInstanceId.NewId(), ItemId.Armor);
            sut.AddToInventory(armor);
            sut.Equipment.TryEquip(armor, out _, out _);

            var stats = sut.GetEffectiveStats();

            Assert.Equal(24, stats.MaxHp); // base 20 + growth(2 * (3-1))
            Assert.Equal(5, stats.Attack); // base 3 + growth 2
            Assert.Equal(5, stats.Defense); // base 1 + growth 2 + armor 2
        }

        private static Actor CreateActor(
            StatGrowth? growth = null,
            LevelProgress? levelProgress = null)
        {
            return new Actor(
                ActorId.NewId(),
                "Player",
                Faction.Player,
                Position.Zero,
                new ActorStats(maxHp: 20, attack: 3, defense: 1, intelligence: 12, sightRadius: 8, maxHunger: 100f),
                growth: growth,
                levelProgress: levelProgress);
        }
    }
}

