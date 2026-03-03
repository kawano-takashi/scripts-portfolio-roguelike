using System;
using System.Collections.Generic;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Dtos;
using Roguelike.Application.Services;

namespace Roguelike.Application.UseCases
{
    /// <summary>
    /// インベントリ表示に必要な参照情報をDTOで提供します。
    /// </summary>
    public sealed class InventoryQueryHandler : IUseCase<Unit, IReadOnlyList<InventoryItemDto>>
    {
        private static readonly IReadOnlyList<InventoryItemDto> EmptyItems = Array.Empty<InventoryItemDto>();

        private readonly InventoryReadModelService _inventoryReadModelService;

        public InventoryQueryHandler(InventoryReadModelService inventoryReadModelService)
        {
            _inventoryReadModelService = inventoryReadModelService
                ?? throw new ArgumentNullException(nameof(inventoryReadModelService));
        }

        public Result<IReadOnlyList<InventoryItemDto>> Handle(Unit _)
        {
            if (!_inventoryReadModelService.TryGetInventoryItems(out var items))
            {
                return Result<IReadOnlyList<InventoryItemDto>>.Failure("Active run was not found.");
            }

            return Result<IReadOnlyList<InventoryItemDto>>.Success(items ?? EmptyItems);
        }

        public Result<IReadOnlyList<InventoryItemDto>> Handle()
        {
            return Handle(Unit.Value);
        }
    }
}
