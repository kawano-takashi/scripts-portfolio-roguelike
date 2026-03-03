// =============================================================================
// IDetectionService.cs
// =============================================================================
// 概要:
//   敵AIがプレイヤーを検知するためのサービスインターフェース。
//   視覚（視線チェック）、聴覚（音の検知）、空間認識（部屋認識）など、
//   複数の感覚モデルによる検知機能を定義します。
//
// 設計意図:
//   - 敵の「気づき」をモデル化し、リアルな追跡・待ち伏せ行動を実現
//   - 検知方法を抽象化することで、敵タイプごとの検知能力を差別化可能
//   - トルネコ/シレンの「起きる」「見つける」「追いかける」を表現
//
// 検知の種類:
//   - 視覚: CanSeePlayer - 視界範囲内かつ視線が通る場合
//   - 空間: IsInSameRoom - 同じ部屋に入った瞬間の検知
//   - 聴覚: CanHearPlayer - 壁越しでも音で気づく
//   - 接近: IsWithinWakeDistance - 睡眠状態からの覚醒判定
//   - 攻撃: CanAttackPlayer - 攻撃射程と視線の両方を考慮
// =============================================================================

using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// 敵のプレイヤー検知サービスのインターフェースです。
    /// </summary>
    /// <remarks>
    /// 視界、部屋認識、音などによる検知を扱います。
    /// 敵AIの状態遷移（睡眠→徘徊→追跡）のトリガーとなる重要な機能です。
    /// </remarks>
    public interface IDetectionService
    {
        /// <summary>
        /// 敵がプレイヤーを視認できるかを判定します。
        /// 視界範囲内かつ視線が通っているかをチェックします。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="player">プレイヤーアクター</param>
        /// <param name="session">ランセッション</param>
        /// <param name="sightRadius">視界半径</param>
        /// <returns>プレイヤーが見えるならtrue</returns>
        bool CanSeePlayer(Actor enemy, Actor player, RunSession session, int sightRadius);

        /// <summary>
        /// 敵とプレイヤーが同じ部屋にいるかを判定します。
        /// 部屋に入った瞬間に気づく処理に使います。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="player">プレイヤーアクター</param>
        /// <param name="map">マップ</param>
        /// <returns>同じ部屋にいるならtrue</returns>
        bool IsInSameRoom(Actor enemy, Actor player, Map map);

        /// <summary>
        /// プレイヤーの行動音が聞こえるかを判定します。
        /// 隣接する部屋など、視界外でも音で気づく処理に使います。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="player">プレイヤーアクター</param>
        /// <param name="map">マップ</param>
        /// <param name="hearingRange">聴覚範囲</param>
        /// <returns>音が聞こえるならtrue</returns>
        bool CanHearPlayer(Actor enemy, Actor player, Map map, int hearingRange);

        /// <summary>
        /// プレイヤーが起動距離内にいるかを判定します。
        /// 睡眠状態の敵が起きる判定に使います。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="player">プレイヤーアクター</param>
        /// <param name="wakeDistance">起動距離</param>
        /// <returns>起動距離内ならtrue</returns>
        bool IsWithinWakeDistance(Actor enemy, Actor player, int wakeDistance);

        /// <summary>
        /// 敵がプレイヤーを攻撃できる射程内にいるかを判定します。
        /// 視線チェックも含みます。
        /// </summary>
        /// <param name="enemy">敵アクター</param>
        /// <param name="player">プレイヤーアクター</param>
        /// <param name="session">ランセッション</param>
        /// <param name="attackRange">攻撃射程</param>
        /// <returns>攻撃可能ならtrue</returns>
        bool CanAttackPlayer(Actor enemy, Actor player, RunSession session, int attackRange);
    }
}


