using Roguelike.Application.Abstractions;
using Xunit;

namespace Roguelike.Tests.Application.Common.Abstractions
{
    /// <summary>
    /// ValidationResult の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class ValidationResultTests
    {
        // 観点: Valid_ReturnsEmptyValidationResult の期待挙動を検証する。
        [Fact]
        public void Valid_ReturnsEmptyValidationResult()
        {
            var result = ValidationResult.Valid();

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        // 観点: AddError_MarksResultAsInvalid の期待挙動を検証する。
        [Fact]
        public void AddError_MarksResultAsInvalid()
        {
            var result = ValidationResult.Valid();

            result.AddError("Field", "Message");

            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Field", error.Field);
            Assert.Equal("Message", error.Message);
        }

        // 観点: ValidationError_AllowsNullInputs_ByNormalizingToEmptyString の期待挙動を検証する。
        [Fact]
        public void ValidationError_AllowsNullInputs_ByNormalizingToEmptyString()
        {
            var error = new ValidationError(null, null);

            Assert.Equal(string.Empty, error.Field);
            Assert.Equal(string.Empty, error.Message);
        }
    }
}
