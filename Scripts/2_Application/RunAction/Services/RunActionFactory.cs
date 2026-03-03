using System;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ApplicationコマンドをDomain Actionへ変換します。
    /// </summary>
    public sealed class RunActionFactory
    {
        public bool TryCreateForCurrentPlayer(RunSession run, RunActionCommand command, out RoguelikeAction action)
        {
            action = null;
            if (run?.Player == null)
            {
                return false;
            }

            return TryCreate(command, run.Player.Id, out action);
        }

        public bool TryCreate(RunActionCommand command, ActorId actorId, out RoguelikeAction action)
        {
            action = null;
            if (command == null)
            {
                return false;
            }

            switch (command)
            {
                case MoveRunActionCommand move:
                    if (!IsDirectionDefined(move.Direction))
                    {
                        return false;
                    }

                    action = new MoveAction(actorId, DirectionMapper.ToDomain(move.Direction));
                    return true;

                case ChangeFacingRunActionCommand changeFacing:
                    if (!IsDirectionDefined(changeFacing.Direction))
                    {
                        return false;
                    }

                    action = new ChangeFacingAction(actorId, DirectionMapper.ToDomain(changeFacing.Direction));
                    return true;

                case PickupItemRunActionCommand _:
                    action = new PickupItemAction(actorId);
                    return true;

                case CastEquippedSpellbookRunActionCommand _:
                    action = new CastEquippedSpellbookAction(actorId);
                    return true;

                case UseItemRunActionCommand useItem:
                    if (useItem.ItemId == Guid.Empty)
                    {
                        return false;
                    }

                    action = new UseItemAction(actorId, new ItemInstanceId(useItem.ItemId));
                    return true;

                case DropItemRunActionCommand dropItem:
                    if (dropItem.ItemId == Guid.Empty)
                    {
                        return false;
                    }

                    action = new DropItemAction(actorId, new ItemInstanceId(dropItem.ItemId));
                    return true;

                case ToggleEquipItemRunActionCommand toggleEquip:
                    if (toggleEquip.ItemId == Guid.Empty)
                    {
                        return false;
                    }

                    action = new ToggleEquipItemAction(actorId, new ItemInstanceId(toggleEquip.ItemId));
                    return true;

                case WaitRunActionCommand _:
                    action = new WaitAction(actorId);
                    return true;

                case SearchRunActionCommand _:
                    action = new SearchAction(actorId);
                    return true;

                case RestRunActionCommand _:
                    action = new RestAction(actorId);
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsDirectionDefined(DirectionDto direction)
        {
            return Enum.IsDefined(typeof(DirectionDto), direction);
        }
    }
}

