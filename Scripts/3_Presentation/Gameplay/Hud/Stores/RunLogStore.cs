using System;
using System.Collections.Generic;
using R3;
using Roguelike.Application.Dtos;
using Roguelike.Application.UseCases;
using Roguelike.Application.Enums;
using Roguelike.Presentation.Gameplay.RunResult.Stores;
using Roguelike.Presentation.Gameplay.Hud.Formatting;
using Roguelike.Presentation.Gameplay.Hud.Types;

namespace Roguelike.Presentation.Gameplay.Hud.Stores
{
    /// <summary>
    /// ランログ表示状態を保持します。
    /// </summary>
    public sealed class RunLogStore : IDisposable
    {
        private const int MaxEntryHistory = 100;

        private readonly RunLogProjectionPolicy _projectionUseCase;
        private readonly RunReadModelQueryHandler _runReadModelQueryService;
        private readonly RunResultStore _runResultStore;
        private readonly RunLogFormatter _formatter;
        private readonly CompositeDisposable _disposables = new();
        private readonly List<RunLogViewEntry> _logHistory = new();
        private readonly Dictionary<Guid, string> _actorNameCache = new();

        public ReactiveProperty<RunLogViewEntry> LatestEntry { get; }
        public IReadOnlyList<RunLogViewEntry> LogHistory => _logHistory;

        public RunLogStore(
            RunLogProjectionPolicy projectionUseCase,
            RunReadModelQueryHandler runReadModelQueryService,
            RunResultStore runResultStore,
            RunLogFormatter formatter)
        {
            _projectionUseCase = projectionUseCase ?? throw new ArgumentNullException(nameof(projectionUseCase));
            _runReadModelQueryService = runReadModelQueryService ?? throw new ArgumentNullException(nameof(runReadModelQueryService));
            _runResultStore = runResultStore ?? throw new ArgumentNullException(nameof(runResultStore));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            LatestEntry = new ReactiveProperty<RunLogViewEntry>().AddTo(_disposables);

            RefreshActorNameCacheFromReadModel();
            _runResultStore.LifecycleEventPublished += HandleLifecycleEvent;
        }

        public void RefreshActorNameCacheFromReadModel()
        {
            var readModelResult = _runReadModelQueryService.Handle();
            if (readModelResult.IsSuccess && readModelResult.Value.HasRun)
            {
                RefreshActorNameCache(readModelResult.Value);
            }
        }

        public void PublishRecord(RunLogRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (!TryCreateViewEntry(record, out var entry))
            {
                return;
            }

            _logHistory.Add(entry);
            while (_logHistory.Count > MaxEntryHistory)
            {
                _logHistory.RemoveAt(0);
            }

            LatestEntry.Value = entry;
        }

        public void PublishRecords(IReadOnlyList<RunLogRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                return;
            }

            for (var i = 0; i < records.Count; i++)
            {
                PublishRecord(records[i]);
            }
        }

        public void ClearHistory()
        {
            _logHistory.Clear();
            _actorNameCache.Clear();
            LatestEntry.Value = null;
        }

        public bool TryCreateViewEntry(RunLogRecord record, out RunLogViewEntry entry)
        {
            var message = _formatter.Format(record, ResolveActorName);
            if (string.IsNullOrWhiteSpace(message))
            {
                entry = null;
                return false;
            }

            entry = new RunLogViewEntry(ToViewType(record.Category), message);
            return true;
        }

        public void Dispose()
        {
            _runResultStore.LifecycleEventPublished -= HandleLifecycleEvent;
            _disposables.Dispose();
        }

        private void HandleLifecycleEvent(RunLifecycleEventDto evt)
        {
            if (!_projectionUseCase.TryProjectLifecycle(evt, out var record))
            {
                return;
            }

            PublishRecord(record);
        }

        private void RefreshActorNameCache(RunReadModelDto readModel)
        {
            if (readModel.Player.ActorId != Guid.Empty)
            {
                _actorNameCache[readModel.Player.ActorId] = readModel.Player.Name;
            }

            for (var i = 0; i < readModel.Enemies.Count; i++)
            {
                var enemy = readModel.Enemies[i];
                if (enemy.ActorId == Guid.Empty)
                {
                    continue;
                }

                _actorNameCache[enemy.ActorId] = enemy.Name;
            }
        }

        private string ResolveActorName(Guid? id)
        {
            if (!id.HasValue)
            {
                return "???";
            }

            if (_actorNameCache.TryGetValue(id.Value, out var name))
            {
                return name;
            }

            RefreshActorNameCacheFromReadModel();
            if (_actorNameCache.TryGetValue(id.Value, out name))
            {
                return name;
            }

            return "???";
        }

        private static RunLogViewType ToViewType(RunLogCategory category)
        {
            return category switch
            {
                RunLogCategory.Damage => RunLogViewType.Damage,
                RunLogCategory.Defeat => RunLogViewType.Defeat,
                RunLogCategory.Heal => RunLogViewType.Heal,
                _ => RunLogViewType.System
            };
        }
    }
}




