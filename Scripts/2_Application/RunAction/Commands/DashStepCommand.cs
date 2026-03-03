using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Commands
{
    /// <summary>
    /// ダッシュ1ステップ実行コマンドです。
    /// </summary>
    public sealed record DashStepCommand(DirectionDto RequestedDirection) : ICommand<DashStepExecutionResultDto>;
}
