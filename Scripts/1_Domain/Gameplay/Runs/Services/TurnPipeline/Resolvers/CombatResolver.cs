using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class CombatResolver
    {
        private readonly IFieldOfViewService _fieldOfViewService;
        private readonly DamageResolver _damageResolver;

        public CombatResolver(IFieldOfViewService fieldOfViewService, DamageResolver damageResolver)
        {
            _fieldOfViewService = fieldOfViewService ?? throw new ArgumentNullException(nameof(fieldOfViewService));
            _damageResolver = damageResolver ?? throw new ArgumentNullException(nameof(damageResolver));
        }

        public void ResolveAttack(RunSession session, Actor attacker, AttackAction action, List<IRoguelikeEvent> events)
        {
            if (session?.Map == null || attacker == null || action == null || events == null)
            {
                return;
            }

            var target = FindActorById(session, action.TargetId);
            if (target != null && TryGetDirectionTo(attacker.Position, target.Position, out var direction))
            {
                UpdateFacing(session, attacker, direction, events);
            }

            EmitAttackDeclared(session, attacker, action.Kind, action.Range, target, events);

            if (target == null || target == attacker || target.IsDead)
            {
                return;
            }

            if (target.Faction == attacker.Faction)
            {
                return;
            }

            var distance = ChebyshevDistance(attacker.Position, target.Position);
            if (!IsWithinAttackRange(action, distance))
            {
                return;
            }

            if (action.Kind != AttackKind.Melee)
            {
                var range = Math.Max(1, action.Range);
                if (!HasLineOfSight(session, attacker.Position, target.Position, range))
                {
                    return;
                }
            }

            ResolveAttackDamage(session, attacker, target, action.Kind, events);
        }

        private void ResolveAttackDamage(RunSession session, Actor attacker, Actor defender, AttackKind kind, List<IRoguelikeEvent> events)
        {
            events.Add(new AttackPerformedEvent(
                attacker.Id,
                defender.Id,
                kind,
                attacker.Position,
                defender.Position,
                AttackSource.Normal));

            var damage = CalculateMeleeDamage(session, attacker, defender);
            _damageResolver.ApplyDamageAndHandleDeath(session, attacker, defender, damage, events);
        }

        private void EmitAttackDeclared(
            RunSession session,
            Actor attacker,
            AttackKind kind,
            int range,
            Actor target,
            List<IRoguelikeEvent> events)
        {
            if (attacker == null || events == null)
            {
                return;
            }

            var declaredRange = GetEffectiveAttackRange(kind, range);
            var targetPosition = ResolveAttackDeclaredTargetPosition(session, attacker, kind, declaredRange, target);

            events.Add(new AttackDeclaredEvent(
                attacker.Id,
                target?.Id,
                kind,
                attacker.Position,
                attacker.Facing,
                targetPosition,
                declaredRange));
        }

        private static Position ResolveAttackDeclaredTargetPosition(
            RunSession session,
            Actor attacker,
            AttackKind kind,
            int range,
            Actor target)
        {
            if (attacker == null)
            {
                return default;
            }

            if (target != null && target != attacker)
            {
                return target.Position;
            }

            if (kind == AttackKind.Melee)
            {
                return ResolveMeleeDeclaredTargetPosition(session, attacker.Position, attacker.Facing);
            }

            return CalculateEndPosition(session, attacker.Position, attacker.Facing, range);
        }

        private static Position ResolveMeleeDeclaredTargetPosition(RunSession session, Position origin, Direction direction)
        {
            var target = DirectionUtility.Apply(origin, direction);

            if (session?.Map == null)
            {
                return target;
            }

            if (!session.Map.Contains(target))
            {
                return origin;
            }

            return target;
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

        private static int GetEffectiveAttackRange(AttackKind kind, int range)
        {
            if (kind == AttackKind.Melee)
            {
                return 1;
            }

            return Math.Max(1, range);
        }

        private static int CalculateMeleeDamage(RunSession session, Actor attacker, Actor defender)
        {
            var roll = session.Random.Next(-1, 2);
            var attackerStats = attacker.GetEffectiveStats();
            var defenderStats = defender.GetEffectiveStats();
            var raw = attackerStats.Attack - defenderStats.Defense + roll;
            return Math.Max(1, raw);
        }

        private static bool IsWithinAttackRange(AttackAction action, int distance)
        {
            if (distance <= 0)
            {
                return false;
            }

            if (action.Kind == AttackKind.Melee)
            {
                return distance == 1;
            }

            var range = Math.Max(1, action.Range);
            return distance <= range;
        }

        private bool HasLineOfSight(RunSession session, Position origin, Position target, int range)
        {
            if (range <= 0)
            {
                return false;
            }

            var visible = _fieldOfViewService.ComputeVisible(session.Map, origin, range);
            foreach (var position in visible)
            {
                if (position == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static Actor FindActorById(RunSession session, ActorId actorId)
        {
            if (session == null)
            {
                return null;
            }

            if (session.Player != null && session.Player.Id == actorId)
            {
                return session.Player;
            }

            for (var i = 0; i < session.Enemies.Count; i++)
            {
                var enemy = session.Enemies[i];
                if (enemy.Id == actorId)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static bool TryGetDirectionTo(Position from, Position to, out Direction direction)
        {
            var dx = Math.Sign(to.X - from.X);
            var dy = Math.Sign(to.Y - from.Y);

            if (dx == 0 && dy == 0)
            {
                direction = Direction.Down;
                return false;
            }

            direction = (dx, dy) switch
            {
                (0, -1) => Direction.Up,
                (1, -1) => Direction.UpRight,
                (1, 0) => Direction.Right,
                (1, 1) => Direction.DownRight,
                (0, 1) => Direction.Down,
                (-1, 1) => Direction.DownLeft,
                (-1, 0) => Direction.Left,
                (-1, -1) => Direction.UpLeft,
                _ => Direction.Down,
            };

            return true;
        }

        private static int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
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
    }
}



