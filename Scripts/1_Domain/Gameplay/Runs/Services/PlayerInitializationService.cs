using System;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// プレイヤー初期化ルールのドメイン実装です。
    /// </summary>
    public sealed class PlayerInitializationService : IPlayerInitializationService
    {
        public Actor CreateInitialPlayer(
            string playerName,
            int maxHp,
            int attack,
            int defense,
            int intelligence,
            int sightRadius,
            float maxHunger,
            Position startPosition)
        {
            var resolvedName = string.IsNullOrWhiteSpace(playerName)
                ? "魔術師"
                : playerName;

            var playerStats = new ActorStats(
                maxHp: Math.Max(1, maxHp),
                attack: Math.Max(0, attack),
                defense: Math.Max(0, defense),
                intelligence: Math.Max(0, intelligence),
                sightRadius: Math.Max(1, sightRadius),
                maxHunger: Math.Max(1f, maxHunger));

            var player = new Actor(
                ActorId.NewId(),
                resolvedName,
                Faction.Player,
                startPosition,
                playerStats,
                enemyArchetype: null,
                facing: Direction.Down,
                inventoryCapacity: Inventory.DefaultCapacity,
                growth: StatGrowth.PlayerDefault);

            AddInitialInventory(player);
            return player;
        }

        public void PreparePlayerForNextFloor(RunSession session, Position startPosition)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (session.Player == null)
            {
                throw new ArgumentException("RunSession must provide a player.", nameof(session));
            }

            if (!session.TrySetActorPosition(session.Player, startPosition, out _))
            {
                throw new InvalidOperationException("Failed to place player on the next floor start position.");
            }

            if (!session.TrySetActorFacing(session.Player, Direction.Down))
            {
                throw new InvalidOperationException("Failed to set the player's facing for the next floor.");
            }
        }

        private static void AddInitialInventory(Actor player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var initialSpellbook = new InventoryItem(ItemInstanceId.NewId(), ItemId.SpellbookMagicFire);
            if (!player.AddToInventory(initialSpellbook))
            {
                throw new InvalidOperationException("Failed to add initial spellbook to inventory.");
            }

            if (!player.Equipment.TryEquip(initialSpellbook, out _, out var equippedSlot))
            {
                throw new InvalidOperationException("Failed to equip initial spellbook.");
            }

            if (equippedSlot != EquipmentSlot.Spellbook)
            {
                throw new InvalidOperationException("Initial spellbook was not equipped to the spellbook slot.");
            }
        }
    }
}
