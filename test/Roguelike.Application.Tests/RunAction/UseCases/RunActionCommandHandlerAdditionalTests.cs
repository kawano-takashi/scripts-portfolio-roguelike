using System;
using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.UseCases
{
    /// <summary>
    /// RunActionCommandHandlerAdditional の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunActionCommandHandlerAdditionalTests
    {
        // 観点: Constructor_Throws_WhenDependenciesAreNull の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenDependenciesAreNull()
        {
            var store = new ApplicationTestFactory.PersistentRunStoreFake();
            var resolver = CreateTurnEngine();
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());
            var factory = new RunActionFactory();
            var validator = new RunActionCommandValidator();

            Assert.Throws<ArgumentNullException>(() => new RunActionCommandHandler(null, resolver, assembler, factory, validator));
            Assert.Throws<ArgumentNullException>(() => new RunActionCommandHandler(store, null, assembler, factory, validator));
            Assert.Throws<ArgumentNullException>(() => new RunActionCommandHandler(store, resolver, null, factory, validator));
            Assert.Throws<ArgumentNullException>(() => new RunActionCommandHandler(store, resolver, assembler, null, validator));
            Assert.Throws<ArgumentNullException>(() => new RunActionCommandHandler(store, resolver, assembler, factory, null));
        }

        // 観点: Execute_ReturnsFailure_WhenActionCreationFails の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenActionCreationFails()
        {
            var store = new ApplicationTestFactory.PersistentRunStoreFake(ApplicationTestFactory.CreateRunSession());
            var handler = new RunActionCommandHandler(
                store,
                CreateTurnEngine(),
                new RunExecutionResultAssembler(new RunEventProjector()),
                new RunActionFactory(),
                new RunActionCommandValidator());

            var result = handler.Handle(new MoveRunActionCommand((DirectionDto)999));

            Assert.True(result.IsFailure);
        }

        // 観点: Execute_ReturnsSuccessAndSnapshot_WhenActionResolves の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsSuccessAndSnapshot_WhenActionResolves()
        {
            var run = ApplicationTestFactory.CreateRunSession();
            var store = new ApplicationTestFactory.PersistentRunStoreFake(run);
            var handler = new RunActionCommandHandler(
                store,
                CreateTurnEngine(),
                new RunExecutionResultAssembler(new RunEventProjector()),
                new RunActionFactory(),
                new RunActionCommandValidator());

            var result = handler.Handle(new WaitRunActionCommand());

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Snapshot.HasRun);
            Assert.Single(store.SavedSessions);
            Assert.Equal(result.Value.Snapshot.TurnCount, store.Current.TurnCount);
            Assert.Equal(Roguelike.Domain.Gameplay.Runs.Enums.RunPhase.InRun, store.Current.Phase);
        }

        private static ITurnEngine CreateTurnEngine()
        {
            return new TurnEngine(new NullEnemyDecisionPolicy(), new FieldOfViewService());
        }
    }
}
