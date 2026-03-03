using System;
using R3;
using Roguelike.Presentation.Gameplay.RunResult.Stores;

namespace Roguelike.Presentation.Gameplay.Shell.Core
{
    /// <summary>
    /// Presentation層におけるUI状態の単一窓口です。
    /// 入力とビューはこのクラスの公開状態/操作APIだけを参照します。
    /// </summary>
    public sealed class RunUiController : IDisposable
    {
        private readonly RunUiModalStack _uiStack;
        private readonly RunUiPolicy _policy;
        private readonly CompositeDisposable _disposables = new();

        /// <summary>スタック最上位のUI状態。</summary>
        public ReactiveProperty<RunUiState> CurrentState => _uiStack.CurrentState;
        /// <summary>メニュー系UIが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsMenuOpen { get; }
        /// <summary>インベントリ系UIが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsInventoryOpen { get; }
        /// <summary>フロア確認UIが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsFloorConfirmOpen { get; }
        /// <summary>操作説明UIが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsGuideOpen { get; }
        /// <summary>詳細メニューが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsDetailMenuOpen { get; }
        /// <summary>説明ビューが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsDescriptionViewOpen { get; }
        /// <summary>呪文プレビューが開いているか。</summary>
        public ReadOnlyReactiveProperty<bool> IsSpellPreviewOpen { get; }

        public RunUiController(RunResultStore runResultStore)
        {
            if (runResultStore == null) throw new ArgumentNullException(nameof(runResultStore));

            _uiStack = new RunUiModalStack();
            _uiStack.AddTo(_disposables);
            _policy = new RunUiPolicy();

            // メニュー系は SpellPreview 表示中だけ false に落とす。
            IsMenuOpen = _uiStack.StackVersion
                .Select(_ => IsInMenuState())
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsGuideOpen = _uiStack.StackVersion
                .Select(_ => _uiStack.Contains(RunUiState.Guide))
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsInventoryOpen = _uiStack.StackVersion
                .Select(_ => IsInInventoryState())
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsDetailMenuOpen = _uiStack.StackVersion
                .Select(_ => IsInDetailMenuState())
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsDescriptionViewOpen = _uiStack.StackVersion
                .Select(_ => IsInDescriptionState())
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsSpellPreviewOpen = _uiStack.StackVersion
                .Select(_ => _uiStack.Contains(RunUiState.SpellPreview))
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsFloorConfirmOpen = _uiStack.StackVersion
                .Select(_ => _uiStack.Contains(RunUiState.FloorConfirm))
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            // 結果画面へ遷移したら、残っているモーダルを干渉防止のため全クリアする。
            runResultStore.HasResult
                .Subscribe(hasResult =>
                {
                    if (hasResult)
                    {
                        ClearAll();
                    }
                })
                .AddTo(_disposables);
        }

        private bool IsInMenuState()
        {
            if (_uiStack.Contains(RunUiState.SpellPreview))
            {
                return false;
            }

            return _uiStack.Contains(RunUiState.Menu)
                || _uiStack.Contains(RunUiState.Inventory)
                || _uiStack.Contains(RunUiState.InventoryDetail)
                || _uiStack.Contains(RunUiState.InventoryDescription)
                || _uiStack.Contains(RunUiState.Guide);
        }

        private bool IsInInventoryState()
        {
            if (_uiStack.Contains(RunUiState.SpellPreview))
            {
                return false;
            }

            return _uiStack.Contains(RunUiState.Inventory)
                || _uiStack.Contains(RunUiState.InventoryDetail)
                || _uiStack.Contains(RunUiState.InventoryDescription);
        }

        private bool IsInDetailMenuState()
        {
            if (_uiStack.Contains(RunUiState.SpellPreview))
            {
                return false;
            }

            return _uiStack.Contains(RunUiState.InventoryDetail)
                || _uiStack.Contains(RunUiState.InventoryDescription);
        }

        private bool IsInDescriptionState()
        {
            if (_uiStack.Contains(RunUiState.SpellPreview))
            {
                return false;
            }

            return _uiStack.Contains(RunUiState.InventoryDescription);
        }

        public bool OpenMenu()
        {
            return _policy.TryOpenMenu(_uiStack);
        }

        public bool CloseMenu()
        {
            return _policy.TryCloseMenu(_uiStack);
        }

        public bool OpenInventory()
        {
            return _policy.TryOpenInventory(_uiStack);
        }

        public bool CloseInventory()
        {
            return _policy.TryCloseInventory(_uiStack);
        }

        public bool OpenDetailMenu()
        {
            return _policy.TryOpenDetailMenu(_uiStack);
        }

        public bool CloseDetailMenu()
        {
            return _policy.TryCloseDetailMenu(_uiStack);
        }

        public bool OpenDescriptionView()
        {
            return _policy.TryOpenDescriptionView(_uiStack);
        }

        public bool CloseDescriptionView()
        {
            return _policy.TryCloseDescriptionView(_uiStack);
        }

        public bool OpenSpellPreview()
        {
            return _policy.TryOpenSpellPreview(_uiStack);
        }

        public bool CloseSpellPreview()
        {
            return _policy.TryCloseSpellPreview(_uiStack);
        }

        public bool OpenGuide()
        {
            return _policy.TryOpenGuide(_uiStack);
        }

        public bool CloseGuide()
        {
            return _policy.TryCloseGuide(_uiStack);
        }

        public bool OpenFloorConfirm()
        {
            return _policy.TryOpenFloorConfirm(_uiStack);
        }

        public bool CloseFloorConfirm()
        {
            return _policy.TryCloseFloorConfirm(_uiStack);
        }

        public void ClearAll()
        {
            _policy.TryClearAll(_uiStack);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}




