using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Domain.Gameplay.Runs.Services.TurnPipeline
{
    internal sealed class EnemyActionPhase
    {
        private readonly IEnemyDecisionPolicy _enemyDecisionPolicy;
        private readonly ActorActionResolver _actionResolver;

        public EnemyActionPhase(
            IEnemyDecisionPolicy enemyDecisionPolicy,
            ActorActionResolver actionResolver)
        {
            _enemyDecisionPolicy = enemyDecisionPolicy ?? throw new ArgumentNullException(nameof(enemyDecisionPolicy));
            _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        }

        public void Execute(RunSession session, List<IRoguelikeEvent> events)
        {
            for (var i = 0; i < session.Enemies.Count; i++)
            {
                var enemy = session.Enemies[i];
                if (enemy.IsDead)
                {
                    continue;
                }

                var actionCount = _enemyDecisionPolicy.GetActionCount(enemy, session.TurnCount);
                for (var a = 0; a < actionCount; a++)
                {
                    if (enemy.IsDead || session.Phase == RunPhase.GameOver)
                    {
                        break;
                    }

                    var action = _enemyDecisionPolicy.Decide(enemy, session);
                    _actionResolver.Execute(session, enemy, action, events, ActorExecutionRole.Enemy);
                }

                if (session.Phase == RunPhase.GameOver)
                {
                    break;
                }
            }
        }
    }
}
