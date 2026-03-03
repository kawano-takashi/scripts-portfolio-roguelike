using System;
using System.Collections.Generic;
using R3;

namespace Roguelike.Presentation.Gameplay.Shell.Core
{
    /// <summary>
    /// Presentation層で扱うUIモーダルスタックです。
    /// 実際の遷移ルールは <see cref="RunUiPolicy"/> に委譲します。
    /// </summary>
    public sealed class RunUiModalStack : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly List<RunUiState> _stack = new();
        // スタック変更をReactiveに通知するためのバージョンカウンタです。
        private readonly ReactiveProperty<int> _stackVersion;

        /// <summary>現在の最前面UI状態。</summary>
        public ReactiveProperty<RunUiState> CurrentState { get; }
        /// <summary>スタック変更通知用のシーケンス。</summary>
        public ReadOnlyReactiveProperty<int> StackVersion { get; }

        public RunUiModalStack()
        {
            CurrentState = new ReactiveProperty<RunUiState>(RunUiState.None)
                .AddTo(_disposables);
            _stackVersion = new ReactiveProperty<int>(0).AddTo(_disposables);
            StackVersion = _stackVersion.ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
        }

        /// <summary>現在のトップ状態（空ならNone）。</summary>
        public RunUiState Top => _stack.Count == 0 ? RunUiState.None : _stack[^1];

        /// <summary>指定状態がスタック内に存在するかを返します。</summary>
        public bool Contains(RunUiState state)
        {
            return _stack.Contains(state);
        }

        /// <summary>状態をスタック最上位に積みます。</summary>
        internal bool Push(RunUiState state)
        {
            if (state == RunUiState.None)
            {
                return false;
            }

            _stack.Add(state);
            NotifyChanged();
            return true;
        }

        /// <summary>最上位の状態を1つ取り除きます。</summary>
        internal bool Pop()
        {
            if (_stack.Count == 0)
            {
                return false;
            }

            _stack.RemoveAt(_stack.Count - 1);
            NotifyChanged();
            return true;
        }

        /// <summary>指定状態まで戻し、それより上位をすべて閉じます。</summary>
        internal bool PopTo(RunUiState state)
        {
            if (state == RunUiState.None)
            {
                return Clear();
            }

            var index = _stack.LastIndexOf(state);
            if (index < 0 || index == _stack.Count - 1)
            {
                return false;
            }

            _stack.RemoveRange(index + 1, _stack.Count - index - 1);
            NotifyChanged();
            return true;
        }

        /// <summary>スタックをすべてクリアします。</summary>
        internal bool Clear()
        {
            if (_stack.Count == 0 && CurrentState.Value == RunUiState.None)
            {
                return false;
            }

            _stack.Clear();
            NotifyChanged();
            return true;
        }

        private void NotifyChanged()
        {
            var top = Top;
            if (CurrentState.Value != top)
            {
                CurrentState.Value = top;
            }

            // CurrentStateが同じでも「構成が変わった」ことを購読側に伝えるため必ず進める。
            _stackVersion.Value++;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



