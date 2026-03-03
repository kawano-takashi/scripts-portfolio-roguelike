namespace Roguelike.Presentation.Gameplay.Shell.InputRouting
{
    /// <summary>
    /// Common interface for handlers that own input subscriptions.
    /// </summary>
    public interface IInputContextHandler
    {
        /// <summary>
        /// Primary context handled by this input handler.
        /// </summary>
        bool IsActiveFor(RunInputContext context);

        /// <summary>
        /// Attach callbacks to input actions. Should be safe to call multiple times.
        /// </summary>
        void Enable();

        /// <summary>
        /// Detach callbacks from input actions. Should be safe to call multiple times.
        /// </summary>
        void Disable();
    }
}



