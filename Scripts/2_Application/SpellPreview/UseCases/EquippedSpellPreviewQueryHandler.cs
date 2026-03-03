using System;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    public sealed record GetEquippedSpellPreviewQuery() : IQuery<SpellPreviewDto>;

    /// <summary>
    /// 装備中の Spellbook から呪文プレビュー用の座標列を計算します。
    /// </summary>
    public sealed class EquippedSpellPreviewQueryHandler : IUseCase<GetEquippedSpellPreviewQuery, SpellPreviewDto>
    {
        private readonly SpellPreviewQueryService _spellPreviewQueryService;

        public EquippedSpellPreviewQueryHandler(SpellPreviewQueryService spellPreviewQueryService)
        {
            _spellPreviewQueryService = spellPreviewQueryService
                ?? throw new ArgumentNullException(nameof(spellPreviewQueryService));
        }

        public Result<SpellPreviewDto> Handle(GetEquippedSpellPreviewQuery query)
        {
            if (query == null)
            {
                return Result<SpellPreviewDto>.Failure("Query is required.");
            }

            if (!_spellPreviewQueryService.TryBuildEquippedSpellPreview(out var previewPositions))
            {
                return Result<SpellPreviewDto>.Success(SpellPreviewDto.Empty);
            }

            return Result<SpellPreviewDto>.Success(new SpellPreviewDto(true, previewPositions));
        }
    }
}
