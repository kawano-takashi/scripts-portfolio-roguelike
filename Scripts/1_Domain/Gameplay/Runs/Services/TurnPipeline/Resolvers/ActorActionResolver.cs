using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Services;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class ActorActionResolver
    {
        private const int FoodRestoreAmount = 20;
        private const int HealingPotionRestoreAmount = 5;
        private const int RestAdditionalHungerCost = 2;
        private const int SearchAdditionalHungerCost = 3;

        private readonly CombatResolver _combatResolver;
        private readonly SpellResolver _spellResolver;

        public ActorActionResolver(
            CombatResolver combatResolver,
            SpellResolver spellResolver)
        {
            _combatResolver = combatResolver ?? throw new ArgumentNullException(nameof(combatResolver));
            _spellResolver = spellResolver ?? throw new ArgumentNullException(nameof(spellResolver));
        }

        public ActionResolution Execute(
            RunSession session,
            Actor actor,
            RoguelikeAction action,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role)
        {
            if (session == null || actor == null || action == null || events == null || action.ActorId != actor.Id)
            {
                return ActionResolution.Unresolved;
            }

            if (actor.HasStatus(StatusEffectType.Sleep))
            {
                if (role == ActorExecutionRole.Player)
                {
                    events.Add(new LogEvent(RunLogCode.ActorAsleep));
                }

                return ActionResolution.Resolved(action.ConsumesTurn);
            }

            switch (action)
            {
                case MoveAction moveAction:
                    ResolveMove(session, actor, moveAction, events);
                    return ActionResolution.Resolved(moveAction.ConsumesTurn);

                case ChangeFacingAction facingAction:
                    ResolveFacing(session, actor, facingAction, events);
                    return ActionResolution.Resolved(facingAction.ConsumesTurn);

                case AttackAction attackAction:
                    _combatResolver.ResolveAttack(session, actor, attackAction, events);
                    return ActionResolution.Resolved(attackAction.ConsumesTurn);

                case CastEquippedSpellbookAction castEquippedSpellbookAction:
                    _spellResolver.ResolveEquippedSpellbookCast(session, actor, events, role);
                    return ActionResolution.Resolved(castEquippedSpellbookAction.ConsumesTurn);

                case CastSpellAction castSpellAction:
                    _spellResolver.ResolveSpellCast(
                        session,
                        actor,
                        castSpellAction.Spell,
                        ItemEnhancements.None,
                        events,
                        role,
                        isEquippedSpellCast: false);
                    return ActionResolution.Resolved(castSpellAction.ConsumesTurn);

                case PickupItemAction:
                    return ResolveInventoryAction(ResolvePickupItem(session, actor, events, role), action);

                case UseItemAction useItemAction:
                    return ResolveInventoryAction(
                        ResolveUseItem(session, actor, useItemAction, events, role),
                        useItemAction);

                case ToggleEquipItemAction toggleEquipItemAction:
                    return ResolveInventoryAction(
                        ResolveToggleEquip(actor, toggleEquipItemAction, events, role),
                        toggleEquipItemAction);

                case DropItemAction dropItemAction:
                    return ResolveInventoryAction(
                        ResolveDropItem(session, actor, dropItemAction, events, role),
                        dropItemAction);

                case WaitAction waitAction:
                    return ActionResolution.Resolved(waitAction.ConsumesTurn);

                case RestAction restAction:
                    ResolveRest(actor, events, role);
                    return ActionResolution.Resolved(restAction.ConsumesTurn);

                case SearchAction searchAction:
                    ResolveSearch(actor, events, role);
                    return ActionResolution.Resolved(searchAction.ConsumesTurn);

                default:
                    return ActionResolution.Unresolved;
            }
        }

        private static ActionResolution ResolveInventoryAction(bool resolved, RoguelikeAction action)
        {
            if (!resolved)
            {
                return ActionResolution.Unresolved;
            }

            return ActionResolution.Resolved(action.ConsumesTurn);
        }

        private void ResolveMove(RunSession session, Actor actor, MoveAction moveAction, List<IRoguelikeEvent> events)
        {
            if (session?.Map == null || actor == null || moveAction == null || events == null)
            {
                return;
            }

            UpdateFacing(session, actor, moveAction.Direction, events);

            var from = actor.Position;
            var target = DirectionUtility.Apply(from, moveAction.Direction);

            if (!session.Map.Contains(target) || !session.Map.IsWalkable(target))
            {
                events.Add(new ActorMovedEvent(actor.Id, from, from, false));
                return;
            }

            if (!DirectionUtility.CanMoveDiagonal(session.Map, from, moveAction.Direction))
            {
                events.Add(new ActorMovedEvent(actor.Id, from, from, false));
                return;
            }

            var occupant = session.GetActorAt(target);
            if (occupant != null && occupant != actor)
            {
                if (occupant.Faction != actor.Faction)
                {
                    if (actor.Faction == Faction.Player &&
                        actor.Equipment != null &&
                        actor.Equipment.TryGetEquippedSpellbook(actor.Inventory, out _))
                    {
                        _spellResolver.ResolveEquippedSpellbookCast(
                            session,
                            actor,
                            events,
                            ActorExecutionRole.Player);
                    }
                    else
                    {
                        events.Add(new LogEvent(RunLogCode.NoSpellbookEquipped));
                    }
                }
                else
                {
                    events.Add(new ActorMovedEvent(actor.Id, from, from, false));
                }

                return;
            }

            if (!session.TrySetActorPosition(actor, target, out _))
            {
                events.Add(new ActorMovedEvent(actor.Id, from, from, false));
                return;
            }
            events.Add(new ActorMovedEvent(actor.Id, from, target, true));

            if (actor.Faction != Faction.Player)
            {
                return;
            }

            var steppedItem = session.GetItemAt(target);
            if (steppedItem != null)
            {
                var itemName = ItemCatalog.GetDefinition(steppedItem.ItemType).DisplayName;
                events.Add(new LogEvent(
                    RunLogCode.SteppedOnItem,
                    new Dictionary<string, string> { ["itemName"] = itemName }));
            }

            CheckMonsterHouseTrigger(session, target, events);
        }

        private static void ResolveFacing(RunSession session, Actor actor, ChangeFacingAction action, List<IRoguelikeEvent> events)
        {
            if (actor == null || action == null || events == null)
            {
                return;
            }

            UpdateFacing(session, actor, action.Direction, events);
        }

        private bool ResolvePickupItem(
            RunSession session,
            Actor actor,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || session == null || actor == null || events == null)
            {
                return false;
            }

            var position = actor.Position;
            var item = session.GetItemAt(position);
            if (item == null)
            {
                events.Add(new LogEvent(RunLogCode.NothingToPickUp));
                return false;
            }

            if (actor.Inventory.IsFull)
            {
                events.Add(new LogEvent(RunLogCode.InventoryFull));
                return false;
            }

            session.RemoveItem(item);
            var inventoryItem = InventoryItem.FromMapItem(item);
            actor.AddToInventory(inventoryItem);

            events.Add(new ItemAddedToInventoryEvent(actor.Id, inventoryItem.Id, item.ItemType, position));
            return true;
        }

        private bool ResolveUseItem(
            RunSession session,
            Actor actor,
            UseItemAction action,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || session == null || actor == null || action == null || events == null)
            {
                return false;
            }

            if (!actor.Inventory.TryGetById(action.ItemId, out var item))
            {
                events.Add(new LogEvent(RunLogCode.ItemNotFoundInInventory));
                return false;
            }

            var definition = ItemCatalog.GetDefinition(item.ItemType);
            if (definition.IsEquippable && !definition.IsSpellbook)
            {
                events.Add(new LogEvent(RunLogCode.ItemCannotBeUsed));
                return false;
            }

            var usedItem = item;
            if (definition.ConsumesOnUse)
            {
                if (!actor.RemoveFromInventory(action.ItemId, out usedItem))
                {
                    events.Add(new LogEvent(RunLogCode.ItemNotFoundInInventory));
                    return false;
                }
            }

            var shouldEmitUsedEvent = true;
            switch (usedItem.ItemType)
            {
                case ItemId.FoodRation:
                    var restoredHunger = actor.RestoreHunger(FoodRestoreAmount);
                    if (restoredHunger > 0)
                    {
                        events.Add(new HungerChangedEvent(actor.Id, restoredHunger, actor.CurrentHunger));
                    }
                    break;

                case ItemId.HealingPotion:
                    var healed = actor.Heal(HealingPotionRestoreAmount);
                    if (healed > 0)
                    {
                        events.Add(new ActorHealedEvent(actor.Id, healed, actor.CurrentHp));
                    }
                    break;

                case ItemId.SpellbookForceBolt:
                case ItemId.SpellbookMagicFire:
                case ItemId.SpellbookSleep:
                case ItemId.SpellbookShield:
                case ItemId.SpellbookBlink:
                case ItemId.SpellbookDetect:
                    _spellResolver.ResolveSpellCast(
                        session,
                        actor,
                        usedItem.ItemType,
                        usedItem.Enhancements,
                        events,
                        role,
                        isEquippedSpellCast: false);
                    shouldEmitUsedEvent = false;
                    break;
            }

            if (shouldEmitUsedEvent)
            {
                events.Add(new ItemUsedEvent(actor.Id, usedItem.Id, usedItem.ItemType));
            }

            return true;
        }

        private static bool ResolveToggleEquip(
            Actor actor,
            ToggleEquipItemAction action,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || actor == null || action == null || events == null)
            {
                return false;
            }

            if (!actor.Inventory.TryGetById(action.ItemId, out var item))
            {
                events.Add(new LogEvent(RunLogCode.ItemNotFoundInInventory));
                return false;
            }

            var definition = ItemCatalog.GetDefinition(item.ItemType);
            if (!definition.IsEquippable)
            {
                events.Add(new LogEvent(RunLogCode.ItemCannotBeEquipped));
                return false;
            }

            if (actor.Equipment.IsEquipped(item.Id))
            {
                if (!actor.Equipment.TryUnequip(item.Id, out var unequipSlot))
                {
                    return false;
                }

                events.Add(new ItemUnequippedEvent(actor.Id, item.Id, item.ItemType, unequipSlot));
                return true;
            }

            if (!actor.Equipment.TryEquip(item, out var replacedItemId, out var slot))
            {
                return false;
            }

            if (replacedItemId.HasValue &&
                replacedItemId.Value != item.Id &&
                actor.Inventory.TryGetById(replacedItemId.Value, out var replacedItem))
            {
                events.Add(new ItemUnequippedEvent(actor.Id, replacedItem.Id, replacedItem.ItemType, slot));
            }

            events.Add(new ItemEquippedEvent(actor.Id, item.Id, item.ItemType, slot));
            return true;
        }

        private static bool ResolveDropItem(
            RunSession session,
            Actor actor,
            DropItemAction action,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || session == null || actor == null || action == null || events == null)
            {
                return false;
            }

            if (!actor.Inventory.TryGetById(action.ItemId, out var item))
            {
                events.Add(new LogEvent(RunLogCode.ItemNotFoundInInventory));
                return false;
            }

            var position = actor.Position;
            if (session.GetItemAt(position) != null)
            {
                events.Add(new LogEvent(RunLogCode.ItemAlreadyOnGround));
                return false;
            }

            if (actor.Equipment.IsEquipped(item.Id))
            {
                if (actor.Equipment.TryUnequip(item.Id, out var slot))
                {
                    events.Add(new ItemUnequippedEvent(actor.Id, item.Id, item.ItemType, slot));
                }
            }

            if (!actor.RemoveFromInventory(action.ItemId, out item))
            {
                events.Add(new LogEvent(RunLogCode.ItemNotFoundInInventory));
                return false;
            }

            var mapItem = new MapItem(item.Id, item.ItemType, position, item.Enhancements);
            session.AddItem(mapItem);

            events.Add(new ItemDroppedEvent(actor.Id, item.Id, item.ItemType, position));
            return true;
        }

        private static void ResolveRest(Actor actor, List<IRoguelikeEvent> events, ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || actor == null || events == null)
            {
                return;
            }

            if (!HasEnoughHunger(actor, RestAdditionalHungerCost))
            {
                events.Add(new LogEvent(RunLogCode.TooHungryToRest));
                return;
            }

            var healed = actor.Heal(1);
            if (healed > 0)
            {
                events.Add(new ActorHealedEvent(actor.Id, healed, actor.CurrentHp));
            }

            var spent = actor.SpendHunger(RestAdditionalHungerCost);
            if (spent > 0)
            {
                events.Add(new HungerChangedEvent(actor.Id, -spent, actor.CurrentHunger));
            }
        }

        private static void ResolveSearch(Actor actor, List<IRoguelikeEvent> events, ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || actor == null || events == null)
            {
                return;
            }

            if (!HasEnoughHunger(actor, SearchAdditionalHungerCost))
            {
                events.Add(new LogEvent(RunLogCode.TooHungryToSearch));
                return;
            }

            var spent = actor.SpendHunger(SearchAdditionalHungerCost);
            if (spent > 0)
            {
                events.Add(new HungerChangedEvent(actor.Id, -spent, actor.CurrentHunger));
            }
        }

        private static bool HasEnoughHunger(Actor actor, float cost)
        {
            return cost <= 0 || actor.CurrentHunger >= cost;
        }

        private static void UpdateFacing(RunSession session, Actor actor, Direction direction, List<IRoguelikeEvent> events)
        {
            if (session == null || actor == null || events == null)
            {
                return;
            }

            if (actor.Facing == direction)
            {
                return;
            }

            if (!session.TrySetActorFacing(actor, direction))
            {
                return;
            }

            events.Add(new ActorFacingChangedEvent(actor.Id, direction));
        }

        private static void CheckMonsterHouseTrigger(RunSession session, Position position, List<IRoguelikeEvent> events)
        {
            if (session?.Map == null || events == null)
            {
                return;
            }

            if (!session.Map.TryGetRoomAt(position, out var room))
            {
                return;
            }

            if (!session.TryTriggerMonsterHouse(room, out var awakenedCount))
            {
                return;
            }

            events.Add(new MonsterHouseTriggeredEvent(room, awakenedCount));
            events.Add(new LogEvent(
                RunLogCode.MonsterHouseTriggered,
                new Dictionary<string, string> { ["awakenedCount"] = awakenedCount.ToString() }));
        }
    }
}





