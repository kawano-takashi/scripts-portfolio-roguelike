/// <summary>
/// 定数クラス
/// ただの数値や文字列を定義するクラスです。
/// これらの値はゲーム全体で共通して使用されるため、ここにまとめて管理します。
/// そのため、UnityEngine 固有のクラスや機能は使用しません。
/// 例えば、ゲームのマップサイズやミニマップの設定、アニメーションの時間などを定義します。
/// </summary>
public static class Constants
{
    // 3D表示用の定数
    public static readonly int VIEW_RANGE_FORWARD = 2;
    public static readonly int VIEW_RANGE_SIDE = 3;
    public static readonly float PREFAB_SIZE = 1.0f;
    public static readonly float CAMERA_HEIGHT = 0.5f;
    public static readonly float FLOOR_HEIGHT = 1f;
    public static readonly float CEILING_HEIGHT = 3f;
    public static readonly float WALL_HEIGHT = 1f;
    public static readonly float TREASURE_HEIGHT = 0.4f;
    public static readonly float ENTRY_HEIGHT = 0.5f;
    public static readonly float STAIRS_DOWN_HEIGHT = -0.78f;

    // アニメーション関連の定数
    public static readonly float MOVE_DURATION = 0.15f;      // 移動アニメーション所要時間（秒）

    // Input System 関連の定数
    public static readonly float INPUT_DEAD_ZONE = 0.3f; // 入力のデッドゾーン（この値以下の入力は無視）
    public static readonly float INPUT_REPEAT_INTERVAL = 0.3f; // 入力のリピート間隔（秒）
}
