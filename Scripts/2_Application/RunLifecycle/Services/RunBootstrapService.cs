using System;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Application.Services
{
    /// <summary>
    /// ラン開始/遷移時のマップ初期化処理を担当します。
    /// </summary>
    public sealed class RunBootstrapService
    {
        public Position ResolveStartPosition(Map map)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            var start = map.StartPosition ?? FindFirstWalkable(map);
            if (!start.HasValue)
            {
                throw new InvalidOperationException("Failed to determine a valid start position.");
            }

            return start.Value;
        }

        public void ApplyInitialVisibility(RunSession session)
        {
            if (session?.Map == null || session.Player == null)
            {
                return;
            }

            session.Map.ApplyVisibilityByRoomOrRadius(session.Player.Position, 1);
        }

        private static Position? FindFirstWalkable(Map map)
        {
            foreach (var position in map.AllPositions())
            {
                if (map.IsWalkable(position))
                {
                    return position;
                }
            }

            return null;
        }
    }
}


