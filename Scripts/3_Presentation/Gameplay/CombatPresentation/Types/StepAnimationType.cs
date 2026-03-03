namespace Roguelike.Presentation.Gameplay.CombatPresentation.Types
{
    /// <summary>
    /// 1ステップで再生するアニメーション種別です。
    /// </summary>
    public enum StepAnimationType
    {
        /// <summary>
        /// アニメーション再生を行わないステップです（ログのみなど）。
        /// </summary>
        None,
        /// <summary>
        /// 通常攻撃アニメーションを再生するステップです。
        /// </summary>
        Attack,
        /// <summary>
        /// スペルアニメーションを再生するステップです。
        /// </summary>
        Spell
    }
}



