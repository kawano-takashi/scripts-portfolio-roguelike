using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Commands
{
    /// <summary>
    /// 探索中アクション実行の共通コマンドです。
    /// ActorIdは含めず、実行時に現在プレイヤーへ束縛します。
    /// </summary>
    public abstract record RunActionCommand : ICommand<RunCommandExecutionResultDto>;

    public sealed record MoveRunActionCommand(DirectionDto Direction) : RunActionCommand;

    public sealed record ChangeFacingRunActionCommand(DirectionDto Direction) : RunActionCommand;

    public sealed record PickupItemRunActionCommand : RunActionCommand;

    public sealed record CastEquippedSpellbookRunActionCommand : RunActionCommand;

    public sealed record UseItemRunActionCommand(Guid ItemId) : RunActionCommand;

    public sealed record DropItemRunActionCommand(Guid ItemId) : RunActionCommand;

    public sealed record ToggleEquipItemRunActionCommand(Guid ItemId) : RunActionCommand;

    public sealed record WaitRunActionCommand : RunActionCommand;

    public sealed record SearchRunActionCommand : RunActionCommand;

    public sealed record RestRunActionCommand : RunActionCommand;
}
