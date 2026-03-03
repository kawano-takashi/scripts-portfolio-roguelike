using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Hud.Formatting;
using Roguelike.Presentation.Gameplay.Hud.Types;
using Roguelike.Presentation.Gameplay.CombatPresentation.Types;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Policies
{
    /// <summary>
    /// ターン内イベントを演出再生用のステップ列へ変換します。
    /// </summary>
    public sealed class TurnEventSequencingPolicy
    {
        private readonly RunLogProjectionPolicy _projectionUseCase;

        public TurnEventSequencingPolicy(RunLogProjectionPolicy projectionUseCase)
        {
            _projectionUseCase = projectionUseCase ?? throw new ArgumentNullException(nameof(projectionUseCase));
        }

        public TurnPresentationPlan Build(RunTurnResultDto resolution)
        {
            var events = resolution.Events;
            if (events == null || events.Count == 0)
            {
                return new TurnPresentationPlan(resolution.TurnNumber, Array.Empty<TurnPresentationStep>());
            }

            var stepBuilders = BuildSteps(events);
            if (stepBuilders.Count == 0)
            {
                return new TurnPresentationPlan(resolution.TurnNumber, Array.Empty<TurnPresentationStep>());
            }

            var steps = new List<TurnPresentationStep>(stepBuilders.Count);
            for (var i = 0; i < stepBuilders.Count; i++)
            {
                steps.Add(CreateStep(stepBuilders[i]));
            }

            return new TurnPresentationPlan(resolution.TurnNumber, steps);
        }

        private List<TurnEventStepBuilder> BuildSteps(IReadOnlyList<IRunEventDto> events)
        {
            var steps = new List<TurnEventStepBuilder>();
            TurnEventStepBuilder currentStep = null;

            for (var i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (ShouldStartNewStep(currentStep, evt))
                {
                    currentStep = new TurnEventStepBuilder(evt);
                    steps.Add(currentStep);
                }

                currentStep.AddEvent(evt);

                if (_projectionUseCase.TryProject(evt, out var record))
                {
                    currentStep.LogRecords.Add(record);
                }
            }

            return steps;
        }

        private static TurnPresentationStep CreateStep(TurnEventStepBuilder stepBuilder)
        {
            if (stepBuilder == null)
            {
                return new TurnPresentationStep(
                    StepAnimationType.None,
                    Array.Empty<IRunEventDto>(),
                    Array.Empty<RunLogRecord>(),
                    Array.Empty<ActorDamagedEventDto>(),
                    new GridPositionDto(0, 0),
                    attackRequest: null,
                    spellRequest: null,
                    soundCue: null);
            }

            if (stepBuilder.AnchorEvent is AttackDeclaredEventDto attackAnchor)
            {
                return CreateAttackStep(stepBuilder, attackAnchor);
            }

            if (stepBuilder.AnchorEvent is SpellCastEventDto spellAnchor)
            {
                return CreateSpellStep(stepBuilder, spellAnchor);
            }

            return new TurnPresentationStep(
                StepAnimationType.None,
                stepBuilder.Events,
                stepBuilder.LogRecords,
                stepBuilder.DamageEvents,
                ResolveDamageFallbackPosition(stepBuilder.Events, new GridPositionDto(0, 0)),
                attackRequest: null,
                spellRequest: null,
                soundCue: null);
        }

        private static TurnPresentationStep CreateAttackStep(
            TurnEventStepBuilder stepBuilder,
            AttackDeclaredEventDto attackEvent)
        {
            var showHitEffect = HasMatchingAttackHit(stepBuilder.Events, attackEvent);
            var request = new AttackAnimationRequest(
                attackerId: attackEvent.AttackerActorId,
                targetId: attackEvent.TargetActorId,
                kind: attackEvent.AttackKind,
                attackerPosition: attackEvent.AttackerPosition,
                targetPosition: attackEvent.TargetPosition,
                showHitEffect: showHitEffect);

            return new TurnPresentationStep(
                StepAnimationType.Attack,
                stepBuilder.Events,
                stepBuilder.LogRecords,
                stepBuilder.DamageEvents,
                ResolveDamageFallbackPosition(stepBuilder.Events, attackEvent.TargetPosition),
                request,
                spellRequest: null,
                soundCue: UiSoundCue.Attack);
        }

        private static TurnPresentationStep CreateSpellStep(
            TurnEventStepBuilder stepBuilder,
            SpellCastEventDto spellEvent)
        {
            var request = new SpellAnimationRequest(
                casterId: spellEvent.CasterActorId,
                spell: spellEvent.ItemType,
                casterPosition: spellEvent.CasterPosition,
                casterFacing: spellEvent.Direction,
                targetPosition: spellEvent.TargetPosition,
                targetId: spellEvent.TargetActorId,
                isEquippedSpellCast: spellEvent.IsEquippedSpellCast);

            return new TurnPresentationStep(
                StepAnimationType.Spell,
                stepBuilder.Events,
                stepBuilder.LogRecords,
                stepBuilder.DamageEvents,
                ResolveDamageFallbackPosition(stepBuilder.Events, spellEvent.TargetPosition),
                attackRequest: null,
                spellRequest: request,
                soundCue: UiSoundCue.SpellCast);
        }

        private static bool ShouldStartNewStep(TurnEventStepBuilder currentStep, IRunEventDto nextEvent)
        {
            if (currentStep == null)
            {
                return true;
            }

            if (IsStepAnchorEvent(nextEvent))
            {
                return true;
            }

            if (!IsStepAnchorEvent(currentStep.AnchorEvent))
            {
                return false;
            }

            return !BelongsToAnchorOutcome(currentStep, nextEvent);
        }

        private static bool IsStepAnchorEvent(IRunEventDto evt)
        {
            return evt is AttackDeclaredEventDto || evt is SpellCastEventDto;
        }

        private static bool BelongsToAnchorOutcome(TurnEventStepBuilder currentStep, IRunEventDto evt)
        {
            if (currentStep == null || evt == null)
            {
                return false;
            }

            switch (currentStep.AnchorEvent)
            {
                case AttackDeclaredEventDto attackAnchor:
                    return BelongsToAttackAnchorOutcome(attackAnchor, currentStep, evt);
                case SpellCastEventDto spellAnchor:
                    return BelongsToSpellAnchorOutcome(spellAnchor, currentStep, evt);
                default:
                    return false;
            }
        }

        private static bool BelongsToAttackAnchorOutcome(
            AttackDeclaredEventDto anchor,
            TurnEventStepBuilder currentStep,
            IRunEventDto evt)
        {
            switch (evt)
            {
                case AttackPerformedEventDto performed:
                    return IsMatchingAttackHitEvent(performed, anchor);
                case ActorDamagedEventDto damage:
                    return IsMatchingDamageEvent(damage, anchor.AttackerActorId, anchor.TargetActorId);
                case MessageEventDto _:
                    return CanAppendAttackResultLog(anchor, currentStep.Events);
                default:
                    return false;
            }
        }

        private static bool BelongsToSpellAnchorOutcome(
            SpellCastEventDto anchor,
            TurnEventStepBuilder currentStep,
            IRunEventDto evt)
        {
            switch (evt)
            {
                case AttackPerformedEventDto performed:
                    return IsMatchingSpellHitEvent(performed, anchor);
                case ActorDamagedEventDto damage:
                    return IsMatchingDamageEvent(damage, anchor.CasterActorId, anchor.TargetActorId);
                case MessageEventDto _:
                    return CanAppendSpellResultLog(anchor, currentStep.Events);
                default:
                    return false;
            }
        }

        private static bool HasMatchingAttackHit(
            IReadOnlyList<IRunEventDto> events,
            AttackDeclaredEventDto attackDeclaration)
        {
            if (events == null)
            {
                return false;
            }

            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] is AttackPerformedEventDto performed &&
                    IsMatchingAttackHitEvent(performed, attackDeclaration))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMatchingAttackHitEvent(AttackPerformedEventDto performed, AttackDeclaredEventDto anchor)
        {
            if (performed.AttackSource != AttackSourceDto.Normal)
            {
                return false;
            }

            if (performed.AttackerActorId != anchor.AttackerActorId)
            {
                return false;
            }

            if (performed.AttackKind != anchor.AttackKind)
            {
                return false;
            }

            if (anchor.TargetActorId.HasValue && performed.TargetActorId != anchor.TargetActorId.Value)
            {
                return false;
            }

            return true;
        }

        private static bool IsMatchingSpellHitEvent(AttackPerformedEventDto performed, SpellCastEventDto anchor)
        {
            if (performed.AttackSource != AttackSourceDto.Spell)
            {
                return false;
            }

            if (performed.AttackerActorId != anchor.CasterActorId)
            {
                return false;
            }

            if (anchor.TargetActorId.HasValue && performed.TargetActorId != anchor.TargetActorId.Value)
            {
                return false;
            }

            return true;
        }

        private static bool IsMatchingDamageEvent(ActorDamagedEventDto damage, Guid sourceId, Guid? targetId)
        {
            if (!damage.SourceActorId.HasValue || damage.SourceActorId.Value != sourceId)
            {
                return false;
            }

            if (targetId.HasValue && damage.TargetActorId != targetId.Value)
            {
                return false;
            }

            return true;
        }

        private static bool CanAppendAttackResultLog(
            AttackDeclaredEventDto anchor,
            IReadOnlyList<IRunEventDto> events)
        {
            if (events == null)
            {
                return false;
            }

            for (var i = 1; i < events.Count; i++)
            {
                if (events[i] is AttackPerformedEventDto performed && IsMatchingAttackHitEvent(performed, anchor))
                {
                    return false;
                }

                if (events[i] is ActorDamagedEventDto damage &&
                    IsMatchingDamageEvent(damage, anchor.AttackerActorId, anchor.TargetActorId))
                {
                    return false;
                }
            }

            return !HasResultLogAfterAnchor(events);
        }

        private static bool CanAppendSpellResultLog(
            SpellCastEventDto anchor,
            IReadOnlyList<IRunEventDto> events)
        {
            if (events == null)
            {
                return false;
            }

            for (var i = 1; i < events.Count; i++)
            {
                if (events[i] is AttackPerformedEventDto performed && IsMatchingSpellHitEvent(performed, anchor))
                {
                    return false;
                }

                if (events[i] is ActorDamagedEventDto damage &&
                    IsMatchingDamageEvent(damage, anchor.CasterActorId, anchor.TargetActorId))
                {
                    return false;
                }
            }

            return !HasResultLogAfterAnchor(events);
        }

        private static bool HasResultLogAfterAnchor(IReadOnlyList<IRunEventDto> events)
        {
            if (events == null)
            {
                return false;
            }

            for (var i = 1; i < events.Count; i++)
            {
                if (events[i] is MessageEventDto)
                {
                    return true;
                }
            }

            return false;
        }

        private static GridPositionDto ResolveDamageFallbackPosition(
            IReadOnlyList<IRunEventDto> events,
            GridPositionDto defaultPosition)
        {
            if (events == null || events.Count == 0)
            {
                return defaultPosition;
            }

            for (var i = 0; i < events.Count; i++)
            {
                if (!(events[i] is AttackPerformedEventDto performed))
                {
                    continue;
                }

                return performed.TargetPosition;
            }

            return defaultPosition;
        }

        private sealed class TurnEventStepBuilder
        {
            public IRunEventDto AnchorEvent { get; }
            public List<IRunEventDto> Events { get; }
            public List<ActorDamagedEventDto> DamageEvents { get; }
            public List<RunLogRecord> LogRecords { get; }

            public TurnEventStepBuilder(IRunEventDto anchorEvent)
            {
                AnchorEvent = anchorEvent;
                Events = new List<IRunEventDto>();
                DamageEvents = new List<ActorDamagedEventDto>();
                LogRecords = new List<RunLogRecord>();
            }

            public void AddEvent(IRunEventDto evt)
            {
                Events.Add(evt);
                if (evt is ActorDamagedEventDto damage)
                {
                    DamageEvents.Add(damage);
                }
            }
        }
    }
}




