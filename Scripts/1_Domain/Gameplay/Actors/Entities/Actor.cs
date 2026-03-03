using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
namespace Roguelike.Domain.Gameplay.Actors.Entities
{
    /// <summary>
    /// マップ上で動くキャラクター（プレイヤーや敵）です。
    /// </summary>
    public class Actor
    {
        // 状態異常の残りターン数を保存します。
        // 例: Silence が 2 なら、あと2ターン効くという意味です。
        private readonly Dictionary<StatusEffectType, int> _statusEffects = new();
        // 自然回復の端数を保持します。
        private float _naturalHpRecoveryAccumulator;

        /// <summary>
        /// キャラクターのID（名札）。
        /// </summary>
        public ActorId Id { get; }

        /// <summary>
        /// 表示用の名前。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 味方か敵か。
        /// </summary>
        public Faction Faction { get; }

        /// <summary>
        /// 敵のタイプ（プレイヤーは null）。
        /// </summary>
        public EnemyArchetype? EnemyArchetype { get; }

        /// <summary>
        /// いまいる位置。
        /// </summary>
        public Position Position { get; private set; }

        /// <summary>
        /// いま向いている向き。
        /// </summary>
        public Direction Facing { get; private set; }

        /// <summary>
        /// 最大値などの基本能力（レベル1時点での値）。
        /// </summary>
        public ActorStats Stats { get; }

        /// <summary>
        /// レベルアップ時のステータス成長率。
        /// </summary>
        public StatGrowth Growth { get; }

        /// <summary>
        /// レベルと経験値の状態。
        /// </summary>
        public LevelProgress LevelProgress { get; private set; }

        /// <summary>
        /// 現在のHP。
        /// </summary>
        public int CurrentHp { get; private set; }

        /// <summary>
        /// 現在の空腹度。
        /// </summary>
        public float CurrentHunger { get; private set; }

        /// <summary>
        /// HPが0なら死んでいる。
        /// </summary>
        public bool IsDead => CurrentHp <= 0;

        /// <summary>
        /// 持ち物袋。
        /// </summary>
        public Inventory Inventory { get; }

        /// <summary>
        /// 装備中のアイテム。
        /// </summary>
        public EquipmentSet Equipment { get; }

        /// <summary>
        /// キャラクターを作るときの入口です。
        /// </summary>
        public Actor(
            ActorId id,
            string name,
            Faction faction,
            Position position,
            ActorStats stats,
            EnemyArchetype? enemyArchetype = null,
            Direction facing = Direction.Down,
            int inventoryCapacity = Inventory.DefaultCapacity,
            StatGrowth? growth = null,
            LevelProgress? levelProgress = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Actor name cannot be empty.", nameof(name));
            }

            Id = id;
            Name = name;
            Faction = faction;
            EnemyArchetype = enemyArchetype;
            Position = position;
            Facing = facing;
            Stats = stats;
            Growth = growth ?? StatGrowth.None;
            LevelProgress = levelProgress ?? LevelProgress.Initial;
            CurrentHp = GetEffectiveMaxHp();
            CurrentHunger = stats.MaxHunger;
            Inventory = new Inventory(inventoryCapacity);
            Equipment = new EquipmentSet();
        }

        /// <summary>
        /// 位置を変更します。
        /// </summary>
        internal void SetPosition(Position position)
        {
            Position = position;
        }

        /// <summary>
        /// 向きを変更します。
        /// </summary>
        internal void SetFacing(Direction direction)
        {
            Facing = direction;
        }

        /// <summary>
        /// ダメージを受けます。
        /// 0未満の値は0として扱います。
        /// </summary>
        public int ApplyDamage(int amount)
        {
            var damage = Math.Max(0, amount);
            CurrentHp = Math.Max(0, CurrentHp - damage);
            return damage;
        }

        /// <summary>
        /// HPを回復します。
        /// 最大HPを超えないようにします。
        /// </summary>
        public int Heal(int amount)
        {
            var healed = Math.Max(0, amount);
            var maxHp = GetEffectiveMaxHp();
            var newHp = Math.Min(maxHp, CurrentHp + healed);
            var actual = newHp - CurrentHp;
            CurrentHp = newHp;
            return actual;
        }

        /// <summary>
        /// 空腹度を消費します（減らします）。
        /// </summary>
        public float SpendHunger(float amount)
        {
            var spent = Math.Max(0f, amount);
            var newHunger = Math.Max(0f, CurrentHunger - spent);
            var actual = CurrentHunger - newHunger;
            CurrentHunger = newHunger;
            return actual;
        }

        /// <summary>
        /// 空腹度を回復します（増やします）。
        /// </summary>
        public float RestoreHunger(float amount)
        {
            var restored = Math.Max(0f, amount);
            var newHunger = Math.Min(Stats.MaxHunger, CurrentHunger + restored);
            var actual = newHunger - CurrentHunger;
            CurrentHunger = newHunger;
            return actual;
        }

        /// <summary>
        /// 自然回復の端数蓄積をリセットします。
        /// </summary>
        public void ResetNaturalHpRecoveryAccumulator()
        {
            _naturalHpRecoveryAccumulator = 0f;
        }

        /// <summary>
        /// 自然回復の端数を加算します。
        /// </summary>
        public void AddNaturalHpRecoveryAccumulator(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _naturalHpRecoveryAccumulator += amount;
        }

        /// <summary>
        /// 蓄積値の整数部分をHP回復量として取り出します。
        /// </summary>
        public int DrainNaturalHpRecoveryAccumulatorAsHp()
        {
            var hp = (int)_naturalHpRecoveryAccumulator;
            if (hp <= 0)
            {
                return 0;
            }

            _naturalHpRecoveryAccumulator -= hp;
            return hp;
        }

        /// <summary>
        /// アイテムをインベントリに追加します。
        /// </summary>
        public bool AddToInventory(InventoryItem item)
        {
            return Inventory.TryAdd(item);
        }

        /// <summary>
        /// インベントリからアイテムを取り出します（削除します）。
        /// </summary>
        public bool RemoveFromInventory(ItemInstanceId itemId, out InventoryItem item)
        {
            return Inventory.TryRemoveById(itemId, out item);
        }

        /// <summary>
        /// レベル補正込みの最大HPを返します。
        /// </summary>
        public int GetEffectiveMaxHp()
        {
            var (hpBonus, _, _) = Growth.CalculateBonusForLevel(LevelProgress.Level);
            return Stats.MaxHp + hpBonus;
        }

        /// <summary>
        /// レベル補正と装備補正込みの能力値を返します。
        /// </summary>
        public ActorStats GetEffectiveStats()
        {
            var (hpBonus, attackBonus, defenseBonus) = Growth.CalculateBonusForLevel(LevelProgress.Level);
            var modifier = Equipment.GetTotalModifier(Inventory);
            var attack = Math.Max(0, Stats.Attack + attackBonus + modifier.Attack);
            var defense = Math.Max(0, Stats.Defense + defenseBonus + modifier.Defense);

            return new ActorStats(
                Stats.MaxHp + hpBonus,
                attack,
                defense,
                Stats.Intelligence,
                Stats.SightRadius,
                Stats.MaxHunger);
        }

        /// <summary>
        /// 経験値を獲得し、レベルアップを処理します。
        /// レベルアップした場合はHPを全回復します。
        /// </summary>
        public (bool leveledUp, int newLevel) GainExperience(int amount)
        {
            if (amount <= 0)
            {
                return (false, LevelProgress.Level);
            }

            var oldMaxHp = GetEffectiveMaxHp();
            LevelProgress = LevelProgress.AddExperience(amount, out var leveledUp, out _);
            var newMaxHp = GetEffectiveMaxHp();

            // レベルアップ時はHPを全回復
            if (leveledUp)
            {
                CurrentHp = newMaxHp;
            }

            return (leveledUp, LevelProgress.Level);
        }

        /// <summary>
        /// 指定の状態異常が付いているかを調べます。
        /// </summary>
        public bool HasStatus(StatusEffectType type)
        {
            return _statusEffects.TryGetValue(type, out var turns) && turns > 0;
        }

        /// <summary>
        /// 指定の状態異常があと何ターン続くかを返します。
        /// </summary>
        public int GetStatusTurns(StatusEffectType type)
        {
            return _statusEffects.TryGetValue(type, out var turns) ? turns : 0;
        }

        /// <summary>
        /// 状態異常を付けます。すでにある場合は長い方を残します。
        /// </summary>
        public void AddStatus(StatusEffectType type, int turns)
        {
            if (turns <= 0)
            {
                return;
            }

            if (_statusEffects.TryGetValue(type, out var existing))
            {
                _statusEffects[type] = Math.Max(existing, turns);
            }
            else
            {
                _statusEffects[type] = turns;
            }
        }

        /// <summary>
        /// 状態異常を解除します。
        /// </summary>
        public bool RemoveStatus(StatusEffectType type)
        {
            return _statusEffects.Remove(type);
        }

        /// <summary>
        /// ターンが進んだので、状態異常の残りを1減らします。
        /// 0になったものは消して、その一覧を返します。
        /// </summary>
        public IReadOnlyList<StatusEffectType> TickStatusEffects()
        {
            if (_statusEffects.Count == 0)
            {
                return Array.Empty<StatusEffectType>();
            }

            var expired = new List<StatusEffectType>();
            var keys = new List<StatusEffectType>(_statusEffects.Keys);

            foreach (var key in keys)
            {
                var remaining = _statusEffects[key] - 1;
                if (remaining <= 0)
                {
                    _statusEffects.Remove(key);
                    expired.Add(key);
                }
                else
                {
                    _statusEffects[key] = remaining;
                }
            }

            return expired;
        }
    }
}





