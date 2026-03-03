using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record HasActiveRunQuery() : IQuery<bool>;

    /// <summary>
    /// ランが存在するかどうかを判定するクエリです。
    /// </summary>
    public sealed class HasActiveRunQueryHandler : IUseCase<HasActiveRunQuery, bool>
    {
        private readonly RunAccessQueryService _runAccessQueryService;

        public HasActiveRunQueryHandler(RunAccessQueryService runAccessQueryService)
        {
            _runAccessQueryService = runAccessQueryService
                ?? throw new ArgumentNullException(nameof(runAccessQueryService));
        }

        public Result<bool> Handle(HasActiveRunQuery query)
        {
            if (query == null)
            {
                return Result<bool>.Failure("Query is required.");
            }

            return Result<bool>.Success(_runAccessQueryService.HasActiveRun());
        }
    }
}
