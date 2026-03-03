using System;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.UseCases;

namespace Roguelike.Presentation.Gameplay.Hud.Stores
{
    /// <summary>
    /// ラン状態の表示データを保持します。
    /// </summary>
    public sealed class RunStatusStore : IDisposable
    {
        private readonly RunSnapshotQueryHandler _queryService;
        private readonly RunTurnStateStore _turnStateStore;
        private readonly CompositeDisposable _disposables = new();

        public ReactiveProperty<RunSnapshotDto> CurrentStatus { get; }

        public RunStatusStore(
            RunSnapshotQueryHandler queryService,
            RunTurnStateStore turnStateStore)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _turnStateStore = turnStateStore ?? throw new ArgumentNullException(nameof(turnStateStore));

            CurrentStatus = new ReactiveProperty<RunSnapshotDto>(RunSnapshotDto.Empty)
                .AddTo(_disposables);

            _turnStateStore.TurnCount
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        public void Refresh()
        {
            var snapshotResult = _queryService.Handle();
            CurrentStatus.Value = snapshotResult.IsSuccess
                ? snapshotResult.Value
                : RunSnapshotDto.Empty;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



