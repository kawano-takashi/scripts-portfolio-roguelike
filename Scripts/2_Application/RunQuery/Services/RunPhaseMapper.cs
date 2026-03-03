using Roguelike.Application.Enums;
using Roguelike.Domain.Gameplay.Runs.Enums;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// Domain RunPhase を Application DTO の値へ変換します。
    /// </summary>
    internal static class RunPhaseMapper
    {
        public static RunPhaseDto ToDto(RunPhase phase)
        {
            return phase switch
            {
                RunPhase.RunStart => RunPhaseDto.RunStart,
                RunPhase.InRun => RunPhaseDto.InRun,
                RunPhase.Pause => RunPhaseDto.Pause,
                RunPhase.Clear => RunPhaseDto.Clear,
                RunPhase.GameOver => RunPhaseDto.GameOver,
                _ => RunPhaseDto.None
            };
        }
    }
}


