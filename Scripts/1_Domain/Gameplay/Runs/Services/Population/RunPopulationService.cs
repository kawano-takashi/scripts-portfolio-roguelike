using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// 敵やアイテムをランに配置するサービスです。
    /// トルネコ1ベースの段階的パイプラインを使用します。
    /// </summary>
    public class RunPopulationService : IRunPopulationService
    {
        private readonly FloorProfileSelector _profileSelector;
        private readonly RoomRoleAssigner _roomAssigner;
        private readonly SpawnBudgetCalculator _budgetCalculator;
        private readonly PopulationPlanner _planner;

        /// <summary>
        /// RunPopulationServiceを作成します。
        /// </summary>
        public RunPopulationService()
        {
            _profileSelector = new FloorProfileSelector();
            _roomAssigner = new RoomRoleAssigner();
            _budgetCalculator = new SpawnBudgetCalculator();
            _planner = new PopulationPlanner();
        }

        /// <summary>
        /// ランに敵とアイテムを置きます。
        /// </summary>
        public void Populate(RunSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (session.Map == null) throw new ArgumentException("RunSession must have a map.", nameof(session));
            if (session.Player == null) throw new ArgumentException("RunSession must have a player.", nameof(session));

            var random = session.Random;
            if (random == null)
            {
                throw new InvalidOperationException("RunSession must provide a deterministic random source.");
            }
            var floor = session.Floor;
            var map = session.Map;
            var playerPosition = session.Player.Position;

            // Stage 1: フロアの種類を決定
            var profile = _profileSelector.Select(floor, random);

            // Stage 2: 部屋に役割を割り当て
            var roomAssignments = _roomAssigner.Assign(map, profile, playerPosition);

            // Stage 3: 配置予算を計算
            var budget = _budgetCalculator.Calculate(profile, random);

            // Stage 4: 配置計画を作成
            var plan = _planner.CreatePlan(map, roomAssignments, budget, playerPosition, random);

            // Stage 5: 配置計画を実行
            ExecutePlan(session, plan);

            // 部屋の役割をセッションに保存（モンスターハウス検知用）
            session.SetRoomAssignments(plan.RoomAssignments);
        }

        /// <summary>
        /// 配置計画を実行します。
        /// </summary>
        private void ExecutePlan(RunSession session, SpawnPlan plan)
        {
            // 敵を配置
            foreach (var entry in plan.Enemies)
            {
                var enemy = CreateEnemy(entry);
                session.AddEnemy(enemy);
            }

            // アイテムを配置
            foreach (var entry in plan.Items)
            {
                var item = CreateItem(entry);
                session.AddItem(item);
            }
        }

        /// <summary>
        /// 敵を作成します。
        /// </summary>
        private static Actor CreateEnemy(EnemySpawnEntry entry)
        {
            var definition = EnemyDefinitionCatalog.Get(entry.Archetype);
            var enemy = new Actor(
                ActorId.NewId(),
                definition.DisplayName,
                Faction.Enemy,
                entry.Position,
                definition.Stats,
                entry.Archetype);

            // モンスターハウスの敵は睡眠状態で開始
            if (entry.StartsAsleep)
            {
                enemy.AddStatus(StatusEffectType.Sleep, int.MaxValue);
            }

            return enemy;
        }

        /// <summary>
        /// アイテムを作成します。
        /// </summary>
        private static MapItem CreateItem(ItemSpawnEntry entry)
        {
            return MapItem.Create(entry.ItemType, entry.Position, entry.Enhancements);
        }
    }
}


