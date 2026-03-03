using System;
using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;

namespace Roguelike.Domain.Gameplay.Maps.Services
{
    /// <summary>
    /// 視界（見えるマス）を計算するサービスです。
    /// Recursive Shadowcasting の考え方を使っています。
    /// </summary>
    public class FieldOfViewService : IFieldOfViewService
    {
        /// <summary>
        /// origin を中心に、radius 以内で見えるマスを集めて返します。
        /// </summary>
        public IReadOnlyCollection<Position> ComputeVisible(Map map, Position origin, int radius)
        {
            var visible = new HashSet<Position>();

            if (map == null)
            {
                return visible;
            }

            if (!map.Contains(origin))
            {
                return visible;
            }

            // 自分の位置は必ず見える。
            visible.Add(origin);

            if (radius <= 0)
            {
                return visible;
            }

            var radiusSquared = radius * radius;

            // 8方向（8つの扇形）に分けて見える場所を調べます。
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, 1, 0, 0, 1);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, 0, 1, 1, 0);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, 0, 1, -1, 0);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, -1, 0, 0, 1);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, -1, 0, 0, -1);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, 0, -1, -1, 0);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, 0, -1, 1, 0);
            CastLight(map, visible, origin, radius, radiusSquared, 1, 1.0f, 0.0f, 1, 0, 0, -1);

            return visible;
        }

        /// <summary>
        /// 1つの扇形（オクタント）で見える範囲を計算します。
        /// </summary>
        private static void CastLight(
            Map map,
            HashSet<Position> visible,
            Position origin,
            int radius,
            int radiusSquared,
            int row,
            float start,
            float end,
            int xx,
            int xy,
            int yx,
            int yy)
        {
            if (start < end)
            {
                return;
            }

            // 壁にぶつかったかどうかを覚えます。
            var blocked = false;
            var newStart = 0.0f;

            // 距離を少しずつ伸ばしながら調べます。
            for (int distance = row; distance <= radius && !blocked; distance++)
            {
                var deltaY = -distance;

                // その距離の左から右へ向けて調べます。
                for (int deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    var rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (start < rightSlope)
                    {
                        continue;
                    }

                    if (end > leftSlope)
                    {
                        break;
                    }

                    // 斜めの座標を実際のマップ座標に変換します。
                    var mapX = origin.X + deltaX * xx + deltaY * xy;
                    var mapY = origin.Y + deltaX * yx + deltaY * yy;

                    var position = new Position(mapX, mapY);

                    if (!map.Contains(position))
                    {
                        continue;
                    }

                    // 半径の円の中なら「見える」候補に入れます。
                    var distanceSquared = deltaX * deltaX + deltaY * deltaY;
                    if (distanceSquared <= radiusSquared)
                    {
                        visible.Add(position);
                    }

                    var isOpaque = map.BlocksSight(position);

                    if (blocked)
                    {
                        // すでに壁で見えなくなっている時。
                        if (isOpaque)
                        {
                            newStart = rightSlope;
                            continue;
                        }

                        // 壁が途切れたら、また見える範囲を広げます。
                        blocked = false;
                        start = newStart;
                    }
                    else if (isOpaque && distance < radius)
                    {
                        // 壁があったら、その先は別の扇形として再帰的に調べます。
                        blocked = true;
                        CastLight(map, visible, origin, radius, radiusSquared, distance + 1, start, leftSlope, xx, xy, yx, yy);
                        newStart = rightSlope;
                    }
                }
            }
        }
    }
}


