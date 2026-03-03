using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Hud.Types;

namespace Roguelike.Presentation.Gameplay.CombatPresentation.Types
{
    /// <summary>
    /// ターン内の1演出ステップです。
    /// </summary>
    public sealed class TurnPresentationStep
    {
        public StepAnimationType AnimationType { get; }
        public IReadOnlyList<IRunEventDto> Events { get; }
        public IReadOnlyList<RunLogRecord> LogEntries { get; }
        public IReadOnlyList<ActorDamagedEventDto> DamageEvents { get; }
        public GridPositionDto DamageFallbackPosition { get; }
        public AttackAnimationRequest AttackRequest { get; }
        public SpellAnimationRequest? SpellRequest { get; }
        public UiSoundCue? SoundCue { get; }

        public TurnPresentationStep(
            StepAnimationType animationType,
            IReadOnlyList<IRunEventDto> events,
            IReadOnlyList<RunLogRecord> logEntries,
            IReadOnlyList<ActorDamagedEventDto> damageEvents,
            GridPositionDto damageFallbackPosition,
            AttackAnimationRequest attackRequest,
            SpellAnimationRequest? spellRequest,
            UiSoundCue? soundCue)
        {
            AnimationType = animationType;
            Events = events ?? Array.Empty<IRunEventDto>();
            LogEntries = logEntries ?? Array.Empty<RunLogRecord>();
            DamageEvents = damageEvents ?? Array.Empty<ActorDamagedEventDto>();
            DamageFallbackPosition = damageFallbackPosition;
            AttackRequest = attackRequest;
            SpellRequest = spellRequest;
            SoundCue = soundCue;
        }
    }
}




