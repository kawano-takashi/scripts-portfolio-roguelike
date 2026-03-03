using System;
using Roguelike.Application.Ports;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Repositories;

namespace Roguelike.Infrastructure.RunContext.Repositories
{
    /// <summary>
    /// メモリ上だけにランを保存するリポジトリです。
    /// ゲームを閉じたら消えます。
    /// </summary>
    public class InMemoryRoguelikeRunRepository : IRoguelikeRunRepository, IRunWriteStore
    {
        // 今のランを保持します。
        private RunSession _current;

        /// <summary>
        /// 現在ランを読み取り専用で参照します。
        /// </summary>
        public bool TryGetCurrent(out RunSession run)
        {
            run = _current;
            return run != null;
        }

        /// <summary>
        /// ランが保存されているかどうか。
        /// </summary>
        public bool HasRun => _current != null;

        /// <summary>
        /// ランを保存します。
        /// </summary>
        public void Save(RunSession session)
        {
            _current = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// ランを消します。
        /// </summary>
        public void Clear()
        {
            _current = null;
        }
    }
}
