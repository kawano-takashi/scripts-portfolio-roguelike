using System.Collections.Generic;

namespace Roguelike.Application.Dtos
{
    /// <summary>
    /// 呪文プレビューの投影データです。
    /// </summary>
    public readonly struct SpellPreviewDto
    {
        public static SpellPreviewDto Empty => new SpellPreviewDto(false, EmptyPositions);

        private static readonly IReadOnlyList<GridPositionDto> EmptyPositions = new GridPositionDto[0];

        public bool CanPreview { get; }
        public IReadOnlyList<GridPositionDto> PreviewPositions { get; }

        public SpellPreviewDto(bool canPreview, IReadOnlyList<GridPositionDto> previewPositions)
        {
            CanPreview = canPreview;
            PreviewPositions = previewPositions ?? EmptyPositions;
        }
    }
}
