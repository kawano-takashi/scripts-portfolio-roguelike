using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Runs.Repositories
{
    /// <summary>
    /// 現在のランを保存・取得するための約束です。
    /// </summary>
    public interface IRoguelikeRunRepository
    {
        /// <summary>
        /// 現在ランを読み取り専用で参照します。
        /// </summary>
        bool TryGetCurrent(out RunSession run);
        /// <summary>
        /// ランが存在するかどうか。
        /// </summary>
        bool HasRun { get; }
        /// <summary>
        /// 現在ランを保存（置換）します。
        /// </summary>
        void Save(RunSession session);
        /// <summary>
        /// ランを消します。
        /// </summary>
        void Clear();
    }
}
