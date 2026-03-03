using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;

namespace Roguelike.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// EnemyArchetypeごとの定義を一元管理します。
    /// スポーン時の基礎能力とAIプロファイルを同じソースで共有します。
    /// </summary>
    public static class EnemyDefinitionCatalog
    {
        private static readonly IReadOnlyDictionary<EnemyArchetype, EnemyArchetypeDefinition> Definitions =
            new Dictionary<EnemyArchetype, EnemyArchetypeDefinition>
            {
                {
                    EnemyArchetype.Melee,
                    new EnemyArchetypeDefinition(
                        archetype: EnemyArchetype.Melee,
                        displayName: "近接",
                        stats: new ActorStats(maxHp: 8, attack: 3, defense: 1, intelligence: 0, sightRadius: 6, maxHunger: 0),
                        profile: new EnemyProfile(
                            id: "melee",
                            displayName: "default",
                            speed: SpeedType.Normal,
                            sightRadius: 6,
                            attackRange: 1,
                            preferredDistance: 1,
                            intelligence: IntelligenceLevel.Medium,
                            specialAbilities: Array.Empty<SpecialAbility>(),
                            fleeHpThresholdPercent: 0,
                            forgetTurns: 10,
                            wakeDistance: 5,
                            initialState: AiState.Wandering,
                            baseHp: 8,
                            baseAttack: 3,
                            baseDefense: 1))
                },
                {
                    EnemyArchetype.Ranged,
                    new EnemyArchetypeDefinition(
                        archetype: EnemyArchetype.Ranged,
                        displayName: "遠距離",
                        stats: new ActorStats(maxHp: 6, attack: 2, defense: 0, intelligence: 0, sightRadius: 8, maxHunger: 0),
                        profile: new EnemyProfile(
                            id: "ranged",
                            displayName: "default",
                            speed: SpeedType.Normal,
                            sightRadius: 8,
                            attackRange: 6,
                            preferredDistance: 3,
                            intelligence: IntelligenceLevel.Medium,
                            specialAbilities: new[] { SpecialAbility.RangedAttack },
                            fleeHpThresholdPercent: 30,
                            forgetTurns: 8,
                            wakeDistance: 6,
                            initialState: AiState.Wandering,
                            baseHp: 6,
                            baseAttack: 2,
                            baseDefense: 0))
                },
                {
                    EnemyArchetype.Disruptor,
                    new EnemyArchetypeDefinition(
                        archetype: EnemyArchetype.Disruptor,
                        displayName: "妨害",
                        stats: new ActorStats(maxHp: 7, attack: 2, defense: 1, intelligence: 0, sightRadius: 7, maxHunger: 0),
                        profile: new EnemyProfile(
                            id: "disruptor",
                            displayName: "default",
                            speed: SpeedType.Normal,
                            sightRadius: 7,
                            attackRange: 5,
                            preferredDistance: 2,
                            intelligence: IntelligenceLevel.Medium,
                            specialAbilities: new[] { SpecialAbility.InflictSilence },
                            fleeHpThresholdPercent: 20,
                            forgetTurns: 8,
                            wakeDistance: 5,
                            initialState: AiState.Wandering,
                            baseHp: 7,
                            baseAttack: 2,
                            baseDefense: 1))
                },
            };

        public static EnemyArchetypeDefinition Get(EnemyArchetype archetype)
        {
            if (TryGet(archetype, out var definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Unknown enemy archetype definition: {archetype}");
        }

        public static bool TryGet(EnemyArchetype archetype, out EnemyArchetypeDefinition definition)
        {
            return Definitions.TryGetValue(archetype, out definition);
        }

        public static EnemyProfile GetProfile(EnemyArchetype archetype)
        {
            return Get(archetype).Profile;
        }

        public static ActorStats GetStats(EnemyArchetype archetype)
        {
            return Get(archetype).Stats;
        }

        public static string GetDisplayName(EnemyArchetype archetype)
        {
            return Get(archetype).DisplayName;
        }
    }

    public readonly struct EnemyArchetypeDefinition
    {
        public EnemyArchetype Archetype { get; }
        public string DisplayName { get; }
        public ActorStats Stats { get; }
        public EnemyProfile Profile { get; }

        public EnemyArchetypeDefinition(
            EnemyArchetype archetype,
            string displayName,
            ActorStats stats,
            EnemyProfile profile)
        {
            Archetype = archetype;
            DisplayName = displayName;
            Stats = stats;
            Profile = profile;
        }
    }
}

