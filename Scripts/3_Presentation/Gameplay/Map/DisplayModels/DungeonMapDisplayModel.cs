using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;

namespace Roguelike.Presentation.Gameplay.Map.DisplayModels
{
    /// <summary>
    /// ダンジョンマップ描画向けの表示モデルです。
    /// </summary>
    public sealed class DungeonMapDisplayModel
    {
        private static readonly IReadOnlyList<GroundItemSnapshotDto> EmptyGroundItems =
            Array.Empty<GroundItemSnapshotDto>();
        private static readonly IReadOnlyList<GridPositionDto> EmptyPreviewPositions =
            Array.Empty<GridPositionDto>();

        public MapSnapshotDto Map { get; }
        public IReadOnlyList<GroundItemSnapshotDto> GroundItems { get; }
        public bool IsSpellPreviewOpen { get; }
        public IReadOnlyList<GridPositionDto> SpellPreviewPositions { get; }

        public DungeonMapDisplayModel(
            MapSnapshotDto map,
            IReadOnlyList<GroundItemSnapshotDto> groundItems,
            bool isSpellPreviewOpen,
            IReadOnlyList<GridPositionDto> spellPreviewPositions)
        {
            Map = map;
            GroundItems = groundItems ?? EmptyGroundItems;
            IsSpellPreviewOpen = isSpellPreviewOpen;
            SpellPreviewPositions = spellPreviewPositions ?? EmptyPreviewPositions;
        }
    }
}



