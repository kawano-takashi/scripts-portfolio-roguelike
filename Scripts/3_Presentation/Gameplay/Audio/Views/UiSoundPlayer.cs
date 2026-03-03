using System.Collections.Generic;
using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using UnityEngine;

namespace Roguelike.Presentation.Gameplay.Audio.Views
{
    /// <summary>
    /// UI効果音の再生を担当します。
    /// </summary>
    public sealed class UiSoundPlayer : MonoBehaviour, IUiSoundPlayer
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _audioSource;

        [Header("UI Clips")]
        [SerializeField] private AudioClip _uiOpenClip;
        [SerializeField, Range(0f, 1f)] private float _uiOpenVolume = 1f;
        [SerializeField, Min(0f)] private float _uiOpenStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _uiCloseClip;
        [SerializeField, Range(0f, 1f)] private float _uiCloseVolume = 1f;
        [SerializeField, Min(0f)] private float _uiCloseStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _inventorySelectClip;
        [SerializeField, Range(0f, 1f)] private float _inventorySelectVolume = 1f;
        [SerializeField, Min(0f)] private float _inventorySelectStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _inventoryDetailOptionExecuteClip;
        [SerializeField, Range(0f, 1f)] private float _inventoryDetailOptionExecuteVolume = 1f;
        [SerializeField, Min(0f)] private float _inventoryDetailOptionExecuteStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _itemPickupClip;
        [SerializeField, Range(0f, 1f)] private float _itemPickupVolume = 1f;
        [SerializeField, Min(0f)] private float _itemPickupStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _menuSelectClip;
        [SerializeField, Range(0f, 1f)] private float _menuSelectVolume = 1f;
        [SerializeField, Min(0f)] private float _menuSelectStartTimeSeconds = 0f;

        [Header("Player Movement Clips")]
        [SerializeField] private AudioClip _playerMoveClip;
        [SerializeField, Range(0f, 1f)] private float _playerMoveVolume = 1f;
        [SerializeField, Min(0f)] private float _playerMoveStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _playerDashMoveClip;
        [SerializeField, Range(0f, 1f)] private float _playerDashMoveVolume = 1f;
        [SerializeField, Min(0f)] private float _playerDashMoveStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _playerMoveFailedClip;
        [SerializeField, Range(0f, 1f)] private float _playerMoveFailedVolume = 1f;
        [SerializeField, Min(0f)] private float _playerMoveFailedStartTimeSeconds = 0f;

        [Header("Combat Clips")]
        [SerializeField] private AudioClip _attackClip;
        [SerializeField, Range(0f, 1f)] private float _attackVolume = 1f;
        [SerializeField, Min(0f)] private float _attackStartTimeSeconds = 0f;
        [SerializeField] private AudioClip _spellCastClip;
        [SerializeField, Range(0f, 1f)] private float _spellCastVolume = 1f;
        [SerializeField, Min(0f)] private float _spellCastStartTimeSeconds = 0f;

        [Header("Playback Settings")]
        [SerializeField, Min(1)] private int _maxSimultaneousUiSounds = 8;

        private AudioClip _fallbackUiOpenClip;
        private AudioClip _fallbackUiCloseClip;
        private AudioClip _fallbackInventorySelectClip;
        private AudioClip _fallbackInventoryDetailOptionExecuteClip;
        private AudioClip _fallbackItemPickupClip;
        private AudioClip _fallbackMenuSelectClip;
        private AudioClip _fallbackPlayerMoveClip;
        private AudioClip _fallbackPlayerDashMoveClip;
        private AudioClip _fallbackPlayerMoveFailedClip;
        private AudioClip _fallbackAttackClip;
        private AudioClip _fallbackSpellCastClip;
        private bool _initialized;
        private bool _warnedMissingUiOpenClip;
        private bool _warnedMissingUiCloseClip;
        private bool _warnedMissingInventorySelectClip;
        private bool _warnedMissingInventoryDetailOptionExecuteClip;
        private bool _warnedMissingItemPickupClip;
        private bool _warnedMissingMenuSelectClip;
        private bool _warnedMissingPlayerMoveClip;
        private bool _warnedMissingPlayerDashMoveClip;
        private bool _warnedMissingPlayerMoveFailedClip;
        private bool _warnedMissingAttackClip;
        private bool _warnedMissingSpellCastClip;
        private readonly List<AudioSource> _audioSourcePool = new();
        private int _nextAudioSourceIndex;

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            InitializeAudioSourcePool();
            _initialized = true;
        }

        public void Play(UiSoundCue cue)
        {
            if (!_initialized)
            {
                Init();
            }

            if (_audioSourcePool.Count == 0)
            {
                return;
            }

            switch (cue)
            {
                case UiSoundCue.UiOpen:
                    PlayUiOpen();
                    return;
                case UiSoundCue.UiClose:
                    PlayUiClose();
                    return;
                case UiSoundCue.InventorySelect:
                    PlayInventorySelect();
                    return;
                case UiSoundCue.InventoryDetailOptionExecute:
                    PlayInventoryDetailOptionExecute();
                    return;
                case UiSoundCue.ItemPickup:
                    PlayItemPickup();
                    return;
                case UiSoundCue.MenuSelect:
                    PlayMenuSelect();
                    return;
                case UiSoundCue.PlayerMove:
                    PlayPlayerMove();
                    return;
                case UiSoundCue.PlayerDashMove:
                    PlayPlayerDashMove();
                    return;
                case UiSoundCue.PlayerMoveFailed:
                    PlayPlayerMoveFailed();
                    return;
                case UiSoundCue.Attack:
                    PlayAttack();
                    return;
                case UiSoundCue.SpellCast:
                    PlaySpellCast();
                    return;
                default:
                    Debug.LogWarning($"Unsupported UI sound cue: {cue}");
                    return;
            }
        }

        private void PlayUiOpen()
        {
            var clip = _uiOpenClip;
            if (clip == null)
            {
                if (!_warnedMissingUiOpenClip)
                {
                    Debug.LogWarning("UI open clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingUiOpenClip = true;
                }

                _fallbackUiOpenClip ??= CreateFallbackClickClip("UiOpenFallback");
                clip = _fallbackUiOpenClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _uiOpenVolume, _uiOpenStartTimeSeconds);
        }

        private void PlayUiClose()
        {
            var clip = _uiCloseClip;
            if (clip == null)
            {
                if (!_warnedMissingUiCloseClip)
                {
                    Debug.LogWarning("UI close clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingUiCloseClip = true;
                }

                _fallbackUiCloseClip ??= CreateFallbackClickClip("UiCloseFallback");
                clip = _fallbackUiCloseClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _uiCloseVolume, _uiCloseStartTimeSeconds);
        }

        private void PlayInventorySelect()
        {
            var clip = _inventorySelectClip;
            if (clip == null)
            {
                if (!_warnedMissingInventorySelectClip)
                {
                    Debug.LogWarning("Inventory select clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingInventorySelectClip = true;
                }

                _fallbackInventorySelectClip ??= CreateFallbackClickClip("InventorySelectFallback");
                clip = _fallbackInventorySelectClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _inventorySelectVolume, _inventorySelectStartTimeSeconds);
        }

        private void PlayInventoryDetailOptionExecute()
        {
            var clip = _inventoryDetailOptionExecuteClip;
            if (clip == null)
            {
                if (!_warnedMissingInventoryDetailOptionExecuteClip)
                {
                    Debug.LogWarning("Inventory detail option execute clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingInventoryDetailOptionExecuteClip = true;
                }

                _fallbackInventoryDetailOptionExecuteClip ??= CreateFallbackClickClip("InventoryDetailOptionExecuteFallback");
                clip = _fallbackInventoryDetailOptionExecuteClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _inventoryDetailOptionExecuteVolume, _inventoryDetailOptionExecuteStartTimeSeconds);
        }

        private void PlayItemPickup()
        {
            var clip = _itemPickupClip;
            if (clip == null)
            {
                if (!_warnedMissingItemPickupClip)
                {
                    Debug.LogWarning("Item pickup clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingItemPickupClip = true;
                }

                _fallbackItemPickupClip ??= CreateFallbackClickClip("ItemPickupFallback");
                clip = _fallbackItemPickupClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _itemPickupVolume, _itemPickupStartTimeSeconds);
        }

        private void PlayMenuSelect()
        {
            var clip = _menuSelectClip;
            if (clip == null)
            {
                if (!_warnedMissingMenuSelectClip)
                {
                    Debug.LogWarning("Menu select clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingMenuSelectClip = true;
                }

                _fallbackMenuSelectClip ??= CreateFallbackClickClip("MenuSelectFallback");
                clip = _fallbackMenuSelectClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _menuSelectVolume, _menuSelectStartTimeSeconds);
        }

        private void PlayPlayerMove()
        {
            var clip = _playerMoveClip;
            if (clip == null)
            {
                if (!_warnedMissingPlayerMoveClip)
                {
                    Debug.LogWarning("Player move clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingPlayerMoveClip = true;
                }

                _fallbackPlayerMoveClip ??= CreateFallbackClickClip("PlayerMoveFallback");
                clip = _fallbackPlayerMoveClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _playerMoveVolume, _playerMoveStartTimeSeconds);
        }

        private void PlayPlayerDashMove()
        {
            var clip = _playerDashMoveClip;
            if (clip == null)
            {
                if (!_warnedMissingPlayerDashMoveClip)
                {
                    Debug.LogWarning("Player dash move clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingPlayerDashMoveClip = true;
                }

                _fallbackPlayerDashMoveClip ??= CreateFallbackClickClip("PlayerDashMoveFallback");
                clip = _fallbackPlayerDashMoveClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _playerDashMoveVolume, _playerDashMoveStartTimeSeconds);
        }

        private void PlayPlayerMoveFailed()
        {
            var clip = _playerMoveFailedClip;
            if (clip == null)
            {
                if (!_warnedMissingPlayerMoveFailedClip)
                {
                    Debug.LogWarning("Player move failed clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingPlayerMoveFailedClip = true;
                }

                _fallbackPlayerMoveFailedClip ??= CreateFallbackClickClip("PlayerMoveFailedFallback");
                clip = _fallbackPlayerMoveFailedClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _playerMoveFailedVolume, _playerMoveFailedStartTimeSeconds);
        }

        private void PlayAttack()
        {
            var clip = _attackClip;
            if (clip == null)
            {
                if (!_warnedMissingAttackClip)
                {
                    Debug.LogWarning("Attack clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingAttackClip = true;
                }

                _fallbackAttackClip ??= CreateFallbackClickClip("AttackFallback");
                clip = _fallbackAttackClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _attackVolume, _attackStartTimeSeconds);
        }

        private void PlaySpellCast()
        {
            var clip = _spellCastClip;
            if (clip == null)
            {
                if (!_warnedMissingSpellCastClip)
                {
                    Debug.LogWarning("Spell cast clip is not assigned. A generated fallback click will be used.");
                    _warnedMissingSpellCastClip = true;
                }

                _fallbackSpellCastClip ??= CreateFallbackClickClip("SpellCastFallback");
                clip = _fallbackSpellCastClip;
            }

            if (clip == null)
            {
                return;
            }

            PlayClip(clip, _spellCastVolume, _spellCastStartTimeSeconds);
        }

        private void InitializeAudioSourcePool()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            if (_audioSource == null)
            {
                return;
            }

            _audioSourcePool.Clear();
            ConfigureAudioSource(_audioSource);
            _audioSourcePool.Add(_audioSource);

            var poolSize = Mathf.Max(1, _maxSimultaneousUiSounds);
            for (var i = 1; i < poolSize; i++)
            {
                var pooledAudioSource = gameObject.AddComponent<AudioSource>();
                CopyAudioSourceSettings(_audioSource, pooledAudioSource);
                ConfigureAudioSource(pooledAudioSource);
                _audioSourcePool.Add(pooledAudioSource);
            }

            _nextAudioSourceIndex = 0;
        }

        private static void CopyAudioSourceSettings(AudioSource source, AudioSource target)
        {
            target.outputAudioMixerGroup = source.outputAudioMixerGroup;
            target.mute = source.mute;
            target.bypassEffects = source.bypassEffects;
            target.bypassListenerEffects = source.bypassListenerEffects;
            target.bypassReverbZones = source.bypassReverbZones;
            target.priority = source.priority;
            target.pitch = source.pitch;
            target.panStereo = source.panStereo;
            target.spatialBlend = source.spatialBlend;
            target.reverbZoneMix = source.reverbZoneMix;
            target.dopplerLevel = source.dopplerLevel;
            target.spread = source.spread;
            target.rolloffMode = source.rolloffMode;
            target.minDistance = source.minDistance;
            target.maxDistance = source.maxDistance;
            target.ignoreListenerVolume = source.ignoreListenerVolume;
            target.ignoreListenerPause = source.ignoreListenerPause;
        }

        private static void ConfigureAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
        }

        private void PlayClip(AudioClip clip, float volume, float startTimeSeconds)
        {
            if (clip == null)
            {
                return;
            }

            var source = AcquireAudioSource();
            if (source == null)
            {
                return;
            }

            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.time = ClampStartTimeSeconds(clip, startTimeSeconds);
            source.Play();
        }

        private AudioSource AcquireAudioSource()
        {
            if (_audioSourcePool.Count == 0)
            {
                return null;
            }

            for (var i = 0; i < _audioSourcePool.Count; i++)
            {
                var index = (_nextAudioSourceIndex + i) % _audioSourcePool.Count;
                var source = _audioSourcePool[index];
                if (source == null)
                {
                    continue;
                }

                if (!source.isPlaying)
                {
                    _nextAudioSourceIndex = (index + 1) % _audioSourcePool.Count;
                    return source;
                }
            }

            var stealIndex = _nextAudioSourceIndex % _audioSourcePool.Count;
            _nextAudioSourceIndex = (stealIndex + 1) % _audioSourcePool.Count;
            var stealSource = _audioSourcePool[stealIndex];
            if (stealSource != null)
            {
                stealSource.Stop();
            }

            return stealSource;
        }

        private static float ClampStartTimeSeconds(AudioClip clip, float startTimeSeconds)
        {
            if (clip.frequency <= 0)
            {
                return 0f;
            }

            var minimumPlayableLengthSeconds = 1f / clip.frequency;
            var maxStartTimeSeconds = Mathf.Max(0f, clip.length - minimumPlayableLengthSeconds);
            return Mathf.Clamp(startTimeSeconds, 0f, maxStartTimeSeconds);
        }

        private static AudioClip CreateFallbackClickClip(string clipName)
        {
            const int sampleRate = 44100;
            const float durationSeconds = 0.07f;
            const float frequency = 900f;
            const float amplitude = 0.25f;

            var sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = 1f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
