using Roguelike.Domain.Gameplay.Runs.Entities;

namespace Roguelike.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// ランの中に敵やアイテムを配置するための約束です。
    /// </summary>
    public interface IRunPopulationService
    {
        /// <summary>
        /// 指定されたランに、敵やアイテムを置きます。
        /// </summary>
        void Populate(RunSession session);
    }
}


