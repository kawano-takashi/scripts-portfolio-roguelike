using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Enemies.Behaviors;
using Roguelike.Domain.Gameplay.Enemies.Entities;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Domain.Gameplay.Enemies.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Tests.Domain.TestSupport;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// EnemyBehaviors の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class EnemyBehaviorsTests
    {
        // 観点: MeleeAttackBehavior_CanExecute_WhenAdjacentAndStateAllows の期待挙動を検証する。
        [Fact]
        public void MeleeAttackBehavior_CanExecute_WhenAdjacentAndStateAllows()
        {
            var behavior = new MeleeAttackBehavior();
            var session = DomainTestFactory.CreateRunSession(
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 2)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(3, 2));
            var memory = new AiMemory(AiState.Pursuing);
            var profile = EnemyDefinitionCatalog.GetProfile(EnemyArchetype.Melee);

            var canExecute = behavior.CanExecute(enemy, session, memory, profile);

            Assert.True(canExecute);
            Assert.IsType<AttackAction>(behavior.Execute(enemy, session, memory, profile));
        }

        // 観点: RangedAttackBehavior_CannotExecute_WhenPlayerIsTooClose の期待挙動を検証する。
        [Fact]
        public void RangedAttackBehavior_CannotExecute_WhenPlayerIsTooClose()
        {
            var behavior = new RangedAttackBehavior(pathfindingService: new LineOfSightPathfinding(true));
            var session = DomainTestFactory.CreateRunSession(
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(2, 2)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(3, 2));
            var memory = new AiMemory(AiState.Attacking);
            var profile = EnemyDefinitionCatalog.GetProfile(EnemyArchetype.Ranged);

            var canExecute = behavior.CanExecute(enemy, session, memory, profile);

            Assert.False(canExecute);
        }

        // 観点: PursuitBehavior_UsesPathfindingStep_WhenAvailable の期待挙動を検証する。
        [Fact]
        public void PursuitBehavior_UsesPathfindingStep_WhenAvailable()
        {
            var pathfinding = new FixedStepPathfinding(new Position(4, 2));
            var behavior = new PursuitBehavior(pathfinding);
            var session = DomainTestFactory.CreateRunSession(
                map: CreateFloorMap(8, 8),
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(6, 2)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(3, 2));
            var memory = new AiMemory(AiState.Pursuing);
            memory.UpdatePlayerSighting(new Position(6, 2));
            var profile = EnemyDefinitionCatalog.GetProfile(EnemyArchetype.Melee);

            var action = behavior.Execute(enemy, session, memory, profile);

            var move = Assert.IsType<MoveAction>(action);
            Assert.Equal(Direction.Right, move.Direction);
        }

        // 観点: FleeBehavior_UsesPathfindingFleeStep_WhenAvailable の期待挙動を検証する。
        [Fact]
        public void FleeBehavior_UsesPathfindingFleeStep_WhenAvailable()
        {
            var pathfinding = new FixedFleeStepPathfinding(new Position(1, 1));
            var behavior = new FleeBehavior(pathfinding);
            var session = DomainTestFactory.CreateRunSession(
                map: CreateFloorMap(8, 8),
                player: DomainTestFactory.CreateActor(name: "Player", faction: Faction.Player, position: new Position(4, 4)));
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 2));
            var memory = new AiMemory(AiState.Fleeing);
            var profile = EnemyDefinitionCatalog.GetProfile(EnemyArchetype.Ranged);

            var action = behavior.Execute(enemy, session, memory, profile);

            var move = Assert.IsType<MoveAction>(action);
            Assert.Equal(Direction.UpLeft, move.Direction);
        }

        // 観点: SleepBehavior_ReturnsWaitAction_WhenSleeping の期待挙動を検証する。
        [Fact]
        public void SleepBehavior_ReturnsWaitAction_WhenSleeping()
        {
            var behavior = new SleepBehavior();
            var session = DomainTestFactory.CreateRunSession();
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 2));
            var memory = new AiMemory(AiState.Sleeping);
            var profile = EnemyDefinitionCatalog.GetProfile(EnemyArchetype.Melee);

            Assert.True(behavior.CanExecute(enemy, session, memory, profile));
            Assert.IsType<WaitAction>(behavior.Execute(enemy, session, memory, profile));
        }

        // 観点: WaitBehavior_AlwaysReturnsWaitAction の期待挙動を検証する。
        [Fact]
        public void WaitBehavior_AlwaysReturnsWaitAction()
        {
            var behavior = new WaitBehavior();
            var session = DomainTestFactory.CreateRunSession();
            var enemy = DomainTestFactory.CreateActor(name: "Enemy", faction: Faction.Enemy, position: new Position(2, 2));
            var memory = new AiMemory(AiState.Wandering);
            var profile = EnemyDefinitionCatalog.GetProfile(EnemyArchetype.Melee);

            Assert.True(behavior.CanExecute(enemy, session, memory, profile));
            Assert.IsType<WaitAction>(behavior.Execute(enemy, session, memory, profile));
        }

        private static Map CreateFloorMap(int width, int height)
        {
            var map = new Map(width, height);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    map.SetTileType(new Position(x, y), TileType.Floor);
                }
            }

            return map;
        }

        private sealed class LineOfSightPathfinding : IPathfindingService
        {
            private readonly bool _lineOfSight;

            public LineOfSightPathfinding(bool lineOfSight)
            {
                _lineOfSight = lineOfSight;
            }

            public IReadOnlyList<Position> FindPath(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false, int maxSearchDistance = 50)
            {
                return new List<Position>();
            }

            public Position? GetNextStep(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false)
            {
                return null;
            }

            public Position? GetFleeStep(Map map, Position start, Position threat, ISet<Position> occupiedPositions = null)
            {
                return null;
            }

            public bool HasLineOfSight(Map map, Position from, Position to)
            {
                return _lineOfSight;
            }

            public int ChebyshevDistance(Position a, Position b)
            {
                return System.Math.Max(System.Math.Abs(a.X - b.X), System.Math.Abs(a.Y - b.Y));
            }

            public int ManhattanDistance(Position a, Position b)
            {
                return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);
            }
        }

        private sealed class FixedStepPathfinding : IPathfindingService
        {
            private readonly Position _nextStep;

            public FixedStepPathfinding(Position nextStep)
            {
                _nextStep = nextStep;
            }

            public IReadOnlyList<Position> FindPath(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false, int maxSearchDistance = 50)
            {
                return new List<Position>();
            }

            public Position? GetNextStep(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false)
            {
                return _nextStep;
            }

            public Position? GetFleeStep(Map map, Position start, Position threat, ISet<Position> occupiedPositions = null)
            {
                return null;
            }

            public bool HasLineOfSight(Map map, Position from, Position to)
            {
                return true;
            }

            public int ChebyshevDistance(Position a, Position b)
            {
                return 0;
            }

            public int ManhattanDistance(Position a, Position b)
            {
                return 0;
            }
        }

        private sealed class FixedFleeStepPathfinding : IPathfindingService
        {
            private readonly Position _fleeStep;

            public FixedFleeStepPathfinding(Position fleeStep)
            {
                _fleeStep = fleeStep;
            }

            public IReadOnlyList<Position> FindPath(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false, int maxSearchDistance = 50)
            {
                return new List<Position>();
            }

            public Position? GetNextStep(Map map, Position start, Position goal, ISet<Position> occupiedPositions = null, bool allowOccupiedGoal = false)
            {
                return null;
            }

            public Position? GetFleeStep(Map map, Position start, Position threat, ISet<Position> occupiedPositions = null)
            {
                return _fleeStep;
            }

            public bool HasLineOfSight(Map map, Position from, Position to)
            {
                return true;
            }

            public int ChebyshevDistance(Position a, Position b)
            {
                return 0;
            }

            public int ManhattanDistance(Position a, Position b)
            {
                return 0;
            }
        }
    }
}
