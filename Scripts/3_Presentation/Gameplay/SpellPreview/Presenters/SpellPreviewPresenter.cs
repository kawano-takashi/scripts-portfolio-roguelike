using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Shell.Core;

namespace Roguelike.Presentation.Gameplay.SpellPreview.Presenters
{
    /// <summary>
    /// 呪文プレビューのUI状態（座標表示・開閉）を管理するコントローラです。
    /// </summary>
    public sealed class SpellPreviewPresenter : IDisposable
    {
        private static readonly IReadOnlyList<GridPositionDto> EmptyPreviewPositions = Array.Empty<GridPositionDto>();

        private readonly SpellPreviewQueryHandler _spellPreviewQueryHandler;
        private readonly EquippedSpellPreviewQueryHandler _equippedSpellPreviewQueryHandler;
        private readonly RunUiController _runUiController;
        private readonly IUiSoundPlayer _uiSoundPlayer;
        private readonly CompositeDisposable _disposables = new();

        public ReactiveProperty<IReadOnlyList<GridPositionDto>> PreviewPositions { get; }

        public SpellPreviewPresenter(
            SpellPreviewQueryHandler spellPreviewQueryUseCase,
            EquippedSpellPreviewQueryHandler equippedSpellPreviewQueryUseCase,
            RunUiController runUiController,
            IUiSoundPlayer uiSoundPlayer)
        {
            _spellPreviewQueryHandler = spellPreviewQueryUseCase ?? throw new ArgumentNullException(nameof(spellPreviewQueryUseCase));
            _equippedSpellPreviewQueryHandler = equippedSpellPreviewQueryUseCase
                ?? throw new ArgumentNullException(nameof(equippedSpellPreviewQueryUseCase));
            _runUiController = runUiController ?? throw new ArgumentNullException(nameof(runUiController));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            PreviewPositions = new ReactiveProperty<IReadOnlyList<GridPositionDto>>(EmptyPreviewPositions).AddTo(_disposables);

            _runUiController.IsSpellPreviewOpen
                .Where(isOpen => !isOpen)
                .Subscribe(_ => ClearPreviewState())
                .AddTo(_disposables);
        }

        public bool OpenFromItem(InventoryItemDto item)
        {
            var previewResult = _spellPreviewQueryHandler.Handle(new GetSpellPreviewQuery(item.ItemId));
            return previewResult.IsSuccess && TryOpenWithPreview(previewResult.Value);
        }

        public bool OpenFromEquippedSpellbook()
        {
            var previewResult = _equippedSpellPreviewQueryHandler.Handle(new GetEquippedSpellPreviewQuery());
            return previewResult.IsSuccess && TryOpenWithPreview(previewResult.Value);
        }

        public void ConfirmSelection()
        {
            if (!_runUiController.IsSpellPreviewOpen.CurrentValue)
            {
                return;
            }

            ClosePreview();
        }

        public void ClosePreview()
        {
            if (!_runUiController.IsSpellPreviewOpen.CurrentValue)
            {
                ClearPreviewState();
                return;
            }

            var closed = _runUiController.CloseSpellPreview();
            if (closed)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiClose);
            }

            ClearPreviewState();
        }

        // プレビュー表示のオープンに成功したらプレビュー情報をセットします。
        private bool TryOpenWithPreview(SpellPreviewDto preview)
        {
            if (!preview.CanPreview)
            {
                return false;
            }

            var wasOpen = _runUiController.IsSpellPreviewOpen.CurrentValue;
            var opened = _runUiController.OpenSpellPreview();
            var isOpenNow = _runUiController.IsSpellPreviewOpen.CurrentValue;
            if (!opened && !isOpenNow)
            {
                return false;
            }

            if (!wasOpen && isOpenNow)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiOpen);
            }

            PreviewPositions.Value = preview.PreviewPositions;
            return true;
        }

        private void ClearPreviewState()
        {
            PreviewPositions.Value = EmptyPreviewPositions;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}





