using System;
using System.Collections.Generic;

namespace Roguelike.Application.Abstractions
{
    /// <summary>
    /// 入力検証のエラー情報です。
    /// </summary>
    public readonly struct ValidationError
    {
        public string Field { get; }
        public string Message { get; }

        public ValidationError(string field, string message)
        {
            Field = field ?? string.Empty;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// 入力検証結果です。
    /// </summary>
    public sealed class ValidationResult
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<ValidationError> Errors => _errors;

        public void AddError(string field, string message)
        {
            _errors.Add(new ValidationError(field, message));
        }

        public static ValidationResult Valid()
        {
            return new ValidationResult();
        }
    }

    /// <summary>
    /// リクエスト検証の契約です。
    /// </summary>
    public interface IValidator<in TRequest>
    {
        ValidationResult Validate(TRequest request);
    }
}
