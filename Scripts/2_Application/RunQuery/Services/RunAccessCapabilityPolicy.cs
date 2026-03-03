using Roguelike.Application.Enums;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ラン状態に応じたアクセス可否を判定するポリシーです。
    /// </summary>
    public sealed class RunAccessCapabilityPolicy
    {
        public bool TryResolve(RunAccessCapability capability, RunPhaseDto phase, out bool canUse)
        {
            switch (capability)
            {
                case RunAccessCapability.OpenMenu:
                case RunAccessCapability.OpenInventory:
                case RunAccessCapability.ExplorationInput:
                    canUse = phase == RunPhaseDto.InRun;
                    return true;

                default:
                    canUse = false;
                    return false;
            }
        }
    }
}
