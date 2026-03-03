namespace Roguelike.Presentation.Gameplay.Shell.InputRouting
{
    /// <summary>
    /// Represents the active input state (screen/situation).
    /// </summary>
    public enum RunInputContext
    {
        Exploration, // Normal exploration input.
        Inventory, // Inventory UI navigation and actions.
        Menu, // Main menu UI navigation.
        Guide, // Operation guide panel.
        FloorConfirm, // Floor transition confirmation dialog.
        SpellPreview, // Spell preview confirmation panel.
        Result, // Clear/GameOver result screen.
        Pause, // Paused state (input usually blocked).
        Blocked // No input should be accepted.
    }
}



