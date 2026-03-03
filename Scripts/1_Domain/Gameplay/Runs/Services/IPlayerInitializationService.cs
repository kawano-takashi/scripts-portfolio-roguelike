using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Runs.Services
{
    /// <summary>
    /// プレイヤー初期化ルールを提供するドメインサービス境界です。
    /// </summary>
    public interface IPlayerInitializationService
    {
        Actor CreateInitialPlayer(
            string playerName,
            int maxHp,
            int attack,
            int defense,
            int intelligence,
            int sightRadius,
            float maxHunger,
            Position startPosition);

        void PreparePlayerForNextFloor(RunSession session, Position startPosition);
    }
}
