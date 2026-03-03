using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.Services;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class SpellResolver
    {
        private readonly DamageResolver _damageResolver;

        public SpellResolver(DamageResolver damageResolver)
        {
            _damageResolver = damageResolver ?? throw new ArgumentNullException(nameof(damageResolver));
        }

        public void ResolveEquippedSpellbookCast(
            RunSession session,
            Actor actor,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role)
        {
            if (role != ActorExecutionRole.Player || session == null || actor == null || events == null)
            {
                return;
            }

            if (actor.Equipment == null || !actor.Equipment.TryGetEquippedSpellbook(actor.Inventory, out var spellbookItem))
            {
                events.Add(new LogEvent(RunLogCode.NoSpellbookEquipped));
                return;
            }

            var definition = ItemCatalog.GetDefinition(spellbookItem.ItemType);
            if (!definition.IsSpellbook)
            {
                events.Add(new LogEvent(RunLogCode.SpellbookHasNoSpell));
                return;
            }

            ResolveSpellCast(
                session,
                actor,
                spellbookItem.ItemType,
                spellbookItem.Enhancements,
                events,
                role,
                isEquippedSpellCast: true);
        }

        public void ResolveSpellCast(
            RunSession session,
            Actor actor,
            ItemId spell,
            ItemEnhancements enhancements,
            List<IRoguelikeEvent> events,
            ActorExecutionRole role,
            bool isEquippedSpellCast)
        {
            if (role != ActorExecutionRole.Player || session == null || actor == null || events == null)
            {
                return;
            }

            if (actor.HasStatus(StatusEffectType.Silence))
            {
                events.Add(new LogEvent(RunLogCode.Silenced));
                return;
            }

            if (!ItemCatalog.TryGetSpellDefinition(spell, out var definition))
            {
                events.Add(new LogEvent(RunLogCode.NothingHappens));
                return;
            }

            var adjustedBaseCost = SpellEnhancementCalculator.CalculateHungerCost(definition, enhancements);
            var cost = CalculateHungerCost(actor, adjustedBaseCost);

            if (!HasEnoughHunger(actor, cost))
            {
                events.Add(new LogEvent(RunLogCode.TooHungryToCast));
                return;
            }

            var spent = actor.SpendHunger(cost);
            if (spent > 0)
            {
                events.Add(new HungerChangedEvent(actor.Id, -spent, actor.CurrentHunger));
            }

            switch (spell)
            {
                case ItemId.SpellbookForceBolt:
                    ResolveForceBoltSpell(session, actor, definition, enhancements, events, isEquippedSpellCast);
                    break;
                case ItemId.SpellbookMagicFire:
                    ResolveMagicFireSpell(session, actor, definition, enhancements, events, isEquippedSpellCast);
                    break;
                case ItemId.SpellbookSleep:
                    ResolveSleepSpell(session, actor, definition, enhancements, events, isEquippedSpellCast);
                    break;
                case ItemId.SpellbookBlink:
                    ResolveBlinkSpell(session, actor, definition, enhancements, events);
                    break;
                default:
                    events.Add(new LogEvent(RunLogCode.NothingHappens));
                    break;
            }
        }

        private void ResolveForceBoltSpell(
            RunSession session,
            Actor caster,
            ItemDefinition definition,
            ItemEnhancements enhancements,
            List<IRoguelikeEvent> events,
            bool isEquippedSpellCast)
        {
            ResolveLineDamageSpell(
                session,
                caster,
                definition,
                enhancements,
                ItemId.SpellbookForceBolt,
                "でんげきは何にも当たらなかった。",
                events,
                isEquippedSpellCast);
        }

        private void ResolveMagicFireSpell(
            RunSession session,
            Actor caster,
            ItemDefinition definition,
            ItemEnhancements enhancements,
            List<IRoguelikeEvent> events,
            bool isEquippedSpellCast)
        {
            ResolveLineDamageSpell(
                session,
                caster,
                definition,
                enhancements,
                ItemId.SpellbookMagicFire,
                "ほのおは何にも当たらなかった。",
                events,
                isEquippedSpellCast);
        }

        private void ResolveLineDamageSpell(
            RunSession session,
            Actor caster,
            ItemDefinition definition,
            ItemEnhancements enhancements,
            ItemId spell,
            string missMessage,
            List<IRoguelikeEvent> events,
            bool isEquippedSpellCast)
        {
            var range = Math.Max(1, SpellEnhancementCalculator.CalculateRange(definition, enhancements));
            var hasTarget = TryGetFirstActorInLine(session, caster.Position, caster.Facing, range, out var target);
            var targetPosition = hasTarget
                ? target.Position
                : CalculateEndPosition(session, caster.Position, caster.Facing, range);

            events.Add(new SpellCastEvent(
                caster.Id,
                spell,
                caster.Position,
                caster.Facing,
                targetPosition,
                hasTarget ? target.Id : null,
                range,
                isEquippedSpellCast));

            if (!hasTarget)
            {
                events.Add(new LogEvent(
                    RunLogCode.SpellMiss,
                    new Dictionary<string, string> { ["missMessage"] = missMessage },
                    fallbackMessage: missMessage));
                return;
            }

            var (damageMin, damageMax) = SpellEnhancementCalculator.CalculateDamageRange(definition, enhancements);
            var damage = session.Random.Next(damageMin, damageMax + 1);
            ApplySpellDamage(session, caster, target, damage, AttackKind.Ranged, events);
        }

        private void ResolveSleepSpell(
            RunSession session,
            Actor caster,
            ItemDefinition definition,
            ItemEnhancements enhancements,
            List<IRoguelikeEvent> events,
            bool isEquippedSpellCast)
        {
            var range = Math.Max(1, SpellEnhancementCalculator.CalculateRange(definition, enhancements));
            var hasTarget = TryGetFirstActorInLine(session, caster.Position, caster.Facing, range, out var target);
            var targetPosition = hasTarget
                ? target.Position
                : CalculateEndPosition(session, caster.Position, caster.Facing, range);

            events.Add(new SpellCastEvent(
                caster.Id,
                ItemId.SpellbookSleep,
                caster.Position,
                caster.Facing,
                targetPosition,
                hasTarget ? target.Id : null,
                range,
                isEquippedSpellCast));

            if (!hasTarget)
            {
                events.Add(new LogEvent(RunLogCode.NoTargetToSleep));
                return;
            }

            var (turnsMin, turnsMax) = SpellEnhancementCalculator.CalculateStatusTurns(definition, enhancements);
            var turns = session.Random.Next(turnsMin, turnsMax + 1);
            target.AddStatus(StatusEffectType.Sleep, turns);
            events.Add(new LogEvent(
                RunLogCode.TargetFallsAsleep,
                new Dictionary<string, string> { ["targetName"] = target.Name }));
        }

        private static void ResolveBlinkSpell(
            RunSession session,
            Actor caster,
            ItemDefinition definition,
            ItemEnhancements enhancements,
            List<IRoguelikeEvent> events)
        {
            var (minDistance, maxDistance) = SpellEnhancementCalculator.CalculateBlinkDistance(definition, enhancements);
            var candidates = CollectBlinkDestinations(session, caster.Position, minDistance, maxDistance);
            if (candidates.Count == 0)
            {
                events.Add(new LogEvent(RunLogCode.BlinkFailed));
                return;
            }

            var destination = candidates[session.Random.Next(candidates.Count)];
            var from = caster.Position;
            if (!session.TrySetActorPosition(caster, destination, out _))
            {
                events.Add(new LogEvent(RunLogCode.BlinkFailed));
                return;
            }
            events.Add(new ActorMovedEvent(caster.Id, from, destination, true));
        }

        private static List<Position> CollectBlinkDestinations(RunSession session, Position origin, int minDistance, int maxDistance)
        {
            var candidates = new List<Position>();
            if (session?.Map == null)
            {
                return candidates;
            }

            var map = session.Map;

            for (var dx = -maxDistance; dx <= maxDistance; dx++)
            {
                for (var dy = -maxDistance; dy <= maxDistance; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var distance = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    if (distance < minDistance || distance > maxDistance)
                    {
                        continue;
                    }

                    var position = new Position(origin.X + dx, origin.Y + dy);
                    if (!map.Contains(position))
                    {
                        continue;
                    }

                    if (!map.IsWalkable(position))
                    {
                        continue;
                    }

                    if (session.IsOccupied(position))
                    {
                        continue;
                    }

                    candidates.Add(position);
                }
            }

            return candidates;
        }

        private static bool TryGetFirstActorInLine(
            RunSession session,
            Position origin,
            Direction direction,
            int range,
            out Actor target)
        {
            target = null;
            if (session?.Map == null || range <= 0)
            {
                return false;
            }

            var position = origin;
            for (var i = 0; i < range; i++)
            {
                position = DirectionUtility.Apply(position, direction);

                if (!session.Map.Contains(position))
                {
                    return false;
                }

                if (session.Map.BlocksSight(position))
                {
                    return false;
                }

                var actor = session.GetActorAt(position);
                if (actor == null)
                {
                    continue;
                }

                target = actor;
                return true;
            }

            return false;
        }

        private static Position CalculateEndPosition(RunSession session, Position start, Direction direction, int range)
        {
            if (session?.Map == null || range <= 0)
            {
                return start;
            }

            var position = start;
            for (var i = 0; i < range; i++)
            {
                var next = DirectionUtility.Apply(position, direction);
                if (!session.Map.Contains(next) || session.Map.BlocksSight(next))
                {
                    break;
                }

                position = next;
            }

            return position;
        }

        private void ApplySpellDamage(
            RunSession session,
            Actor caster,
            Actor target,
            int damage,
            AttackKind kind,
            List<IRoguelikeEvent> events)
        {
            if (session == null || caster == null || target == null || events == null)
            {
                return;
            }

            events.Add(new AttackPerformedEvent(
                caster.Id,
                target.Id,
                kind,
                caster.Position,
                target.Position,
                AttackSource.Spell));

            _damageResolver.ApplyDamageAndHandleDeath(session, caster, target, damage, events);
        }

        private static int CalculateHungerCost(Actor actor, int baseCost)
        {
            if (baseCost <= 0)
            {
                return 0;
            }

            var intelligence = actor.Stats.Intelligence;
            if (intelligence <= 14)
            {
                return baseCost;
            }

            if (intelligence == 15)
            {
                return (int)Math.Ceiling(baseCost * 0.5f);
            }

            if (intelligence == 16)
            {
                return (int)Math.Ceiling(baseCost * 0.25f);
            }

            return 0;
        }

        private static bool HasEnoughHunger(Actor actor, float cost)
        {
            return cost <= 0 || actor.CurrentHunger >= cost;
        }
    }
}


