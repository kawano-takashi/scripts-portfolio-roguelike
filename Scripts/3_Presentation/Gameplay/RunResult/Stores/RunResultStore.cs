using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;
using Roguelike.Presentation.Gameplay.Hud.Types;
using Roguelike.Presentation.Gameplay.RunResult.Types;

namespace Roguelike.Presentation.Gameplay.RunResult.Stores
{
    /// <summary>
    /// ラン終了結果を保持するPresentationストアです。
    /// </summary>
    public sealed class RunResultStore : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        public ReactiveProperty<RunResultData> ResultData { get; }
        public ReactiveProperty<bool> HasResult { get; }

        public event Action<RunLifecycleEventDto> LifecycleEventPublished;

        public RunResultStore()
        {
            ResultData = new ReactiveProperty<RunResultData>(null).AddTo(_disposables);
            HasResult = new ReactiveProperty<bool>(false).AddTo(_disposables);
        }

        public void Clear()
        {
            ResultData.Value = null;
            HasResult.Value = false;
        }

        public void ApplyLifecycleEvents(IReadOnlyList<RunLifecycleEventDto> lifecycleEvents)
        {
            if (lifecycleEvents == null || lifecycleEvents.Count == 0)
            {
                return;
            }

            for (var i = 0; i < lifecycleEvents.Count; i++)
            {
                ApplyLifecycleEvent(lifecycleEvents[i]);
            }
        }

        private void ApplyLifecycleEvent(RunLifecycleEventDto lifecycleEvent)
        {
            if (lifecycleEvent.Kind == RunLifecycleEventKind.None)
            {
                return;
            }

            LifecycleEventPublished?.Invoke(lifecycleEvent);

            switch (lifecycleEvent.Kind)
            {
                case RunLifecycleEventKind.RunCleared:
                    ResultData.Value = new RunResultData(
                        isVictory: true,
                        finalFloor: lifecycleEvent.Floor,
                        totalTurns: lifecycleEvent.TotalTurns,
                        playerLevel: lifecycleEvent.PlayerLevel);
                    HasResult.Value = true;
                    return;
                case RunLifecycleEventKind.RunGameOver:
                    ResultData.Value = new RunResultData(
                        isVictory: false,
                        finalFloor: lifecycleEvent.Floor,
                        totalTurns: lifecycleEvent.TotalTurns,
                        playerLevel: lifecycleEvent.PlayerLevel);
                    HasResult.Value = true;
                    return;
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}




