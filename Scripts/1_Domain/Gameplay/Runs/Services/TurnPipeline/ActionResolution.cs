namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    public readonly struct ActionResolution
    {
        public static readonly ActionResolution Unresolved = new ActionResolution(false, false);

        public bool IsResolved { get; }
        public bool ConsumesTurn { get; }

        private ActionResolution(bool isResolved, bool consumesTurn)
        {
            IsResolved = isResolved;
            ConsumesTurn = consumesTurn;
        }

        public static ActionResolution Resolved(bool consumesTurn)
            => new ActionResolution(true, consumesTurn);
    }
}
