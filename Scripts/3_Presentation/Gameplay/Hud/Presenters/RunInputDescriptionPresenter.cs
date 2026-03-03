using System;
using R3;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Hud.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Hud.Presenters
{
    /// <summary>
    /// 入力コンテキストに応じた表示文言を組み立てるPresenterです。
    /// </summary>
    public sealed class RunInputDescriptionPresenter : IRunInputDescriptionPresenter, IDisposable
    {
        private readonly InputContextManager _inputContextManager;
        private readonly IRunInputDescriptionView _view;
        private readonly CompositeDisposable _disposables = new();
        private bool _initialized;

        public RunInputDescriptionPresenter(
            InputContextManager inputContextManager,
            IRunInputDescriptionView view)
        {
            _inputContextManager = inputContextManager ?? throw new ArgumentNullException(nameof(inputContextManager));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _inputContextManager.CurrentContext
                .ObserveOnMainThread()
                .Subscribe(Apply)
                .AddTo(_disposables);

            Apply(_inputContextManager.CurrentContext.Value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void Apply(RunInputContext context)
        {
            var model = new RunInputDescriptionDisplayModel(ResolveText(context), isVisible: true);
            _view.Render(model);
        }

        private static string ResolveText(RunInputContext context)
        {
            return context switch
            {
                RunInputContext.Exploration => "WASD: いどう\nShift + WASD: ダッシュ\nJ + WASD: ナナメいどう\nI: 装備魔法プレビュー\nU: メニュー",
                RunInputContext.Inventory => "WASD: せんたく\nI: 決定\nO: とじる",
                RunInputContext.Guide => "O: とじる",
                RunInputContext.FloorConfirm => "WS: せんたく\nI: 決定\nO: とじる",
                RunInputContext.SpellPreview => "I: とじる",
                RunInputContext.Result => "I / クリック: つぎへ",
                RunInputContext.Pause => "ポーズ中",
                RunInputContext.Menu => "WS: せんたく\nI: 決定\nO: とじる",
                _ => "入力を受け付けていません"
            };
        }
    }
}




