using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// DirectionMapper の仕様を検証するユニットテストです。
    /// </summary>
    public sealed class DirectionMapperTests
    {
        // 観点: 定義済みの全 DirectionDto が対応する Domain Direction へ変換されることを検証する。
        [Theory]
        [InlineData(DirectionDto.Up, Direction.Up)]
        [InlineData(DirectionDto.UpRight, Direction.UpRight)]
        [InlineData(DirectionDto.Right, Direction.Right)]
        [InlineData(DirectionDto.DownRight, Direction.DownRight)]
        [InlineData(DirectionDto.Down, Direction.Down)]
        [InlineData(DirectionDto.DownLeft, Direction.DownLeft)]
        [InlineData(DirectionDto.Left, Direction.Left)]
        [InlineData(DirectionDto.UpLeft, Direction.UpLeft)]
        public void ToDomain_MapsAllDefinedValues(DirectionDto input, Direction expected)
        {
            var result = DirectionMapper.ToDomain(input);
            Assert.Equal(expected, result);
        }

        // 観点: 未定義の DirectionDto に対してデフォルト値 (Down) を返すことを検証する。
        [Fact]
        public void ToDomain_ReturnsDown_WhenValueIsUndefined()
        {
            var result = DirectionMapper.ToDomain((DirectionDto)999);
            Assert.Equal(Direction.Down, result);
        }
    }
}
