using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;

namespace Roguelike.Presentation.Gameplay.Map.DisplayModels
{
    /// <summary>
    /// 敵レイヤー描画向けの表示モデルです。
    /// </summary>
    public sealed class EnemyLayerDisplayModel
    {
        private static readonly IReadOnlyList<EnemySnapshotDto> EmptyEnemies = Array.Empty<EnemySnapshotDto>();

        public IReadOnlyList<EnemySnapshotDto> Enemies { get; }

        public EnemyLayerDisplayModel(IReadOnlyList<EnemySnapshotDto> enemies)
        {
            Enemies = enemies ?? EmptyEnemies;
        }
    }
}



