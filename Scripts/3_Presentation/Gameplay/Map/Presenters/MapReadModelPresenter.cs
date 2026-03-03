using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.Map.DisplayModels;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;

namespace Roguelike.Presentation.Gameplay.Map.Presenters
{
    /// <summary>
    /// ReadModel取得とマップ系ビュー更新を仲介するPresenterです。
    /// </summary>
    public sealed class MapReadModelPresenter : IMapReadModelPresenter, IDisposable
    {
        private static readonly IReadOnlyList<GridPositionDto> EmptyPreviewPositions =
            Array.Empty<GridPositionDto>();

        private readonly RunReadModelQueryHandler _runReadModelQueryHandler;
        private readonly RunTurnStateStore _runTurnStateStore;
        private readonly SpellPreviewPresenter _spellPreviewController;
        private readonly RunUiController _runUiController;
        private readonly IDungeonMapView _dungeonMapView;
        private readonly IMiniMapView _miniMapView;
        private readonly IEnemyLayerView _enemyLayerView;
        private readonly CompositeDisposable _disposables = new();

        private bool _initialized;
        private RunReadModelDto _latestReadModel;

        public MapReadModelPresenter(
            RunReadModelQueryHandler runReadModelQueryUseCase,
            RunTurnStateStore runTurnStateStore,
            SpellPreviewPresenter spellPreviewPresenter,
            RunUiController runUiController,
            IDungeonMapView dungeonMapView,
            IMiniMapView miniMapView,
            IEnemyLayerView enemyLayerView)
        {
            _runReadModelQueryHandler = runReadModelQueryUseCase
                ?? throw new ArgumentNullException(nameof(runReadModelQueryUseCase));
            _runTurnStateStore = runTurnStateStore ?? throw new ArgumentNullException(nameof(runTurnStateStore));
            _spellPreviewController = spellPreviewPresenter ?? throw new ArgumentNullException(nameof(spellPreviewPresenter));
            _runUiController = runUiController ?? throw new ArgumentNullException(nameof(runUiController));
            _dungeonMapView = dungeonMapView ?? throw new ArgumentNullException(nameof(dungeonMapView));
            _miniMapView = miniMapView ?? throw new ArgumentNullException(nameof(miniMapView));
            _enemyLayerView = enemyLayerView ?? throw new ArgumentNullException(nameof(enemyLayerView));
        }

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _runTurnStateStore.TurnCount
                .Subscribe(_ => RefreshAll())
                .AddTo(_disposables);

            _spellPreviewController.PreviewPositions
                .Subscribe(_ => RenderDungeonIfReady())
                .AddTo(_disposables);

            _runUiController.IsSpellPreviewOpen
                .Subscribe(_ => RenderDungeonIfReady())
                .AddTo(_disposables);

            RefreshAll();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void RefreshAll()
        {
            var readModelResult = _runReadModelQueryHandler.Handle();
            if (!readModelResult.IsSuccess || !readModelResult.Value.HasRun)
            {
                return;
            }

            _latestReadModel = readModelResult.Value;

            var miniMapModel = new MiniMapDisplayModel(
                _latestReadModel.Map,
                _latestReadModel.Player,
                _latestReadModel.Enemies,
                _latestReadModel.GroundItems);
            _miniMapView.Render(miniMapModel);

            var enemyLayerModel = new EnemyLayerDisplayModel(_latestReadModel.Enemies);
            _enemyLayerView.Render(enemyLayerModel);

            RenderDungeonIfReady();
        }

        private void RenderDungeonIfReady()
        {
            if (!_latestReadModel.HasRun)
            {
                return;
            }

            var isSpellPreviewOpen = _runUiController.IsSpellPreviewOpen.CurrentValue;
            var previewPositions = isSpellPreviewOpen
                ? _spellPreviewController.PreviewPositions.Value ?? EmptyPreviewPositions
                : EmptyPreviewPositions;

            var model = new DungeonMapDisplayModel(
                _latestReadModel.Map,
                _latestReadModel.GroundItems,
                isSpellPreviewOpen,
                previewPositions);
            _dungeonMapView.Render(model);
        }
    }
}






