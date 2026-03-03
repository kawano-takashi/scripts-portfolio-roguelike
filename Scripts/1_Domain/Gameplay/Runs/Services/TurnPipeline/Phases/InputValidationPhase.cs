using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;
using System.Collections.Generic;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class InputValidationPhase
    {
        public bool TryExecute(RunSession session, RoguelikeAction playerAction, List<IRoguelikeEvent> events)
        {
            if (session.Phase != RunPhase.InRun)
            {
                events?.Add(new LogEvent(RunLogCode.RunIsNotActive));
                return false;
            }

            if (session.Player == null || playerAction == null || playerAction.ActorId != session.Player.Id)
            {
                events?.Add(new LogEvent(RunLogCode.InvalidPlayerAction));
                return false;
            }

            return true;
        }
    }
}
