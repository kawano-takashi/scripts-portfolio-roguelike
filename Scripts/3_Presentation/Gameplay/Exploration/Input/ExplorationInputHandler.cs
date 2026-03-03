using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Application.Commands;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Menu.Presenters;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Types;
using static Constants;

namespace Roguelike.Presentation.Gameplay.Exploration.Input
{
    /// <summary>
    /// 探索中の入力を担当します。
    /// </summary>
    public sealed class ExplorationInputHandler : IInputContextHandler, IDisposable
    {
        [Inject] private readonly InputContextManager _inputContextManager;
        [Inject] private readonly RunInputSettings _settings;
        [Inject] private readonly RunActionCommandHandler _runActionCommandHandler;
        [Inject] private readonly ExecuteDashStepCommandHandler _dashStepCommandHandler;
        [Inject] private readonly RunTurnStateStore _runTurnStateStore;
        [Inject] private readonly RunResultStore _runResultStore;
        [Inject] private readonly CanUseCapabilityQueryHandler _canUseCapabilityQueryHandler;
        [Inject] private readonly MainMenuPresenter _mainMenuController;
        [Inject] private readonly SpellPreviewPresenter _spellPreviewController;
        [Inject] private readonly IUiSoundPlayer _uiSoundPlayer;

        private float _moveInterval = INPUT_REPEAT_INTERVAL;
        private float _dashMoveInterval = 0.08f;
        private bool _useRoguelikeRun = true;

        private CancellationTokenSource _moveCts;
        private Vector2 _currentMoveInput = Vector2.zero;
        private DirectionDto _dashDirection = DirectionDto.Down;
        private bool _hasDashDirection;
        private bool _isEnabled;

        public bool IsActiveFor(RunInputContext context)
        {
            return context == RunInputContext.Exploration;
        }

        public void Init()
        {
            if (_inputContextManager == null)
            {
                Debug.LogError("InputContextManager is not injected!");
                return;
            }

            ApplySettings();
            Enable();
        }

        public void Enable()
        {
            if (_isEnabled)
            {
                return;
            }

            SubscribeMovementInput();
            SubscribePickupInput();
            SubscribeAttackInput();
            SubscribeConfirmInput();
            SubscribeMenuInput();
            _isEnabled = true;
        }

        public void Disable()
        {
            StopMoveLoop();

            if (!_isEnabled)
            {
                return;
            }

            var moveAction = _inputContextManager?.InputActions?.Player.Move;
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }

            var pickupAction = _inputContextManager?.InputActions?.Player.Pickup;
            if (pickupAction != null)
            {
                pickupAction.performed -= OnPickupPerformed;
            }

            var attackAction = _inputContextManager?.InputActions?.Player.Attack;
            if (attackAction != null)
            {
                attackAction.performed -= OnAttackPerformed;
            }

            var confirmAction = _inputContextManager?.InputActions?.Player.Confirm;
            if (confirmAction != null)
            {
                confirmAction.performed -= OnConfirmPerformed;
            }

            var openMenuAction = _inputContextManager?.InputActions?.Player.OpenMenu;
            if (openMenuAction != null)
            {
                openMenuAction.performed -= OnOpenMenuPerformed;
            }

            _isEnabled = false;
        }

        private void SubscribeMovementInput()
        {
            var moveAction = _inputContextManager?.InputActions?.Player.Move;
            if (moveAction == null)
            {
                return;
            }

            moveAction.performed -= OnMovePerformed;
            moveAction.performed += OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            moveAction.canceled += OnMoveCanceled;
        }

        private void SubscribePickupInput()
        {
            var pickupAction = _inputContextManager?.InputActions?.Player.Pickup;
            if (pickupAction == null)
            {
                return;
            }

            pickupAction.performed -= OnPickupPerformed;
            pickupAction.performed += OnPickupPerformed;
        }

        private void SubscribeAttackInput()
        {
            var attackAction = _inputContextManager?.InputActions?.Player.Attack;
            if (attackAction == null)
            {
                return;
            }

            attackAction.performed -= OnAttackPerformed;
            attackAction.performed += OnAttackPerformed;
        }

        private void SubscribeConfirmInput()
        {
            var confirmAction = _inputContextManager?.InputActions?.Player.Confirm;
            if (confirmAction == null)
            {
                return;
            }

            confirmAction.performed -= OnConfirmPerformed;
            confirmAction.performed += OnConfirmPerformed;
        }

        private void SubscribeMenuInput()
        {
            var openMenuAction = _inputContextManager?.InputActions?.Player.OpenMenu;
            if (openMenuAction == null)
            {
                return;
            }

            openMenuAction.performed -= OnOpenMenuPerformed;
            openMenuAction.performed += OnOpenMenuPerformed;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!IsCurrentContextExploration())
            {
                StopMoveLoop();
                return;
            }

            _currentMoveInput = context.ReadValue<Vector2>();

            if (_currentMoveInput.magnitude <= INPUT_DEAD_ZONE)
            {
                StopMoveLoop();
                return;
            }

            ProcessMovementDirection(_currentMoveInput, shouldResetDashDirection: true);

            if (_currentMoveInput.magnitude <= INPUT_DEAD_ZONE)
            {
                return;
            }

            StartMoveLoopIfNeeded();
        }

        private void OnMoveCanceled(InputAction.CallbackContext _)
        {
            StopMoveLoop();
        }

        private void OnPickupPerformed(InputAction.CallbackContext _)
        {
            if (!UseRoguelikeInput())
            {
                return;
            }

            var executionResult = _runActionCommandHandler.Handle(new PickupItemRunActionCommand());
            ExecuteCommand(executionResult);
            TryPlayItemPickupSound(executionResult);
        }

        private void OnAttackPerformed(InputAction.CallbackContext _)
        {
            if (!UseRoguelikeInput())
            {
                return;
            }

            // 攻撃入力は装備中の魔法書を発射する。
            ExecuteCommand(_runActionCommandHandler.Handle(new CastEquippedSpellbookRunActionCommand()));
        }

        private void OnConfirmPerformed(InputAction.CallbackContext _)
        {
            if (!UseRoguelikeInput())
            {
                return;
            }

            _spellPreviewController.OpenFromEquippedSpellbook();
        }

        private void OnOpenMenuPerformed(InputAction.CallbackContext _)
        {
            if (!IsCurrentContextExploration())
            {
                return;
            }

            // UI状態変更はController経由に統一。
            _mainMenuController?.OpenMenu();
        }

        private void StartMoveLoopIfNeeded()
        {
            if (_moveCts != null && !_moveCts.IsCancellationRequested)
            {
                return;
            }

            _moveCts?.Cancel();
            _moveCts?.Dispose();

            _moveCts = new CancellationTokenSource();
            MoveRepeatLoop(_moveCts.Token).Forget();
        }

        private async UniTaskVoid MoveRepeatLoop(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(ResolveCurrentMoveIntervalSeconds()),
                        cancellationToken: token);

                    if (!IsCurrentContextExploration())
                    {
                        StopMoveLoop();
                        return;
                    }

                    if (_currentMoveInput.magnitude <= INPUT_DEAD_ZONE)
                    {
                        StopMoveLoop();
                        return;
                    }

                    ProcessMovementDirection(_currentMoveInput, shouldResetDashDirection: false);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

        private void StopMoveLoop()
        {
            _moveCts?.Cancel();
            _moveCts?.Dispose();
            _moveCts = null;

            _currentMoveInput = Vector2.zero;
            _hasDashDirection = false;
        }

        private void ProcessMovementDirection(Vector2 moveInput, bool shouldResetDashDirection)
        {
            if (!UseRoguelikeInput())
            {
                return;
            }

            ProcessRoguelikeMovement(moveInput, shouldResetDashDirection);
        }

        private void ProcessRoguelikeMovement(Vector2 moveInput, bool shouldResetDashDirection)
        {
            if (!TryGetRoguelikeDirection(moveInput, out var direction))
            {
                return;
            }

            if (IsTurnDirectionPressed())
            {
                _hasDashDirection = false;

                if (_runTurnStateStore.PlayerFacingValue.Value == (int)direction)
                {
                    return;
                }

                _runTurnStateStore.ResetMoveStyle();
                ExecuteCommand(_runActionCommandHandler.Handle(new ChangeFacingRunActionCommand(ToDirectionDto(direction))));
                return;
            }

            if (IsSprintPressed())
            {
                _runTurnStateStore.SetMoveStyle(PlayerMoveStyle.Dash);
                if (shouldResetDashDirection || !_hasDashDirection)
                {
                    _dashDirection = direction;
                    _hasDashDirection = true;
                }

                var dashExecutionResult = ExecuteDashStep(_dashDirection);
                var dashResult = dashExecutionResult.DashStepResult;
                if (dashResult.ShouldContinue)
                {
                    _dashDirection = (DirectionDto)dashResult.NextDirectionValue;
                    _hasDashDirection = true;
                    return;
                }

                StopMoveLoop();
                return;
            }

            _hasDashDirection = false;
            _runTurnStateStore.ResetMoveStyle();
            ExecuteCommand(_runActionCommandHandler.Handle(new MoveRunActionCommand(ToDirectionDto(direction))));
        }

        private bool TryGetRoguelikeDirection(Vector2 moveInput, out DirectionDto direction)
        {
            direction = DirectionDto.Down;

            if (moveInput.magnitude <= INPUT_DEAD_ZONE)
            {
                return false;
            }

            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                direction = moveInput.x > 0 ? DirectionDto.Right : DirectionDto.Left;
            }
            else
            {
                direction = moveInput.y > 0 ? DirectionDto.Down : DirectionDto.Up;
            }

            if (IsRotationKeyPressed())
            {
                direction = RotateClockwise45(direction);
            }

            return true;
        }

        private bool IsRotationKeyPressed()
        {
            var rotateAction = _inputContextManager?.InputActions?.Player.RotateMove;
            return rotateAction != null && rotateAction.IsPressed();
        }

        private bool IsTurnDirectionPressed()
        {
            var turnDirectionAction = _inputContextManager?.InputActions?.Player.TurnDirection;
            return turnDirectionAction != null && turnDirectionAction.IsPressed();
        }

        private bool IsSprintPressed()
        {
            var sprintAction = _inputContextManager?.InputActions?.Player.Sprint;
            return sprintAction != null && sprintAction.IsPressed();
        }

        private static DirectionDto RotateClockwise45(DirectionDto direction)
        {
            return direction switch
            {
                DirectionDto.Up => DirectionDto.UpLeft,
                DirectionDto.Right => DirectionDto.UpRight,
                DirectionDto.Down => DirectionDto.DownRight,
                DirectionDto.Left => DirectionDto.DownLeft,
                _ => direction
            };
        }

        private bool UseRoguelikeInput()
        {
            if (!IsCurrentContextExploration())
            {
                return false;
            }

            if (_runTurnStateStore?.IsPlayerAnimating?.CurrentValue == true)
            {
                return false;
            }

            var explorationAccess = _canUseCapabilityQueryHandler?.Handle(
                    new GetRunAccessCapabilityQuery(RunAccessCapability.ExplorationInput))
                ?? Result<bool>.Failure("Run access query use case is unavailable.");

            return _useRoguelikeRun &&
                _runActionCommandHandler != null &&
                _dashStepCommandHandler != null &&
                _runTurnStateStore != null &&
                _canUseCapabilityQueryHandler != null &&
                explorationAccess.IsSuccess &&
                explorationAccess.Value;
        }

        private bool IsCurrentContextExploration()
        {
            return _inputContextManager != null
                && _inputContextManager.CurrentContext.Value == RunInputContext.Exploration;
        }

        public void Dispose()
        {
            Disable();
        }

        private void ExecuteCommand(Result<RunCommandExecutionResultDto> executionResult)
        {
            if (_runTurnStateStore == null || !executionResult.IsSuccess)
            {
                return;
            }

            _runResultStore?.ApplyLifecycleEvents(executionResult.Value.LifecycleEvents);
            _runTurnStateStore.ApplyCommandExecutionResult(executionResult.Value);
        }

        private void TryPlayItemPickupSound(Result<RunCommandExecutionResultDto> executionResult)
        {
            if (_uiSoundPlayer == null || !executionResult.IsSuccess)
            {
                return;
            }

            var events = executionResult.Value.TurnResult.Events;
            if (events == null || events.Count == 0)
            {
                return;
            }

            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] is not ItemAddedToInventoryEventDto)
                {
                    continue;
                }

                _uiSoundPlayer.Play(UiSoundCue.ItemPickup);
                return;
            }
        }

        private DashStepExecutionResultDto ExecuteDashStep(DirectionDto requestedDirection)
        {
            if (_dashStepCommandHandler == null || _runTurnStateStore == null)
            {
                return DashStepExecutionResultDto.Empty;
            }

            var executionResult = _dashStepCommandHandler.Handle(new DashStepCommand(ToDirectionDto(requestedDirection)));
            if (!executionResult.IsSuccess)
            {
                return DashStepExecutionResultDto.Empty;
            }

            _runResultStore?.ApplyLifecycleEvents(executionResult.Value.LifecycleEvents);
            _runTurnStateStore.ApplyDashExecutionResult(executionResult.Value);
            return executionResult.Value;
        }

        private float ResolveCurrentMoveIntervalSeconds()
        {
            var interval = IsSprintPressed() ? _dashMoveInterval : _moveInterval;
            return Mathf.Max(0.01f, interval);
        }

        private void ApplySettings()
        {
            if (_settings == null)
            {
                return;
            }

            _moveInterval = _settings.MoveInterval;
            _dashMoveInterval = _settings.DashMoveInterval;
            _useRoguelikeRun = _settings.UseRoguelikeRun;
        }

        private static DirectionDto ToDirectionDto(DirectionDto direction)
        {
            return direction;
        }
    }
}









