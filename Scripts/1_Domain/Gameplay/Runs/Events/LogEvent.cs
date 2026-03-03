using System;
using System.Collections.Generic;

namespace Roguelike.Domain.Gameplay.Runs.Events
{
    /// <summary>
    /// ログに出す出来事を運びます。
    /// 文言を直接運ぶことも、意味コードとパラメータを運ぶこともできます。
    /// </summary>
    public sealed class LogEvent : IRoguelikeEvent
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyParameters =
            new Dictionary<string, string>();

        /// <summary>
        /// ログの意味コード。
        /// </summary>
        public RunLogCode Code { get; }

        /// <summary>
        /// 表示したい文章。
        /// 互換のため保持します。空の場合はCode/Parametersから生成します。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 文言生成に使う補助パラメータです。
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>
        /// 既存互換のため、文字列メッセージを直接指定して生成します。
        /// </summary>
        public LogEvent(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("ログメッセージは空にできません。", nameof(message));
            }

            Code = RunLogCode.None;
            Message = message;
            Parameters = EmptyParameters;
        }

        /// <summary>
        /// 意味コードとパラメータで生成します。
        /// 必要なら fallbackMessage を渡して互換表示を維持できます。
        /// </summary>
        public LogEvent(
            RunLogCode code,
            IReadOnlyDictionary<string, string> parameters = null,
            string fallbackMessage = null)
        {
            if (code == RunLogCode.None && string.IsNullOrWhiteSpace(fallbackMessage))
            {
                throw new ArgumentException("ログコードまたはフォールバックメッセージのどちらかを指定してください。", nameof(code));
            }

            Code = code;
            Parameters = parameters ?? EmptyParameters;
            Message = fallbackMessage ?? string.Empty;
        }
    }
}
