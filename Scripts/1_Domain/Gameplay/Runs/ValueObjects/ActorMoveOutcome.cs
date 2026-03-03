using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.ValueObjects
{
    /// <summary>
    /// 1回の移動アクション解決結果を表します。
    /// </summary>
    public readonly struct ActorMoveOutcome
    {
        public static readonly ActorMoveOutcome None = default;

        public bool HasValue { get; }
        public bool Success { get; }
        public Position From { get; }
        public Position To { get; }

        public ActorMoveOutcome(bool success, Position from, Position to)
        {
            HasValue = true;
            Success = success;
            From = from;
            To = to;
        }
    }
}


