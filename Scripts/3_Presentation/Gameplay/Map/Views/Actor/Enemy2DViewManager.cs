using System;
using System.Collections.Generic;
using UnityEngine;
using Roguelike.Presentation.Gameplay.Hud.Contracts;
using Roguelike.Presentation.Gameplay.Map.Contracts;
using Roguelike.Presentation.Gameplay.RunResult.Contracts;
using Roguelike.Presentation.Gameplay.Hud.DisplayModels;
using Roguelike.Presentation.Gameplay.Map.DisplayModels;
using Roguelike.Presentation.Gameplay.RunResult.DisplayModels;

namespace Roguelike.Presentation.Gameplay.Map.Views.Actor
{
    /// <summary>
    /// 敵ビュー群を同期する受動ビューです。
    /// </summary>
    public sealed class Enemy2DViewManager : MonoBehaviour, IEnemyLayerView
    {
        [SerializeField] private Enemy2DView _enemyViewPrefab;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private Vector2 _offset = Vector2.zero;

        [Header("Roguelike Run")]
        [SerializeField] private bool _useRoguelikeRun = true;

        private readonly Dictionary<Guid, Enemy2DView> _enemyViews = new();
        private bool _missingPrefabLogged;

        public void Init()
        {
            if (!UseRoguelikeRun())
            {
                return;
            }

            if (_enemyRoot == null)
            {
                _enemyRoot = transform;
            }
        }

        public void Render(EnemyLayerDisplayModel model)
        {
            if (!UseRoguelikeRun() || model == null)
            {
                return;
            }

            var aliveIds = new HashSet<Guid>();
            var enemies = model.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy.ActorId == Guid.Empty)
                {
                    continue;
                }

                var actorId = enemy.ActorId;
                aliveIds.Add(actorId);

                if (!_enemyViews.TryGetValue(actorId, out var view))
                {
                    view = CreateEnemyView(actorId);
                    if (view == null)
                    {
                        continue;
                    }

                    _enemyViews.Add(actorId, view);
                }

                view.Apply(enemy, _offset);
            }

            if (_enemyViews.Count == 0)
            {
                return;
            }

            var removeIds = new List<Guid>();
            foreach (var pair in _enemyViews)
            {
                if (!aliveIds.Contains(pair.Key))
                {
                    removeIds.Add(pair.Key);
                }
            }

            for (var i = 0; i < removeIds.Count; i++)
            {
                var id = removeIds[i];
                if (_enemyViews.TryGetValue(id, out var view) && view != null)
                {
                    Destroy(view.gameObject);
                }

                _enemyViews.Remove(id);
            }
        }

        /// <summary>
        /// 指定した敵IDのビューを取得します（演出側が参照するため）。
        /// </summary>
        public bool TryGetView(Guid id, out Enemy2DView view)
        {
            return _enemyViews.TryGetValue(id, out view);
        }

        private Enemy2DView CreateEnemyView(Guid id)
        {
            if (_enemyViewPrefab == null)
            {
                if (!_missingPrefabLogged)
                {
                    Debug.LogWarning("Enemy2DViewManager: Enemy view prefab is not assigned.");
                    _missingPrefabLogged = true;
                }

                return null;
            }

            var view = Instantiate(_enemyViewPrefab, _enemyRoot);
            view.name = $"Enemy2DView_{id}";
            view.Init();
            return view;
        }

        private bool UseRoguelikeRun()
        {
            return _useRoguelikeRun;
        }
    }
}




