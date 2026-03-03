using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;

namespace Roguelike.Domain.Gameplay.Maps.Services
{
    /// <summary>
    /// 「どこが見えるか」を計算するための約束です。
    /// </summary>
    public interface IFieldOfViewService
    {
        /// <summary>
        /// origin から radius 以内で見えるマスを返します。
        /// </summary>
        IReadOnlyCollection<Position> ComputeVisible(Map map, Position origin, int radius);
    }
}


