using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class DamageResolver
    {
        public void ApplyDamageAndHandleDeath(
            RunSession session,
            Actor attacker,
            Actor defender,
            int damage,
            List<IRoguelikeEvent> events)
        {
            if (session == null || attacker == null || defender == null || events == null)
            {
                return;
            }

            var actualDamage = defender.ApplyDamage(damage);
            events.Add(new ActorDamagedEvent(attacker.Id, defender.Id, actualDamage, defender.CurrentHp));

            if (!defender.IsDead)
            {
                return;
            }

            if (defender.Faction == Faction.Enemy && attacker.Faction == Faction.Player)
            {
                GrantExperience(session, attacker, defender, events);
            }

            session.ResolveDeath(defender, events);
        }

        private static void GrantExperience(RunSession session, Actor player, Actor defeatedEnemy, List<IRoguelikeEvent> events)
        {
            var expAmount = CalculateExperienceReward(defeatedEnemy, session.Floor);
            var oldLevel = player.LevelProgress.Level;
            var (leveledUp, newLevel) = player.GainExperience(expAmount);

            events.Add(new ExperienceGainedEvent(
                player.Id,
                expAmount,
                player.LevelProgress.CurrentExp,
                player.LevelProgress.ExpToNextLevel,
                defeatedEnemy.Id));

            if (!leveledUp)
            {
                return;
            }

            var newStats = player.GetEffectiveStats();
            events.Add(new LevelUpEvent(
                player.Id,
                oldLevel,
                newLevel,
                newStats.MaxHp,
                newStats.Attack,
                newStats.Defense));
            events.Add(new LogEvent(
                RunLogCode.LevelUp,
                new Dictionary<string, string>
                {
                    ["oldLevel"] = oldLevel.ToString(),
                    ["newLevel"] = newLevel.ToString(),
                }));
        }

        private static int CalculateExperienceReward(Actor enemy, int floor)
        {
            var baseExp = enemy.EnemyArchetype switch
            {
                EnemyArchetype.Melee => 5,
                EnemyArchetype.Ranged => 7,
                EnemyArchetype.Disruptor => 10,
                _ => 5,
            };

            var floorMultiplier = 1f + floor * 0.15f;
            return (int)(baseExp * floorMultiplier);
        }
    }
}
