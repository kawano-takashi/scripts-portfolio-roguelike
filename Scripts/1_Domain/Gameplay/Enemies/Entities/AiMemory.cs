// =============================================================================
// AiMemory.cs
// =============================================================================
// 概要:
//   敵AIの記憶を保持するクラス。各敵ごとにインスタンスを持ち、
//   プレイヤーの最後に見た位置、現在のAI状態、見失ってからの経過ターン等を
//   追跡します。
//
// コンテキスト配置:
//   EnemyAIContext/Entities
//   - ActorContextではなくEnemyAIContextに配置している理由:
//     - CurrentState, LastKnownPlayerPosition等は純粋なAI状態
//     - Actorのドメインモデル（HP、位置等）とは独立した概念
//     - AIシステム固有の関心事
//
// 状態管理:
//   - CurrentState: 現在のAI状態（AiState enum）
//   - StateTimer: 現在の状態に入ってからの経過ターン数
//
// プレイヤー追跡:
//   - LastKnownPlayerPosition: 最後にプレイヤーを見た位置
//   - TurnsSinceLastSeen: プレイヤーを見失ってからの経過ターン
//   - HasForgottenPlayer(): ForgetTurnsを超えたかの判定
//
// 特殊状態:
//   - IsAmbushing: 待ち伏せ中フラグ（Ambush能力用）
//   - FleeStartHp: 逃走開始時のHP（逃走判定用）
//   - PatrolTarget: 巡回ターゲット位置
//   - ActionsThisTurn: このターンの行動回数（倍速敵用）
//
// ライフサイクル:
//   - コンストラクタで初期状態を設定
//   - UpdatePlayerSighting(): プレイヤーを見たときに呼び出し
//   - IncrementLostTurns(): プレイヤーを見失ったターンに呼び出し
//   - ChangeState(): 状態遷移時に呼び出し（タイマーリセット）
//   - Reset(): 新フロア移動時等に完全リセット
// =============================================================================

using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Enemies.Entities
{
    /// <summary>
    /// 敵AIの記憶を保持するクラスです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 各敵ごとにインスタンスを持ち、プレイヤーの最後に見た位置、
    /// 現在のAI状態、見失ってからの経過ターン等を追跡します。
    /// </para>
    /// <para>
    /// ActorContextではなくEnemyAIContextに配置している理由は、
    /// これらの状態がAI固有の関心事であり、
    /// Actorのドメインモデルとは独立しているためです。
    /// </para>
    /// </remarks>
    public class AiMemory
    {
        /// <summary>
        /// 現在のAI状態。
        /// </summary>
        public AiState CurrentState { get; private set; }

        /// <summary>
        /// 最後にプレイヤーを見た位置。
        /// nullの場合はプレイヤーを見たことがありません。
        /// </summary>
        public Position? LastKnownPlayerPosition { get; private set; }

        /// <summary>
        /// プレイヤーを見失ってからの経過ターン数。
        /// プレイヤーが見えている間は0です。
        /// </summary>
        public int TurnsSinceLastSeen { get; private set; }

        /// <summary>
        /// 巡回のターゲット位置（徘徊時など）。
        /// </summary>
        public Position? PatrolTarget { get; private set; }

        /// <summary>
        /// 現在の状態に入ってからの経過ターン数。
        /// </summary>
        public int StateTimer { get; private set; }

        /// <summary>
        /// 分裂元の敵ID（分裂で生まれた敵の場合）。
        /// </summary>
        public string ParentId { get; private set; }

        /// <summary>
        /// 逃走開始時のHP。
        /// 逃走判定に使います。
        /// </summary>
        public int FleeStartHp { get; private set; }

        /// <summary>
        /// 待ち伏せ中かどうか。
        /// Ambush能力を持つ敵が使います。
        /// </summary>
        public bool IsAmbushing { get; private set; }

        /// <summary>
        /// このターンで既に行動したかどうか。
        /// 倍速敵の行動管理に使います。
        /// </summary>
        public int ActionsThisTurn { get; private set; }

        /// <summary>
        /// デフォルトの記憶を作成します。
        /// </summary>
        public AiMemory()
        {
            CurrentState = AiState.Wandering;
            LastKnownPlayerPosition = null;
            TurnsSinceLastSeen = 0;
            PatrolTarget = null;
            StateTimer = 0;
            ParentId = null;
            FleeStartHp = 0;
            IsAmbushing = false;
            ActionsThisTurn = 0;
        }

        /// <summary>
        /// 初期状態を指定して記憶を作成します。
        /// </summary>
        public AiMemory(AiState initialState)
        {
            CurrentState = initialState;
            LastKnownPlayerPosition = null;
            TurnsSinceLastSeen = 0;
            PatrolTarget = null;
            StateTimer = 0;
            ParentId = null;
            FleeStartHp = 0;
            IsAmbushing = initialState == AiState.Sleeping;
            ActionsThisTurn = 0;
        }

        /// <summary>
        /// プレイヤーを見た位置を記録します。
        /// 見失いカウンターをリセットします。
        /// </summary>
        public void UpdatePlayerSighting(Position position)
        {
            LastKnownPlayerPosition = position;
            TurnsSinceLastSeen = 0;
        }

        /// <summary>
        /// 最後に見たプレイヤー位置を明示的に消去します。
        /// </summary>
        public void ClearLastKnownPlayerPosition()
        {
            LastKnownPlayerPosition = null;
        }

        /// <summary>
        /// プレイヤーを見失ったターンを記録します。
        /// </summary>
        public void IncrementLostTurns()
        {
            TurnsSinceLastSeen++;
        }

        /// <summary>
        /// 指定ターン数を超えて見失っているかを判定します。
        /// </summary>
        public bool HasForgottenPlayer(int forgetTurns)
        {
            return TurnsSinceLastSeen >= forgetTurns;
        }

        /// <summary>
        /// 状態を変更します。
        /// タイマーをリセットします。
        /// </summary>
        public void ChangeState(AiState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                StateTimer = 0;
            }
        }

        /// <summary>
        /// 状態タイマーを進めます。
        /// </summary>
        public void TickStateTimer()
        {
            StateTimer++;
        }

        /// <summary>
        /// 逃走開始時のHPを記録します。
        /// </summary>
        public void SetFleeStartHp(int hp)
        {
            FleeStartHp = hp;
        }

        /// <summary>
        /// 巡回ターゲットを設定します。
        /// </summary>
        public void SetPatrolTarget(Position? patrolTarget)
        {
            PatrolTarget = patrolTarget;
        }

        /// <summary>
        /// 分裂元IDを設定します。
        /// </summary>
        public void SetParentId(string parentId)
        {
            ParentId = parentId;
        }

        /// <summary>
        /// 待ち伏せ状態を切り替えます。
        /// </summary>
        public void SetAmbushing(bool isAmbushing)
        {
            IsAmbushing = isAmbushing;
        }

        /// <summary>
        /// ターン開始時にリセットします。
        /// </summary>
        public void ResetTurn()
        {
            ActionsThisTurn = 0;
        }

        /// <summary>
        /// 行動カウントを増やします。
        /// </summary>
        public void IncrementActionCount()
        {
            ActionsThisTurn++;
        }

        /// <summary>
        /// 記憶を完全にリセットします。
        /// 新しいフロアに移動したときなどに使います。
        /// </summary>
        public void Reset(AiState initialState = AiState.Wandering)
        {
            CurrentState = initialState;
            LastKnownPlayerPosition = null;
            TurnsSinceLastSeen = 0;
            PatrolTarget = null;
            StateTimer = 0;
            ParentId = null;
            FleeStartHp = 0;
            IsAmbushing = false;
            ActionsThisTurn = 0;
        }
    }
}




