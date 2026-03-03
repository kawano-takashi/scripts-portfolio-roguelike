using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.Hud.Types;

namespace Roguelike.Presentation.Gameplay.Hud.Formatting
{
    /// <summary>
    /// ApplicationイベントDTOをログ表示レコードへ投影します。
    /// </summary>
    public sealed class RunLogProjectionPolicy
    {
        public IReadOnlyList<RunLogRecord> Project(IReadOnlyList<IRunEventDto> events)
        {
            if (events == null || events.Count == 0)
            {
                return Array.Empty<RunLogRecord>();
            }

            var records = new List<RunLogRecord>(events.Count);
            for (var i = 0; i < events.Count; i++)
            {
                if (TryProject(events[i], out var record))
                {
                    records.Add(record);
                }
            }

            return records;
        }

        public bool TryProject(IRunEventDto evt, out RunLogRecord record)
        {
            switch (evt)
            {
                case AttackDeclaredEventDto attack:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.AttackDeclared,
                        attack.TurnNumber,
                        subjectActorId: attack.AttackerActorId,
                        targetActorId: attack.TargetActorId,
                        attackKindValue: attack.AttackKindValue);
                    return true;

                case SpellCastEventDto spell:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.SpellCast,
                        spell.TurnNumber,
                        subjectActorId: spell.CasterActorId,
                        targetActorId: spell.TargetActorId,
                        itemTypeValue: spell.ItemTypeValue);
                    return true;

                case ActorDamagedEventDto damage:
                    record = new RunLogRecord(
                        RunLogCategory.Damage,
                        RunLogEventKind.ActorDamaged,
                        damage.TurnNumber,
                        subjectActorId: damage.SourceActorId,
                        targetActorId: damage.TargetActorId,
                        amount: damage.Amount);
                    return true;

                case ActorDiedEventDto died:
                    record = new RunLogRecord(
                        RunLogCategory.Defeat,
                        RunLogEventKind.ActorDied,
                        died.TurnNumber,
                        subjectActorId: died.ActorId);
                    return true;

                case ActorHealedEventDto healed:
                    record = new RunLogRecord(
                        RunLogCategory.Heal,
                        RunLogEventKind.ActorHealed,
                        healed.TurnNumber,
                        subjectActorId: healed.ActorId,
                        amount: healed.Amount);
                    return true;

                case ExperienceGainedEventDto exp:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.ExperienceGained,
                        exp.TurnNumber,
                        subjectActorId: exp.ActorId,
                        amount: exp.Amount);
                    return true;

                case LevelUpEventDto levelUp:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.LevelUp,
                        levelUp.TurnNumber,
                        subjectActorId: levelUp.ActorId,
                        oldValue: levelUp.OldLevel,
                        newValue: levelUp.NewLevel);
                    return true;

                case ItemAddedToInventoryEventDto added:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.ItemAddedToInventory,
                        added.TurnNumber,
                        subjectActorId: added.ActorId,
                        itemTypeValue: added.ItemTypeValue);
                    return true;

                case ItemUsedEventDto used:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.ItemUsed,
                        used.TurnNumber,
                        subjectActorId: used.ActorId,
                        itemTypeValue: used.ItemTypeValue);
                    return true;

                case ItemDroppedEventDto dropped:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.ItemDropped,
                        dropped.TurnNumber,
                        subjectActorId: dropped.ActorId,
                        itemTypeValue: dropped.ItemTypeValue);
                    return true;

                case ItemEquippedEventDto equipped:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.ItemEquipped,
                        equipped.TurnNumber,
                        subjectActorId: equipped.ActorId,
                        itemTypeValue: equipped.ItemTypeValue);
                    return true;

                case ItemUnequippedEventDto unequipped:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.ItemUnequipped,
                        unequipped.TurnNumber,
                        subjectActorId: unequipped.ActorId,
                        itemTypeValue: unequipped.ItemTypeValue);
                    return true;

                case MessageEventDto message:
                    record = new RunLogRecord(
                        RunLogCategory.System,
                        RunLogEventKind.Message,
                        message.TurnNumber,
                        rawMessage: message.Message);
                    return true;

                default:
                    record = null;
                    return false;
            }
        }

        public bool TryProjectLifecycle(RunLifecycleEventDto evt, out RunLogRecord record)
        {
            record = evt.Kind switch
            {
                RunLifecycleEventKind.RunCleared => new RunLogRecord(
                    RunLogCategory.System,
                    RunLogEventKind.RunCleared,
                    turnNumber: 0,
                    floor: evt.Floor,
                    totalTurns: evt.TotalTurns,
                    playerLevel: evt.PlayerLevel),

                RunLifecycleEventKind.RunGameOver => new RunLogRecord(
                    RunLogCategory.System,
                    RunLogEventKind.RunGameOver,
                    turnNumber: 0,
                    floor: evt.Floor,
                    totalTurns: evt.TotalTurns,
                    playerLevel: evt.PlayerLevel),

                _ => null
            };

            return record != null;
        }
    }
}



