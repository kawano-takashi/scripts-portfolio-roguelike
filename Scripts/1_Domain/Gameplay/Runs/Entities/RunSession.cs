using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Items.Entities;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Services.Population.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Domain.Gameplay.Runs.Entities
{
    /// <summary>
    /// 1回の冒険（ラン）の情報をまとめたクラスです。
    /// </summary>
    public class RunSession
    {
        private static readonly Direction[] CardinalDirections =
        {
            Direction.Up,
            Direction.Right,
            Direction.Down,
            Direction.Left
        };

        // 敵のリスト。
        private readonly List<Actor> _enemies = new();
        private readonly ReadOnlyCollection<Actor> _readOnlyEnemies;
        // マップに置かれているアイテムのリスト。
        private readonly List<MapItem> _items = new();
        private readonly ReadOnlyCollection<MapItem> _readOnlyItems;
        // ラン全体の状態遷移イベントを一時的に保持します。
        private readonly List<IRunLifecycleEvent> _lifecycleEvents = new();
        // 部屋の役割割り当て。
        private IReadOnlyList<RoomAssignment> _roomAssignments = Array.Empty<RoomAssignment>();
        // 発動済みのモンスターハウス。
        private readonly HashSet<MapRect> _triggeredMonsterHouses = new();

        /// <summary>
        /// 生成に使ったシード値。
        /// </summary>
        public int Seed { get; }
        /// <summary>
        /// 何階か。
        /// </summary>
        public int Floor { get; private set; }
        /// <summary>
        /// Target floor for clearing the run.
        /// </summary>
        public int ClearFloor { get; }
        /// <summary>
        /// 今までのターン数。
        /// </summary>
        public int TurnCount { get; private set; }
        /// <summary>
        /// ランの状態（プレイ中など）。
        /// </summary>
        public RunPhase Phase { get; private set; }
        /// <summary>
        /// マップ。
        /// </summary>
        public Map Map { get; }
        /// <summary>
        /// プレイヤー。
        /// </summary>
        public Actor Player { get; }
        /// <summary>
        /// 乱数（同じシードなら同じ結果）。
        /// </summary>
        public Random Random { get; }

        /// <summary>
        /// 敵の一覧（読み取り専用）。
        /// </summary>
        public IReadOnlyList<Actor> Enemies => _readOnlyEnemies;
        /// <summary>
        /// アイテムの一覧（読み取り専用）。
        /// </summary>
        public IReadOnlyList<MapItem> Items => _readOnlyItems;

        /// <summary>
        /// 部屋の役割割り当て（読み取り専用）。
        /// </summary>
        public IReadOnlyList<RoomAssignment> RoomAssignments => _roomAssignments;

        /// <summary>
        /// ランを作るときの入口です。
        /// </summary>
        public RunSession(int seed, int floor, Map map, Actor player, int clearFloor, IEnumerable<Actor> enemies = null)
        {
            if (floor <= 0) throw new ArgumentOutOfRangeException(nameof(floor), "Floor must be positive.");
            if (clearFloor <= 0) throw new ArgumentOutOfRangeException(nameof(clearFloor), "Clear floor must be positive.");
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Player = player ?? throw new ArgumentNullException(nameof(player));

            Seed = seed;
            Floor = floor;
            ClearFloor = clearFloor;
            Phase = RunPhase.RunStart;
            Random = new Random(seed);
            _readOnlyEnemies = _enemies.AsReadOnly();
            _readOnlyItems = _items.AsReadOnly();

            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    AddEnemy(enemy);
                }
            }
        }

        /// <summary>
        /// ランをプレイ可能状態に遷移させます。
        /// </summary>
        public void StartRun()
        {
            EnsureTransitionAllowed(from: RunPhase.RunStart, to: RunPhase.InRun);
            Phase = RunPhase.InRun;
        }

        /// <summary>
        /// ランを一時停止状態に遷移させます。
        /// </summary>
        public void PauseRun()
        {
            EnsureTransitionAllowed(from: RunPhase.InRun, to: RunPhase.Pause);
            Phase = RunPhase.Pause;
        }

        /// <summary>
        /// 一時停止中のランを再開します。
        /// </summary>
        public void ResumeRun()
        {
            EnsureTransitionAllowed(from: RunPhase.Pause, to: RunPhase.InRun);
            Phase = RunPhase.InRun;
        }

        /// <summary>
        /// ゲームオーバーを確定し、イベントを記録します。
        /// </summary>
        public void MarkGameOver()
        {
            if (Phase == RunPhase.GameOver || Phase == RunPhase.Clear)
            {
                return;
            }

            if (Phase != RunPhase.RunStart &&
                Phase != RunPhase.InRun &&
                Phase != RunPhase.Pause)
            {
                throw new InvalidOperationException($"Cannot transition to {RunPhase.GameOver} from {Phase}.");
            }

            Phase = RunPhase.GameOver;
            _lifecycleEvents.Add(new RunGameOverEvent(
                floor: Floor,
                totalTurns: TurnCount,
                playerLevel: Player?.LevelProgress.Level ?? 1));
        }

        /// <summary>
        /// クリアを確定し、イベントを記録します。
        /// </summary>
        public void MarkCleared()
        {
            if (Phase == RunPhase.Clear || Phase == RunPhase.GameOver)
            {
                return;
            }

            if (Phase != RunPhase.InRun && Phase != RunPhase.Pause)
            {
                throw new InvalidOperationException($"Cannot transition to {RunPhase.Clear} from {Phase}.");
            }

            Phase = RunPhase.Clear;
            _lifecycleEvents.Add(new RunClearedEvent(
                finalFloor: Floor,
                totalTurns: TurnCount,
                playerLevel: Player?.LevelProgress.Level ?? 1));
        }

        /// <summary>
        /// たまっているラン終了イベントを取り出してクリアします。
        /// </summary>
        public IReadOnlyList<IRunLifecycleEvent> DrainLifecycleEvents()
        {
            if (_lifecycleEvents.Count == 0)
            {
                return Array.Empty<IRunLifecycleEvent>();
            }

            var snapshot = _lifecycleEvents.ToArray();
            _lifecycleEvents.Clear();
            return snapshot;
        }

        /// <summary>
        /// ターン数を1進めます。
        /// </summary>
        public void AdvanceTurn()
        {
            TurnCount++;
        }

        /// <summary>
        /// 敵を追加します。
        /// </summary>
        public void AddEnemy(Actor enemy)
        {
            if (enemy == null) throw new ArgumentNullException(nameof(enemy));
            if (enemy.Faction != Faction.Enemy)
            {
                throw new ArgumentException("Only enemy faction actors can be added to enemies.", nameof(enemy));
            }

            if (enemy.IsDead)
            {
                throw new ArgumentException("Dead actor cannot be added to enemies.", nameof(enemy));
            }

            if (!Map.Contains(enemy.Position))
            {
                throw new ArgumentOutOfRangeException(nameof(enemy), "Enemy position must be inside the map.");
            }

            if (!Map.IsWalkable(enemy.Position))
            {
                throw new ArgumentException("Enemy position must be walkable.", nameof(enemy));
            }

            if (IsOccupied(enemy.Position))
            {
                throw new InvalidOperationException($"Position {enemy.Position} is already occupied.");
            }

            if (ContainsEnemyId(enemy.Id))
            {
                throw new InvalidOperationException($"Enemy id already exists: {enemy.Id}.");
            }

            _enemies.Add(enemy);
        }

        /// <summary>
        /// 敵を削除します。
        /// </summary>
        public void RemoveEnemy(Actor enemy)
        {
            if (enemy == null) return;
            _enemies.Remove(enemy);
        }

        /// <summary>
        /// アイテムを追加します。
        /// </summary>
        public void AddItem(MapItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!Map.Contains(item.Position))
            {
                throw new ArgumentOutOfRangeException(nameof(item), "Item position must be inside the map.");
            }

            if (!Map.IsWalkable(item.Position))
            {
                throw new ArgumentException("Item position must be walkable.", nameof(item));
            }

            if (HasItemAt(item.Position))
            {
                throw new InvalidOperationException($"Item already exists at position {item.Position}.");
            }

            if (ContainsItemId(item.Id))
            {
                throw new InvalidOperationException($"Item id already exists: {item.Id}.");
            }

            _items.Add(item);
        }

        /// <summary>
        /// アイテムを削除します。
        /// </summary>
        public void RemoveItem(MapItem item)
        {
            if (item == null) return;
            _items.Remove(item);
        }

        /// <summary>
        /// 指定位置にあるアイテムを1つ探します。
        /// </summary>
        public MapItem GetItemAt(Position position)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.Position == position)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定位置にいるキャラクター（プレイヤー or 敵）を探します。
        /// </summary>
        public Actor GetActorAt(Position position)
        {
            if (Player.Position == position)
            {
                return Player;
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy.Position == position)
                {
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定位置にいる敵を探します。
        /// </summary>
        public Actor GetEnemyAt(Position position)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy.Position == position)
                {
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>
        /// その位置にだれかがいるかを調べます。
        /// </summary>
        public bool IsOccupied(Position position) => GetActorAt(position) != null;

        /// <summary>
        /// 指定位置にアイテムがあるかを調べます。
        /// </summary>
        public bool HasItemAt(Position position) => GetItemAt(position) != null;
        /// <summary>
        /// セッション管理下のアクターの位置を更新します。
        /// </summary>
        internal bool TrySetActorPosition(Actor actor, Position target, out Actor blocker)
        {
            blocker = null;
            if (!IsManagedActor(actor) || Map == null)
            {
                return false;
            }

            if (!Map.Contains(target) || !Map.IsWalkable(target))
            {
                return false;
            }

            var occupant = GetActorAt(target);
            if (occupant != null && !ReferenceEquals(occupant, actor))
            {
                blocker = occupant;
                return false;
            }

            actor.SetPosition(target);
            return true;
        }

        /// <summary>
        /// セッション管理下のアクターの向きを更新します。
        /// </summary>
        internal bool TrySetActorFacing(Actor actor, Direction direction)
        {
            if (!IsManagedActor(actor))
            {
                return false;
            }

            actor.SetFacing(direction);
            return true;
        }

        /// <summary>
        /// 指定位置に敵対者がいるかを調べます。
        /// </summary>
        public bool HasHostileAt(Position position, Faction selfFaction)
        {
            // プレイヤー/敵を区別せず、そのマスの占有者が敵対勢力かだけを返します。
            var actor = GetActorAt(position);
            return actor != null && !actor.IsDead && actor.Faction != selfFaction;
        }

        /// <summary>
        /// 指定位置の周囲（チェビシェフ距離）に敵対者がいるかを調べます。
        /// </summary>
        public bool HasHostileWithinRange(Position center, Faction selfFaction, int maxChebyshevDistance)
        {
            if (maxChebyshevDistance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxChebyshevDistance), "Max chebyshev distance must be positive.");
            }

            // プレイヤー自身が探索主体でないケース（敵AI等）でも使えるように、全陣営を汎用的に走査します。
            if (Player != null &&
                !Player.IsDead &&
                Player.Faction != selfFaction &&
                IsWithinChebyshevDistance(center, Player.Position, maxChebyshevDistance))
            {
                return true;
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || enemy.IsDead || enemy.Faction == selfFaction)
                {
                    continue;
                }

                if (IsWithinChebyshevDistance(center, enemy.Position, maxChebyshevDistance))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 指定した部屋に敵対者がいるかを調べます。
        /// </summary>
        public bool ContainsHostileInRoom(MapRect room, Faction selfFaction)
        {
            if (Player != null &&
                !Player.IsDead &&
                Player.Faction != selfFaction &&
                room.Contains(Player.Position))
            {
                return true;
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || enemy.IsDead || enemy.Faction == selfFaction)
                {
                    continue;
                }

                if (room.Contains(enemy.Position))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 部屋境界をまたいだ移動かを調べます。
        /// </summary>
        public bool IsRoomBoundaryTransition(Position from, Position to)
        {
            // 「部屋内/外の切り替え」または「異なる部屋間の移動」を境界遷移とみなします。
            var fromInRoom = Map.TryGetRoomAt(from, out var fromRoom);
            var toInRoom = Map.TryGetRoomAt(to, out var toRoom);

            if (fromInRoom != toInRoom)
            {
                return true;
            }

            return fromInRoom && toInRoom && !fromRoom.Equals(toRoom);
        }

        /// <summary>
        /// 指定位置から1歩進めるかを調べます。
        /// </summary>
        public bool CanActorStepFrom(Actor actor, Position from, Direction direction, out Actor blocker)
        {
            blocker = null;
            if (Map == null || actor == null)
            {
                return false;
            }

            var target = DirectionUtility.Apply(from, direction);
            if (!Map.Contains(target) || !Map.IsWalkable(target))
            {
                return false;
            }

            // 斜め移動時の角抜け禁止判定（縦横2マス両方が通行可能であること）。
            if (!DirectionUtility.CanMoveDiagonal(Map, from, direction))
            {
                return false;
            }

            // 呼び出し側が停止理由を組み立てられるよう blocker を返します。
            blocker = GetActorAt(target);
            return blocker == null || blocker == actor;
        }

        /// <summary>
        /// 通路での前進先が1方向だけかどうかを調べます。
        /// </summary>
        public bool TryGetSingleCorridorForwardDirection(
            Actor actor,
            Position current,
            Position previous,
            out Direction nextDirection,
            out CorridorPathState pathState)
        {
            nextDirection = Direction.Down;
            pathState = CorridorPathState.DeadEnd;

            if (Map == null || actor == null)
            {
                return false;
            }

            var candidateCount = 0;
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                var direction = CardinalDirections[i];
                var target = DirectionUtility.Apply(current, direction);
                // 直前マスへの引き返しは候補から除外します。
                if (target == previous)
                {
                    continue;
                }

                if (!CanActorStepFrom(actor, current, direction, out _))
                {
                    continue;
                }

                nextDirection = direction;
                candidateCount++;
                // 候補が2つ以上なら分岐なので「自動旋回しない」と判断します。
                if (candidateCount >= 2)
                {
                    pathState = CorridorPathState.Junction;
                    return false;
                }
            }

            if (candidateCount == 0)
            {
                pathState = CorridorPathState.DeadEnd;
                return false;
            }

            pathState = CorridorPathState.SinglePath;
            return true;
        }

        /// <summary>
        /// 部屋の役割割り当てを設定します。
        /// </summary>
        public void SetRoomAssignments(IReadOnlyList<RoomAssignment> assignments)
        {
            if (assignments == null || assignments.Count == 0)
            {
                _roomAssignments = Array.Empty<RoomAssignment>();
            }
            else
            {
                var copied = new RoomAssignment[assignments.Count];
                for (var i = 0; i < assignments.Count; i++)
                {
                    copied[i] = assignments[i];
                }

                _roomAssignments = copied;
            }

            _triggeredMonsterHouses.Clear();
        }

        /// <summary>
        /// 指定した部屋の役割を取得します。
        /// </summary>
        public RoomRole GetRoomRole(MapRect room)
        {
            for (int i = 0; i < _roomAssignments.Count; i++)
            {
                var assignment = _roomAssignments[i];
                if (assignment.Room.Equals(room))
                {
                    return assignment.Role;
                }
            }

            return RoomRole.Normal;
        }

        /// <summary>
        /// モンスターハウスの発動を試みます。
        /// 発動に成功した場合はtrueを返し、部屋内の敵を起床させます。
        /// </summary>
        public bool TryTriggerMonsterHouse(MapRect room, out int awakenedCount)
        {
            awakenedCount = 0;

            // 既に発動済みなら何もしない
            if (_triggeredMonsterHouses.Contains(room))
            {
                return false;
            }

            // モンスターハウスでなければ何もしない
            var role = GetRoomRole(room);
            if (role != RoomRole.MonsterHouse)
            {
                return false;
            }

            // 発動済みとしてマーク
            _triggeredMonsterHouses.Add(room);

            // 部屋内の敵を起床させる
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy.IsDead)
                {
                    continue;
                }

                if (!room.Contains(enemy.Position))
                {
                    continue;
                }

                // 睡眠状態を解除
                if (enemy.HasStatus(StatusEffectType.Sleep))
                {
                    enemy.RemoveStatus(StatusEffectType.Sleep);
                    awakenedCount++;
                }
            }

            return true;
        }

        /// <summary>
        /// 指定した部屋がモンスターハウスかどうかを判定します。
        /// </summary>
        public bool IsMonsterHouseRoom(MapRect room)
        {
            return GetRoomRole(room) == RoomRole.MonsterHouse;
        }

        /// <summary>
        /// 指定した部屋のモンスターハウスが既に発動済みかどうかを判定します。
        /// </summary>
        public bool IsMonsterHouseTriggered(MapRect room)
        {
            return _triggeredMonsterHouses.Contains(room);
        }

        /// <summary>
        /// 死亡状態のアクターに対するセッション更新を一元処理します。
        /// </summary>
        public bool ResolveDeath(Actor victim, List<IRoguelikeEvent> events)
        {
            if (victim == null || !victim.IsDead)
            {
                return false;
            }

            events?.Add(new ActorDiedEvent(victim.Id));

            if (victim.Faction == Faction.Enemy)
            {
                RemoveEnemy(victim);
                return true;
            }

            MarkGameOver();
            events?.Add(new LogEvent(RunLogCode.PlayerDied));
            return true;
        }

        private bool IsManagedActor(Actor actor)
        {
            if (actor == null)
            {
                return false;
            }

            if (ReferenceEquals(Player, actor))
            {
                return true;
            }

            for (var i = 0; i < _enemies.Count; i++)
            {
                if (ReferenceEquals(_enemies[i], actor))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool IsWithinChebyshevDistance(Position center, Position target, int maxDistance)
        {
            var dx = Math.Abs(target.X - center.X);
            var dy = Math.Abs(target.Y - center.Y);
            if (dx == 0 && dy == 0)
            {
                return false;
            }

            return Math.Max(dx, dy) <= maxDistance;
        }

        private void EnsureTransitionAllowed(RunPhase from, RunPhase to)
        {
            if (Phase != from)
            {
                throw new InvalidOperationException($"Cannot transition to {to} from {Phase}.");
            }
        }

        private bool ContainsEnemyId(ActorId id)
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsItemId(ItemInstanceId id)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}



