using System.Collections.Generic;
using Roguelike.Application.Abstractions;
using Xunit;

namespace Roguelike.Tests.Application.Common.Abstractions
{
    /// <summary>
    /// Result の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class ResultTests
    {
        // 観点: Failure_UsesFallbackMessage_WhenErrorMessageIsBlank の期待挙動を検証する。
        [Fact]
        public void Failure_UsesFallbackMessage_WhenErrorMessageIsBlank()
        {
            var result = Result.Failure(" ");

            Assert.True(result.IsFailure);
            Assert.Equal("Unexpected failure.", result.ErrorMessage);
        }

        // 観点: ValidationFailure_StoresValidationErrors の期待挙動を検証する。
        [Fact]
        public void ValidationFailure_StoresValidationErrors()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("Field", "Invalid")
            };

            var result = Result.ValidationFailure(errors);

            Assert.True(result.IsFailure);
            Assert.Equal("Validation failed.", result.ErrorMessage);
            Assert.Single(result.ValidationErrors);
        }

        // 観点: GenericSuccess_StoresValue の期待挙動を検証する。
        [Fact]
        public void GenericSuccess_StoresValue()
        {
            var result = Result<int>.Success(42);

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
            Assert.Empty(result.ValidationErrors);
        }

        // 観点: GenericFailure_UsesFallbackMessage_WhenBlank の期待挙動を検証する。
        [Fact]
        public void GenericFailure_UsesFallbackMessage_WhenBlank()
        {
            var result = Result<int>.Failure(string.Empty);

            Assert.True(result.IsFailure);
            Assert.Equal("Unexpected failure.", result.ErrorMessage);
            Assert.Equal(default, result.Value);
        }

        // 観点: GenericValidationFailure_KeepsErrors の期待挙動を検証する。
        [Fact]
        public void GenericValidationFailure_KeepsErrors()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("A", "B"),
                new ValidationError("C", "D")
            };

            var result = Result<int>.ValidationFailure(errors);

            Assert.True(result.IsFailure);
            Assert.Equal(2, result.ValidationErrors.Count);
        }
    }
}
