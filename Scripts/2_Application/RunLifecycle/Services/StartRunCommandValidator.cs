using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ラン開始コマンドの入力妥当性を検証します。
    /// </summary>
    public sealed class StartRunCommandValidator : IValidator<StartRunCommand>
    {
        public ValidationResult Validate(StartRunCommand command)
        {
            var result = ValidationResult.Valid();
            if (command == null)
            {
                result.AddError(nameof(command), "Command is required.");
                return result;
            }

            if (command.Floor <= 0)
            {
                result.AddError(nameof(command.Floor), "Floor must be greater than zero.");
            }

            if (command.ClearFloor <= 0)
            {
                result.AddError(nameof(command.ClearFloor), "ClearFloor must be greater than zero.");
            }
            else if (command.ClearFloor < command.Floor)
            {
                result.AddError(nameof(command.ClearFloor), "ClearFloor must be greater than or equal to Floor.");
            }

            if (command.Width.HasValue ^ command.Height.HasValue)
            {
                result.AddError("MapSize", "Width and Height must be set together.");
            }

            if (command.Width.HasValue && command.Width.Value <= 0)
            {
                result.AddError(nameof(command.Width), "Width must be greater than zero.");
            }

            if (command.Height.HasValue && command.Height.Value <= 0)
            {
                result.AddError(nameof(command.Height), "Height must be greater than zero.");
            }

            if (command.PlayerMaxHp <= 0)
            {
                result.AddError(nameof(command.PlayerMaxHp), "PlayerMaxHp must be greater than zero.");
            }

            if (command.PlayerAttack < 0)
            {
                result.AddError(nameof(command.PlayerAttack), "PlayerAttack must be zero or greater.");
            }

            if (command.PlayerDefense < 0)
            {
                result.AddError(nameof(command.PlayerDefense), "PlayerDefense must be zero or greater.");
            }

            if (command.PlayerIntelligence < 0)
            {
                result.AddError(nameof(command.PlayerIntelligence), "PlayerIntelligence must be zero or greater.");
            }

            if (command.PlayerSightRadius <= 0)
            {
                result.AddError(nameof(command.PlayerSightRadius), "PlayerSightRadius must be greater than zero.");
            }

            if (command.PlayerMaxHunger <= 0f)
            {
                result.AddError(nameof(command.PlayerMaxHunger), "PlayerMaxHunger must be greater than zero.");
            }

            return result;
        }
    }
}
