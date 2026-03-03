using System;
using Xunit;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Runs.Events;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// RunEventProjector の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunEventProjectorTests
    {
        // 観点: ProjectDomainEvents_UnknownEvent_ProjectsUnknownDto の期待挙動を検証する。
        [Fact]
        public void ProjectDomainEvents_UnknownEvent_ProjectsUnknownDto()
        {
            // 未知のドメインイベントが Unknown DTO として投影され、種別とターン番号が保持されることを確認する。
            var projector = new RunEventProjector();
            var events = new IRoguelikeEvent[] { new UnknownDomainEvent() };

            var projected = projector.ProjectDomainEvents(events, turnNumber: 7);

            var unknown = Assert.IsType<UnknownRunEventDto>(Assert.Single(projected));
            Assert.Equal(RunEventKind.Unknown, unknown.Kind);
            Assert.Equal(7, unknown.TurnNumber);
            Assert.Equal(typeof(UnknownDomainEvent).FullName, unknown.SourceEventTypeName);
        }

        // 観点: ProjectDomainLifecycleEvents_UnknownEvent_ProjectsUnknownDto の期待挙動を検証する。
        [Fact]
        public void ProjectDomainLifecycleEvents_UnknownEvent_ProjectsUnknownDto()
        {
            // 未知のライフサイクルイベントが Unknown DTO として投影され、元イベント型名が残ることを確認する。
            var projector = new RunEventProjector();
            var events = new IRunLifecycleEvent[] { new UnknownLifecycleEvent() };

            var projected = projector.ProjectDomainLifecycleEvents(events);

            var projectedEvent = Assert.Single(projected);
            Assert.Equal(RunLifecycleEventKind.Unknown, projectedEvent.Kind);
            Assert.Equal(typeof(UnknownLifecycleEvent).FullName, projectedEvent.SourceEventTypeName);
        }

        private sealed class UnknownDomainEvent : IRoguelikeEvent
        {
        }

        private sealed class UnknownLifecycleEvent : IRunLifecycleEvent
        {
        }
    }
}




