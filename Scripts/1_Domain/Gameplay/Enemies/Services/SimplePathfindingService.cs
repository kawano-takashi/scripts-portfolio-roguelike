// =============================================================================
// SimplePathfindingService.cs
// =============================================================================
// 概要:
//   IPathfindingServiceを実装するシンプルなパス探索サービス。
//   A* Pathfinding Project Proがない場合のフォールバック実装として機能します。
//
// アルゴリズム:
//   - 経路探索: A*（Chebyshevヒューリスティック、8方向）
//   - 視線判定: Bresenhamのラインアルゴリズム
//   - 距離計算: チェビシェフ距離（8方向移動）、マンハッタン距離（4方向移動）
//
// 制限事項:
//   - タイルコストは全て同一（地形コストは未対応）
//   - maxSearchDistanceを超える経路は探索しない
//   - フォールバック実装として軽量性を優先
//
// 移動ルール:
//   - 8方向移動をサポート
//   - 斜め移動時は角抜けチェック（L字の両方が歩行可能な場合のみ許可）
// =============================================================================

using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// シンプルなパス探索サービスの実装クラスです。
    /// </summary>
    public class SimplePathfindingService : IPathfindingService
    {
        private const int DefaultMaxSearchDistance = 50;

        // 8方向の移動オフセット
        private static readonly (int dx, int dy, Direction dir)[] Directions =
        {
            (0, -1, Direction.Up),
            (1, -1, Direction.UpRight),
            (1, 0, Direction.Right),
            (1, 1, Direction.DownRight),
            (0, 1, Direction.Down),
            (-1, 1, Direction.DownLeft),
            (-1, 0, Direction.Left),
            (-1, -1, Direction.UpLeft)
        };

        /// <inheritdoc/>
        public IReadOnlyList<Position> FindPath(
            Map map,
            Position start,
            Position goal,
            ISet<Position> occupiedPositions = null,
            bool allowOccupiedGoal = false,
            int maxSearchDistance = 50)
        {
            if (!TryFindPath(
                map,
                start,
                goal,
                occupiedPositions,
                allowOccupiedGoal,
                maxSearchDistance,
                out var path))
            {
                return Array.Empty<Position>();
            }

            return path;
        }

        /// <inheritdoc/>
        public Position? GetNextStep(
            Map map,
            Position start,
            Position goal,
            ISet<Position> occupiedPositions = null,
            bool allowOccupiedGoal = false)
        {
            // 1歩だけ必要な場合もA*で最短経路を作り、先頭を返します。
            // allowOccupiedGoal=true のときは「占有されているゴール（例: 追跡対象セル）」への到達を許可します。
            var path = FindPath(
                map,
                start,
                goal,
                occupiedPositions,
                allowOccupiedGoal,
                DefaultMaxSearchDistance);
            if (path.Count == 0)
            {
                return null;
            }

            return path[0];
        }

        private bool TryFindPath(
            Map map,
            Position start,
            Position goal,
            ISet<Position> occupiedPositions,
            bool allowOccupiedGoal,
            int maxSearchDistance,
            out List<Position> path)
        {
            path = new List<Position>();

            // 早期リターン: 無効入力や探索不要のケースを弾く。
            if (map == null)
            {
                return false;
            }

            if (!map.Contains(start) || !map.Contains(goal))
            {
                return false;
            }

            if (start == goal)
            {
                return true;
            }

            if (maxSearchDistance <= 0)
            {
                return false;
            }

            if (!map.IsWalkable(start) || !map.IsWalkable(goal))
            {
                return false;
            }

            // ゴールが占有されていても、追跡文脈では許可できるようにする。
            // ここで常に弾いてしまうと、敵追跡のA*が実質無効化される。
            if (IsBlocked(goal, goal, occupiedPositions, allowOccupiedGoal))
            {
                return false;
            }

            // A*の状態管理: open=未確定、closed=確定、gScore=開始からのコスト。
            var openSet = new NodeHeap();
            var cameFrom = new Dictionary<Position, Position>();
            var gScore = new Dictionary<Position, int>
            {
                [start] = 0
            };
            var closedSet = new HashSet<Position>();
            var sequence = 0;

            var startH = ChebyshevDistance(start, goal);
            openSet.Enqueue(new Node(start, 0, startH, sequence++));

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                // 既に確定済み、または古い情報なら無視。
                if (closedSet.Contains(current.Position))
                {
                    continue;
                }

                if (gScore.TryGetValue(current.Position, out var bestKnown)
                    && current.GScore != bestKnown)
                {
                    continue;
                }

                if (current.Position == goal)
                {
                    // ゴール到達: 親リンクを辿って経路を構築。
                    path = ReconstructPath(cameFrom, current.Position);
                    return true;
                }

                closedSet.Add(current.Position);

                // 探索距離制限。これ以上は掘らない。
                if (current.GScore >= maxSearchDistance)
                {
                    continue;
                }

                foreach (var (dx, dy, dir) in Directions)
                {
                    var next = new Position(current.Position.X + dx, current.Position.Y + dy);

                    if (!map.Contains(next))
                    {
                        continue;
                    }

                    if (!map.IsWalkable(next))
                    {
                        continue;
                    }

                    // 斜め移動の角抜けチェック
                    if (dx != 0 && dy != 0)
                    {
                        var horizontal = new Position(current.Position.X + dx, current.Position.Y);
                        var vertical = new Position(current.Position.X, current.Position.Y + dy);
                        if (!map.IsWalkable(horizontal) || !map.IsWalkable(vertical))
                        {
                            continue;
                        }
                    }

                    if (IsBlocked(next, goal, occupiedPositions, allowOccupiedGoal))
                    {
                        // ゴール以外の占有セルは進入不可。
                        // ゴールのみ allowOccupiedGoal によって例外許可される。
                        continue;
                    }

                    // 移動コストは全て1。
                    var tentativeG = current.GScore + 1;
                    if (tentativeG > maxSearchDistance)
                    {
                        continue;
                    }

                    if (gScore.TryGetValue(next, out var existingG) && tentativeG >= existingG)
                    {
                        continue;
                    }

                    cameFrom[next] = current.Position;
                    gScore[next] = tentativeG;

                    // f = g + h（Chebyshev）で優先順位付け。
                    var hScore = ChebyshevDistance(next, goal);
                    openSet.Enqueue(new Node(next, tentativeG, hScore, sequence++));
                }
            }

            return false;
        }

        private static List<Position> ReconstructPath(
            IReadOnlyDictionary<Position, Position> cameFrom,
            Position current)
        {
            var path = new List<Position>();

            // goal -> start の順に辿ってから反転。
            while (cameFrom.TryGetValue(current, out var previous))
            {
                path.Add(current);
                current = previous;
            }

            path.Reverse();
            return path;
        }

        private sealed class NodeHeap
        {
            private readonly List<Node> _nodes = new List<Node>();

            public int Count => _nodes.Count;

            // fScore最小を取り出すための最小ヒープ。
            public void Enqueue(Node node)
            {
                _nodes.Add(node);
                HeapifyUp(_nodes.Count - 1);
            }

            public Node Dequeue()
            {
                var root = _nodes[0];
                var lastIndex = _nodes.Count - 1;
                var last = _nodes[lastIndex];
                _nodes.RemoveAt(lastIndex);

                if (_nodes.Count > 0)
                {
                    _nodes[0] = last;
                    HeapifyDown(0);
                }

                return root;
            }

            private void HeapifyUp(int index)
            {
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (HasHigherPriority(_nodes[parent], _nodes[index]))
                    {
                        break;
                    }

                    var temp = _nodes[parent];
                    _nodes[parent] = _nodes[index];
                    _nodes[index] = temp;
                    index = parent;
                }
            }

            private void HeapifyDown(int index)
            {
                while (true)
                {
                    int left = (index * 2) + 1;
                    int right = left + 1;
                    int smallest = index;

                    if (left < _nodes.Count && HasHigherPriority(_nodes[left], _nodes[smallest]))
                    {
                        smallest = left;
                    }

                    if (right < _nodes.Count && HasHigherPriority(_nodes[right], _nodes[smallest]))
                    {
                        smallest = right;
                    }

                    if (smallest == index)
                    {
                        break;
                    }

                    var temp = _nodes[smallest];
                    _nodes[smallest] = _nodes[index];
                    _nodes[index] = temp;
                    index = smallest;
                }
            }

            private static bool HasHigherPriority(Node left, Node right)
            {
                // fScore優先、同値ならhScore、最後に挿入順で安定化。
                if (left.FScore != right.FScore)
                {
                    return left.FScore < right.FScore;
                }

                if (left.HScore != right.HScore)
                {
                    return left.HScore < right.HScore;
                }

                return left.Sequence < right.Sequence;
            }
        }

        private readonly struct Node
        {
            // 探索中ノードの最小情報（位置とスコア）。
            public Position Position { get; }
            public int GScore { get; }
            public int HScore { get; }
            public int FScore { get; }
            public int Sequence { get; }

            public Node(Position position, int gScore, int hScore, int sequence)
            {
                Position = position;
                GScore = gScore;
                HScore = hScore;
                FScore = gScore + hScore;
                Sequence = sequence;
            }
        }

        /// <inheritdoc/>
        public Position? GetFleeStep(
            Map map,
            Position start,
            Position threat,
            ISet<Position> occupiedPositions = null)
        {
            if (map == null)
                return null;

            Position? bestPos = null;
            int bestDistance = -1;

            foreach (var (dx, dy, dir) in Directions)
            {
                var next = new Position(start.X + dx, start.Y + dy);

                if (!map.Contains(next))
                    continue;

                if (!map.IsWalkable(next))
                    continue;

                // 斜め移動の角抜けチェック
                if (dx != 0 && dy != 0)
                {
                    var horizontal = new Position(start.X + dx, start.Y);
                    var vertical = new Position(start.X, start.Y + dy);
                    if (!map.IsWalkable(horizontal) || !map.IsWalkable(vertical))
                        continue;
                }

                if (IsOccupied(occupiedPositions, next))
                    continue;

                // 脅威から最も遠い位置を選ぶ
                var distance = ChebyshevDistance(next, threat);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestPos = next;
                }
            }

            return bestPos;
        }

        /// <inheritdoc/>
        public bool HasLineOfSight(Map map, Position from, Position to)
        {
            if (map == null)
                return false;

            // Bresenhamのラインアルゴリズム
            int x0 = from.X, y0 = from.Y;
            int x1 = to.X, y1 = to.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // 開始・終了位置以外で視界をブロックするかチェック
                if ((x0 != from.X || y0 != from.Y) && (x0 != to.X || y0 != to.Y))
                {
                    var pos = new Position(x0, y0);
                    if (!map.Contains(pos) || map.BlocksSight(pos))
                        return false;
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public int ChebyshevDistance(Position a, Position b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        /// <inheritdoc/>
        public int ManhattanDistance(Position a, Position b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static bool IsBlocked(
            Position position,
            Position goal,
            ISet<Position> occupiedPositions,
            bool allowOccupiedGoal)
        {
            // そもそも占有されていないなら通行可能。
            if (!IsOccupied(occupiedPositions, position))
            {
                return false;
            }

            // 追跡対象セルなど、ゴール占有を明示許可するケースのみ通過可。
            if (allowOccupiedGoal && position == goal)
            {
                return false;
            }

            // 占有セルはデフォルトで通行不可。
            return true;
        }

        private static bool IsOccupied(ISet<Position> occupiedPositions, Position position)
        {
            // nullは「追加占有なし」と同義。Mapの地形判定とは独立して扱う。
            return occupiedPositions != null && occupiedPositions.Contains(position);
        }
    }
}


