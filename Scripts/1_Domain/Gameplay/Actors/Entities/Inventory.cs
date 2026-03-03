using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.ValueObjects;

namespace Roguelike.Domain.Gameplay.Actors.Entities
{
    /// <summary>
    /// プレイヤーが持っているアイテムの袋です。
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// デフォルトの最大容量。
        /// </summary>
        public const int DefaultCapacity = 20;

        private readonly List<InventoryItem> _items = new();
        private readonly ReadOnlyCollection<InventoryItem> _readOnlyItems;

        /// <summary>
        /// 持っているアイテムの一覧。
        /// </summary>
        public IReadOnlyList<InventoryItem> Items => _readOnlyItems;

        /// <summary>
        /// 今持っているアイテムの数。
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 最大容量。
        /// </summary>
        public int MaxCapacity { get; }

        /// <summary>
        /// 袋がいっぱいかどうか。
        /// </summary>
        public bool IsFull => _items.Count >= MaxCapacity;

        /// <summary>
        /// インベントリを作るときの入口です。
        /// </summary>
        public Inventory(int maxCapacity = DefaultCapacity)
        {
            if (maxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Max capacity must be positive.");
            }

            MaxCapacity = maxCapacity;
            _readOnlyItems = _items.AsReadOnly();
        }

        /// <summary>
        /// アイテムを追加します。
        /// 袋がいっぱいなら失敗します。
        /// </summary>
        public bool TryAdd(InventoryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (IsFull)
            {
                return false;
            }

            _items.Add(item);
            return true;
        }

        /// <summary>
        /// 指定した位置のアイテムを取得します。
        /// 範囲外ならnullを返します。
        /// </summary>
        public InventoryItem GetAt(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return null;
            }

            return _items[index];
        }

        /// <summary>
        /// 指定したIDのアイテムを探します。
        /// </summary>
        public bool TryGetById(ItemInstanceId itemId, out InventoryItem item)
        {
            item = null;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == itemId)
                {
                    item = _items[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 指定したIDのアイテムを取り出します（削除します）。
        /// </summary>
        public bool TryRemoveById(ItemInstanceId itemId, out InventoryItem item)
        {
            item = null;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == itemId)
                {
                    item = _items[i];
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 指定した位置のアイテムを取り出します（削除します）。
        /// </summary>
        public bool TryRemoveAt(int index, out InventoryItem item)
        {
            item = null;

            if (index < 0 || index >= _items.Count)
            {
                return false;
            }

            item = _items[index];
            _items.RemoveAt(index);
            return true;
        }
    }
}
