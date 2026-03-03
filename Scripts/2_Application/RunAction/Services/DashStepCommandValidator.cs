using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ダッシュ1ステップコマンドの入力妥当性を検証します。
    /// </summary>
    public sealed class DashStepCommandValidator : IValidator<DashStepCommand>
    {
        public ValidationResult Validate(DashStepCommand command)
        {
            var result = ValidationResult.Valid();
            if (command == null)
            {
                result.AddError(nameof(command), "Command is required.");
                return result;
            }

            if (!Enum.IsDefined(typeof(DirectionDto), command.RequestedDirection))
            {
                result.AddError(nameof(command.RequestedDirection), "RequestedDirection is invalid.");
            }

            return result;
        }
    }
}
