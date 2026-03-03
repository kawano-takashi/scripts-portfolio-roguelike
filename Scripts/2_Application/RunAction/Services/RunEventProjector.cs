using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// DomainイベントをApplication契約DTOへ投影します。
    /// </summary>
    public sealed class RunEventProjector
    {
        private static readonly IReadOnlyDictionary<Type, Func<IRoguelikeEvent, int, IRunEventDto>> EventMappers =
            new Dictionary<Type, Func<IRoguelikeEvent, int, IRunEventDto>>
            {
                [typeof(AttackDeclaredEvent)] = (evt, turn) =>
                {
                    var attack = (AttackDeclaredEvent)evt;
                    return new AttackDeclaredEventDto(
                        turn,
                        attack.AttackerId.Value,
                        attack.TargetId?.Value,
                        (int)attack.Kind,
                        (int)attack.AttackerFacing,
                        attack.Range,
                        ToPositionDto(attack.AttackerPosition),
                        ToPositionDto(attack.TargetPosition));
                },
                [typeof(SpellCastEvent)] = (evt, turn) =>
                {
                    var spell = (SpellCastEvent)evt;
                    return new SpellCastEventDto(
                        turn,
                        spell.CasterId.Value,
                        spell.TargetId?.Value,
                        (int)spell.Spell,
                        (int)spell.CasterFacing,
                        spell.Range,
                        spell.IsEquippedSpellCast,
                        ToPositionDto(spell.CasterPosition),
                        ToPositionDto(spell.TargetPosition));
                },
                [typeof(AttackPerformedEvent)] = (evt, turn) =>
                {
                    var performed = (AttackPerformedEvent)evt;
                    return new AttackPerformedEventDto(
                        turn,
                        performed.AttackerId.Value,
                        performed.TargetId.Value,
                        (int)performed.Kind,
                        (int)performed.Source,
                        ToPositionDto(performed.AttackerPosition),
                        ToPositionDto(performed.TargetPosition));
                },
                [typeof(ActorDamagedEvent)] = (evt, turn) =>
                {
                    var damage = (ActorDamagedEvent)evt;
                    return new ActorDamagedEventDto(
                        turn,
                        damage.SourceId?.Value,
                        damage.TargetId.Value,
                        damage.Amount,
                        damage.RemainingHp);
                },
                [typeof(ActorDiedEvent)] = (evt, turn) =>
                {
                    var died = (ActorDiedEvent)evt;
                    return new ActorDiedEventDto(turn, died.ActorId.Value);
                },
                [typeof(ActorHealedEvent)] = (evt, turn) =>
                {
                    var healed = (ActorHealedEvent)evt;
                    return new ActorHealedEventDto(turn, healed.ActorId.Value, healed.Amount, healed.CurrentHp);
                },
                [typeof(ExperienceGainedEvent)] = (evt, turn) =>
                {
                    var exp = (ExperienceGainedEvent)evt;
                    return new ExperienceGainedEventDto(
                        turn,
                        exp.ActorId.Value,
                        exp.SourceEnemyId?.Value,
                        exp.Amount,
                        exp.CurrentExp,
                        exp.ExpToNextLevel);
                },
                [typeof(LevelUpEvent)] = (evt, turn) =>
                {
                    var levelUp = (LevelUpEvent)evt;
                    return new LevelUpEventDto(turn, levelUp.ActorId.Value, levelUp.OldLevel, levelUp.NewLevel);
                },
                [typeof(ItemAddedToInventoryEvent)] = (evt, turn) =>
                {
                    var added = (ItemAddedToInventoryEvent)evt;
                    return new ItemAddedToInventoryEventDto(
                        turn,
                        added.ActorId.Value,
                        added.ItemId.Value,
                        (int)added.ItemType,
                        ToPositionDto(added.PickupPosition));
                },
                [typeof(ItemUsedEvent)] = (evt, turn) =>
                {
                    var used = (ItemUsedEvent)evt;
                    return new ItemUsedEventDto(
                        turn,
                        used.ActorId.Value,
                        used.ItemId.Value,
                        (int)used.ItemType);
                },
                [typeof(ItemDroppedEvent)] = (evt, turn) =>
                {
                    var dropped = (ItemDroppedEvent)evt;
                    return new ItemDroppedEventDto(
                        turn,
                        dropped.ActorId.Value,
                        dropped.ItemId.Value,
                        (int)dropped.ItemType,
                        ToPositionDto(dropped.DropPosition));
                },
                [typeof(ItemEquippedEvent)] = (evt, turn) =>
                {
                    var equipped = (ItemEquippedEvent)evt;
                    return new ItemEquippedEventDto(
                        turn,
                        equipped.ActorId.Value,
                        equipped.ItemId.Value,
                        (int)equipped.ItemType,
                        (int)equipped.Slot);
                },
                [typeof(ItemUnequippedEvent)] = (evt, turn) =>
                {
                    var unequipped = (ItemUnequippedEvent)evt;
                    return new ItemUnequippedEventDto(
                        turn,
                        unequipped.ActorId.Value,
                        unequipped.ItemId.Value,
                        (int)unequipped.ItemType,
                        (int)unequipped.Slot);
                },
                [typeof(ItemPickedEvent)] = (evt, turn) =>
                {
                    var picked = (ItemPickedEvent)evt;
                    return new ItemPickedEventDto(
                        turn,
                        picked.ActorId.Value,
                        (int)picked.ItemType,
                        ToPositionDto(picked.Position));
                },
                [typeof(ActorMovedEvent)] = (evt, turn) =>
                {
                    var moved = (ActorMovedEvent)evt;
                    return new ActorMovedEventDto(
                        turn,
                        moved.ActorId.Value,
                        moved.Success,
                        ToPositionDto(moved.From),
                        ToPositionDto(moved.To));
                },
                [typeof(ActorFacingChangedEvent)] = (evt, turn) =>
                {
                    var facing = (ActorFacingChangedEvent)evt;
                    return new ActorFacingChangedEventDto(turn, facing.ActorId.Value, (int)facing.Facing);
                },
                [typeof(HungerChangedEvent)] = (evt, turn) =>
                {
                    var hunger = (HungerChangedEvent)evt;
                    return new HungerChangedEventDto(turn, hunger.ActorId.Value, hunger.Delta);
                },
                [typeof(TurnEndedEvent)] = (evt, _) =>
                {
                    var ended = (TurnEndedEvent)evt;
                    return new TurnEndedEventDto(ended.TurnNumber);
                },
                [typeof(MonsterHouseTriggeredEvent)] = (evt, turn) =>
                {
                    var monsterHouse = (MonsterHouseTriggeredEvent)evt;
                    return new MonsterHouseTriggeredEventDto(turn, monsterHouse.AwakenedEnemyCount);
                },
                [typeof(LogEvent)] = (evt, turn) =>
                {
                    var log = (LogEvent)evt;
                    return new MessageEventDto(turn, ResolveLogMessage(log));
                }
            };

        private static readonly IReadOnlyDictionary<Type, Func<IRunLifecycleEvent, RunLifecycleEventDto>> LifecycleEventMappers =
            new Dictionary<Type, Func<IRunLifecycleEvent, RunLifecycleEventDto>>
            {
                [typeof(RunClearedEvent)] = evt =>
                {
                    var cleared = (RunClearedEvent)evt;
                    return new RunLifecycleEventDto(
                        kind: RunLifecycleEventKind.RunCleared,
                        floor: cleared.FinalFloor,
                        totalTurns: cleared.TotalTurns,
                        playerLevel: cleared.PlayerLevel);
                },
                [typeof(RunGameOverEvent)] = evt =>
                {
                    var gameOver = (RunGameOverEvent)evt;
                    return new RunLifecycleEventDto(
                        kind: RunLifecycleEventKind.RunGameOver,
                        floor: gameOver.Floor,
                        totalTurns: gameOver.TotalTurns,
                        playerLevel: gameOver.PlayerLevel);
                }
            };

        public IReadOnlyList<IRunEventDto> ProjectDomainEvents(IReadOnlyList<IRoguelikeEvent> events, int turnNumber)
        {
            if (events == null || events.Count == 0)
            {
                return Array.Empty<IRunEventDto>();
            }

            var projected = new List<IRunEventDto>(events.Count);
            for (var i = 0; i < events.Count; i++)
            {
                if (TryProjectDomainEvent(events[i], turnNumber, out var dto))
                {
                    projected.Add(dto);
                }
            }

            return projected;
        }

        public bool TryProjectDomainEvent(IRoguelikeEvent evt, int turnNumber, out IRunEventDto dto)
        {
            dto = default;
            if (evt == null)
            {
                return false;
            }

            if (!EventMappers.TryGetValue(evt.GetType(), out var mapper))
            {
                dto = CreateUnknownEventDto(evt, turnNumber);
                return true;
            }

            dto = mapper(evt, turnNumber);
            return true;
        }

        public IReadOnlyList<RunLifecycleEventDto> ProjectDomainLifecycleEvents(IReadOnlyList<IRunLifecycleEvent> events)
        {
            if (events == null || events.Count == 0)
            {
                return Array.Empty<RunLifecycleEventDto>();
            }

            var projected = new List<RunLifecycleEventDto>(events.Count);
            for (var i = 0; i < events.Count; i++)
            {
                if (TryProjectDomainLifecycleEvent(events[i], out var dto))
                {
                    projected.Add(dto);
                }
            }

            return projected;
        }

        public bool TryProjectDomainLifecycleEvent(IRunLifecycleEvent evt, out RunLifecycleEventDto dto)
        {
            dto = default;
            if (evt == null)
            {
                return false;
            }

            if (!LifecycleEventMappers.TryGetValue(evt.GetType(), out var mapper))
            {
                dto = CreateUnknownLifecycleEventDto(evt);
                return true;
            }

            dto = mapper(evt);
            return true;
        }

        private static UnknownRunEventDto CreateUnknownEventDto(IRoguelikeEvent evt, int turnNumber)
        {
            var eventTypeName = evt?.GetType().FullName;
            return new UnknownRunEventDto(turnNumber, eventTypeName ?? "UnknownEventType");
        }

        private static RunLifecycleEventDto CreateUnknownLifecycleEventDto(IRunLifecycleEvent evt)
        {
            var eventTypeName = evt?.GetType().FullName;
            return new RunLifecycleEventDto(
                kind: RunLifecycleEventKind.Unknown,
                floor: 0,
                totalTurns: 0,
                playerLevel: 0,
                sourceEventTypeName: eventTypeName ?? "UnknownLifecycleEventType");
        }

        private static GridPositionDto ToPositionDto(Position position)
        {
            return new GridPositionDto(position.X, position.Y);
        }

        private static string ResolveLogMessage(LogEvent log)
        {
            if (log == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(log.Message))
            {
                return log.Message;
            }

            return log.Code switch
            {
                RunLogCode.NoSpellbookEquipped => "魔導書を装備していない。",
                RunLogCode.SteppedOnItem => $"{GetLogParam(log, "itemName")} の上に乗った！",
                RunLogCode.PlayerDied => "プレイヤーは倒れた。",
                RunLogCode.LevelUp => $"レベルアップ！ Lv.{GetLogParam(log, "oldLevel")} → Lv.{GetLogParam(log, "newLevel")}",
                RunLogCode.TooHungryToRest => "空腹で休めない。",
                RunLogCode.TooHungryToSearch => "空腹で探索できない。",
                RunLogCode.SpellbookHasNoSpell => "その魔導書には呪文がない。",
                RunLogCode.Silenced => "沈黙していて呪文を唱えられない。",
                RunLogCode.NothingHappens => "何も起こらなかった。",
                RunLogCode.TooHungryToCast => "空腹で呪文を唱えられない。",
                RunLogCode.SpellMiss => GetLogParam(log, "missMessage"),
                RunLogCode.NoTargetToSleep => "眠らせる対象がいない。",
                RunLogCode.TargetFallsAsleep => $"{GetLogParam(log, "targetName")} は眠ってしまった。",
                RunLogCode.BlinkFailed => "瞬間移動に失敗した。",
                RunLogCode.Starving => "飢餓状態だ！",
                RunLogCode.WakeUp => "目を覚ました。",
                RunLogCode.NothingToPickUp => "拾えるアイテムがない。",
                RunLogCode.InventoryFull => "インベントリがいっぱいだ。",
                RunLogCode.ItemNotFoundInInventory => "インベントリにそのアイテムはない。",
                RunLogCode.ItemCannotBeUsed => "そのアイテムは使用できない。",
                RunLogCode.ItemCannotBeEquipped => "そのアイテムは装備できない。",
                RunLogCode.ItemAlreadyOnGround => "この場所にはすでにアイテムがある。",
                RunLogCode.MonsterHouseTriggered => $"モンスターハウスだ！ {GetLogParam(log, "awakenedCount")}体の敵が目を覚ました！",
                RunLogCode.RunIsNotActive => "ランは進行中ではない。",
                RunLogCode.InvalidPlayerAction => "プレイヤーの行動が無効だ。",
                RunLogCode.ActorAsleep => "眠っていて行動できない。",
                _ => log.Code.ToString()
            };
        }

        private static string GetLogParam(LogEvent log, string key)
        {
            if (log?.Parameters == null || string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            return log.Parameters.TryGetValue(key, out var value) ? value : string.Empty;
        }
    }
}



