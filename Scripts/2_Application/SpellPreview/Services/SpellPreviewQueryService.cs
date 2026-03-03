using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Application.Ports;
using Roguelike.Domain.Gameplay.Runs.Services;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// 呪文プレビュー表示向けの座標列を取得するクエリサービスです。
    /// </summary>
    public sealed class SpellPreviewQueryService
    {
        private static readonly IReadOnlyList<GridPositionDto> EmptyPreviewPositions = Array.Empty<GridPositionDto>();

        private readonly InventoryReadModelService _inventoryReadModelService;
        private readonly ISpellTrajectoryService _spellTrajectoryService;
        private readonly IRunReadStore _runReadStore;

        public SpellPreviewQueryService(
            InventoryReadModelService inventoryReadModelService,
            ISpellTrajectoryService spellTrajectoryService,
            IRunReadStore runReadStore)
        {
            _inventoryReadModelService = inventoryReadModelService
                ?? throw new ArgumentNullException(nameof(inventoryReadModelService));
            _spellTrajectoryService = spellTrajectoryService ?? throw new ArgumentNullException(nameof(spellTrajectoryService));
            _runReadStore = runReadStore ?? throw new ArgumentNullException(nameof(runReadStore));
        }

        public bool TryBuildSpellPreview(Guid itemId, out IReadOnlyList<GridPositionDto> previewPositions)
        {
            previewPositions = EmptyPreviewPositions;
            if (!_inventoryReadModelService.TryGetSpellPreviewContext(itemId, out var context))
            {
                return false;
            }

            var domainPositions = _spellTrajectoryService.BuildLinearTrajectory(
                context.Map,
                context.Origin,
                context.Facing,
                context.Range);

            var projected = new List<GridPositionDto>(domainPositions.Count);
            for (var i = 0; i < domainPositions.Count; i++)
            {
                var position = domainPositions[i];
                projected.Add(new GridPositionDto(position.X, position.Y));
            }

            previewPositions = projected;
            return true;
        }

        public bool TryBuildEquippedSpellPreview(out IReadOnlyList<GridPositionDto> previewPositions)
        {
            previewPositions = EmptyPreviewPositions;

            if (!_runReadStore.TryGetCurrent(out var run) ||
                run?.Player?.Equipment == null)
            {
                return false;
            }

            var spellbookId = run.Player.Equipment.SpellbookItemId;
            if (!spellbookId.HasValue)
            {
                return false;
            }

            return TryBuildSpellPreview(spellbookId.Value.Value, out previewPositions);
        }
    }
}


