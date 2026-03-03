using System;

namespace Roguelike.Domain.Gameplay.Actors.ValueObjects
{
    public readonly struct ActorStatModifier : IEquatable<ActorStatModifier>
    {
        public int Attack { get; }
        public int Defense { get; }

        public ActorStatModifier(int attack, int defense)
        {
            Attack = attack;
            Defense = defense;
        }

        public static ActorStatModifier None => new ActorStatModifier(0, 0);

        public bool Equals(ActorStatModifier other)
        {
            return Attack == other.Attack && Defense == other.Defense;
        }

        public override bool Equals(object obj) => obj is ActorStatModifier other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(Attack, Defense);
        }

        public static ActorStatModifier operator +(ActorStatModifier left, ActorStatModifier right)
        {
            return new ActorStatModifier(left.Attack + right.Attack, left.Defense + right.Defense);
        }
    }
}


