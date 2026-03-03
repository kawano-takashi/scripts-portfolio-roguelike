using UnityEngine;
using VContainer;
using VContainer.Unity;
using Roguelike.Application.UseCases;
using Roguelike.Application.Services;
using Roguelike.Application.Abstractions;
using Roguelike.Application.Commands;
using Roguelike.Application.Ports;
using Roguelike.Domain.Gameplay.Maps.Entities;
using Roguelike.Domain.Gameplay.Maps.ValueObjects;
using Roguelike.Domain.Gameplay.Maps.Services;
using Roguelike.Domain.Gameplay.Maps.Enums;
using Roguelike.Domain.Gameplay.Runs.Entities;
using Roguelike.Domain.Gameplay.Runs.ValueObjects;
using Roguelike.Domain.Gameplay.Runs.Services;
using Roguelike.Domain.Gameplay.Runs.Services.Population;
using Roguelike.Domain.Gameplay.Runs.Actions;
using Roguelike.Domain.Gameplay.Runs.Events;
using Roguelike.Domain.Gameplay.Runs.Repositories;
using Roguelike.Domain.Gameplay.Runs.Enums;
using Roguelike.Domain.Gameplay.Enemies.Services;
using Roguelike.Infrastructure.RunContext.Repositories;
using Roguelike.Presentation.Gameplay.CombatPresentation.Policies;
using Roguelike.Presentation.Gameplay.CombatPresentation.Sequencing;
using Roguelike.Presentation.Gameplay.CombatPresentation.Views;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Presenters;
using Roguelike.Presentation.Gameplay.Audio.Services;
using Roguelike.Presentation.Gameplay.Audio.Views;
using Roguelike.Presentation.Gameplay.Exploration.Input;
using Roguelike.Presentation.Gameplay.FloorTransition.Input;
using Roguelike.Presentation.Gameplay.FloorTransition.Presenters;
using Roguelike.Presentation.Gameplay.FloorTransition.Views;
using Roguelike.Presentation.Gameplay.Guide.Input;
using Roguelike.Presentation.Gameplay.Guide.Presenters;
using Roguelike.Presentation.Gameplay.Guide.Views;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Hud.Formatting;
using Roguelike.Presentation.Gameplay.Hud.Presenters;
using Roguelike.Presentation.Gameplay.Hud.Stores;
using Roguelike.Presentation.Gameplay.Hud.Views;
using Roguelike.Presentation.Gameplay.Inventory.Formatting;
using Roguelike.Presentation.Gameplay.Inventory.Input;
using Roguelike.Presentation.Gameplay.Inventory.Presenters;
using Roguelike.Presentation.Gameplay.Inventory.Views;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.Map.Presenters;
using Roguelike.Presentation.Gameplay.Map.Services;
using Roguelike.Presentation.Gameplay.Map.Views.Actor;
using Roguelike.Presentation.Gameplay.Map.Views.Map;
using Roguelike.Presentation.Gameplay.Menu.Input;
using Roguelike.Presentation.Gameplay.Menu.Presenters;
using Roguelike.Presentation.Gameplay.Menu.Views;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Input;
using Roguelike.Presentation.Gameplay.RunResult.Presenters;
using Roguelike.Presentation.Gameplay.RunResult.Services;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.RunResult.Views;
using Roguelike.Presentation.Gameplay.Shell.Core;
using Roguelike.Presentation.Gameplay.Shell.InputRouting;
using Roguelike.Presentation.Gameplay.SpellPreview.Input;
using Roguelike.Presentation.Gameplay.SpellPreview.Presenters;
using Roguelike.Presentation.Gameplay.SpellPreview.Views;

namespace Roguelike.Infrastructure.RunContext.DI
{
    /// <summary>
    /// DungeonSceneの「道具箱」を作るクラスです。
    /// Roguelikeに必要な部品を登録します。
    /// </summary>
    public class DungeonBootStrap : LifetimeScope
    {
        [SerializeField] private Dungeon2DView _dungeon2DView;
        [SerializeField] private RoguelikeRunInitializer _roguelikeRunInitializer;
        [SerializeField] private MiniMapView _miniMapView;
        [SerializeField] private Player2DView _player2DView;
        [SerializeField] private Enemy2DViewManager _enemy2DViewManager;
        // 入力設定はシーン側で調整可能にします。
        [SerializeField] private RunInputSettings _runInputSettings = new RunInputSettings();
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private SpellPreviewPanelView _spellPreviewPanelView;
        [SerializeField] private FloorTransitionView _floorTransitionView;
        [SerializeField] private RunStatusHudView _runStatusHudView;
        [SerializeField] private RunLogView _runLogView;
        [SerializeField] private RunInputDescriptionView _runInputDescriptionView;
        [SerializeField] private AttackAnimationView _attackAnimationView;
        [SerializeField] private SpellAnimationView _spellAnimationView;
        [SerializeField] private TurnEventSequencer _turnEventSequencer;
        [SerializeField] private DamagePopupView _damagePopupView;
        [SerializeField] private UiSoundPlayer _uiSoundPlayer;
        // クリア/ゲームオーバー時の結果画面ビュー。
        [SerializeField] private RunResultView _runResultView;
        // メインメニュービュー。
        [SerializeField] private MainMenuView _mainMenuView;
        // 操作説明ビュー。
        [SerializeField] private OperationGuideView _operationGuideView;

        protected override void Configure(IContainerBuilder builder)
        {
            // 下の順番で、必要な部品を登録します。
            RegisterInfrastructureComponents(builder);
            RegisterDomainServices(builder);
            RegisterApplicationComponents(builder);
            RegisterPresentationComponents(builder);
            RegisterSceneComponents(builder);
        }

        private void RegisterInfrastructureComponents(IContainerBuilder builder)
        {
            // ランを覚える箱（メモリ上の保存）を登録します。
            builder.Register<InMemoryRoguelikeRunRepository>(Lifetime.Singleton)
                .As<IRoguelikeRunRepository>()
                .As<IRunReadStore>()
                .As<IRunWriteStore>();
        }

        private void RegisterDomainServices(IContainerBuilder builder)
        {
            // ルールの本体（ドメインのサービス）を登録します。
            builder.Register<MapGenerationService>(Lifetime.Singleton)
                .As<IMapGenerationService>();

            builder.Register<FieldOfViewService>(Lifetime.Singleton)
                .As<IFieldOfViewService>();

            // 敵AIシステム（トルネコ/シレン風）
            builder.Register<SimplePathfindingService>(Lifetime.Singleton)
                .As<IPathfindingService>();

            builder.Register<DetectionService>(Lifetime.Singleton)
                .As<IDetectionService>();

            builder.Register<EnemyAiService>(Lifetime.Singleton)
                .As<IEnemyDecisionPolicy>();

            builder.Register<RunPopulationService>(Lifetime.Singleton)
                .As<IRunPopulationService>();

            builder.Register<DashContinuationPolicy>(Lifetime.Singleton)
                .As<IDashContinuationPolicy>();
            builder.Register<SpellTrajectoryService>(Lifetime.Singleton)
                .As<ISpellTrajectoryService>();
            builder.Register<PlayerInitializationService>(Lifetime.Singleton)
                .As<IPlayerInitializationService>();
            builder.Register<TurnEngine>(Lifetime.Singleton)
                .As<ITurnEngine>();
        }

        private void RegisterApplicationComponents(IContainerBuilder builder)
        {
            // Application層: statelessなユースケースを登録します。
            builder.Register<RunEventProjector>(Lifetime.Singleton);
            builder.Register<RunAccessQueryService>(Lifetime.Singleton);
            builder.Register<RunAccessCapabilityPolicy>(Lifetime.Singleton);
            builder.Register<RunActorLocatorQueryService>(Lifetime.Singleton);
            builder.Register<RunSnapshotQueryService>(Lifetime.Singleton);
            builder.Register<RunReadModelQueryService>(Lifetime.Singleton);
            builder.Register<InventoryReadModelService>(Lifetime.Singleton);
            builder.Register<SpellPreviewQueryService>(Lifetime.Singleton);
            builder.Register<RunExecutionResultAssembler>(Lifetime.Singleton);
            builder.Register<StartRunCommandValidator>(Lifetime.Singleton)
                .As<IValidator<StartRunCommand>>();
            builder.Register<DashStepCommandValidator>(Lifetime.Singleton)
                .As<IValidator<DashStepCommand>>();
            builder.Register<RunActionCommandValidator>(Lifetime.Singleton)
                .As<IValidator<RunActionCommand>>();
            builder.Register<RunActionFactory>(Lifetime.Singleton);
            builder.Register<PlayerInitializationPolicy>(Lifetime.Singleton);
            builder.Register<RunBootstrapService>(Lifetime.Singleton);
            builder.Register<RunSessionOrchestrator>(Lifetime.Singleton);
            builder.Register<DashStopPolicy>(Lifetime.Singleton);
            builder.Register<DashStepExecutionService>(Lifetime.Singleton);
            builder.Register<HasActiveRunQueryHandler>(Lifetime.Singleton);
            builder.Register<CanUseCapabilityQueryHandler>(Lifetime.Singleton);
            builder.Register<GetCurrentRunPhaseQueryHandler>(Lifetime.Singleton);
            builder.Register<GetPlayerIdQueryHandler>(Lifetime.Singleton);
            builder.Register<IsPlayerActorQueryHandler>(Lifetime.Singleton);
            builder.Register<GetStairsDownPositionQueryHandler>(Lifetime.Singleton);
            builder.Register<GetActorPositionQueryHandler>(Lifetime.Singleton);
            builder.Register<RunSnapshotQueryHandler>(Lifetime.Singleton);
            builder.Register<RunReadModelQueryHandler>(Lifetime.Singleton);
            builder.Register<StartRunCommandHandler>(Lifetime.Singleton);
            builder.Register<AdvanceFloorCommandHandler>(Lifetime.Singleton);
            builder.Register<RunActionCommandHandler>(Lifetime.Singleton);
            builder.Register<ExecuteDashStepCommandHandler>(Lifetime.Singleton);

            // UIから呼び出される読み取り/コマンド用ユースケース群。
            builder.Register<InventoryQueryHandler>(Lifetime.Singleton);
            builder.Register<SpellPreviewQueryHandler>(Lifetime.Singleton);
            builder.Register<EquippedSpellPreviewQueryHandler>(Lifetime.Singleton);
        }

        private void RegisterPresentationComponents(IContainerBuilder builder)
        {
            // Presentation層: UI状態保持・画面遷移を担うController群。
            builder.Register<RunUiController>(Lifetime.Singleton);
            builder.Register<OperationGuidePresenter>(Lifetime.Singleton);
            builder.Register<SpellPreviewPresenter>(Lifetime.Singleton);
            builder.Register<InventoryPresenter>(Lifetime.Singleton);
            builder.Register<MainMenuPresenter>(Lifetime.Singleton);
            builder.Register<FloorTransitionPresenter>(Lifetime.Singleton);
            builder.Register<PlayerMoveSoundPresenter>(Lifetime.Singleton);
            builder.Register<RunStatusStore>(Lifetime.Singleton);
            builder.Register<RunLogFormatter>(Lifetime.Singleton);
            builder.Register<RunLogProjectionPolicy>(Lifetime.Singleton);
            builder.Register<RunLogStore>(Lifetime.Singleton);
            builder.Register<TurnEventSequencingPolicy>(Lifetime.Singleton);
            builder.Register<InventoryFormatter>(Lifetime.Singleton);

            if (_dungeon2DView != null && _miniMapView != null && _enemy2DViewManager != null)
            {
                builder.Register<MapReadModelPresenter>(Lifetime.Singleton)
                    .As<IMapReadModelPresenter>();
            }
            if (_runResultView != null)
            {
                builder.Register<RunResultPresenter>(Lifetime.Singleton)
                    .As<IRunResultPresenter>();
            }
            if (_runInputDescriptionView != null)
            {
                builder.Register<RunInputDescriptionPresenter>(Lifetime.Singleton)
                    .As<IRunInputDescriptionPresenter>();
            }

            // 入力の管理を登録します。
            builder.Register<InputContextManager>(Lifetime.Singleton);
            if (_runInputSettings == null)
            {
                _runInputSettings = new RunInputSettings();
            }

            // Presentation層が保持する状態ストア。
            builder.Register<RunTurnStateStore>(Lifetime.Singleton);
            builder.Register<RunResultStore>(Lifetime.Singleton);

            // 入力設定はインスタンスとして共有します。
            builder.RegisterInstance(_runInputSettings);
            builder.Register<GameplayActorViewLocator>(Lifetime.Singleton)
                .As<IGameplayActorViewLocator>();
            builder.Register<GameplayResultNavigation>(Lifetime.Singleton)
                .As<IGameplayResultNavigation>();
            builder.Register<ExplorationInputHandler>(Lifetime.Singleton);
            builder.Register<InventoryInputHandler>(Lifetime.Singleton);
            builder.Register<SpellPreviewInputHandler>(Lifetime.Singleton);
            builder.Register<FloorTransitionInputHandler>(Lifetime.Singleton);
            builder.Register<RunResultInputHandler>(Lifetime.Singleton);
            builder.Register<MainMenuInputHandler>(Lifetime.Singleton);
            builder.Register<OperationGuideInputHandler>(Lifetime.Singleton);
        }

        private void RegisterSceneComponents(IContainerBuilder builder)
        {
            // シーンに置いてあるビューを登録します。
            if (_dungeon2DView != null)
            {
                builder.RegisterComponent(_dungeon2DView);
                builder.RegisterInstance<IDungeonMapView>(_dungeon2DView);
            }
            if (_roguelikeRunInitializer != null)
            {
                builder.RegisterComponent(_roguelikeRunInitializer);
            }
            if (_miniMapView != null)
            {
                builder.RegisterComponent(_miniMapView);
                builder.RegisterInstance<IMiniMapView>(_miniMapView);
            }
            builder.RegisterComponent(_player2DView);
            if (_enemy2DViewManager != null)
            {
                builder.RegisterComponent(_enemy2DViewManager);
                builder.RegisterInstance<IEnemyLayerView>(_enemy2DViewManager);
            }
            if (_inventoryView != null)
            {
                builder.RegisterComponent(_inventoryView);
            }
            if (_spellPreviewPanelView != null)
            {
                builder.RegisterComponent(_spellPreviewPanelView);
            }
            if (_floorTransitionView != null)
            {
                builder.RegisterComponent(_floorTransitionView);
            }
            if (_runStatusHudView != null)
            {
                builder.RegisterComponent(_runStatusHudView);
            }
            if (_runLogView != null)
            {
                builder.RegisterComponent(_runLogView);
            }
            if (_runInputDescriptionView != null)
            {
                builder.RegisterComponent(_runInputDescriptionView);
                builder.RegisterInstance<IRunInputDescriptionView>(_runInputDescriptionView);
            }
            if (_attackAnimationView != null)
            {
                // 攻撃演出のビューを登録します。
                builder.RegisterComponent(_attackAnimationView);
            }
            if (_spellAnimationView != null)
            {
                // スペル演出のビューを登録します。
                builder.RegisterComponent(_spellAnimationView);
            }
            if (_turnEventSequencer != null)
            {
                builder.RegisterComponent(_turnEventSequencer);
            }
            if (_damagePopupView != null)
            {
                builder.RegisterComponent(_damagePopupView);
            }
            if (_uiSoundPlayer != null)
            {
                builder.RegisterComponent(_uiSoundPlayer);
                builder.RegisterInstance<IUiSoundPlayer>(_uiSoundPlayer);
            }
            else
            {
                builder.Register<NoopUiSoundPlayer>(Lifetime.Singleton)
                    .As<IUiSoundPlayer>();
            }
            if (_runResultView != null)
            {
                builder.RegisterComponent(_runResultView);
                builder.RegisterInstance<IRunResultView>(_runResultView);
            }
            if (_mainMenuView != null)
            {
                builder.RegisterComponent(_mainMenuView);
            }
            if (_operationGuideView != null)
            {
                builder.RegisterComponent(_operationGuideView);
            }
        }

        private void Start()
        {
            // 画面の部品を順番に初期化します。
            _roguelikeRunInitializer?.Init();
            _dungeon2DView?.Init();
            _miniMapView?.Init();
            _player2DView?.Init();
            _enemy2DViewManager?.Init();
            if (_dungeon2DView != null && _miniMapView != null && _enemy2DViewManager != null)
            {
                Container.Resolve<IMapReadModelPresenter>()?.Init();
            }
            _inventoryView?.Init();
            _spellPreviewPanelView?.Init();
            _floorTransitionView?.Init();
            _runStatusHudView?.Init();
            _runLogView?.Init();
            _runInputDescriptionView?.Init();
            // 攻撃演出・スペル演出の初期化は最後に行います。
            _attackAnimationView?.Init();
            _spellAnimationView?.Init();
            _damagePopupView?.Init();
            _turnEventSequencer?.Init();
            _uiSoundPlayer?.Init();
            Container.Resolve<PlayerMoveSoundPresenter>();
            // 結果画面の初期化。
            _runResultView?.Init();
            if (_runResultView != null)
            {
                Container.Resolve<IRunResultPresenter>()?.Init();
            }
            // メインメニューの初期化。
            _mainMenuView?.Init();
            // 操作説明の初期化。
            _operationGuideView?.Init();
            // 入力ハンドラーはMonoBehaviourではないので手動初期化します。
            Container.Resolve<ExplorationInputHandler>()?.Init();
            Container.Resolve<InventoryInputHandler>()?.Init();
            Container.Resolve<SpellPreviewInputHandler>()?.Init();
            Container.Resolve<FloorTransitionInputHandler>()?.Init();
            Container.Resolve<RunResultInputHandler>()?.Init();
            Container.Resolve<MainMenuInputHandler>()?.Init();
            Container.Resolve<OperationGuideInputHandler>()?.Init();
            // 入力状態の管理を開始します。
            Container.Resolve<InputContextManager>()?.Init();
            if (_runInputDescriptionView != null)
            {
                Container.Resolve<IRunInputDescriptionPresenter>()?.Init();
            }
        }
    }
}







