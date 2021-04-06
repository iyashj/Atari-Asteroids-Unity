using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.Spawners
{
    /// <summary>
    /// Interface for SpaceshipSpawner. Adding Spawn method to BaseSpawner facilities.
    /// </summary>
    public interface ISpaceshipSpawner : IBaseSpawner
    {
        void Spawn();
    }

    /// <summary>
    /// Class responsible for spawning spaceship and publishing this spawn event.
    /// </summary>
    public class SpaceshipSpawner : ISpaceshipSpawner
    {
        private GameObject _spaceshipPrefab;
        private bool _bIsReady;
        private ISpaceship _spawnedSpaceship;

        #region CONSTRUCTOR
        public SpaceshipSpawner()
        {
            _bIsReady = false;
            _spaceshipPrefab = null;
        }
        #endregion

        #region API
        public void Preload()
        {
            Logger.Info($"Preload spaceship spawner");
            SpawnerUtility.GetAsyncAssetLoad(GameConstants.Prefabs.SpaceshipSpawnKey).Completed += OnLoadingComplete;
        }
        public void Spawn()
        {
            Logger.Info($"Spawn spaceship");

            _spawnedSpaceship = Object.Instantiate(_spaceshipPrefab, Vector3.zero, Quaternion.identity)
                .GetComponent<ISpaceship>();

            _spawnedSpaceship.Initialize();
            
            _bIsReady = true;

            PublishSpaceshipSpawnedEvent(_spawnedSpaceship);
        }
        public bool IsReady() => _bIsReady;
        #endregion

        private void PublishSpaceshipSpawnedEvent(ISpaceship spacehship)
        {
            Logger.Info($"Publishing spaceship spawn event");
            EventBus.GetInstance().Publish(new SpaceshipSpawnedPayload(spacehship));
        }
        private void OnLoadingComplete(AsyncOperationHandle<GameObject> spawnHandle)
        {
            if (spawnHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Info($"Async opertaion to load spaceship success");
                _spaceshipPrefab = spawnHandle.Result;
                _bIsReady = true;
            }
            else
            {
                Logger.Error($"AsyncOperation failed with following expection: {spawnHandle.OperationException.Message}");
            }
        }
    }
}

