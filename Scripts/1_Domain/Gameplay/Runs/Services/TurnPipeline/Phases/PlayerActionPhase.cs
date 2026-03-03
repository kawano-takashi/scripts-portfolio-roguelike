using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class PlayerActionPhase
    {
        private readonly ActorActionResolver _actionResolver;

        public PlayerActionPhase(ActorActionResolver actionResolver)
        {
            _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        }

        public ActionResolution Execute(
            RunSession session,
            RoguelikeAction playerAction,
            List<IRoguelikeEvent> events)
        {
            return _actionResolver.Execute(
                session,
                session?.Player,
                playerAction,
                events,
                ActorExecutionRole.Player);
        }
    }
}
