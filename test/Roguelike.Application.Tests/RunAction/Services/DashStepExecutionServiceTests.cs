using Roguelike.Application.Enums;
using Roguelike.Application.Services;
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

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// DashStepExecutionService の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class DashStepExecutionServiceTests
    {
        // 観点: BuildInvalidStateResult_ReturnsInvalidStateStopReason の期待挙動を検証する。
        [Fact]
        public void BuildInvalidStateResult_ReturnsInvalidStateStopReason()
        {
            var sut = CreateService(new StubDashContinuationPolicy());

            var result = sut.BuildInvalidStateResult((int)Direction.Right);

            Assert.False(result.DashStepResult.ShouldContinue);
            Assert.Equal(DashStopReason.InvalidState, result.DashStepResult.StopReason);
            Assert.False(result.Snapshot.HasRun);
        }

        // 観点: ExecuteStep_ReturnsInvalidState_WhenRunOrPlayerIsMissing の期待挙動を検証する。
        [Fact]
        public void ExecuteStep_ReturnsInvalidState_WhenRunOrPlayerIsMissing()
        {
            var sut = CreateService(new StubDashContinuationPolicy());

            var result = sut.ExecuteStep(run: null, requestedDirection: Direction.Right);

            Assert.False(result.DashStepResult.ShouldContinue);
            Assert.Equal(DashStopReason.InvalidState, result.DashStepResult.StopReason);
        }

        // 観点: ExecuteStep_ReturnsNoopResult_WhenBeforeStepPolicyStops の期待挙動を検証する。
        [Fact]
        public void ExecuteStep_ReturnsNoopResult_WhenBeforeStepPolicyStops()
        {
            var policy = new StubDashContinuationPolicy
            {
                BeforeDecision = DashContinuationDecision.Stop(DashContinuationStopReason.EnemyAhead, Direction.Right)
            };
            var sut = CreateService(policy);
            var run = ApplicationTestFactory.CreateRunSession();

            var result = sut.ExecuteStep(run, Direction.Right);

            Assert.False(result.DashStepResult.ShouldContinue);
            Assert.Equal(DashStopReason.EnemyAhead, result.DashStepResult.StopReason);
            Assert.False(result.DashStepResult.Resolution.TurnConsumed);
            Assert.Equal(run.TurnCount, result.DashStepResult.Resolution.TurnNumber);
        }

        // 観点: ExecuteStep_UsesAfterStepDecision_WhenMoveSucceeds の期待挙動を検証する。
        [Fact]
        public void ExecuteStep_UsesAfterStepDecision_WhenMoveSucceeds()
        {
            var policy = new StubDashContinuationPolicy
            {
                BeforeDecision = DashContinuationDecision.Continue(Direction.Right),
                AfterDecision = DashContinuationDecision.Continue(Direction.Down)
            };
            var sut = CreateService(policy);
            var run = ApplicationTestFactory.CreateRunSession(
                map: ApplicationTestFactory.CreateMap(width: 8, height: 8, start: new Position(1, 1)),
                player: ApplicationTestFactory.CreateActor(position: new Position(1, 1)));

            var result = sut.ExecuteStep(run, Direction.Right);

            Assert.True(result.DashStepResult.Resolution.ActionResolved);
            Assert.True(result.DashStepResult.Resolution.TurnConsumed);
            Assert.True(result.DashStepResult.ShouldContinue);
            Assert.Equal((int)Direction.Down, result.DashStepResult.NextDirectionValue);
        }

        private static DashStepExecutionService CreateService(IDashContinuationPolicy domainPolicy)
        {
            return new DashStepExecutionService(
                new TurnEngine(new NullEnemyDecisionPolicy(), new FieldOfViewService()),
                new DashStopPolicy(domainPolicy),
                new RunExecutionResultAssembler(new RunEventProjector()));
        }

        private sealed class StubDashContinuationPolicy : IDashContinuationPolicy
        {
            public DashContinuationDecision BeforeDecision { get; set; } = DashContinuationDecision.Continue(Direction.Right);
            public DashContinuationDecision AfterDecision { get; set; } = DashContinuationDecision.Stop(DashContinuationStopReason.DeadEnd, Direction.Right);

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
