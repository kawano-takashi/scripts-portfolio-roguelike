using System;
using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Application.Ports
{
    /// <summary>
    /// ラン状態の更新契約です。
    /// </summary>
    public interface IRunWriteStore : IRunReadStore
    {
        void Save(RunSession session);
        void Clear();
    }
}


