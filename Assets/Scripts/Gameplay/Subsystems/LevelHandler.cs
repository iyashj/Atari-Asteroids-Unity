using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using Assets.Scripts.Data;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.Spawners;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Methods publicly accessible for LevelHandler.
    /// </summary>
    public interface ILevelHandler
    {
        /// <summary>
        /// Call to start the level with given index.
        /// </summary>
        /// <param name="levelIndex">index of the level to be started.</param>
        void Start(int levelIndex);
    }

    /// <summary>
    /// DataSet required to construct the level handler.
    /// Since otherwise the constructor call would be a tetrad function.
    /// For cleaner code all the requisite data has been encapsulated in a readonly struct.
    /// </summary>
    public readonly struct LevelHandlerConstructionData
    {
        public readonly IAsteroidSpawner AsteroidSpawner;
        public readonly IScreenHandler ScreenHandler;
        public readonly ILevelingConfig LevelingConfig;
        public readonly MonoBehaviour CoroutineReferenceMonoBehaviour;

        public LevelHandlerConstructionData(IAsteroidSpawner asteroidSpawner, IScreenHandler screenHandler, ILevelingConfig levelingConfig, MonoBehaviour _coroutineReferenceMonoBehaviour)
        {
            CoroutineReferenceMonoBehaviour = _coroutineReferenceMonoBehaviour;
            AsteroidSpawner = asteroidSpawner;
            ScreenHandler = screenHandler;
            LevelingConfig = levelingConfig;
        }
    }

    /// <summary>
    /// Class to setup level and maintain asteroid count for progression to next level.
    /// </summary>
    public class LevelHandler : ILevelHandler
    {
        #region DEPENDENCIES

        private readonly IAsteroidSpawner _asteroidSpawner;
        private readonly IScreenHandler _screenHandler;
        private readonly ILevelingConfig _levelingConfig;
        private readonly MonoBehaviour _coroutineReferenceMonoBehaviour;

        #endregion

        private float _safeDistance;
        private int _asteroidRemainingInLevel;
        private int _currentLevelIndex;
        private Transform _spaceshipTransform;

        #region CONSTRUCTOR
        public LevelHandler(LevelHandlerConstructionData constructionData)
        {
            Logger.Info($"constructed level handler");


            _levelingConfig = constructionData.LevelingConfig;
            _asteroidSpawner = constructionData.AsteroidSpawner;
            _screenHandler = constructionData.ScreenHandler;
            _coroutineReferenceMonoBehaviour = constructionData.CoroutineReferenceMonoBehaviour;
            _safeDistance = _levelingConfig.GetSafeDistanceFromPlayerShip();

            _currentLevelIndex = 0;
            _asteroidRemainingInLevel = 0;

            SubscribeToGameEvents();
        }
        ~LevelHandler()
        {
            UnsubscribeFromGameEvents();
        }
        #endregion

        #region API
        public void Start(int levelIndex)
        {
            Logger.Info($"starting level with levelindex {levelIndex}");

            _currentLevelIndex = levelIndex;
            _coroutineReferenceMonoBehaviour.StartCoroutine(SetupRoutine());
        }
        #endregion

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(EventKey.SpaceshipSpawned, OnSpaceshipLoaded);
            EventBus.GetInstance().Subscribe(EventKey.AsteroidExploded, OnAsteroidDestroyed);
        }
        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(EventKey.SpaceshipSpawned, OnSpaceshipLoaded);
            EventBus.GetInstance().Unsubscribe(EventKey.AsteroidExploded, OnAsteroidDestroyed);
        }
        private void OnSpaceshipLoaded(IBasePayload basePayload)
        {
            SpaceshipSpawnedPayload explosionPayload = (SpaceshipSpawnedPayload)basePayload;
            _spaceshipTransform = explosionPayload.spaceshipBehaviour.GetTransform();
        }
        private void OnAsteroidDestroyed(IBasePayload basePayload)
        {
            AsteroidExplodedPayload explosionPayload = (AsteroidExplodedPayload)basePayload;
            ChangeAsteroidRemainingCount(-1);
        }

        private void SetupLevel(Vector2 playerShipPosition, int asteroidsToBeGeneratedCount)
        {
            for (var i0 = 0; i0 < asteroidsToBeGeneratedCount; i0++)
            {
                _asteroidSpawner.SpawnAsteroid(Asteroids.Level3Id,
                    _screenHandler.GetRandomPointAtSafeDistanceFromTarget(ref playerShipPosition, ref _safeDistance));
            }
        }

        private IEnumerator SetupRoutine()
        {
            yield return new WaitForSeconds(_levelingConfig.GetWaitBeforeLevelSetup());
            SetupLevel();
        }

        private void  SetupLevel()
        {
            Logger.Info($"setting up level with levelindex {_currentLevelIndex}");

            var asteroidCountForLevel = GetAsteroidsCountForLevel(_currentLevelIndex);
            SetupLevel(_spaceshipTransform.position, GetAsteroidsCountForLevel(_currentLevelIndex));
            _asteroidRemainingInLevel = asteroidCountForLevel * Asteroids.Level3AsteroidWeight;
        }

        private bool IsThereAnyAsteroidRemainingOnThisLevel() => _asteroidRemainingInLevel > 0;
        private int GetAsteroidsCountForLevel(int levelId) => _levelingConfig.GetMinimumAsteroidsInALevel() +
                                                              (levelId - 1) *
                                                              _levelingConfig.GetAsteroidsIncreasedPerLevel();
        private void ChangeAsteroidRemainingCount(int changedValue)
        {
            _asteroidRemainingInLevel += changedValue;
            if (!IsThereAnyAsteroidRemainingOnThisLevel())
            {
                IncreaseLevelCount();
                _coroutineReferenceMonoBehaviour.StartCoroutine(SetupRoutine());
            }
        }

        private void IncreaseLevelCount() => ++_currentLevelIndex;
    }
}

