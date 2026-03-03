using System;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.Map.Views.Actor;
using UnityEngine;

namespace Roguelike.Presentation.Gameplay.Map.Services
{
    /// <summary>
    /// アクターIDから描画中のビューを解決します。
    /// </summary>
    public sealed class GameplayActorViewLocator : IGameplayActorViewLocator
    {
        private readonly GetPlayerIdQueryHandler _getPlayerIdQueryHandler;
        private readonly Player2DView _player2DView;
        private readonly Enemy2DViewManager _enemy2DViewManager;

        public GameplayActorViewLocator(
            GetPlayerIdQueryHandler getPlayerIdQueryUseCase,
            Player2DView player2DView,
            Enemy2DViewManager enemy2DViewManager)
        {
            _getPlayerIdQueryHandler = getPlayerIdQueryUseCase;
            _player2DView = player2DView;
            _enemy2DViewManager = enemy2DViewManager;
        }

        public bool TryResolve(Guid actorId, out Transform actorTransform, out SpriteRenderer spriteRenderer)
        {
            actorTransform = null;
            spriteRenderer = null;

            var playerIdResult = _getPlayerIdQueryHandler?.Handle(new GetPlayerIdQuery());
            if (playerIdResult.HasValue &&
                playerIdResult.Value.IsSuccess &&
                playerIdResult.Value.Value == actorId &&
                _player2DView != null)
            {
                actorTransform = _player2DView.transform;
                spriteRenderer = _player2DView.SpriteRenderer;
                return actorTransform != null;
            }

            if (_enemy2DViewManager != null &&
                _enemy2DViewManager.TryGetView(actorId, out var enemyView) &&
                enemyView != null)
            {
                actorTransform = enemyView.transform;
                spriteRenderer = enemyView.SpriteRenderer;
                return actorTransform != null;
            }

            return false;
        }
    }
}


