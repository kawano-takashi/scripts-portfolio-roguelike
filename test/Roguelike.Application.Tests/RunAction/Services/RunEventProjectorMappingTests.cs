using System;
using System.Collections.Generic;
using Roguelike.Application.Dtos;
using Roguelike.Application.Enums;
using Roguelike.Application.Services;
using Roguelike.Domain.Gameplay.Actors.Enums;
using Roguelike.Domain.Gameplay.Actors.ValueObjects;
using Roguelike.Domain.Gameplay.Items.Enums;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Runs.Events;
using Xunit;

namespace Roguelike.Tests.Application.RunAction.Services
{
    /// <summary>
    /// RunEventProjectorMapping の仕様を検証するユニットテストです。
    /// 現在の挙動（正常系・異常系・境界条件）を回帰防止の観点で確認します。
    /// </summary>
    public sealed class RunEventProjectorMappingTests
    {
        // 観点: TryProjectDomainEvent_MapsAttackDeclaredEvent の期待挙動を検証する。
        [Fact]
        public void TryProjectDomainEvent_MapsAttackDeclaredEvent()
        {
            var projector = new RunEventProjector();
            var attackerId = ActorId.NewId();
            var targetId = ActorId.NewId();
            var domainEvent = new AttackDeclaredEvent(
                attackerId,
                targetId,
                AttackKind.Ranged,
                new Position(1, 2),
                Direction.Right,
                new Position(5, 2),
                range: 4);

            var projected = projector.TryProjectDomainEvent(domainEvent, turnNumber: 7, out var dto);

            var attack = Assert.IsType<AttackDeclaredEventDto>(dto);
            Assert.True(projected);
            Assert.Equal(7, attack.TurnNumber);
            Assert.Equal(attackerId.Value, attack.AttackerActorId);
            Assert.Equal(targetId.Value, attack.TargetActorId);
            Assert.Equal(4, attack.Range);
        }

        // 観点: TryProjectDomainEvent_MapsSpellCastEvent の期待挙動を検証する。
        [Fact]
        public void TryProjectDomainEvent_MapsSpellCastEvent()
        {
            var projector = new RunEventProjector();
            var casterId = ActorId.NewId();
            var targetId = ActorId.NewId();
            var domainEvent = new SpellCastEvent(
                casterId,
                ItemId.SpellbookSleep,
                new Position(1, 1),
                Direction.Down,
                new Position(1, 4),
                targetId,
                range: 3,
                isEquippedSpellCast: true);

            var projected = projector.TryProjectDomainEvent(domainEvent, turnNumber: 11, out var dto);

            var spell = Assert.IsType<SpellCastEventDto>(dto);
            Assert.True(projected);
            Assert.Equal(11, spell.TurnNumber);
            Assert.Equal((int)ItemId.SpellbookSleep, spell.ItemTypeValue);
            Assert.True(spell.IsEquippedSpellCast);
        }

        // 観点: TryProjectDomainEvent_MapsLogEvent_WithGeneratedMessageFromCode の期待挙動を検証する。
        [Fact]
        public void TryProjectDomainEvent_MapsLogEvent_WithGeneratedMessageFromCode()
        {
            var projector = new RunEventProjector();
            var domainEvent = new LogEvent(
                RunLogCode.LevelUp,
                new Dictionary<string, string>
                {
                    ["oldLevel"] = "2",
                    ["newLevel"] = "3"
                });

            var projected = projector.TryProjectDomainEvent(domainEvent, turnNumber: 4, out var dto);

            var message = Assert.IsType<MessageEventDto>(dto);
            Assert.True(projected);
            Assert.Equal("レベルアップ！ Lv.2 → Lv.3", message.Message);
        }

        [Theory]
        [InlineData(RunLogCode.NoSpellbookEquipped, "魔導書を装備していない。")]
        [InlineData(RunLogCode.PlayerDied, "プレイヤーは倒れた。")]
        [InlineData(RunLogCode.TooHungryToRest, "空腹で休めない。")]
        [InlineData(RunLogCode.TooHungryToSearch, "空腹で探索できない。")]
        [InlineData(RunLogCode.SpellbookHasNoSpell, "その魔導書には呪文がない。")]
        [InlineData(RunLogCode.Silenced, "沈黙していて呪文を唱えられない。")]
        [InlineData(RunLogCode.NothingHappens, "何も起こらなかった。")]
        [InlineData(RunLogCode.TooHungryToCast, "空腹で呪文を唱えられない。")]
        [InlineData(RunLogCode.NoTargetToSleep, "眠らせる対象がいない。")]
        [InlineData(RunLogCode.BlinkFailed, "瞬間移動に失敗した。")]
        [InlineData(RunLogCode.Starving, "飢餓状態だ！")]
        [InlineData(RunLogCode.WakeUp, "目を覚ました。")]
        [InlineData(RunLogCode.NothingToPickUp, "拾えるアイテムがない。")]
        [InlineData(RunLogCode.InventoryFull, "インベントリがいっぱいだ。")]
        [InlineData(RunLogCode.ItemNotFoundInInventory, "インベントリにそのアイテムはない。")]
        [InlineData(RunLogCode.ItemCannotBeUsed, "そのアイテムは使用できない。")]
        [InlineData(RunLogCode.ItemCannotBeEquipped, "そのアイテムは装備できない。")]
        [InlineData(RunLogCode.ItemAlreadyOnGround, "この場所にはすでにアイテムがある。")]
        [InlineData(RunLogCode.RunIsNotActive, "ランは進行中ではない。")]
        [InlineData(RunLogCode.InvalidPlayerAction, "プレイヤーの行動が無効だ。")]
        [InlineData(RunLogCode.ActorAsleep, "眠っていて行動できない。")]
        public void TryProjectDomainEvent_MapsLogEvent_StaticCodesToJapanese(RunLogCode code, string expectedMessage)
        {
            var projector = new RunEventProjector();
            var actualMessage = ProjectMessage(projector, new LogEvent(code));

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void TryProjectDomainEvent_MapsLogEvent_ParameterizedCodesToJapanese()
        {
            var projector = new RunEventProjector();

            var steppedOnItemMessage = ProjectMessage(
                projector,
                new LogEvent(
                    RunLogCode.SteppedOnItem,
                    new Dictionary<string, string> { ["itemName"] = "食料" }));
            var spellMissMessage = ProjectMessage(
                projector,
                new LogEvent(
                    RunLogCode.SpellMiss,
                    new Dictionary<string, string> { ["missMessage"] = "でんげきは何にも当たらなかった。" },
                    fallbackMessage: "でんげきは何にも当たらなかった。"));
            var targetFallsAsleepMessage = ProjectMessage(
                projector,
                new LogEvent(
                    RunLogCode.TargetFallsAsleep,
                    new Dictionary<string, string> { ["targetName"] = "スライム" }));
            var monsterHouseTriggeredMessage = ProjectMessage(
                projector,
                new LogEvent(
                    RunLogCode.MonsterHouseTriggered,
                    new Dictionary<string, string> { ["awakenedCount"] = "5" }));

            Assert.Equal("食料 の上に乗った！", steppedOnItemMessage);
            Assert.Equal("でんげきは何にも当たらなかった。", spellMissMessage);
            Assert.Equal("スライム は眠ってしまった。", targetFallsAsleepMessage);
            Assert.Equal("モンスターハウスだ！ 5体の敵が目を覚ました！", monsterHouseTriggeredMessage);
        }

        // 観点: TryProjectDomainEvent_MapsHungerChangedEvent の期待挙動を検証する。
        [Fact]
        public void TryProjectDomainEvent_MapsHungerChangedEvent()
        {
            var projector = new RunEventProjector();
            var actorId = ActorId.NewId();
            var domainEvent = new HungerChangedEvent(actorId, delta: -1.5f, currentHunger: 88.5f);

            var projected = projector.TryProjectDomainEvent(domainEvent, turnNumber: 2, out var dto);

            var hunger = Assert.IsType<HungerChangedEventDto>(dto);
            Assert.True(projected);
            Assert.Equal(-1.5f, hunger.Delta);
            Assert.Equal(actorId.Value, hunger.ActorId);
        }

        // 観点: TryProjectDomainLifecycleEvent_MapsKnownLifecycleEvents の期待挙動を検証する。
        [Fact]
        public void TryProjectDomainLifecycleEvent_MapsKnownLifecycleEvents()
        {
            var projector = new RunEventProjector();

            var clearedMapped = projector.TryProjectDomainLifecycleEvent(new RunClearedEvent(finalFloor: 10, totalTurns: 123, playerLevel: 5), out var clearedDto);
            var gameOverMapped = projector.TryProjectDomainLifecycleEvent(new RunGameOverEvent(floor: 3, totalTurns: 45, playerLevel: 2), out var gameOverDto);

            Assert.True(clearedMapped);
            Assert.Equal(RunLifecycleEventKind.RunCleared, clearedDto.Kind);
            Assert.True(gameOverMapped);
            Assert.Equal(RunLifecycleEventKind.RunGameOver, gameOverDto.Kind);
        }

        private static string ProjectMessage(RunEventProjector projector, LogEvent domainEvent)
        {
            var projected = projector.TryProjectDomainEvent(domainEvent, turnNumber: 1, out var dto);

            Assert.True(projected);
            var message = Assert.IsType<MessageEventDto>(dto);
            return message.Message;
        }
    }
}
