using System;
using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 1回の行動実行結果DTOです。
    /// </summary>
    public readonly struct RunTurnResultDto
    {
        private static readonly IReadOnlyList<IRunEventDto> EmptyEvents = Array.Empty<IRunEventDto>();

        public static RunTurnResultDto Empty => new RunTurnResultDto(
            turnConsumed: false,
            turnNumber: 0,
            actionResolved: false,
            events: EmptyEvents);

        public bool TurnConsumed { get; }
        public int TurnNumber { get; }
        public bool ActionResolved { get; }
        public IReadOnlyList<IRunEventDto> Events { get; }

        public RunTurnResultDto(
            bool turnConsumed,
            int turnNumber,
            bool actionResolved,
            IReadOnlyList<IRunEventDto> events)
        {
            TurnConsumed = turnConsumed;
            TurnNumber = turnNumber;
            ActionResolved = actionResolved;
            Events = events ?? EmptyEvents;
        }
    }
}
