using System;
using Roguelike.Domain.Gameplay.Runs.Events;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Events
{
    public sealed class LogEventTests
    {
        [Fact]
        public void Constructor_WithWhitespaceMessage_ThrowsArgumentExceptionWithJapaneseMessage()
        {
            var exception = Assert.Throws<ArgumentException>(() => new LogEvent(" "));

            Assert.StartsWith("ログメッセージは空にできません。", exception.Message);
            Assert.Equal("message", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNoneCodeAndNoFallback_ThrowsArgumentExceptionWithJapaneseMessage()
        {
            var exception = Assert.Throws<ArgumentException>(() => new LogEvent(RunLogCode.None));

            Assert.StartsWith("ログコードまたはフォールバックメッセージのどちらかを指定してください。", exception.Message);
            Assert.Equal("code", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithCode_SetsCodeAndEmptyMessage()
        {
            var logEvent = new LogEvent(RunLogCode.ActorAsleep);

            Assert.Equal(RunLogCode.ActorAsleep, logEvent.Code);
            Assert.Equal(string.Empty, logEvent.Message);
            Assert.NotNull(logEvent.Parameters);
            Assert.Empty(logEvent.Parameters);
        }

        [Fact]
        public void Constructor_WithFallbackMessage_AllowsNoneCode()
        {
            var logEvent = new LogEvent(RunLogCode.None, fallbackMessage: "テストメッセージ");

            Assert.Equal(RunLogCode.None, logEvent.Code);
            Assert.Equal("テストメッセージ", logEvent.Message);
            Assert.NotNull(logEvent.Parameters);
            Assert.Empty(logEvent.Parameters);
        }
    }
}
