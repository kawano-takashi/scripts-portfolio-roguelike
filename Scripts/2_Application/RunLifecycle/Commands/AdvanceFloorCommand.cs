using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;

namespace Roguelike.Application.Commands
{
    /// <summary>
    /// 次フロア遷移コマンドです。
    /// </summary>
    public sealed record AdvanceFloorCommand() : ICommand<FloorAdvanceResultDto>;
}
