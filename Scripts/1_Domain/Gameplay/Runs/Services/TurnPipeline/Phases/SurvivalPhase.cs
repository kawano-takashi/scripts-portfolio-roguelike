using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class SurvivalPhase
    {
        private const float HungerDecayPerTurn = 0.1f;
        private const int StarvationDamagePerTurn = 1;
        private const int NaturalHpRecoveryConstant = 150;

        public void Execute(RunSession session, List<IRoguelikeEvent> events)
        {
            ApplyNaturalRegeneration(session, events);
            ApplyHungerDecay(session, events);
        }

        private static void ApplyNaturalRegeneration(RunSession session, List<IRoguelikeEvent> events)
        {
            var actor = session?.Player;
            if (actor == null || actor.IsDead)
            {
                return;
            }

            if (actor.CurrentHunger <= 0)
            {
                return;
            }

            var effectiveMaxHp = actor.GetEffectiveMaxHp();
            if (actor.CurrentHp >= effectiveMaxHp)
            {
                actor.ResetNaturalHpRecoveryAccumulator();
                return;
            }

            var recoveryPerTurn = (float)effectiveMaxHp / NaturalHpRecoveryConstant;
            actor.AddNaturalHpRecoveryAccumulator(recoveryPerTurn);

            var hpToRecover = actor.DrainNaturalHpRecoveryAccumulatorAsHp();
            if (hpToRecover <= 0 || events == null)
            {
                return;
            }

            var healed = actor.Heal(hpToRecover);
            if (healed > 0)
            {
                events.Add(new ActorHealedEvent(actor.Id, healed, actor.CurrentHp));
            }
        }

        private static void ApplyHungerDecay(RunSession session, List<IRoguelikeEvent> events)
        {
            var actor = session?.Player;
            if (actor == null || actor.IsDead || events == null)
            {
                return;
            }

            if (actor.CurrentHunger <= 0)
            {
                actor.ApplyDamage(StarvationDamagePerTurn);
                events.Add(new LogEvent(RunLogCode.Starving));

                if (actor.IsDead)
                {
                    session.ResolveDeath(actor, events);
                    return;
                }
            }

            var spent = actor.SpendHunger(HungerDecayPerTurn);
            if (spent > 0)
            {
                events.Add(new HungerChangedEvent(actor.Id, -spent, actor.CurrentHunger));
            }
        }
    }
}
