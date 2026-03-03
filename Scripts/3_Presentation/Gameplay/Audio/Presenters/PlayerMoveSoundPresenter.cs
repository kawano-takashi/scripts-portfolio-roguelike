using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Hud.Types;

namespace Roguelike.Presentation.Gameplay.Audio.Presenters
{
    /// <summary>
    /// プレイヤー移動結果に応じたSE再生を担当します。
    /// </summary>
    public sealed class PlayerMoveSoundPresenter : IDisposable
    {
        private readonly RunTurnStateStore _runTurnStateStore;
        private readonly GetPlayerIdQueryHandler _getPlayerIdQueryHandler;
        private readonly IUiSoundPlayer _uiSoundPlayer;
        private readonly CompositeDisposable _disposables = new();

        public PlayerMoveSoundPresenter(
            RunTurnStateStore runTurnStateStore,
            GetPlayerIdQueryHandler getPlayerIdQueryHandler,
            IUiSoundPlayer uiSoundPlayer)
        {
            _runTurnStateStore = runTurnStateStore ?? throw new ArgumentNullException(nameof(runTurnStateStore));
            _getPlayerIdQueryHandler = getPlayerIdQueryHandler ?? throw new ArgumentNullException(nameof(getPlayerIdQueryHandler));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            _runTurnStateStore.LatestResolution
                .Subscribe(OnResolutionPublished)
                .AddTo(_disposables);
        }

        private void OnResolutionPublished(RunTurnResultDto resolution)
        {
            if (resolution.Events == null || resolution.Events.Count == 0)
            {
                return;
            }

            var playerIdResult = _getPlayerIdQueryHandler.Handle(new GetPlayerIdQuery());
            if (!playerIdResult.IsSuccess)
            {
                return;
            }

            var cue = TryResolveCue(resolution.Events, playerIdResult.Value);
            if (!cue.HasValue)
            {
                return;
            }

            _uiSoundPlayer.Play(cue.Value);
        }

        private UiSoundCue? TryResolveCue(IReadOnlyList<IRunEventDto> events, Guid playerId)
        {
            for (var i = 0; i < events.Count; i++)
            {
                if (!(events[i] is ActorMovedEventDto movedEvent))
                {
                    continue;
                }

                if (movedEvent.ActorId != playerId)
                {
                    continue;
                }

                if (!movedEvent.Success)
                {
                    return UiSoundCue.PlayerMoveFailed;
                }

                var moveStyle = _runTurnStateStore.CurrentPlayerMoveStyle.CurrentValue;
                return moveStyle == PlayerMoveStyle.Dash
                    ? UiSoundCue.PlayerDashMove
                    : UiSoundCue.PlayerMove;
            }

            return null;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
