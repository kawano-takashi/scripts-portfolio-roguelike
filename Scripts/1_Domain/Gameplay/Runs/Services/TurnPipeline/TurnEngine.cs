using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    public sealed class TurnEngine : ITurnEngine
    {
        private readonly InputValidationPhase _inputValidationPhase;
        private readonly PlayerActionPhase _playerActionPhase;
        private readonly SurvivalPhase _survivalPhase;
        private readonly EnemyActionPhase _enemyActionPhase;
        private readonly TurnEndPhase _turnEndPhase;

        public TurnEngine(IEnemyDecisionPolicy enemyDecisionPolicy, IFieldOfViewService fieldOfViewService)
        {
            if (enemyDecisionPolicy == null)
            {
                throw new ArgumentNullException(nameof(enemyDecisionPolicy));
            }

            if (fieldOfViewService == null)
            {
                throw new ArgumentNullException(nameof(fieldOfViewService));
            }

            var damageResolver = new DamageResolver();
            var spellResolver = new SpellResolver(damageResolver);
            var combatResolver = new CombatResolver(fieldOfViewService, damageResolver);
            var actionResolver = new ActorActionResolver(combatResolver, spellResolver);

            _inputValidationPhase = new InputValidationPhase();
            _playerActionPhase = new PlayerActionPhase(actionResolver);
            _survivalPhase = new SurvivalPhase();
            _enemyActionPhase = new EnemyActionPhase(enemyDecisionPolicy, actionResolver);
            _turnEndPhase = new TurnEndPhase();
        }

        public TurnResolution Resolve(RunSession session, RoguelikeAction playerAction)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var events = new List<IRoguelikeEvent>();

            // 入力の検証
            if (!_inputValidationPhase.TryExecute(session, playerAction, events))
            {
                return BuildResolution(session, playerAction, events,
                    ActionResolution.Unresolved, turnConsumed: false);
            }

            // ターン処理開始
            var playerActionResolution = _playerActionPhase.Execute(session, playerAction, events);
            if (!playerActionResolution.IsResolved || !playerActionResolution.ConsumesTurn)
            {
                return BuildResolution(session, playerAction, events,
                    playerActionResolution, turnConsumed: false);
            }

            _enemyActionPhase.Execute(session, events);
            _survivalPhase.Execute(session, events);
            _turnEndPhase.Execute(session, events);

            return BuildResolution(session, playerAction, events, playerActionResolution, turnConsumed: true);
        }

        private static TurnResolution BuildResolution(
            RunSession session,
            RoguelikeAction playerAction,
            IReadOnlyList<IRoguelikeEvent> events,
            ActionResolution playerActionResolution,
            bool turnConsumed)
        {
            return new TurnResolution(
                turnConsumed: turnConsumed,
                turnNumber: session.TurnCount,
                events: events,
                actionResolved: playerActionResolution.IsResolved,
                playerMoveOutcome: ResolvePlayerMoveOutcome(playerAction, events));
        }

        private static ActorMoveOutcome ResolvePlayerMoveOutcome(
            RoguelikeAction playerAction,
            IReadOnlyList<IRoguelikeEvent> events)
        {
            if (!(playerAction is MoveAction moveAction) || events == null || events.Count == 0)
            {
                return ActorMoveOutcome.None;
            }

            var actorId = moveAction.ActorId;
            for (var i = 0; i < events.Count; i++)
            {
                if (!(events[i] is ActorMovedEvent movedEvent))
                {
                    continue;
                }

                if (movedEvent.ActorId != actorId)
                {
                    continue;
                }

                return new ActorMoveOutcome(
                    success: movedEvent.Success,
                    from: movedEvent.From,
                    to: movedEvent.To);
            }

            return ActorMoveOutcome.None;
        }
    }
}
