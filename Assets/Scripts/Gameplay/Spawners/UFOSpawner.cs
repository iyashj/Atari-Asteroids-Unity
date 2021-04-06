using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.Spawners
{
    /// <summary>
    /// Interface for UfoBaseSpawner. Adding Spawn method to BaseSpawner facilities
    /// </summary>
    public interface IUFOSpawner : IBaseSpawner, IGameOverObserver
    {
        void Spawn();
    }

    /// <summary>
    /// Class responsible for loading, spawning UFO from time to time in the game.
    /// </summary>
    public class UFOSpawner : IUFOSpawner
    {
        #region DEPENDENCIES
        /// <summary>
        /// Cached reference to smaller ufo after spawning
        /// </summary>
        private IUFO _ufoSmall;
        /// <summary>
        /// Cached reference to larger ufo after spawning
        /// </summary>
        private IUFO _ufoLarge;
        #endregion

        private MonoBehaviour _coroutineReferenceMonoBehaviour;
        private Coroutine _ufoEnablingRoutine;
        private readonly Rect _rect;

        /// <summary>
        /// Reference to smallUFO asset acting as a prefab
        /// </summary>
        private GameObject _ufoSmallGameObject;
        /// <summary>
        /// Reference to largeUFO asset acting as a prefab
        /// </summary>
        private GameObject _ufoLargeGameObject;

        #region CONSTRUCTOR
        public UFOSpawner(Rect rect, MonoBehaviour coroutineReferenceMonoBehaviour)
        {
            _coroutineReferenceMonoBehaviour = coroutineReferenceMonoBehaviour;
            _rect = rect;
            SubscribeToGameEvents();
        }
        ~UFOSpawner()
        {
            UnsubscribeFromGameEvents();
        }
        #endregion

        #region API
        public bool IsReady() => (_ufoSmallGameObject != null) && (_ufoSmallGameObject != null);
        public void Preload()
        {
            Logger.Info($"Preload ufo spawner");
            SpawnerUtility.GetAsyncAssetLoad(GameConstants.Prefabs.UFOSmallKey).Completed += OnLoadingCompleteSmallUFO;
            SpawnerUtility.GetAsyncAssetLoad(GameConstants.Prefabs.UFOLargeKey).Completed += OnLoadingCompleteLargeUFO;
        }
        public void Spawn()
        {
            Logger.Info($"start ufo spawn routine");
            _ufoEnablingRoutine = _coroutineReferenceMonoBehaviour.StartCoroutine(UFOSpawnRoutine());
        }
        public void OnGameOver(IBasePayload basePayload)
        {
            ReleaseAllUFO();
        }
        #endregion

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(EventKey.UFOExploded, OnUFOExploded);
            EventBus.GetInstance().Subscribe(EventKey.GameOver, OnGameOver);
            EventBus.GetInstance().Subscribe(EventKey.PlayAgainPayload, OnPlayAgain);

        }
        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(EventKey.UFOExploded, OnUFOExploded);
            EventBus.GetInstance().Unsubscribe(EventKey.GameOver, OnGameOver);
            EventBus.GetInstance().Unsubscribe(EventKey.PlayAgainPayload, OnPlayAgain);

        }
        private void OnPlayAgain(IBasePayload obj)
        {
            Spawn();
        }
        private void OnUFOExploded(IBasePayload basePayload)
        {
            UfoExplodedPayload ufoExplodedPayload = (UfoExplodedPayload)basePayload;
            _ufoEnablingRoutine = _coroutineReferenceMonoBehaviour.StartCoroutine(UFOSpawnRoutine());
        }

        private void OnLoadingCompleteLargeUFO(AsyncOperationHandle<GameObject> spawnHandle)
        {
            if (spawnHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Info($"Async opertaion to load large ufo success");
                _ufoLargeGameObject = spawnHandle.Result;
                _ufoLarge = InstantiateAndGetUFO(ref _ufoLargeGameObject);
                InitializeUFO(ref _ufoLarge);
            }
            else
            {
                Logger.Error($"AsyncOperation failed with following expection: {spawnHandle.OperationException.Message}");
            }
        }
        private void OnLoadingCompleteSmallUFO(AsyncOperationHandle<GameObject> spawnHandle)
        {
            if (spawnHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Info($"Async opertaion to load small ufo success");
                _ufoSmallGameObject = spawnHandle.Result;
                _ufoSmall = InstantiateAndGetUFO(ref _ufoSmallGameObject);
                InitializeUFO(ref _ufoSmall);
            }
            else
            {
                Logger.Error($"AsyncOperation failed with following expection: {spawnHandle.OperationException.Message}");
            }
        }

        private IUFO InstantiateAndGetUFO(ref GameObject instantiatedObject)
        {
            return Object.Instantiate(instantiatedObject, Vector3.zero, Quaternion.identity)
                .GetComponent<IUFO>();
        }
        private void InitializeUFO(ref IUFO targetUFO)
        {
            targetUFO.CacheScreenRect(_rect);
            targetUFO.Initialize();
            targetUFO.ToggleShipRenderer(false);
        }
        private void EnableRandomUFO()
        {
            if (Random.Range(0, 1.0f) < 0.5f)
            {
                _ufoLarge.Enable();
            }
            else
            {
                _ufoSmall.Enable();
            }
        }

        /// <summary>
        /// Spawns UFO after a predefined delay.
        /// Called when there is no active UFO in the game.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UFOSpawnRoutine()
        {
            yield return new WaitForSeconds(GameConstants.UFO.DelayBetweenSubsequentSpawns);
            EnableRandomUFO();
        }

        /// <summary>
        /// Method disables all ufo registered with UFOSpawner
        /// </summary>
        private void ReleaseAllUFO()
        {
            if (_ufoEnablingRoutine != null)
            {
                _coroutineReferenceMonoBehaviour.StopCoroutine(_ufoEnablingRoutine);
            }
            _ufoLarge.ToggleShipRenderer(false);
            _ufoSmall.ToggleShipRenderer(false);
        }
    }
}

