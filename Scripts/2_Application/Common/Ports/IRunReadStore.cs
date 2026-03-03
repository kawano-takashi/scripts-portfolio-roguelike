using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Application.Ports
{
    /// <summary>
    /// ラン状態の参照契約です。
    /// </summary>
    public interface IRunReadStore
    {
        bool TryGetCurrent(out RunSession run);
        bool HasRun { get; }
    }
}
