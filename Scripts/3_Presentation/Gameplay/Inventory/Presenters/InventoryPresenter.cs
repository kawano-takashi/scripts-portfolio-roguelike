using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;
using Roguelike.Presentation.Gameplay.Inventory.Formatting;
using Roguelike.Presentation.Gameplay.Inventory.Types;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;

namespace Roguelike.Presentation.Gameplay.Inventory.Presenters
{
    /// <summary>
    /// インベントリUI状態（選択位置・ページ・詳細メニュー）を保持するコントローラです。
    /// </summary>
    public sealed class InventoryPresenter : IDisposable
    {
        private static readonly IReadOnlyList<ItemDetailMenuOption> DefaultDetailMenuOptions = new[]
        {
            ItemDetailMenuOption.Use,
            ItemDetailMenuOption.Drop,
            ItemDetailMenuOption.Description
        };

        private static readonly IReadOnlyList<ItemDetailMenuOption> EquipDetailMenuOptions = new[]
        {
            ItemDetailMenuOption.Equip,
            ItemDetailMenuOption.Drop,
            ItemDetailMenuOption.Description
        };

        private static readonly IReadOnlyList<ItemDetailMenuOption> SpellbookDetailMenuOptions = new[]
        {
            ItemDetailMenuOption.Use,
            ItemDetailMenuOption.Equip,
            ItemDetailMenuOption.Drop,
            ItemDetailMenuOption.Description
        };

        private static readonly IReadOnlyList<ItemDetailMenuOption> SpellbookPreviewDetailMenuOptions = new[]
        {
            ItemDetailMenuOption.Use,
            ItemDetailMenuOption.Equip,
            ItemDetailMenuOption.Drop,
            ItemDetailMenuOption.SpellPreview,
            ItemDetailMenuOption.Description
        };

        public const int ItemsPerPage = 10;
        public const int MaxItems = 20;
        public const int TotalPages = 2;

        private readonly CanUseCapabilityQueryHandler _canUseCapabilityQueryHandler;
        private readonly InventoryQueryHandler _queryHandler;
        private readonly RunActionCommandHandler _commandHandler;
        private readonly RunUiController _runUiController;
        private readonly SpellPreviewPresenter _spellPreviewController;
        private readonly InventoryFormatter _inventoryFormatter;
        private readonly RunTurnStateStore _runTurnStateStore;
        private readonly RunResultStore _runResultStore;
        private readonly IUiSoundPlayer _uiSoundPlayer;
        private readonly CompositeDisposable _disposables = new();

        public ReactiveProperty<IReadOnlyList<InventoryItemDto>> InventoryItems { get; }
        public ReactiveProperty<int> SelectedIndex { get; }
        public ReactiveProperty<int> DetailMenuSelectedIndex { get; }
        public ReactiveProperty<IReadOnlyList<ItemDetailMenuOption>> DetailMenuOptions { get; }
        public ReactiveProperty<int> CurrentPageIndex { get; }

        public InventoryPresenter(
            CanUseCapabilityQueryHandler canUseCapabilityQueryUseCase,
            InventoryQueryHandler queryUseCase,
            RunActionCommandHandler commandUseCase,
            RunUiController runUiController,
            SpellPreviewPresenter spellPreviewPresenter,
            InventoryFormatter inventoryFormatter,
            RunTurnStateStore runTurnStateStore,
            RunResultStore runResultStore,
            IUiSoundPlayer uiSoundPlayer)
        {
            _canUseCapabilityQueryHandler = canUseCapabilityQueryUseCase
                ?? throw new ArgumentNullException(nameof(canUseCapabilityQueryUseCase));
            _queryHandler = queryUseCase ?? throw new ArgumentNullException(nameof(queryUseCase));
            _commandHandler = commandUseCase ?? throw new ArgumentNullException(nameof(commandUseCase));
            _runUiController = runUiController ?? throw new ArgumentNullException(nameof(runUiController));
            _spellPreviewController = spellPreviewPresenter ?? throw new ArgumentNullException(nameof(spellPreviewPresenter));
            _inventoryFormatter = inventoryFormatter ?? throw new ArgumentNullException(nameof(inventoryFormatter));
            _runTurnStateStore = runTurnStateStore ?? throw new ArgumentNullException(nameof(runTurnStateStore));
            _runResultStore = runResultStore ?? throw new ArgumentNullException(nameof(runResultStore));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            InventoryItems = new ReactiveProperty<IReadOnlyList<InventoryItemDto>>(Array.Empty<InventoryItemDto>())
                .AddTo(_disposables);
            SelectedIndex = new ReactiveProperty<int>(0).AddTo(_disposables);
            DetailMenuSelectedIndex = new ReactiveProperty<int>(0).AddTo(_disposables);
            DetailMenuOptions = new ReactiveProperty<IReadOnlyList<ItemDetailMenuOption>>(
                BuildDetailMenuOptions(null)).AddTo(_disposables);
            CurrentPageIndex = new ReactiveProperty<int>(0).AddTo(_disposables);
        }

        public bool OpenInventory()
        {
            var canOpen = _canUseCapabilityQueryHandler.Handle(
                new GetRunAccessCapabilityQuery(RunAccessCapability.OpenInventory));
            if (!canOpen.IsSuccess || !canOpen.Value)
            {
                return false;
            }

            var wasOpen = _runUiController.IsInventoryOpen.CurrentValue;
            var opened = _runUiController.OpenInventory();
            var isOpenNow = _runUiController.IsInventoryOpen.CurrentValue;
            if (!opened && !isOpenNow)
            {
                return false;
            }

            if (!wasOpen && isOpenNow)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiOpen);
            }

            RefreshFromCurrentRun();
            return true;
        }

        public bool CloseInventory()
        {
            if (!_runUiController.IsInventoryOpen.CurrentValue)
            {
                return false;
            }

            var closed = _runUiController.CloseInventory();
            if (closed)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiClose);
            }

            return closed;
        }

        public void RefreshFromCurrentRun()
        {
            var queryResult = _queryHandler.Handle();
            var items = queryResult.IsSuccess ? queryResult.Value : Array.Empty<InventoryItemDto>();
            InventoryItems.Value = items;

            if (items.Count == 0)
            {
                SelectedIndex.Value = 0;
                CurrentPageIndex.Value = 0;
                DetailMenuOptions.Value = BuildDetailMenuOptions(null);
                DetailMenuSelectedIndex.Value = 0;
                return;
            }

            if (SelectedIndex.Value >= items.Count)
            {
                SelectedIndex.Value = Math.Max(0, items.Count - 1);
            }

            CurrentPageIndex.Value = SelectedIndex.Value / ItemsPerPage;
            DetailMenuOptions.Value = BuildDetailMenuOptions(GetSelectedItem());
            DetailMenuSelectedIndex.Value = 0;
        }

        public bool IsItemEquippable(InventoryItemDto item)
        {
            return item.IsEquippable;
        }

        public bool IsItemEquipped(InventoryItemDto item)
        {
            return item.IsEquipped;
        }

        public string GetItemDisplayName(InventoryItemDto item)
        {
            return _inventoryFormatter.FormatItemDisplayName(item);
        }

        public string GetDetailedItemDescription(InventoryItemDto item)
        {
            return _inventoryFormatter.FormatDetailedDescription(item);
        }

        public string GetDetailMenuLabel(ItemDetailMenuOption option, InventoryItemDto item)
        {
            return _inventoryFormatter.GetDetailMenuLabel(
                option,
                IsItemEquippable(item),
                IsItemEquipped(item));
        }

        public void SelectNext()
        {
            var count = InventoryItems.Value.Count;
            if (count == 0)
            {
                return;
            }

            var currentIndex = SelectedIndex.Value;
            var newIndex = (currentIndex + 1) % count;

            // 選択インデックスが実際に変わらない場合は処理をスキップ
            if (newIndex == currentIndex)
            {
                return;
            }

            CurrentPageIndex.Value = newIndex / ItemsPerPage;
            SelectedIndex.Value = newIndex;
            _uiSoundPlayer.Play(UiSoundCue.InventorySelect);
        }

        public void SelectPrevious()
        {
            var count = InventoryItems.Value.Count;
            if (count == 0)
            {
                return;
            }

            var currentIndex = SelectedIndex.Value;

            // 選択インデックスが実際に変わらない場合は処理をスキップ
            var newIndex = (currentIndex - 1 + count) % count;
            if (newIndex == currentIndex)
            {
                return;
            }

            CurrentPageIndex.Value = newIndex / ItemsPerPage;
            SelectedIndex.Value = newIndex;
            _uiSoundPlayer.Play(UiSoundCue.InventorySelect);
        }

        public void NextPage()
        {
            if (CurrentPageIndex.Value >= TotalPages - 1)
            {
                return;
            }

            CurrentPageIndex.Value++;
            MoveSelectionToCurrentPageStart();
        }

        public void PreviousPage()
        {
            if (CurrentPageIndex.Value <= 0)
            {
                return;
            }

            CurrentPageIndex.Value--;
            MoveSelectionToCurrentPageStart();
        }

        public InventoryItemDto? GetSelectedItem()
        {
            var items = InventoryItems.Value;
            var index = SelectedIndex.Value;
            if (items == null || index < 0 || index >= items.Count)
            {
                return null;
            }

            return items[index];
        }

        public bool OpenDetailMenu()
        {
            if (!_runUiController.IsInventoryOpen.CurrentValue)
            {
                return false;
            }

            if (!GetSelectedItem().HasValue)
            {
                return false;
            }

            DetailMenuOptions.Value = BuildDetailMenuOptions(GetSelectedItem());
            DetailMenuSelectedIndex.Value = 0;
            var opened = _runUiController.OpenDetailMenu();
            if (opened)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiOpen);
            }

            return opened;
        }

        public bool CloseDetailMenu(bool playCloseSound = true)
        {
            var closed = _runUiController.CloseDetailMenu();
            if (closed && playCloseSound)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiClose);
            }

            return closed;
        }

        public bool OpenDescriptionView()
        {
            if (!_runUiController.IsDetailMenuOpen.CurrentValue)
            {
                return false;
            }

            var opened = _runUiController.OpenDescriptionView();
            if (opened)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiOpen);
            }

            return opened;
        }

        public bool CloseDescriptionView(bool playCloseSound = true)
        {
            var closed = _runUiController.CloseDescriptionView();
            if (closed && playCloseSound)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiClose);
            }

            return closed;
        }

        public void DetailMenuSelectNext()
        {
            if (!_runUiController.IsDetailMenuOpen.CurrentValue)
            {
                return;
            }

            var optionCount = DetailMenuOptions.Value?.Count ?? 0;
            if (optionCount <= 0)
            {
                return;
            }

            var currentIndex = DetailMenuSelectedIndex.Value;
            var newIndex = (currentIndex + 1) % optionCount;
            if (newIndex == currentIndex)
            {
                return;
            }

            DetailMenuSelectedIndex.Value = newIndex;
            _uiSoundPlayer.Play(UiSoundCue.MenuSelect);
        }

        public void DetailMenuSelectPrevious()
        {
            if (!_runUiController.IsDetailMenuOpen.CurrentValue)
            {
                return;
            }

            var optionCount = DetailMenuOptions.Value?.Count ?? 0;
            if (optionCount <= 0)
            {
                return;
            }

            var currentIndex = DetailMenuSelectedIndex.Value;
            var newIndex = (currentIndex - 1 + optionCount) % optionCount;
            if (newIndex == currentIndex)
            {
                return;
            }

            DetailMenuSelectedIndex.Value = newIndex;
            _uiSoundPlayer.Play(UiSoundCue.MenuSelect);
        }

        public ItemDetailMenuOption GetSelectedDetailOption()
        {
            var options = DetailMenuOptions.Value;
            if (options == null || options.Count == 0)
            {
                return ItemDetailMenuOption.Use;
            }

            var selectedIndex = Math.Clamp(DetailMenuSelectedIndex.Value, 0, options.Count - 1);
            if (selectedIndex != DetailMenuSelectedIndex.Value)
            {
                DetailMenuSelectedIndex.Value = selectedIndex;
            }

            return options[selectedIndex];
        }

        public void ExecuteSelectedDetailOption()
        {
            var selectedItem = GetSelectedItem();
            if (!selectedItem.HasValue)
            {
                return;
            }

            var item = selectedItem.Value;
            switch (GetSelectedDetailOption())
            {
                case ItemDetailMenuOption.Use:
                    if (item.CanUse)
                    {
                        var execution = _commandHandler.Handle(new UseItemRunActionCommand(item.ItemId));
                        if (execution.IsSuccess && execution.Value.TurnResult.ActionResolved)
                        {
                            // アイテム使用のサウンドはアイテム側で再生するため、ここでは再生しません。
                            // _uiSoundPlayer.Play(UiSoundCue.InventoryDetailOptionExecute);
                            ApplyExecution(execution.Value);
                            _runUiController.CloseMenu();
                            RefreshFromCurrentRun();
                        }
                    }
                    break;
                case ItemDetailMenuOption.Equip:
                    if (item.CanToggleEquip)
                    {
                        var execution = _commandHandler.Handle(new ToggleEquipItemRunActionCommand(item.ItemId));
                        if (execution.IsSuccess && execution.Value.TurnResult.ActionResolved)
                        {
                            _uiSoundPlayer.Play(UiSoundCue.InventoryDetailOptionExecute);
                            ApplyExecution(execution.Value);
                            CloseDetailMenu(playCloseSound: false);
                            RefreshFromCurrentRun();
                        }
                    }
                    break;
                case ItemDetailMenuOption.Drop:
                    if (item.CanDrop)
                    {
                        var execution = _commandHandler.Handle(new DropItemRunActionCommand(item.ItemId));
                        if (execution.IsSuccess && execution.Value.TurnResult.ActionResolved)
                        {
                            _uiSoundPlayer.Play(UiSoundCue.InventoryDetailOptionExecute);
                            ApplyExecution(execution.Value);
                            _runUiController.CloseMenu();
                            RefreshFromCurrentRun();
                        }
                    }
                    break;
                case ItemDetailMenuOption.SpellPreview:
                    if (item.CanShowSpellPreview)
                    {
                        _spellPreviewController.OpenFromItem(item);
                    }
                    break;
                case ItemDetailMenuOption.Description:
                    OpenDescriptionView();
                    break;
            }
        }

        public bool DropSelectedItemFromShortcut()
        {
            if (!_runUiController.IsInventoryOpen.CurrentValue)
            {
                return false;
            }

            var selectedItem = GetSelectedItem();
            if (!selectedItem.HasValue)
            {
                return false;
            }

            if (!selectedItem.Value.CanDrop)
            {
                return false;
            }

            var execution = _commandHandler.Handle(new DropItemRunActionCommand(selectedItem.Value.ItemId));
            if (!execution.IsSuccess || !execution.Value.TurnResult.ActionResolved)
            {
                return false;
            }

            ApplyExecution(execution.Value);
            _runUiController.CloseMenu();
            RefreshFromCurrentRun();
            return true;
        }

        private IReadOnlyList<ItemDetailMenuOption> BuildDetailMenuOptions(InventoryItemDto? item)
        {
            if (!item.HasValue)
            {
                return DefaultDetailMenuOptions;
            }

            if (item.Value.IsSpellbook)
            {
                return item.Value.CanShowSpellPreview
                    ? SpellbookPreviewDetailMenuOptions
                    : SpellbookDetailMenuOptions;
            }

            return item.Value.IsEquippable ? EquipDetailMenuOptions : DefaultDetailMenuOptions;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void MoveSelectionToCurrentPageStart()
        {
            var count = InventoryItems.Value.Count;
            if (count == 0)
            {
                SelectedIndex.Value = 0;
                return;
            }

            var pageStart = CurrentPageIndex.Value * ItemsPerPage;
            SelectedIndex.Value = Math.Clamp(pageStart, 0, count - 1);
        }

        private void ApplyExecution(RunCommandExecutionResultDto execution)
        {
            _runResultStore.ApplyLifecycleEvents(execution.LifecycleEvents);
            _runTurnStateStore.ApplyCommandExecutionResult(execution);
        }
    }
}






