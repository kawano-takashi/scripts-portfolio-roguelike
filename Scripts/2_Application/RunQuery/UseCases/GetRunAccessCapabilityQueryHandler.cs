using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetRunAccessCapabilityQuery(RunAccessCapability Capability) : IQuery<bool>;

    /// <summary>
    /// 現在のラン状態に対するUI操作可否を判定するクエリです。
    /// </summary>
    public sealed class CanUseCapabilityQueryHandler : IUseCase<GetRunAccessCapabilityQuery, bool>
    {
        private readonly RunAccessQueryService _runAccessQueryService;
        private readonly RunAccessCapabilityPolicy _runAccessCapabilityPolicy;

        public CanUseCapabilityQueryHandler(
            RunAccessQueryService runAccessQueryService,
            RunAccessCapabilityPolicy runAccessCapabilityPolicy)
        {
            _runAccessQueryService = runAccessQueryService
                ?? throw new ArgumentNullException(nameof(runAccessQueryService));
            _runAccessCapabilityPolicy = runAccessCapabilityPolicy
                ?? throw new ArgumentNullException(nameof(runAccessCapabilityPolicy));
        }

        public Result<bool> Handle(GetRunAccessCapabilityQuery query)
        {
            if (query == null)
            {
                return Result<bool>.Failure("Query is required.");
            }

            if (!_runAccessQueryService.TryGetCurrentRunPhase(out var phase))
            {
                return Result<bool>.Failure("Active run was not found.");
            }

            if (!_runAccessCapabilityPolicy.TryResolve(query.Capability, phase, out var canUse))
            {
                return Result<bool>.Failure("Unsupported access capability.");
            }

            return Result<bool>.Success(canUse);
        }
    }
}
