using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetSpellPreviewQuery(Guid ItemId) : IQuery<SpellPreviewDto>;

    /// <summary>
    /// 現在のラン状態から、呪文プレビュー用の座標列を計算します。
    /// </summary>
    public sealed class SpellPreviewQueryHandler : IUseCase<GetSpellPreviewQuery, SpellPreviewDto>
    {
        private readonly SpellPreviewQueryService _spellPreviewQueryService;

        public SpellPreviewQueryHandler(SpellPreviewQueryService spellPreviewQueryService)
        {
            _spellPreviewQueryService = spellPreviewQueryService
                ?? throw new ArgumentNullException(nameof(spellPreviewQueryService));
        }

        public Result<SpellPreviewDto> Handle(GetSpellPreviewQuery query)
        {
            if (query == null)
            {
                return Result<SpellPreviewDto>.Failure("Query is required.");
            }

            if (query.ItemId == Guid.Empty)
            {
                return Result<SpellPreviewDto>.Failure("ItemId is required.");
            }

            if (!_spellPreviewQueryService.TryBuildSpellPreview(query.ItemId, out var previewPositions))
            {
                return Result<SpellPreviewDto>.Failure("Spell preview is unavailable.");
            }

            return Result<SpellPreviewDto>.Success(new SpellPreviewDto(true, previewPositions));
        }
    }
}
