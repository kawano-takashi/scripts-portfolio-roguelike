using System;
using System.Collections.Generic;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Tests.Application.TestSupport;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// RunExecutionResultAssembler の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunExecutionResultAssemblerTests
    {
        // 観点: Constructor_Throws_WhenProjectorIsNull の期待挙動を検証する。
        [Fact]
        public void Constructor_Throws_WhenProjectorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RunExecutionResultAssembler(null));
        }

        // 観点: ToTurnResult_ProjectsDomainResolution の期待挙動を検証する。
        [Fact]
        public void ToTurnResult_ProjectsDomainResolution()
        {
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());
            var events = new List<IRoguelikeEvent> { new TurnEndedEvent(5) };
            var resolution = new TurnResolution(
                turnConsumed: true,
                turnNumber: 5,
                events: events,
                actionResolved: true,
                playerMoveOutcome: ActorMoveOutcome.None);

            var result = assembler.ToTurnResult(resolution);

            Assert.True(result.TurnConsumed);
            Assert.True(result.ActionResolved);
            Assert.Equal(5, result.TurnNumber);
            Assert.Single(result.Events);
        }

        // 観点: CreateNoopCommandExecutionResult_ReturnsEmpty_WhenRunIsNull の期待挙動を検証する。
        [Fact]
        public void CreateNoopCommandExecutionResult_ReturnsEmpty_WhenRunIsNull()
        {
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());

            var result = assembler.CreateNoopCommandExecutionResult(run: null);

            Assert.False(result.Snapshot.HasRun);
            Assert.False(result.TurnResult.TurnConsumed);
        }

        // 観点: DrainLifecycleEvents_ProjectsAndClearsRunEvents の期待挙動を検証する。
        [Fact]
        public void DrainLifecycleEvents_ProjectsAndClearsRunEvents()
        {
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());
            var run = ApplicationTestFactory.CreateRunSession();
            run.MarkGameOver();

            var first = assembler.DrainLifecycleEvents(run);
            var second = assembler.DrainLifecycleEvents(run);

            var evt = Assert.Single(first);
            Assert.Equal(Roguelike.Application.Enums.RunLifecycleEventKind.RunGameOver, evt.Kind);
            Assert.Empty(second);
        }

        // 観点: BuildRunStartResult_UsesStartedFlagForSnapshotHasRun の期待挙動を検証する。
        [Fact]
        public void BuildRunStartResult_UsesStartedFlagForSnapshotHasRun()
        {
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());
            var run = ApplicationTestFactory.CreateRunSession();

            var result = assembler.BuildRunStartResult(run, started: false);

            Assert.False(result.Started);
            Assert.False(result.Snapshot.HasRun);
        }

        // 観点: BuildFloorAdvanceResult_UsesAdvancedFlagForSnapshotHasRun の期待挙動を検証する。
        [Fact]
        public void BuildFloorAdvanceResult_UsesAdvancedFlagForSnapshotHasRun()
        {
            var assembler = new RunExecutionResultAssembler(new RunEventProjector());
            var run = ApplicationTestFactory.CreateRunSession();

            var result = assembler.BuildFloorAdvanceResult(run, advanced: true);

            Assert.True(result.Advanced);
            Assert.True(result.Snapshot.HasRun);
        }
    }
}
