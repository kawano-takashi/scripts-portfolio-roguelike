using System;
using R3;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.Hud.DisplayModels;
using Roguelike.Presentation.Gameplay.Map.DisplayModels;
using Roguelike.Presentation.Gameplay.RunResult.DisplayModels;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;
using Roguelike.Presentation.Gameplay.Hud.Types;
using Roguelike.Presentation.Gameplay.RunResult.Types;
using Roguelike.Presentation.Gameplay.RunResult.Stores;

namespace Roguelike.Presentation.Gameplay.RunResult.Presenters
{
    /// <summary>
    /// ラン結果ストアと結果ビューを仲介するPresenterです。
    /// </summary>
    public sealed class RunResultPresenter : IRunResultPresenter, IDisposable
    {
        private readonly RunResultStore _runResultStore;
        private readonly IGameplayResultNavigation _resultNavigation;
        private readonly IRunResultView _runResultView;
        private readonly CompositeDisposable _disposables = new();
        private bool _initialized;

        public RunResultPresenter(
            RunResultStore runResultStore,
            IGameplayResultNavigation resultNavigation,
            IRunResultView runResultView)
        {
            _runResultStore = runResultStore ?? throw new ArgumentNullException(nameof(runResultStore));
            _resultNavigation = resultNavigation ?? throw new ArgumentNullException(nameof(resultNavigation));
            _runResultView = runResultView ?? throw new ArgumentNullException(nameof(runResultView));
        }

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _runResultView.GoToTitleRequested += OnGoToTitleRequested;

            _runResultStore.ResultData
                .Subscribe(OnResultDataChanged)
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _runResultView.GoToTitleRequested -= OnGoToTitleRequested;
            _disposables.Dispose();
        }

        private void OnResultDataChanged(RunResultData resultData)
        {
            if (resultData == null)
            {
                _runResultView.Hide();
                return;
            }

            var model = new RunResultDisplayModel(
                resultData.IsVictory,
                resultData.FinalFloor,
                resultData.TotalTurns,
                resultData.PlayerLevel);
            _runResultView.Render(model);
        }

        private void OnGoToTitleRequested()
        {
            _resultNavigation.TryGoToTitle();
        }
    }
}





