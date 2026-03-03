using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// 探索中アクションコマンドの入力妥当性を検証します。
    /// </summary>
    public sealed class RunActionCommandValidator : IValidator<RunActionCommand>
    {
        public ValidationResult Validate(RunActionCommand command)
        {
            var result = ValidationResult.Valid();
            if (command == null)
            {
                result.AddError(nameof(command), "Command is required.");
                return result;
            }

            switch (command)
            {
                case MoveRunActionCommand move:
                    ValidateDirection(result, move.Direction, nameof(move.Direction));
                    break;

                case ChangeFacingRunActionCommand changeFacing:
                    ValidateDirection(result, changeFacing.Direction, nameof(changeFacing.Direction));
                    break;

                case UseItemRunActionCommand useItem:
                    ValidateItemId(result, useItem.ItemId, nameof(useItem.ItemId));
                    break;

                case DropItemRunActionCommand dropItem:
                    ValidateItemId(result, dropItem.ItemId, nameof(dropItem.ItemId));
                    break;

                case ToggleEquipItemRunActionCommand toggleEquip:
                    ValidateItemId(result, toggleEquip.ItemId, nameof(toggleEquip.ItemId));
                    break;
            }

            return result;
        }

        private static void ValidateDirection(ValidationResult result, DirectionDto direction, string fieldName)
        {
            if (!Enum.IsDefined(typeof(DirectionDto), direction))
            {
                result.AddError(fieldName, "Direction is invalid.");
            }
        }

        private static void ValidateItemId(ValidationResult result, Guid itemId, string fieldName)
        {
            if (itemId == Guid.Empty)
            {
                result.AddError(fieldName, "ItemId is required.");
            }
        }
    }
}
