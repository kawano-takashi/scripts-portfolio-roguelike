using System;
using UnityEngine;

namespace Roguelike.Presentation.Gameplay.Map.Contracts
{
    /// <summary>
    /// アクターIDから表示用ビューのTransform/SpriteRendererを引く契約です。
    /// </summary>
    public interface IGameplayActorViewLocator
    {
        bool TryResolve(Guid actorId, out Transform actorTransform, out SpriteRenderer spriteRenderer);
    }
}

