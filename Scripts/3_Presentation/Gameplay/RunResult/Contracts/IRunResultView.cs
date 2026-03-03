using System;
using Roguelike.Presentation.Gameplay.RunResult.DisplayModels;

namespace Roguelike.Presentation.Gameplay.RunResult.Contracts
{
    /// <summary>
    /// ラン結果画面の受動ビュー契約です。
    /// </summary>
    public interface IRunResultView
    {
        event Action GoToTitleRequested;

        void Render(RunResultDisplayModel model);
        void Hide();
    }
}
