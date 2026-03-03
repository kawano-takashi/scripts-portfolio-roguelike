using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.ValueObjects
{
    /// <summary>
    /// EnemyProfile の仕様を検証するユニットテストです。
    /// </summary>
    public sealed class EnemyProfileTests
    {
        // 観点: Constructor_CopiesSpecialAbilitiesInput の期待挙動を検証する。
        [Fact]
        public void Constructor_CopiesSpecialAbilitiesInput()
        {
            var abilities = new List<SpecialAbility> { SpecialAbility.RangedAttack };
            var profile = new EnemyProfile(
                id: "enemy",
                displayName: "Enemy",
                speed: SpeedType.Normal,
                sightRadius: 6,
                attackRange: 1,
                preferredDistance: 1,
                intelligence: IntelligenceLevel.Medium,
                specialAbilities: abilities,
                fleeHpThresholdPercent: 0,
                forgetTurns: 5,
                wakeDistance: 3,
                initialState: AiState.Wandering,
                baseHp: 10,
                baseAttack: 2,
                baseDefense: 1);

            abilities[0] = SpecialAbility.InflictSilence;
            abilities.Add(SpecialAbility.Divide);

            Assert.Single(profile.SpecialAbilities);
            Assert.True(profile.HasAbility(SpecialAbility.RangedAttack));
            Assert.False(profile.HasAbility(SpecialAbility.InflictSilence));
            Assert.False(profile.HasAbility(SpecialAbility.Divide));
        }
    }
}
