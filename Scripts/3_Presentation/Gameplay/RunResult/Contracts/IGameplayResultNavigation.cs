namespace Roguelike.Presentation.Gameplay.RunResult.Contracts
{
    /// <summary>
    /// 結果画面からの遷移要求を処理するナビゲーション契約です。
    /// </summary>
    public interface IGameplayResultNavigation
    {
        bool TryGoToTitle();
    }
}

