using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.Spawners;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Pooling;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Enum describing various sfx events
    /// </summary>
    public enum ESfxEvent
    {
        NONE,

        ExplosionAsteroid,
        ExplosionUFO,
        ExplosionSpaceship,

        Shoot,

        Hyperspace,

        MAX
    }

    /// <summary>
    /// Specifies public methods for sfx spawner
    /// </summary>
    public interface ISfxSpawner : IGameOverObserver
    {
        /// <summary>
        /// Play sfx with given event
        /// </summary>
        /// <param name="sfxEvent">given sfx event</param>
        void Play(ESfxEvent sfxEvent);
    }

    /// <summary>
    /// Construction data for sfx handler injecting dependencies
    /// </summary>
    public readonly struct SfxSpawnerConstructionData
    {
        public readonly int InitialSize;
        public readonly ISFXConfig SfxConfig;
        public readonly MonoBehaviour CoroutineReferenceMonoBehaviour;

        public SfxSpawnerConstructionData(int initialSize, ISFXConfig sfxConfig,
            MonoBehaviour coroutineReferenceMonoBehaviour)
        {
            InitialSize = initialSize;
            SfxConfig = sfxConfig;
            CoroutineReferenceMonoBehaviour = coroutineReferenceMonoBehaviour;
        }
    }

    /// <summary>
    /// Class responsible for playing sfx on game events
    /// </summary>
    public class SfxSpawner : ISfxSpawner
    {
        private const int AppendLength = 5;

        private readonly ISFXConfig _sfxConfig;
        private readonly GameObject _sfxHandlerObject;
        private readonly List<AudioSource> _engagedAudioSources;
        private readonly List<AudioSource> _availableAudioSources;
        private readonly MonoBehaviour _coroutineReferenceMonoBehaviour;

        #region CONSTRUCTOR

        public SfxSpawner(SfxSpawnerConstructionData sfxSpawnerConstructionData)
        {
            _sfxConfig = sfxSpawnerConstructionData.SfxConfig;
            _coroutineReferenceMonoBehaviour = sfxSpawnerConstructionData.CoroutineReferenceMonoBehaviour;
            _sfxHandlerObject = new GameObject("SfxHandler");

            _availableAudioSources = new List<AudioSource>();
            _engagedAudioSources = new List<AudioSource>();

            FillPool(sfxSpawnerConstructionData.InitialSize);
            SubscribeEvents();
        }
        ~SfxSpawner()
        {
            UnsubscribeEvents();
        }

        #endregion

        #region API
        public void Play(ESfxEvent sfxEvent)
        {
            // Logger.Info($"Play for {sfxEvent}");
            var freeItem = GetFreeItem();
            freeItem.clip = _sfxConfig.GetAudioClip(sfxEvent);
            freeItem.Play();
            _coroutineReferenceMonoBehaviour.StartCoroutine(DelayedRelease(freeItem.clip.length, freeItem));
        }
        public void OnGameOver(IBasePayload basePayload)
        {
            for (int i0 = 0; i0 < _engagedAudioSources.Count; i0++)
            {
                ResetAudioSource(_engagedAudioSources[i0]);
                ReturnToPool(_engagedAudioSources[i0]);
            }
        }

        #endregion

        private void FillPool(int size)
        {
            for (int i0 = 0; i0 < size; i0++)
            {
                var newAudioSource = _sfxHandlerObject.AddComponent<AudioSource>();
                ResetAudioSource(newAudioSource);
                _availableAudioSources.Add(newAudioSource);
            }
        }
        private AudioSource GetFreeItem()
        {
            if (_availableAudioSources.Count == 0)
            {
                FillPool(AppendLength);
            }

            var freeItem = _availableAudioSources[0];
            _availableAudioSources.Remove(freeItem);
            _engagedAudioSources.Add(freeItem);
            freeItem.enabled = true;
            return freeItem;
        }
        private void ReturnToPool(AudioSource audioSource)
        {
            if (_engagedAudioSources.Contains(audioSource)) 
            {
                _engagedAudioSources.Remove(audioSource);
            }
            if (!_availableAudioSources.Contains(audioSource))
            {
                _availableAudioSources.Add(audioSource);
            }

        }
        private void ResetAudioSource(AudioSource audioSource)
        {
            audioSource.enabled = false;
            audioSource.playOnAwake = false;
            audioSource.clip = null;
            audioSource.loop = false;
            audioSource.volume = _sfxConfig.GetDefaultVolume();
        }

        private void SubscribeEvents()
        {
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.AsteroidExploded, OnAsteroidExplosion);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.SpaceshipExploded, OnSpaceshipExploded);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.UFOExploded, OnUFOExploded);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.Shoot, OnShootEvent);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.GameOver, OnGameOver);
        }
        private void UnsubscribeEvents()
        {
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.AsteroidExploded, OnAsteroidExplosion);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.SpaceshipExploded, OnSpaceshipExploded);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.UFOExploded, OnUFOExploded);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.Shoot, OnShootEvent);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.GameOver, OnGameOver);
        }
        private void OnAsteroidExplosion(IBasePayload basePayload)
        {
            Play(ESfxEvent.ExplosionAsteroid);
        }
        private void OnSpaceshipExploded(IBasePayload basePayload)
        {
            Play(ESfxEvent.ExplosionSpaceship);
        }
        private void OnUFOExploded(IBasePayload basePayload)
        {
            Play(ESfxEvent.ExplosionUFO);
        }
        private void OnShootEvent(IBasePayload basePayload)
        {
            Play(ESfxEvent.Shoot);
        }
        private IEnumerator DelayedRelease(float delay, AudioSource audioSource)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(audioSource);
        }
    }
}