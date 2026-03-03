using System.Collections.Generic;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.Services
{
    /// <summary>
    /// EnemyDefinitionCatalog の仕様を検証するユニットテストです。
    /// </summary>
    public sealed class EnemyDefinitionCatalogTests
    {
        // 観点: Get_Throws_WhenArchetypeIsUnknown の期待挙動を検証する。
        [Fact]
        public void Get_Throws_WhenArchetypeIsUnknown()
        {
            Assert.Throws<KeyNotFoundException>(() => EnemyDefinitionCatalog.Get((EnemyArchetype)999));
        }

        // 観点: TryGet_ReturnsFalse_WhenArchetypeIsUnknown の期待挙動を検証する。
        [Fact]
        public void TryGet_ReturnsFalse_WhenArchetypeIsUnknown()
        {
            var result = EnemyDefinitionCatalog.TryGet((EnemyArchetype)999, out var definition);

            Assert.False(result);
            Assert.Equal(default, definition);
        }
    }
}
