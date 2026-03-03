using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class TurnEndPhase
    {
        public void Execute(RunSession session, List<IRoguelikeEvent> events)
        {
            TickStatusEffects(session, events);
            UpdateVisibility(session);

            session.AdvanceTurn();
            events.Add(new TurnEndedEvent(session.TurnCount));
        }

        private static void TickStatusEffects(RunSession session, List<IRoguelikeEvent> events)
        {
            if (session == null || events == null)
            {
                return;
            }

            TickActorStatusEffects(session.Player, events, isPlayer: true);
            for (var i = 0; i < session.Enemies.Count; i++)
            {
                TickActorStatusEffects(session.Enemies[i], events, isPlayer: false);
            }
        }

        private static void TickActorStatusEffects(Actor actor, List<IRoguelikeEvent> events, bool isPlayer)
        {
            if (actor == null || actor.IsDead)
            {
                return;
            }

            var expired = actor.TickStatusEffects();
            if (expired == null || expired.Count == 0 || !isPlayer)
            {
                return;
            }

            for (var i = 0; i < expired.Count; i++)
            {
                if (expired[i] != StatusEffectType.Sleep)
                {
                    continue;
                }

                events.Add(new LogEvent(RunLogCode.WakeUp));
                return;
            }
        }

        private static void UpdateVisibility(RunSession session)
        {
            if (session?.Map == null || session.Player == null)
            {
                return;
            }

            session.Map.ApplyVisibilityByRoomOrRadius(session.Player.Position, 1);
        }
    }
}
