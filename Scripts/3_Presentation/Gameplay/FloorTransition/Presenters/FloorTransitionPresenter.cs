using System;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Hud.Stores;

namespace Roguelike.Presentation.Gameplay.FloorTransition.Presenters
{
    /// <summary>
    /// フロア遷移確認UIの状態と確定処理を管理するコントローラです。
    /// </summary>
    public sealed class FloorTransitionPresenter : IDisposable
    {
        private const int OptionCount = 2;

        private readonly RunTurnStateStore _runTurnStateStore;
        private readonly AdvanceFloorCommandHandler _advanceFloorCommandHandler;
        private readonly RunResultStore _runResultStore;
        private readonly HasActiveRunQueryHandler _hasActiveRunQueryHandler;
        private readonly GetPlayerIdQueryHandler _getPlayerIdQueryHandler;
        private readonly GetStairsDownPositionQueryHandler _getStairsDownPositionQueryHandler;
        private readonly RunSnapshotQueryHandler _runSnapshotQueryHandler;
        private readonly RunUiController _runUiController;
        private readonly IUiSoundPlayer _uiSoundPlayer;
        private readonly CompositeDisposable _disposables = new();

        public ReactiveProperty<int> SelectedIndex { get; }

        public FloorTransitionPresenter(
            RunTurnStateStore runTurnStateStore,
            AdvanceFloorCommandHandler advanceFloorCommandUseCase,
            RunResultStore runResultStore,
            HasActiveRunQueryHandler hasActiveRunQueryUseCase,
            GetPlayerIdQueryHandler getPlayerIdQueryUseCase,
            GetStairsDownPositionQueryHandler getStairsDownPositionQueryUseCase,
            RunSnapshotQueryHandler runSnapshotQueryUseCase,
            RunUiController runUiController,
            IUiSoundPlayer uiSoundPlayer)
        {
            _runTurnStateStore = runTurnStateStore ?? throw new ArgumentNullException(nameof(runTurnStateStore));
            _advanceFloorCommandHandler = advanceFloorCommandUseCase ?? throw new ArgumentNullException(nameof(advanceFloorCommandUseCase));
            _runResultStore = runResultStore ?? throw new ArgumentNullException(nameof(runResultStore));
            _hasActiveRunQueryHandler = hasActiveRunQueryUseCase
                ?? throw new ArgumentNullException(nameof(hasActiveRunQueryUseCase));
            _getPlayerIdQueryHandler = getPlayerIdQueryUseCase
                ?? throw new ArgumentNullException(nameof(getPlayerIdQueryUseCase));
            _getStairsDownPositionQueryHandler = getStairsDownPositionQueryUseCase
                ?? throw new ArgumentNullException(nameof(getStairsDownPositionQueryUseCase));
            _runSnapshotQueryHandler = runSnapshotQueryUseCase ?? throw new ArgumentNullException(nameof(runSnapshotQueryUseCase));
            _runUiController = runUiController ?? throw new ArgumentNullException(nameof(runUiController));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            SelectedIndex = new ReactiveProperty<int>(0).AddTo(_disposables);

            _runTurnStateStore.LatestResolution
                .Subscribe(ProcessResolution)
                .AddTo(_disposables);
        }

        public void OpenConfirm()
        {
            var hasRun = _hasActiveRunQueryHandler.Handle(new HasActiveRunQuery());
            if (!hasRun.IsSuccess || !hasRun.Value || _runUiController.IsFloorConfirmOpen.CurrentValue)
            {
                return;
            }

            _runUiController.OpenFloorConfirm();
            SelectedIndex.Value = 0;
        }

        public void CloseConfirm()
        {
            _runUiController.CloseFloorConfirm();
        }

        public void SelectNext()
        {
            if (!_runUiController.IsFloorConfirmOpen.CurrentValue)
            {
                return;
            }

            SelectedIndex.Value = (SelectedIndex.Value + 1) % OptionCount;
            _uiSoundPlayer.Play(UiSoundCue.MenuSelect);
        }

        public void SelectPrevious()
        {
            if (!_runUiController.IsFloorConfirmOpen.CurrentValue)
            {
                return;
            }

            SelectedIndex.Value = (SelectedIndex.Value - 1 + OptionCount) % OptionCount;
            _uiSoundPlayer.Play(UiSoundCue.MenuSelect);
        }

        public void ConfirmSelection()
        {
            if (!_runUiController.IsFloorConfirmOpen.CurrentValue)
            {
                return;
            }

            if (SelectedIndex.Value == 0)
            {
                AdvanceToNextFloor();
            }
            else
            {
                CloseConfirm();
            }
        }

        public void Cancel()
        {
            CloseConfirm();
        }

        private void ProcessResolution(RunTurnResultDto resolution)
        {
            if (resolution.Events == null || resolution.Events.Count == 0)
            {
                return;
            }

            if (ContainsStairsEvent(resolution.Events))
            {
                OpenConfirm();
            }
        }

        private bool ContainsStairsEvent(System.Collections.Generic.IReadOnlyList<IRunEventDto> events)
        {
            if (events == null)
            {
                return false;
            }

            var playerId = _getPlayerIdQueryHandler.Handle(new GetPlayerIdQuery());
            var stairs = _getStairsDownPositionQueryHandler.Handle(new GetStairsDownPositionQuery());
            if (!playerId.IsSuccess || !stairs.IsSuccess)
            {
                return false;
            }

            for (var i = 0; i < events.Count; i++)
            {
                if (!(events[i] is ActorMovedEventDto movedEvent))
                {
                    continue;
                }

                if (!movedEvent.Success)
                {
                    continue;
                }

                if (movedEvent.ActorId != playerId.Value)
                {
                    continue;
                }

                var to = movedEvent.ToPosition;
                if (to.X == stairs.Value.X && to.Y == stairs.Value.Y)
                {
                    return true;
                }
            }

            return false;
        }

        private void AdvanceToNextFloor()
        {
            var result = _advanceFloorCommandHandler.Handle();
            if (!result.IsSuccess || !result.Value.Advanced)
            {
                CloseConfirm();
                return;
            }

            _runResultStore.ApplyLifecycleEvents(result.Value.LifecycleEvents);

            var snapshot = _runSnapshotQueryHandler.Handle();
            if (snapshot.IsSuccess)
            {
                _runTurnStateStore.InitializeFromSnapshot(snapshot.Value);
            }

            SelectedIndex.Value = 0;
            _runUiController.CloseFloorConfirm();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}






