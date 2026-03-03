// =============================================================================
// IPathfindingService.cs
// =============================================================================
// 概要:
//   パス探索サービスのインターフェース。敵AIの追跡・逃走経路計算に使用します。
//
// 設計意図:
//   - インターフェースのみをDomain層で定義し、実装はInfrastructure層で提供
//   - A* Pathfinding Project Pro等の外部ライブラリを差し替え可能にする
//   - SimplePathfindingService（簡易A*）とAStarProPathfindingService（A*）を
//     DIで切り替え可能
//
// 提供メソッド:
//   - FindPath: 完全な経路探索
//   - GetNextStep: 次の一歩のみ取得（軽量版）
//   - GetFleeStep: 逃走方向の次の一歩
//   - HasLineOfSight: 射線チェック
//   - ChebyshevDistance/ManhattanDistance: 距離計算
// =============================================================================

using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// パス探索サービスのインターフェースです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// Domain層ではインターフェースのみ定義し、
    /// 実装はInfrastructure層で行います（A* Pathfinding Project Pro等）。
    /// </para>
    /// <para>
    /// 敵AIの追跡・逃走・射線チェックに使用されます。
    /// </para>
    /// </remarks>
    public interface IPathfindingService
    {
        /// <summary>
        /// 開始位置から目標位置までの最短経路を計算します。
        /// </summary>
        /// <param name="map">マップ情報</param>
        /// <param name="start">開始位置</param>
        /// <param name="goal">目標位置</param>
        /// <param name="occupiedPositions">
        /// 追加で通行不可として扱う占有位置（他アクター等）。
        /// 可変ロジックをFuncで注入せず、入力データとして渡すことで探索ルールをサービス内に閉じ込めます。
        /// </param>
        /// <param name="allowOccupiedGoal">
        /// ゴール占有を許可するか。
        /// 追跡時に「敵対ターゲットが立っているセル」をゴールとして扱うために使用します。
        /// </param>
        /// <param name="maxSearchDistance">探索の最大距離（パフォーマンス制限）</param>
        /// <returns>経路のリスト（startを含まない）。経路が見つからない場合は空リスト</returns>
        IReadOnlyList<Position> FindPath(
            Map map,
            Position start,
            Position goal,
            ISet<Position> occupiedPositions = null,
            bool allowOccupiedGoal = false,
            int maxSearchDistance = 50);

        /// <summary>
        /// 目標に向かう次の一歩を取得します。
        /// FindPathより軽量で、1歩だけ必要な場合に使います。
        /// </summary>
        /// <param name="map">マップ情報</param>
        /// <param name="start">開始位置</param>
        /// <param name="goal">目標位置</param>
        /// <param name="occupiedPositions">追加で通行不可として扱う占有位置（他アクター等）</param>
        /// <param name="allowOccupiedGoal">ゴール占有を許可するか（追跡対象セルを許可する場合など）</param>
        /// <returns>次の位置。移動できない場合はnull</returns>
        Position? GetNextStep(
            Map map,
            Position start,
            Position goal,
            ISet<Position> occupiedPositions = null,
            bool allowOccupiedGoal = false);

        /// <summary>
        /// 目標から離れる次の一歩を取得します（逃走用）。
        /// </summary>
        /// <param name="map">マップ情報</param>
        /// <param name="start">開始位置</param>
        /// <param name="threat">脅威の位置（逃げる対象）</param>
        /// <param name="occupiedPositions">追加で通行不可として扱う占有位置（他アクター等）</param>
        /// <returns>次の位置。移動できない場合はnull</returns>
        Position? GetFleeStep(
            Map map,
            Position start,
            Position threat,
            ISet<Position> occupiedPositions = null);

        /// <summary>
        /// 2点間の直線移動が可能かを判定します（射線チェック）。
        /// </summary>
        /// <param name="map">マップ情報</param>
        /// <param name="from">開始位置</param>
        /// <param name="to">終了位置</param>
        /// <returns>直線移動可能ならtrue</returns>
        bool HasLineOfSight(Map map, Position from, Position to);

        /// <summary>
        /// 2点間のチェビシェフ距離（8方向移動での距離）を計算します。
        /// </summary>
        int ChebyshevDistance(Position a, Position b);

        /// <summary>
        /// 2点間のマンハッタン距離（4方向移動での距離）を計算します。
        /// </summary>
        int ManhattanDistance(Position a, Position b);
    }
}


