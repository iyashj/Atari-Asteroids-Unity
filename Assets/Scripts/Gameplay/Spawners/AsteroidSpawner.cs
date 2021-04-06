using UnityEngine;

using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Assets.Scripts.GameStructure.Pooling;
using Assets.Scripts.GameConstants;

namespace Assets.Scripts.Gameplay.Spawners
{
    /// <summary>
    /// Interface for AsteroidSpawner. Adding Spawn method to BaseSpawner facilities
    /// </summary>
    public interface IAsteroidSpawner : IBaseSpawner, IGameOverObserver
    {
        void SpawnAsteroid(int targetAsteroidLevel, Vector3 spawnPosition);
    }

    /// <summary>
    /// Class responsible for spawning asteroids.
    /// </summary>
    public class AsteroidSpawner : IAsteroidSpawner
    {
        private BaseObjectPool<IAsteroid>[] _asteroidPools;
        private bool canSpawn = true;

        #region CONSTRUCTOR

        public AsteroidSpawner()
        {
            SubscribeToGameEvents();
            canSpawn = true;
        }
        ~AsteroidSpawner()
        {
            UnsubscribeFromGameEvents();
        }

        #endregion

        #region API
        public void SpawnAsteroid(int targetAsteroidLevel, Vector3 spawnPosition)
        {
            if (!canSpawn) return;

            Logger.Info($"Spawning Asteroid of level {targetAsteroidLevel} at {spawnPosition}");
            var spawnedAsteroid = _asteroidPools[targetAsteroidLevel].GetFreeItem();
            spawnedAsteroid.Init();
            spawnedAsteroid.SetPosition(spawnPosition);
            spawnedAsteroid.Engage();
            spawnedAsteroid.Launch();
        }
        public void Preload()
        {
            Logger.Info($"Preloading asteroid spawner");
            _asteroidPools = new BaseObjectPool<IAsteroid>[3];
            _asteroidPools[0] = new BaseObjectPool<IAsteroid>(GameConstants.Prefabs.AsteroidLevel0, InitialPoolSize.VfxExplosion);
            _asteroidPools[1] = new BaseObjectPool<IAsteroid>(GameConstants.Prefabs.AsteroidLevel1, InitialPoolSize.VfxExplosion);
            _asteroidPools[2] = new BaseObjectPool<IAsteroid>(GameConstants.Prefabs.AsteroidLevel2, InitialPoolSize.VfxExplosion);
        }
        public bool IsReady()
        {
            bool isReady = true;
            foreach (var asteroidPool in _asteroidPools) isReady &= asteroidPool.IsReady();
            return isReady;
        }
        #endregion

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(EventKey.AsteroidExploded, OnAsteroidExplosion);
            EventBus.GetInstance().Subscribe(EventKey.GameOver, OnGameOver);
            EventBus.GetInstance().Subscribe(EventKey.PlayAgainPayload, OnRestart);
        }
        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(EventKey.AsteroidExploded, OnAsteroidExplosion);
            EventBus.GetInstance().Unsubscribe(EventKey.GameOver, OnGameOver);
            EventBus.GetInstance().Unsubscribe(EventKey.PlayAgainPayload, OnRestart);
        }
        private void OnAsteroidExplosion(IBasePayload basePayload)
        {
            AsteroidExplodedPayload explosionPayload = (AsteroidExplodedPayload)basePayload;

            if (explosionPayload.Level > 0)
            {
                var newAsteroidLevel = explosionPayload.Level - 1;
                for (int i = 0; i < 2; i++)
                {
                    SpawnAsteroid(newAsteroidLevel, explosionPayload.Position);
                }
            }
        }
        public void OnGameOver(IBasePayload basePayload)
        {
            ReleaseAllAsteroids();
        }
        
        private void OnRestart(IBasePayload obj)
        {
            canSpawn = true;
        }
        private void ReleaseAllAsteroids()
        {
            canSpawn = false;
            for (int i0 = 0; i0 < _asteroidPools.Length; i0++)
            {
                _asteroidPools[i0].ReleaseAll();
            }
        }
    }
}

