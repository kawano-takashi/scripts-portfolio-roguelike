using System;
using UnityEngine;

namespace Roguelike.Presentation.Gameplay.Shell.InputRouting
{
    /// <summary>
    /// Configuration for input handlers.
    /// </summary>
    [Serializable]
    public sealed class RunInputSettings
    {
        // Serialized in DungeonBootStrap to allow scene-level tuning.
        [Range(0.1f, 1.0f)]
        [Tooltip("Interval for repeated movement input (seconds).")]
        public float MoveInterval = Constants.INPUT_REPEAT_INTERVAL;

        [Range(0.02f, 1.0f)]
        [Tooltip("Interval for repeated dash movement input (seconds).")]
        public float DashMoveInterval = 0.08f;

        [Tooltip("Whether roguelike input is enabled.")]
        public bool UseRoguelikeRun = true;

        [Tooltip("Scene name to load after clear/game over.")]
        public string ResultTargetSceneName = "DungeonScene";
    }
}



