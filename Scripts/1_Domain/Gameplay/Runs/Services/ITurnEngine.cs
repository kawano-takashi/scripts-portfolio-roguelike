using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    public interface ITurnEngine
    {
        TurnResolution Resolve(RunSession session, RoguelikeAction playerAction);
    }
}
