using System;

namespace Roguelike.Presentation.Gameplay.Hud.Types
{
    /// <summary>
    /// 表示用ログの分類です。
    /// </summary>
    public enum RunLogViewType
    {
        Damage,
        Defeat,
        Heal,
        System
    }

    /// <summary>
    /// 表示用ログの1行データです。
    /// </summary>
    public sealed class RunLogViewEntry
    {
        public RunLogViewType Type { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public RunLogViewEntry(RunLogViewType type, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("message cannot be empty.", nameof(message));
            }

            Type = type;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}



