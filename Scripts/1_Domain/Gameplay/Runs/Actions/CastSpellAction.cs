using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    /// <summary>
    /// 呪文を使う行動です。
    /// </summary>
    public sealed class CastSpellAction : RoguelikeAction
    {
        /// <summary>
        /// どの呪文を使うか。
        /// </summary>
        public ItemId Spell { get; }
        /// <summary>
        /// ねらう位置。必要ない呪文は null でOKです。
        /// </summary>
        public Position? TargetPosition { get; }

        /// <summary>
        /// 行動を作るときの入口です。
        /// </summary>
        public CastSpellAction(ActorId actorId, ItemId spell, Position? targetPosition = null) : base(actorId)
        {
            Spell = spell;
            TargetPosition = targetPosition;
        }
    }
}
