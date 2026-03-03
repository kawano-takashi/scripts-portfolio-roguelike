using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.UseCases;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using UnityEngine;
using VContainer;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Hud.Types;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;
using Roguelike.Presentation.Gameplay.CombatPresentation.Views;
using Roguelike.Presentation.Gameplay.CombatPresentation.Policies;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Sequencing
{
    /// <summary>
    /// ターン内イベントを順番に処理し、演出とログを同期させます。
    /// </summary>
    public sealed class TurnEventSequencer : MonoBehaviour
    {
        [Header("Animation Timing")]
        [SerializeField, Range(0f, 1.5f)]
        private float _postAnimationDelaySeconds = 0.25f;

        [Header("Log Display Timing")]
        [SerializeField, Range(0f, 1.5f)]
        private float _logDisplayIntervalSeconds = 0.75f;

        [Inject] private RunTurnStateStore _turnStateStore;
        [Inject] private GetActorPositionQueryHandler _getActorPositionQueryHandler;
        [Inject] private RunLogStore _runLogStore;
        [Inject] private TurnEventSequencingPolicy _turnEventSequencingPolicy;
        [Inject] private AttackAnimationView _attackAnimationView;
        [Inject] private SpellAnimationView _spellAnimationView;
        [Inject] private DamagePopupView _damagePopupView;
        [Inject] private IUiSoundPlayer _uiSoundPlayer;

        private readonly CompositeDisposable _disposables = new();
        private readonly Queue<RunTurnResultDto> _pendingResolutions = new();
        private bool _isProcessing;

        public void Init()
        {
            if (_turnStateStore == null)
            {
                Debug.LogWarning("RunTurnStateStore is not injected.");
                return;
            }

            if (_runLogStore == null)
            {
                Debug.LogWarning("RunLogStore is not injected.");
                return;
            }

            if (_turnEventSequencingPolicy == null)
            {
                Debug.LogWarning("TurnEventSequencingPolicy is not injected.");
                return;
            }

            _turnStateStore.LatestResolution
                .Subscribe(EnqueueResolution)
                .AddTo(_disposables);
        }

        private void EnqueueResolution(RunTurnResultDto resolution)
        {
            if (resolution.Events == null || resolution.Events.Count == 0)
            {
                return;
            }

            _pendingResolutions.Enqueue(resolution);
            if (!_isProcessing)
            {
                ProcessQueue(this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private async UniTaskVoid ProcessQueue(CancellationToken token)
        {
            _isProcessing = true;
            try
            {
                while (_pendingResolutions.Count > 0)
                {
                    var resolution = _pendingResolutions.Dequeue();
                    await ProcessResolution(resolution, token);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async UniTask ProcessResolution(RunTurnResultDto resolution, CancellationToken token)
        {
            _runLogStore.RefreshActorNameCacheFromReadModel();

            var plan = _turnEventSequencingPolicy.Build(resolution);
            var steps = plan.Steps;
            if (steps == null || steps.Count == 0)
            {
                return;
            }

            IDisposable animationLock = null;
            if (NeedsAnimationLock(steps))
            {
                animationLock = _turnStateStore?.AcquireAnimationLock();
            }

            try
            {
                for (var i = 0; i < steps.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var isFirst = i == 0 && steps.Count != 1;
                    await ProcessStep(steps[i], token, isFirst);
                }
            }
            finally
            {
                animationLock?.Dispose();
            }
        }

        private bool NeedsAnimationLock(IReadOnlyList<TurnPresentationStep> steps)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step == null)
                {
                    continue;
                }

                if (step.AnimationType == StepAnimationType.Attack && _attackAnimationView != null)
                {
                    return true;
                }

                if (step.AnimationType == StepAnimationType.Spell && _spellAnimationView != null)
                {
                    return true;
                }
            }

            return false;
        }

        private async UniTask ProcessStep(TurnPresentationStep step, CancellationToken token, bool isFirst = false)
        {
            if (step == null)
            {
                return;
            }

            PlayStepSound(step);

            UniTask animationTask = default;
            var hasAnimation = false;

            if (step.AnimationType == StepAnimationType.Attack)
            {
                animationTask = PlayAttackEffect(step.AttackRequest, token);
                hasAnimation = true;
                ShowDamagePopups(step.DamageEvents, step.DamageFallbackPosition);
            }
            else if (step.AnimationType == StepAnimationType.Spell)
            {
                animationTask = PlaySpellEffect(step.SpellRequest, token);
                hasAnimation = true;
                ShowDamagePopups(step.DamageEvents, step.DamageFallbackPosition);
            }

            var logTask = DisplayLogsWithInterval(step.LogEntries, token);

            if (hasAnimation)
            {
                await UniTask.WhenAll(animationTask, logTask);
                await WaitAfterAnimation(token);
            }
            else
            {
                if (!isFirst)
                {
                    await UniTask.Yield();
                }
                else
                {
                    await logTask;
                }
            }
        }

        private void PlayStepSound(TurnPresentationStep step)
        {
            if (_uiSoundPlayer == null || step == null)
            {
                return;
            }

            var cue = step.SoundCue;
            if (!cue.HasValue)
            {
                return;
            }

            _uiSoundPlayer.Play(cue.Value);
        }

        private async UniTask DisplayLogsWithInterval(
            IReadOnlyList<RunLogRecord> entries,
            CancellationToken token)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                _runLogStore.PublishRecord(entries[i]);

                if (_logDisplayIntervalSeconds > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_logDisplayIntervalSeconds), cancellationToken: token);
                }
            }
        }

        private async UniTask PlayAttackEffect(AttackAnimationRequest request, CancellationToken token)
        {
            if (_attackAnimationView == null || request == null)
            {
                return;
            }

            await _attackAnimationView.PlayAttackAsync(request, token);
        }

        private async UniTask PlaySpellEffect(SpellAnimationRequest? request, CancellationToken token)
        {
            if (_spellAnimationView == null || !request.HasValue)
            {
                return;
            }

            await _spellAnimationView.PlaySpellAsync(request.Value, token);
        }

        private async UniTask WaitAfterAnimation(CancellationToken token)
        {
            if (_postAnimationDelaySeconds <= 0f)
            {
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_postAnimationDelaySeconds), cancellationToken: token);
        }

        private void ShowDamagePopups(IReadOnlyList<ActorDamagedEventDto> damageEvents, GridPositionDto fallbackPosition)
        {
            if (_damagePopupView == null || damageEvents == null || damageEvents.Count == 0)
            {
                return;
            }

            for (var i = 0; i < damageEvents.Count; i++)
            {
                var evt = damageEvents[i];
                if (evt.Amount <= 0)
                {
                    continue;
                }

                var actorId = evt.TargetActorId;
                var position = GetActorPosition(actorId, fallbackPosition);
                _damagePopupView.ShowDamage(actorId, position, evt.Amount);
            }
        }

        private GridPositionDto GetActorPosition(Guid actorId, GridPositionDto fallbackPosition)
        {
            if (_getActorPositionQueryHandler == null)
            {
                return fallbackPosition;
            }

            var result = _getActorPositionQueryHandler.Handle(new GetActorPositionQuery(actorId));
            if (!result.IsSuccess)
            {
                return fallbackPosition;
            }

            return result.Value;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}







