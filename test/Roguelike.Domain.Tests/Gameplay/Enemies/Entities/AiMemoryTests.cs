using Roguelike.Domain.Gameplay.Enemies.Entities;
using Roguelike.Domain.Gameplay.Enemies.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Xunit;

namespace Roguelike.Tests.Domain.Gameplay.Enemies.Entities
{
    /// <summary>
    /// AiMemory の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class AiMemoryTests
    {
        // 観点: Constructor_SetsDefaults の期待挙動を検証する。
        [Fact]
        public void Constructor_SetsDefaults()
        {
            var sut = new AiMemory();

            Assert.Equal(AiState.Wandering, sut.CurrentState);
            Assert.Equal(0, sut.TurnsSinceLastSeen);
            Assert.Null(sut.LastKnownPlayerPosition);
            Assert.Equal(0, sut.ActionsThisTurn);
        }

        // 観点: UpdatePlayerSighting_StoresPosition_AndResetsLostTurns の期待挙動を検証する。
        [Fact]
        public void UpdatePlayerSighting_StoresPosition_AndResetsLostTurns()
        {
            var sut = new AiMemory();
            sut.IncrementLostTurns();

            sut.UpdatePlayerSighting(new Position(3, 4));

            Assert.Equal(new Position(3, 4), sut.LastKnownPlayerPosition);
            Assert.Equal(0, sut.TurnsSinceLastSeen);
        }

        // 観点: ChangeState_ResetsTimer_WhenStateChanges の期待挙動を検証する。
        [Fact]
        public void ChangeState_ResetsTimer_WhenStateChanges()
        {
            var sut = new AiMemory(AiState.Sleeping);
            sut.TickStateTimer();

            sut.ChangeState(AiState.Wandering);

            Assert.Equal(AiState.Wandering, sut.CurrentState);
            Assert.Equal(0, sut.StateTimer);
        }

        // 観点: HasForgottenPlayer_ReturnsTrue_WhenLostTurnsReachThreshold の期待挙動を検証する。
        [Fact]
        public void HasForgottenPlayer_ReturnsTrue_WhenLostTurnsReachThreshold()
        {
            var sut = new AiMemory();
            sut.IncrementLostTurns();
            sut.IncrementLostTurns();

            Assert.True(sut.HasForgottenPlayer(2));
            Assert.False(sut.HasForgottenPlayer(3));
        }

        // 観点: ResetTurn_ClearsActionCount の期待挙動を検証する。
        [Fact]
        public void ResetTurn_ClearsActionCount()
        {
            var sut = new AiMemory();
            sut.IncrementActionCount();
            sut.IncrementActionCount();

            sut.ResetTurn();

            Assert.Equal(0, sut.ActionsThisTurn);
        }

        // 観点: Reset_RestoresInitialState の期待挙動を検証する。
        [Fact]
        public void Reset_RestoresInitialState()
        {
            var sut = new AiMemory();
            sut.UpdatePlayerSighting(new Position(5, 5));
            sut.IncrementLostTurns();
            sut.SetAmbushing(true);
            sut.SetPatrolTarget(new Position(1, 2));

            sut.Reset(AiState.Sleeping);

            Assert.Equal(AiState.Sleeping, sut.CurrentState);
            Assert.Null(sut.LastKnownPlayerPosition);
            Assert.Equal(0, sut.TurnsSinceLastSeen);
            Assert.False(sut.IsAmbushing);
            Assert.Null(sut.PatrolTarget);
        }
    }
}
