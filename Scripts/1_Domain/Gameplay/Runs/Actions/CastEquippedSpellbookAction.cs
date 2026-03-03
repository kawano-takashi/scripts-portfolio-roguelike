using Roguelike.Domain.Gameplay.Actors.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Actions
{
    /// <summary>
    /// 装備中の魔法書を詠唱するアクションです。
    /// </summary>
    public sealed class CastEquippedSpellbookAction : RoguelikeAction
    {
        // 呪文内容/射程は装備中の魔法書から解決するため追加パラメータは不要。
        public CastEquippedSpellbookAction(ActorId actorId) : base(actorId)
        {
        }
    }
}


