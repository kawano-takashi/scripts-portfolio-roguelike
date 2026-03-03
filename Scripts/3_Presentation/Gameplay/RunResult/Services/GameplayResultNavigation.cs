using UnityEngine.SceneManagement;
using Roguelike.Presentation.Gameplay.FloorTransition.Presenters;
using Roguelike.Presentation.Gameplay.Guide.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Inventory.Presenters;
using Roguelike.Presentation.Gameplay.Menu.Presenters;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;

namespace Roguelike.Presentation.Gameplay.RunResult.Services
{
    /// <summary>
    /// 結果画面からタイトルへ戻る遷移を実装します。
    /// </summary>
    public sealed class GameplayResultNavigation : IGameplayResultNavigation
    {
        private readonly InputContextManager _inputContextManager;
        private readonly RunInputSettings _settings;
        private readonly RunResultStore _runResultStore;
        private string _targetSceneName = "DungeonScene";

        public GameplayResultNavigation(
            InputContextManager inputContextManager,
            RunInputSettings settings,
            RunResultStore runResultStore)
        {
            _inputContextManager = inputContextManager;
            _settings = settings;
            _runResultStore = runResultStore;

            if (_settings != null && !string.IsNullOrWhiteSpace(_settings.ResultTargetSceneName))
            {
                _targetSceneName = _settings.ResultTargetSceneName;
            }
        }

        public bool TryGoToTitle()
        {
            if (_inputContextManager != null &&
                _inputContextManager.CurrentContext.Value != RunInputContext.Result)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_targetSceneName))
            {
                return false;
            }

            _runResultStore?.Clear();
            SceneManager.LoadScene(_targetSceneName);
            return true;
        }
    }
}


