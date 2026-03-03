using System;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;
using Roguelike.Presentation.Gameplay.Hud.Types;
using Roguelike.Presentation.Gameplay.RunResult.Types;

namespace Roguelike.Presentation.Gameplay.Hud.Stores
{
    /// <summary>
    /// ターン進行に伴う表示状態を保持するPresentationストアです。
    /// </summary>
    public sealed class RunTurnStateStore : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private int _animationLockCount;

        public ReactiveProperty<GridPositionDto> PlayerPosition { get; }
        public ReactiveProperty<int> PlayerFacingValue { get; }
        public ReactiveProperty<int> TurnCount { get; }
        public ReactiveProperty<PlayerMoveStyle> CurrentPlayerMoveStyle { get; }
        public ReactiveProperty<RunTurnResultDto> LatestResolution { get; }
        public ReactiveProperty<bool> IsPlayerAnimating { get; }

        public RunTurnStateStore()
        {
            PlayerPosition = new ReactiveProperty<GridPositionDto>(default).AddTo(_disposables);
            PlayerFacingValue = new ReactiveProperty<int>(0).AddTo(_disposables);
            TurnCount = new ReactiveProperty<int>(0).AddTo(_disposables);
            CurrentPlayerMoveStyle = new ReactiveProperty<PlayerMoveStyle>(PlayerMoveStyle.Normal).AddTo(_disposables);
            LatestResolution = new ReactiveProperty<RunTurnResultDto>(RunTurnResultDto.Empty).AddTo(_disposables);
            IsPlayerAnimating = new ReactiveProperty<bool>(false).AddTo(_disposables);
        }

        public void InitializeFromSnapshot(RunSnapshotDto snapshot)
        {
            PlayerPosition.Value = snapshot.PlayerPosition;
            PlayerFacingValue.Value = snapshot.PlayerFacingValue;
            TurnCount.Value = snapshot.TurnCount;
            CurrentPlayerMoveStyle.Value = PlayerMoveStyle.Normal;
            LatestResolution.Value = RunTurnResultDto.Empty;
        }

        public void ApplyCommandExecutionResult(RunCommandExecutionResultDto executionResult)
        {
            LatestResolution.Value = executionResult.TurnResult;
            ApplySnapshot(executionResult.Snapshot);
            CurrentPlayerMoveStyle.Value = PlayerMoveStyle.Normal;
        }

        public void ApplyDashExecutionResult(DashStepExecutionResultDto executionResult)
        {
            LatestResolution.Value = executionResult.DashStepResult.Resolution;
            ApplySnapshot(executionResult.Snapshot);
            CurrentPlayerMoveStyle.Value = executionResult.DashStepResult.ShouldContinue
                ? PlayerMoveStyle.Dash
                : PlayerMoveStyle.Normal;
        }

        public IDisposable AcquireAnimationLock()
        {
            _animationLockCount++;
            UpdateAnimationLock();
            return new AnimationLock(this);
        }

        public void SetMoveStyle(PlayerMoveStyle moveStyle)
        {
            CurrentPlayerMoveStyle.Value = moveStyle;
        }

        public void ResetMoveStyle()
        {
            CurrentPlayerMoveStyle.Value = PlayerMoveStyle.Normal;
        }

        private void ApplySnapshot(RunSnapshotDto snapshot)
        {
            PlayerPosition.Value = snapshot.PlayerPosition;
            PlayerFacingValue.Value = snapshot.PlayerFacingValue;
            TurnCount.Value = snapshot.TurnCount;
        }

        private void ReleaseAnimationLock()
        {
            if (_animationLockCount > 0)
            {
                _animationLockCount--;
            }

            UpdateAnimationLock();
        }

        private void UpdateAnimationLock()
        {
            IsPlayerAnimating.Value = _animationLockCount > 0;
        }

        private sealed class AnimationLock : IDisposable
        {
            private RunTurnStateStore _owner;

            public AnimationLock(RunTurnStateStore owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }

                _owner.ReleaseAnimationLock();
                _owner = null;
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}




