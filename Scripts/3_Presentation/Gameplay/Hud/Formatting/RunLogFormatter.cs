using System;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.Hud.Types;

namespace Roguelike.Presentation.Gameplay.Hud.Formatting
{
    /// <summary>
    /// ログレコードを表示文言へ変換します。
    /// </summary>
    public sealed class RunLogFormatter
    {
        public string Format(RunLogRecord record, Func<Guid?, string> actorNameResolver)
        {
            if (record == null)
            {
                return null;
            }

            if (actorNameResolver == null)
            {
                throw new ArgumentNullException(nameof(actorNameResolver));
            }

            return record.EventKind switch
            {
                RunLogEventKind.AttackDeclared => FormatAttackDeclared(record, actorNameResolver),
                RunLogEventKind.SpellCast => FormatSpellCast(record, actorNameResolver),
                RunLogEventKind.ActorDamaged => FormatDamage(record, actorNameResolver),
                RunLogEventKind.ActorDied => $"{actorNameResolver(record.SubjectActorId)} は倒れた！",
                RunLogEventKind.ExperienceGained => $"{record.Amount.GetValueOrDefault()} 経験値を獲得！",
                RunLogEventKind.LevelUp => $"{actorNameResolver(record.SubjectActorId)} はレベルアップ！ Lv.{record.OldValue.GetValueOrDefault()} → Lv.{record.NewValue.GetValueOrDefault()}",
                RunLogEventKind.ItemAddedToInventory => $"{actorNameResolver(record.SubjectActorId)} は {GetItemName(record.ItemType)} を拾った！",
                RunLogEventKind.ItemUsed => $"{actorNameResolver(record.SubjectActorId)} は {GetItemName(record.ItemType)} を使った！",
                RunLogEventKind.ItemDropped => $"{actorNameResolver(record.SubjectActorId)} は {GetItemName(record.ItemType)} を落とした！",
                RunLogEventKind.ItemEquipped => $"{actorNameResolver(record.SubjectActorId)} は {GetItemName(record.ItemType)} を装備した！",
                RunLogEventKind.ItemUnequipped => $"{actorNameResolver(record.SubjectActorId)} は {GetItemName(record.ItemType)} を外した！",
                RunLogEventKind.RunCleared => $"ダンジョンをクリアした！ {record.Floor.GetValueOrDefault()}F / {record.TotalTurns.GetValueOrDefault()}ターン / Lv.{record.PlayerLevel.GetValueOrDefault()}",
                RunLogEventKind.RunGameOver => $"冒険は終わった... {record.Floor.GetValueOrDefault()}F / {record.TotalTurns.GetValueOrDefault()}ターン / Lv.{record.PlayerLevel.GetValueOrDefault()}",
                RunLogEventKind.Message => record.RawMessage,
                _ => null
            };
        }

        private static string FormatAttackDeclared(RunLogRecord record, Func<Guid?, string> actorNameResolver)
        {
            var kind = record.AttackKind ?? AttackKindDto.Melee;

            var kindText = kind switch
            {
                AttackKindDto.Melee => "攻撃した",
                AttackKindDto.Ranged => "矢を放った",
                AttackKindDto.Disruptor => "攻撃した",
                _ => "攻撃した"
            };

            return $"{actorNameResolver(record.SubjectActorId)} は {kindText}！";
        }

        private static string FormatSpellCast(RunLogRecord record, Func<Guid?, string> actorNameResolver)
        {
            return $"{actorNameResolver(record.SubjectActorId)} は {GetSpellDisplayName(record.ItemType)} を唱えた！";
        }

        private static string FormatDamage(RunLogRecord record, Func<Guid?, string> actorNameResolver)
        {
            return $"{actorNameResolver(record.SubjectActorId)} は {actorNameResolver(record.TargetActorId)} に {record.Amount.GetValueOrDefault()} ダメージを与えた！";
        }

        private static string GetItemName(ItemTypeDto? itemType)
        {
            if (!itemType.HasValue)
            {
                return "???";
            }

            return itemType.Value switch
            {
                ItemTypeDto.FoodRation => "食料",
                ItemTypeDto.HealingPotion => "回復薬",
                ItemTypeDto.Armor => "防具",
                ItemTypeDto.SpellbookForceBolt => "魔法書:でんげき",
                ItemTypeDto.SpellbookMagicFire => "魔法書:ほのお",
                ItemTypeDto.SpellbookSleep => "魔法書:スリープ",
                ItemTypeDto.SpellbookShield => "魔法書:シールド",
                ItemTypeDto.SpellbookBlink => "魔法書:ブリンク",
                ItemTypeDto.SpellbookDetect => "魔法書:ディテクト",
                _ => "???"
            };
        }

        private static string GetSpellDisplayName(ItemTypeDto? spell)
        {
            if (!spell.HasValue)
            {
                return "???";
            }

            return spell.Value switch
            {
                ItemTypeDto.SpellbookForceBolt => "でんげき",
                ItemTypeDto.SpellbookMagicFire => "ほのお",
                ItemTypeDto.SpellbookSleep => "スリープ",
                ItemTypeDto.SpellbookShield => "シールド",
                ItemTypeDto.SpellbookBlink => "ブリンク",
                ItemTypeDto.SpellbookDetect => "ディテクト",
                _ => GetItemName(spell)
            };
        }
    }
}





