using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;

namespace Roguelike.Application.Commands
{
    /// <summary>
    /// 新規ラン開始コマンドです。
    /// </summary>
    public sealed record StartRunCommand(
        string PlayerName,
        int Floor,
        int ClearFloor,
        int? Seed,
        int? Width,
        int? Height,
        bool StartImmediately,
        int PlayerMaxHp,
        int PlayerAttack,
        int PlayerDefense,
        int PlayerIntelligence,
        int PlayerSightRadius,
        float PlayerMaxHunger) : ICommand<RunStartResultDto>;
}
