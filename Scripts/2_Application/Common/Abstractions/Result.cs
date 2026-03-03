using System;
using System.Collections.Generic;

namespace Roguelike.Application.Abstractions
{
    /// <summary>
    /// 値を持たない実行結果です。
    /// </summary>
    public readonly struct Result
    {
        private static readonly IReadOnlyList<ValidationError> EmptyValidationErrors = Array.Empty<ValidationError>();

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; }
        public IReadOnlyList<ValidationError> ValidationErrors { get; }

        private Result(bool isSuccess, string errorMessage, IReadOnlyList<ValidationError> validationErrors)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage ?? string.Empty;
            ValidationErrors = validationErrors ?? EmptyValidationErrors;
        }

        public static Result Success()
        {
            return new Result(
                isSuccess: true,
                errorMessage: string.Empty,
                validationErrors: EmptyValidationErrors);
        }

        public static Result Failure(string errorMessage)
        {
            return new Result(
                isSuccess: false,
                errorMessage: string.IsNullOrWhiteSpace(errorMessage) ? "Unexpected failure." : errorMessage,
                validationErrors: EmptyValidationErrors);
        }

        public static Result ValidationFailure(IReadOnlyList<ValidationError> errors)
        {
            return new Result(
                isSuccess: false,
                errorMessage: "Validation failed.",
                validationErrors: errors ?? EmptyValidationErrors);
        }
    }

    /// <summary>
    /// 値を持つ実行結果です。
    /// </summary>
    public readonly struct Result<T>
    {
        private static readonly IReadOnlyList<ValidationError> EmptyValidationErrors = Array.Empty<ValidationError>();

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; }
        public IReadOnlyList<ValidationError> ValidationErrors { get; }
        public T Value { get; }

        private Result(
            bool isSuccess,
            T value,
            string errorMessage,
            IReadOnlyList<ValidationError> validationErrors)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage ?? string.Empty;
            ValidationErrors = validationErrors ?? EmptyValidationErrors;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(
                isSuccess: true,
                value: value,
                errorMessage: string.Empty,
                validationErrors: EmptyValidationErrors);
        }

        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(
                isSuccess: false,
                value: default,
                errorMessage: string.IsNullOrWhiteSpace(errorMessage) ? "Unexpected failure." : errorMessage,
                validationErrors: EmptyValidationErrors);
        }

        public static Result<T> ValidationFailure(IReadOnlyList<ValidationError> errors)
        {
            return new Result<T>(
                isSuccess: false,
                value: default,
                errorMessage: "Validation failed.",
                validationErrors: errors ?? EmptyValidationErrors);
        }
    }
}
