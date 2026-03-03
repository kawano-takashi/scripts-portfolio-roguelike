// =============================================================================
// EnemyAiService.cs (AdvancedEnemyAiService)
// =============================================================================
// 概要:
//   EnemyAIContextの中核サービス。トルネコ/シレン風の状態マシンベースの
//   敵AIを実装します。RunContextのIEnemyAiServiceインターフェースを実装し、
//   Customer-Supplierパターンに基づいてTurnResolverに敵の行動を提供します。
//
// 設計パターン:
//   - State Machine: AiState列挙型による状態遷移管理
//   - Strategy Pattern: IEnemyBehaviorによる行動選択
//   - Priority Queue: 行動を優先度順に評価し、最初に実行可能なものを選択
//
// 状態遷移:
//   Sleeping → (プレイヤー接近) → Wandering
//   Wandering → (プレイヤー発見) → Pursuing
//   Pursuing → (攻撃範囲内) → Attacking
//   Pursuing → (HP低下) → Fleeing
//   Pursuing → (見失う) → Returning
//   Attacking → (範囲外) → Pursuing
//   Fleeing → (十分距離) → Wandering
//   Returning → (目標到達) → Wandering
//
// コンテキストマッピング:
//   - RunContext (Customer): IEnemyAiServiceインターフェースを定義
//   - EnemyAIContext (Supplier): このクラスがインターフェースを実装
// =============================================================================

using System;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Enemies.Behaviors;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;

namespace Roguelike.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// 敵AIサービスの実装クラスです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// トルネコ/シレン風の状態マシンベースのAIを提供します。
    /// 敵ごとにAiMemoryで状態を管理し、EnemyProfileで行動パラメータを定義します。
    /// </para>
    /// <para>
    /// 行動選択はStrategyパターンを使用し、IEnemyBehaviorの実装を
    /// 優先度順に評価して最初に実行可能なものを選択します。
    /// </para>
    /// </remarks>
    public class EnemyAiService : IEnemyDecisionPolicy
    {
        private readonly IFieldOfViewService _fieldOfViewService;
        private readonly IPathfindingService _pathfindingService;
        private readonly IDetectionService _detectionService;

        // 敵ごとのAI記憶
        private readonly Dictionary<ActorId, AiMemory> _memories = new();

        // 行動リスト（優先度順にソートされる）
        private readonly List<IEnemyBehavior> _behaviors;

        private int _currentSeed;
        private bool _hasSeed;

        /// <summary>
        /// EnemyAiServiceを作成します。
        /// </summary>
        public EnemyAiService(
            IFieldOfViewService fieldOfViewService = null,
            IPathfindingService pathfindingService = null,
            IDetectionService detectionService = null)
        {
            _fieldOfViewService = fieldOfViewService ?? new FieldOfViewService();
            _pathfindingService = pathfindingService ?? new SimplePathfindingService();
            _detectionService = detectionService ?? new DetectionService(_fieldOfViewService, _pathfindingService);

            // 行動を登録（優先度順にソートされる）
            _behaviors = new List<IEnemyBehavior>
            {
                new FleeBehavior(_pathfindingService),
                new MeleeAttackBehavior(_detectionService),
                new RangedAttackBehavior(_pathfindingService, _detectionService),
                new PursuitBehavior(_pathfindingService),
                new WanderBehavior(),
                new SleepBehavior(),
                new WaitBehavior()
            };

            // 優先度の高い順にソート
            _behaviors.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <inheritdoc/>
        public RoguelikeAction Decide(Actor enemy, RunSession session)
        {
            if (enemy == null)
                return null;

            if (session == null || session.Player == null)
                return new WaitAction(enemy.Id);

            EnsureSeed(session.Seed);

            if (enemy.IsDead || session.Player.IsDead)
                return new WaitAction(enemy.Id);

            if (session.Phase != RunPhase.InRun)
                return new WaitAction(enemy.Id);

            // メモリとプロファイルを取得
            var memory = GetOrCreateMemory(enemy);
            var profile = GetProfile(enemy);

            // 状態遷移を更新
            UpdateState(enemy, session, memory, profile);

            // 最も優先度の高い実行可能な行動を選択
            foreach (var behavior in _behaviors)
            {
                if (behavior.CanExecute(enemy, session, memory, profile))
                {
                    return behavior.Execute(enemy, session, memory, profile);
                }
            }

            return new WaitAction(enemy.Id);
        }

        /// <inheritdoc/>
        public int GetActionCount(Actor enemy, int turnNumber)
        {
            var profile = GetProfile(enemy);

            return profile.Speed switch
            {
                SpeedType.Half => (turnNumber % 2 == 0) ? 1 : 0,
                SpeedType.Normal => 1,
                SpeedType.Double => 2,
                SpeedType.Triple => 3,
                _ => 1
            };
        }

        /// <inheritdoc/>
        public void ResetMemory(int seed)
        {
            _currentSeed = seed;
            _hasSeed = true;
            _memories.Clear();
        }

        /// <summary>
        /// 敵のAI記憶を取得または作成します。
        /// </summary>
        private AiMemory GetOrCreateMemory(Actor enemy)
        {
            if (!_memories.TryGetValue(enemy.Id, out var memory))
            {
                var profile = GetProfile(enemy);
                memory = new AiMemory(profile.InitialState);
                _memories[enemy.Id] = memory;
            }
            return memory;
        }

        /// <summary>
        /// 敵のプロファイルを取得します。
        /// </summary>
        private EnemyProfile GetProfile(Actor enemy)
        {
            var archetype = enemy?.EnemyArchetype ?? EnemyArchetype.Melee;
            return EnemyDefinitionCatalog.GetProfile(archetype);
        }

        /// <summary>
        /// 状態遷移を更新します。
        /// </summary>
        private void UpdateState(Actor enemy, RunSession session, AiMemory memory, EnemyProfile profile)
        {
            var player = session.Player;
            var canSee = CanSeePlayer(enemy, player, session, profile.SightRadius);
            var distance = ChebyshevDistance(enemy.Position, player.Position);

            // プレイヤーを見たら位置を記録
            if (canSee)
            {
                memory.UpdatePlayerSighting(player.Position);
            }
            else
            {
                memory.IncrementLostTurns();
            }

            // 状態タイマーを進める
            memory.TickStateTimer();

            // 状態遷移
            switch (memory.CurrentState)
            {
                case AiState.Sleeping:
                    // 起動距離内にプレイヤーがいたら起きる
                    if (distance <= profile.WakeDistance || canSee)
                    {
                        memory.ChangeState(AiState.Wandering);
                    }
                    break;

                case AiState.Wandering:
                    // プレイヤーを見たら追跡開始
                    if (canSee)
                    {
                        memory.ChangeState(AiState.Pursuing);
                    }
                    break;

                case AiState.Pursuing:
                    // HP閾値以下なら逃走
                    if (ShouldFlee(enemy, profile))
                    {
                        memory.SetFleeStartHp(enemy.CurrentHp);
                        memory.ChangeState(AiState.Fleeing);
                    }
                    // 攻撃射程内なら攻撃状態
                    else if (canSee && distance <= profile.AttackRange)
                    {
                        memory.ChangeState(AiState.Attacking);
                    }
                    // 見失って忘れたら帰還
                    else if (!canSee && memory.HasForgottenPlayer(profile.ForgetTurns))
                    {
                        memory.ChangeState(AiState.Returning);
                    }
                    break;

                case AiState.Attacking:
                    // HP閾値以下なら逃走
                    if (ShouldFlee(enemy, profile))
                    {
                        memory.SetFleeStartHp(enemy.CurrentHp);
                        memory.ChangeState(AiState.Fleeing);
                    }
                    // 射程外に出たら追跡
                    else if (!canSee || distance > profile.AttackRange)
                    {
                        memory.ChangeState(AiState.Pursuing);
                    }
                    break;

                case AiState.Fleeing:
                    // 十分離れたら徘徊に戻る
                    if (distance > profile.SightRadius * 2)
                    {
                        memory.ChangeState(AiState.Wandering);
                        memory.ClearLastKnownPlayerPosition();
                    }
                    break;

                case AiState.Returning:
                    // 目標位置に到達したら徘徊
                    if (!memory.LastKnownPlayerPosition.HasValue ||
                        enemy.Position == memory.LastKnownPlayerPosition.Value)
                    {
                        memory.ChangeState(AiState.Wandering);
                        memory.ClearLastKnownPlayerPosition();
                    }
                    // プレイヤーを見つけたら追跡
                    else if (canSee)
                    {
                        memory.ChangeState(AiState.Pursuing);
                    }
                    break;

                case AiState.Confused:
                case AiState.Paralyzed:
                    // 状態異常は別のシステムで管理
                    break;
            }
        }

        /// <summary>
        /// 逃走すべきかを判定します。
        /// </summary>
        private bool ShouldFlee(Actor enemy, EnemyProfile profile)
        {
            if (profile.FleeHpThresholdPercent <= 0)
                return false;

            var hpPercent = (enemy.CurrentHp * 100) / enemy.Stats.MaxHp;
            return hpPercent <= profile.FleeHpThresholdPercent;
        }

        /// <summary>
        /// プレイヤーが見えているかを判定します。
        /// </summary>
        private bool CanSeePlayer(Actor enemy, Actor player, RunSession session, int sightRadius)
        {
            if (sightRadius <= 0)
                return false;

            var distance = ChebyshevDistance(enemy.Position, player.Position);
            if (distance > sightRadius)
                return false;

            var visible = _fieldOfViewService.ComputeVisible(session.Map, enemy.Position, sightRadius);
            foreach (var pos in visible)
            {
                if (pos == player.Position)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// シードが変わったら記憶をリセットします。
        /// </summary>
        private void EnsureSeed(int seed)
        {
            if (_hasSeed && _currentSeed == seed)
                return;

            _currentSeed = seed;
            _hasSeed = true;
            _memories.Clear();
        }

        private static int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
    }
}




