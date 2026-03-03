using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Items.Services
{
    /// <summary>
    /// アイテム定義の単一カタログです。
    /// ここだけを参照すれば通常アイテム・魔法書の両方を解決できます。
    /// </summary>
    public static class ItemCatalog
    {
        // 全アイテムの静的定義。魔法書は「呪文ごとの個別アイテム」として登録します。
        private static readonly Dictionary<ItemId, ItemDefinition> Items = new()
        {
            {
                ItemId.FoodRation,
                new ItemDefinition(
                    ItemId.FoodRation,
                    "食料",
                    ItemCategory.Consumable,
                    EquipmentSlot.None,
                    0,
                    0,
                    true,
                    description: "食料\n\n使用すると空腹度を20回復します。\n空腹度が0になると飢餓ダメージを受けます。\n探索を続けるために必要不可欠なアイテムです。")
            },
            {
                ItemId.HealingPotion,
                new ItemDefinition(
                    ItemId.HealingPotion,
                    "回復ポーション",
                    ItemCategory.Consumable,
                    EquipmentSlot.None,
                    0,
                    0,
                    true,
                    description: "回復ポーション\n\n使用するとHPを5回復します。\n戦闘で受けたダメージを癒すのに役立ちます。")
            },
            {
                ItemId.Armor,
                new ItemDefinition(
                    ItemId.Armor,
                    "防具",
                    ItemCategory.Equipment,
                    EquipmentSlot.Armor,
                    0,
                    2,
                    false,
                    description: "防具\n\n装備すると防御力が2上がります。\n装備はインベントリ内で管理されます。")
            },
            {
                ItemId.SpellbookForceBolt,
                new ItemDefinition(
                    ItemId.SpellbookForceBolt,
                    "魔導書：でんげき",
                    ItemCategory.Equipment,
                    EquipmentSlot.Spellbook,
                    0,
                    0,
                    false,
                    description: "魔導書：でんげき\n\n前方に直線攻撃を放ちます。\n射程6、遮蔽物で停止。\nダメージ: 6〜10\n消費空腹度: 10",
                    shortSpellName: "でんげき",
                    spellRange: 6,
                    spellBaseHungerCost: 10,
                    spellDamageMin: 6,
                    spellDamageMax: 10)
            },
            {
                ItemId.SpellbookMagicFire,
                new ItemDefinition(
                    ItemId.SpellbookMagicFire,
                    "魔導書：ほのお",
                    ItemCategory.Equipment,
                    EquipmentSlot.Spellbook,
                    0,
                    0,
                    false,
                    description: "魔導書：ほのお\n\n前方1マスを焼く火炎魔法です。\n射程1、ダメージ: 3〜5\n消費空腹度: 0",
                    shortSpellName: "ほのお",
                    spellRange: 1,
                    spellBaseHungerCost: 0,
                    spellDamageMin: 3,
                    spellDamageMax: 5)
            },
            {
                ItemId.SpellbookSleep,
                new ItemDefinition(
                    ItemId.SpellbookSleep,
                    "魔導書：ねむり",
                    ItemCategory.Equipment,
                    EquipmentSlot.Spellbook,
                    0,
                    0,
                    false,
                    description: "魔導書：ねむり\n\n前方の相手を眠らせます。\n射程5、2〜4ターン行動不能。\n消費空腹度: 14",
                    shortSpellName: "ねむり",
                    spellRange: 5,
                    spellBaseHungerCost: 14,
                    spellStatusTurnsMin: 2,
                    spellStatusTurnsMax: 4)
            },
            {
                ItemId.SpellbookShield,
                new ItemDefinition(
                    ItemId.SpellbookShield,
                    "魔導書：シールド",
                    ItemCategory.Equipment,
                    EquipmentSlot.Spellbook,
                    0,
                    0,
                    false,
                    description: "魔導書：シールド\n\nしばらく守りを固くします。\n消費空腹度: 8",
                    shortSpellName: "シールド",
                    spellRange: 0,
                    spellBaseHungerCost: 8)
            },
            {
                ItemId.SpellbookBlink,
                new ItemDefinition(
                    ItemId.SpellbookBlink,
                    "魔導書：いどう",
                    ItemCategory.Equipment,
                    EquipmentSlot.Spellbook,
                    0,
                    0,
                    false,
                    description: "魔導書：いどう\n\n自分を2〜4マス瞬間移動します。\n壁や他者のいるマスには移動しません。\n消費空腹度: 12",
                    shortSpellName: "いどう",
                    spellRange: 0,
                    spellBaseHungerCost: 12,
                    spellBlinkMinDistance: 2,
                    spellBlinkMaxDistance: 4)
            },
            {
                ItemId.SpellbookDetect,
                new ItemDefinition(
                    ItemId.SpellbookDetect,
                    "魔導書：ディテクト",
                    ItemCategory.Equipment,
                    EquipmentSlot.Spellbook,
                    0,
                    0,
                    false,
                    description: "魔導書：ディテクト\n\n周囲の情報を把握しやすくします。\n消費空腹度: 6",
                    shortSpellName: "ディテクト",
                    spellRange: 0,
                    spellBaseHungerCost: 6)
            }
        };
        // ドロップ時に出現させる魔法書の候補。
        // 生成重みの「Spellbook枠」からこの中のどれかを選びます。
        private static readonly IReadOnlyList<ItemId> SpellbookDropPool = new[]
        {
            ItemId.SpellbookForceBolt,
            ItemId.SpellbookSleep,
            ItemId.SpellbookBlink
        };

        public static ItemDefinition GetDefinition(ItemId itemType)
        {
            if (TryGetDefinition(itemType, out var definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Unknown item definition: {itemType}");
        }

        public static bool TryGetDefinition(ItemId itemType, out ItemDefinition definition)
        {
            return Items.TryGetValue(itemType, out definition);
        }

        public static bool TryGetSpellDefinition(ItemId itemType, out ItemDefinition definition)
        {
            if (!TryGetDefinition(itemType, out definition))
            {
                return false;
            }

            // ItemId を増やしても、判定ルールは EquipSlot だけで完結させます。
            return definition.IsSpellbook;
        }

        public static IReadOnlyList<ItemId> GetSpellbookDropPool()
        {
            return SpellbookDropPool;
        }
    }
}


