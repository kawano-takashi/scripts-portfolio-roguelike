using System;
using Roguelike.Application.Enums;

namespace Roguelike.Presentation.Gameplay.Hud.Types
{
    /// <summary>
    /// ログ表示に用いる投影データです。
    /// </summary>
    public sealed class RunLogRecord
    {
        public RunLogCategory Category { get; }
        public RunLogEventKind EventKind { get; }
        public int TurnNumber { get; }
        public Guid? SubjectActorId { get; }
        public Guid? TargetActorId { get; }
        public int? ItemTypeValue { get; }
        public ItemTypeDto? ItemType => ItemTypeValue.HasValue ? (ItemTypeDto?)ItemTypeValue.Value : null;
        public int? AttackKindValue { get; }
        public AttackKindDto? AttackKind => AttackKindValue.HasValue ? (AttackKindDto?)AttackKindValue.Value : null;
        public int? Amount { get; }
        public int? OldValue { get; }
        public int? NewValue { get; }
        public int? Floor { get; }
        public int? TotalTurns { get; }
        public int? PlayerLevel { get; }
        public string RawMessage { get; }

        public RunLogRecord(
            RunLogCategory category,
            RunLogEventKind eventKind,
            int turnNumber,
            Guid? subjectActorId = null,
            Guid? targetActorId = null,
            int? itemTypeValue = null,
            int? attackKindValue = null,
            int? amount = null,
            int? oldValue = null,
            int? newValue = null,
            int? floor = null,
            int? totalTurns = null,
            int? playerLevel = null,
            string rawMessage = null)
        {
            Category = category;
            EventKind = eventKind;
            TurnNumber = turnNumber;
            SubjectActorId = subjectActorId;
            TargetActorId = targetActorId;
            ItemTypeValue = itemTypeValue;
            AttackKindValue = attackKindValue;
            Amount = amount;
            OldValue = oldValue;
            NewValue = newValue;
            Floor = floor;
            TotalTurns = totalTurns;
            PlayerLevel = playerLevel;
            RawMessage = rawMessage;
        }
    }
}




