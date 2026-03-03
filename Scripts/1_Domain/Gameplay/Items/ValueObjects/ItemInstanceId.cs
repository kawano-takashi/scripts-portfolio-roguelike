using System;

namespace Roguelike.Domain.Gameplay.Items.ValueObjects
{
    /// <summary>
    /// アイテム個体の識別子です。
    /// </summary>
    public readonly struct ItemInstanceId : IEquatable<ItemInstanceId>
    {
        public Guid Value { get; }

        public ItemInstanceId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Item instance ID cannot be empty.", nameof(value));
            }

            Value = value;
        }

        public static ItemInstanceId NewId() => new ItemInstanceId(Guid.NewGuid());

        public bool Equals(ItemInstanceId other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is ItemInstanceId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public static bool operator ==(ItemInstanceId left, ItemInstanceId right) => left.Equals(right);

        public static bool operator !=(ItemInstanceId left, ItemInstanceId right) => !left.Equals(right);
    }
}
