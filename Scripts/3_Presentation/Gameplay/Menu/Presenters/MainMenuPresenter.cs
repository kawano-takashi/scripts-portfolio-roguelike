using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Enums;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Inventory.Presenters;
using Roguelike.Presentation.Gameplay.Guide.Presenters;
using Roguelike.Presentation.Gameplay.Menu.Types;

namespace Roguelike.Presentation.Gameplay.Menu.Presenters
{
    /// <summary>
    /// メインメニューのUI状態（選択位置）を保持し、実行判断を行うコントローラです。
    /// </summary>
    public sealed class MainMenuPresenter : IDisposable
    {
        /// <summary>メインメニューの固定表示順。</summary>
        private static readonly IReadOnlyList<MainMenuOption> DefaultOptions = new[]
        {
            MainMenuOption.Inventory,
            MainMenuOption.Status,
            MainMenuOption.Settings,
            MainMenuOption.OperationGuide,
            MainMenuOption.Close
        };

        private readonly CanUseCapabilityQueryHandler _canUseCapabilityQueryHandler;
        private readonly RunUiController _runUiController;
        private readonly InventoryPresenter _inventoryController;
        private readonly OperationGuidePresenter _operationGuideController;
        private readonly IUiSoundPlayer _uiSoundPlayer;
        private readonly CompositeDisposable _disposables = new();

        /// <summary>現在選択中のメニューインデックス。</summary>
        public ReactiveProperty<int> SelectedIndex { get; }
        /// <summary>表示用のメニュー一覧。</summary>
        public IReadOnlyList<MainMenuOption> MenuOptions => DefaultOptions;

        public MainMenuPresenter(
            CanUseCapabilityQueryHandler canUseCapabilityQueryUseCase,
            RunUiController runUiController,
            InventoryPresenter inventoryPresenter,
            OperationGuidePresenter operationGuidePresenter,
            IUiSoundPlayer uiSoundPlayer)
        {
            _canUseCapabilityQueryHandler = canUseCapabilityQueryUseCase
                ?? throw new ArgumentNullException(nameof(canUseCapabilityQueryUseCase));
            _runUiController = runUiController ?? throw new ArgumentNullException(nameof(runUiController));
            _inventoryController = inventoryPresenter ?? throw new ArgumentNullException(nameof(inventoryPresenter));
            _operationGuideController = operationGuidePresenter ?? throw new ArgumentNullException(nameof(operationGuidePresenter));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            SelectedIndex = new ReactiveProperty<int>(0).AddTo(_disposables);
        }

        /// <summary>
        /// メニューを開き、選択を先頭に戻します。
        /// </summary>
        public bool OpenMenu()
        {
            var canOpen = _canUseCapabilityQueryHandler.Handle(
                new GetRunAccessCapabilityQuery(RunAccessCapability.OpenMenu));
            if (!canOpen.IsSuccess || !canOpen.Value)
            {
                return false;
            }

            var wasOpen = _runUiController.IsMenuOpen.CurrentValue;
            var opened = _runUiController.OpenMenu();
            var isOpenNow = _runUiController.IsMenuOpen.CurrentValue;
            if (!opened && !isOpenNow)
            {
                return false;
            }

            if (!wasOpen && isOpenNow)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiOpen);
            }

            SelectedIndex.Value = 0;
            return true;
        }

        /// <summary>
        /// メニューを閉じます。
        /// </summary>
        public bool CloseMenu()
        {
            if (!_runUiController.IsMenuOpen.CurrentValue)
            {
                return false;
            }

            var closed = _runUiController.CloseMenu();
            if (closed)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiClose);
            }

            return closed;
        }

        /// <summary>次の項目を選択します（末尾で先頭に循環）。</summary>
        public void SelectNext()
        {
            if (!_runUiController.IsMenuOpen.CurrentValue)
            {
                return;
            }

            SelectedIndex.Value = (SelectedIndex.Value + 1) % MenuOptions.Count;
            _uiSoundPlayer.Play(UiSoundCue.MenuSelect);
        }

        /// <summary>前の項目を選択します（先頭で末尾に循環）。</summary>
        public void SelectPrevious()
        {
            if (!_runUiController.IsMenuOpen.CurrentValue)
            {
                return;
            }

            SelectedIndex.Value = (SelectedIndex.Value - 1 + MenuOptions.Count) % MenuOptions.Count;
            _uiSoundPlayer.Play(UiSoundCue.MenuSelect);
        }

        /// <summary>
        /// 現在選択中の項目を確定し、対応処理を実行します。
        /// </summary>
        public void ConfirmSelection()
        {
            if (!_runUiController.IsMenuOpen.CurrentValue)
            {
                return;
            }

            var option = GetSelectedOption();
            if (!IsOptionEnabled(option))
            {
                return;
            }

            switch (option)
            {
                case MainMenuOption.Inventory:
                    _inventoryController.OpenInventory();
                    break;
                case MainMenuOption.OperationGuide:
                    _operationGuideController.Open();
                    break;
                case MainMenuOption.Close:
                    CloseMenu();
                    break;
            }
        }

        /// <summary>現在選択されているメニュー項目を返します。</summary>
        public MainMenuOption GetSelectedOption()
        {
            return MenuOptions[SelectedIndex.Value];
        }

        /// <summary>指定項目が現在有効かを返します。</summary>
        public bool IsOptionEnabled(MainMenuOption option)
        {
            return option == MainMenuOption.Inventory
                || option == MainMenuOption.OperationGuide
                || option == MainMenuOption.Close;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}





