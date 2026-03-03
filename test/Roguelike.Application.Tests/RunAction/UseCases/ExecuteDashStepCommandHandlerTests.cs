using Roguelike.Application.Commands;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Application.UseCases;
using Roguelike.Domain.Gameplay.Actors.Entities;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.UseCases
{
    /// <summary>
    /// ExecuteDashStepCommandHandler の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class ExecuteDashStepCommandHandlerTests
    {
        // 観点: Execute_ReturnsValidationFailure_WhenCommandIsNull の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsValidationFailure_WhenCommandIsNull()
        {
            var store = new ApplicationTestFactory.PersistentRunStoreFake(ApplicationTestFactory.CreateRunSession());
            var handler = CreateHandler(store, new StubDashContinuationPolicy());

            var result = handler.Handle(command: null);

            Assert.True(result.IsFailure);
            Assert.Equal("Validation failed.", result.ErrorMessage);
            Assert.NotEmpty(result.ValidationErrors);
        }

        // 観点: Execute_ReturnsFailure_WhenActiveRunIsMissing の期待挙動を検証する。
        [Fact]
        public void Execute_ReturnsFailure_WhenActiveRunIsMissing()
        {
            var store = new ApplicationTestFactory.PersistentRunStoreFake();
            var handler = CreateHandler(store, new StubDashContinuationPolicy());

            var result = handler.Handle(new DashStepCommand(DirectionDto.Right));

            Assert.True(result.IsFailure);
            Assert.Equal("Active run was not found.", result.ErrorMessage);
        }

        // 観点: Execute_SavesRunAndReturnsResult_WhenExecutionSucceeds の期待挙動を検証する。
        [Fact]
        public void Execute_SavesRunAndReturnsResult_WhenExecutionSucceeds()
        {
            var run = ApplicationTestFactory.CreateRunSession(
                map: ApplicationTestFactory.CreateMap(width: 8, height: 8, start: new Position(1, 1)),
                player: ApplicationTestFactory.CreateActor(position: new Position(1, 1)));
            var store = new ApplicationTestFactory.PersistentRunStoreFake(run);
            var handler = CreateHandler(
                store,
                new StubDashContinuationPolicy
                {
                    BeforeDecision = DashContinuationDecision.Continue(Direction.Right),
                    AfterDecision = DashContinuationDecision.Stop(DashContinuationStopReason.DeadEnd, Direction.Right)
                });

            var result = handler.Handle(new DashStepCommand(DirectionDto.Right));

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.DashStepResult.ShouldContinue);
            Assert.Single(store.SavedSessions);
            Assert.Equal(result.Value.Snapshot.TurnCount, store.Current.TurnCount);
            Assert.Equal(result.Value.Snapshot.PlayerPosition.X, store.Current.Player.Position.X);
            Assert.Equal(result.Value.Snapshot.PlayerPosition.Y, store.Current.Player.Position.Y);
        }

        private static ExecuteDashStepCommandHandler CreateHandler(
            ApplicationTestFactory.PersistentRunStoreFake store,
            IDashContinuationPolicy dashPolicy)
        {
            var executionService = new DashStepExecutionService(
                new TurnEngine(new NullEnemyDecisionPolicy(), new FieldOfViewService()),
                new DashStopPolicy(dashPolicy),
                new RunExecutionResultAssembler(new RunEventProjector()));

            return new ExecuteDashStepCommandHandler(
                store,
                executionService,
                new DashStepCommandValidator());
        }

        private sealed class StubDashContinuationPolicy : IDashContinuationPolicy
        {
            public DashContinuationDecision BeforeDecision { get; set; } = DashContinuationDecision.Continue(Direction.Right);
            public DashContinuationDecision AfterDecision { get; set; } = DashContinuationDecision.Stop(DashContinuationStopReason.BlockedAhead, Direction.Right);

            public DashContinuationDecision EvaluateBeforeStep(RunSession run, Actor actor, Direction direction)
            {
                return BeforeDecision;
            }

            public DashContinuationDecision EvaluateAfterStep(RunSession run, Actor actor, Position previousPosition, Direction currentDirection)
            {
                return AfterDecision;
            }
        }
    }
}
