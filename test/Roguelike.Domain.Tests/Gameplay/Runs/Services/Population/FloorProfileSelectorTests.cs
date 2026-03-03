using System;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Domain.Gameplay.Runs.Services.Population.Enums;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Runs.Services.Population
{
    /// <summary>
    /// FloorProfileSelector の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class FloorProfileSelectorTests
    {
        // 観点: Select_ReturnsNormal_OnLowFloorWhereProbabilityIsZero の期待挙動を検証する。
        [Fact]
        public void Select_ReturnsNormal_OnLowFloorWhereProbabilityIsZero()
        {
            var sut = new FloorProfileSelector();

            var profile = sut.Select(floorNumber: 2, random: new FixedRandom(0.0));

            Assert.Equal(FloorProfileType.Normal, profile.Type);
            Assert.False(profile.HasMonsterHouse);
        }

        // 観点: Select_ReturnsMonsterHouse_WhenRollIsBelowThreshold の期待挙動を検証する。
        [Fact]
        public void Select_ReturnsMonsterHouse_WhenRollIsBelowThreshold()
        {
            var sut = new FloorProfileSelector();

            var profile = sut.Select(floorNumber: 9, random: new FixedRandom(0.1));

            Assert.Equal(FloorProfileType.MonsterHouse, profile.Type);
            Assert.True(profile.HasMonsterHouse);
        }

        // 観点: Select_ReturnsNormal_WhenRollIsAboveThreshold の期待挙動を検証する。
        [Fact]
        public void Select_ReturnsNormal_WhenRollIsAboveThreshold()
        {
            var sut = new FloorProfileSelector();

            var profile = sut.Select(floorNumber: 9, random: new FixedRandom(0.9));

            Assert.Equal(FloorProfileType.Normal, profile.Type);
            Assert.False(profile.HasMonsterHouse);
        }

        // 観点: Select_Throws_WhenRandomIsNull の期待挙動を検証する。
        [Fact]
        public void Select_Throws_WhenRandomIsNull()
        {
            var sut = new FloorProfileSelector();

            Assert.Throws<ArgumentNullException>(() => sut.Select(floorNumber: 3, random: null));
        }

        private sealed class FixedRandom : System.Random
        {
            private readonly double _value;

            public FixedRandom(double value)
            {
                _value = value;
            }

            public override double NextDouble()
            {
                return _value;
            }
        }
    }
}
