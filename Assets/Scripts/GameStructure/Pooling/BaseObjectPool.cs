using System.Collections.Generic;

using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.GameStructure.Pooling
{
    /// <summary>
    /// Interface for BaseObjectPool
    /// </summary>
    /// <typeparam name="T">ItemType being pooled</typeparam>
    public interface IBaseObjectPool<T> where T : IPoolableItem
    {
        /// <summary>
        /// Obtain free item from the pool.
        /// Append more items to the pool in case, no free item is available
        /// </summary>
        /// <returns>FreeItem</returns>
        T GetFreeItem();

        /// <summary>
        /// Returns true if the pool is ready to use
        /// </summary>
        /// <returns></returns>
        bool IsReady();

        /// <summary>
        /// Relase all the items back to the available list
        /// </summary>
        void ReleaseAll();
    }

    /// <summary>
    /// BasePool of poolable item. 
    /// Used to get free item and return pool item for recycle.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseObjectPool<T> : IBaseObjectPool<T> where T : IPoolableItem
    {
        public void ReleaseAll()
        {
            for(int i0 = 0; i0 < _engagedItems.Count; i0++)
            {
                _engagedItems[i0].Release();
            }
        }

        /// <summary>
        /// Indicates the length by which the pool is extended on hitting exhaustion
        /// </summary>
        private const int AppendLengthOnExhaustion = 5;

        /// <summary>
        /// Parent for all the items in the pool.
        /// </summary>
        private readonly GameObject _parentObject;

        /// <summary>
        /// List of items currently in use
        /// </summary>
        private List<T> _engagedItems;
        /// <summary>
        /// List of items available in the pool
        /// </summary>
        private List<T> _availableItems;

        private readonly int _initialPoolSize;

        #region CONSTRUCTOR
        /// <summary>
        /// Constructor to initialize pooled item and initial pool size.
        /// </summary>
        /// <param name="objectPrefabKey">key of the item to be pooled</param>
        /// <param name="initialPoolSize">initial capacity of the pool</param>
        public BaseObjectPool(string objectPrefabKey, int initialPoolSize)
        {
            PoolPrefabKey = objectPrefabKey;
            _initialPoolSize = initialPoolSize;
            _parentObject = new GameObject(objectPrefabKey + "_parent");
            Initialize();
            CachePrefab();
        }

        #endregion

        #region API

        /// <summary>
        /// Implements <see cref="IBaseObjectPool{T}"/>
        /// </summary>
        /// <returns></returns>
        public bool IsReady() => PrefabObject != null;

        /// <summary>
        /// Implements <see cref="IBaseObjectPool{T}"/>
        /// </summary>
        /// <returns></returns>
        public T GetFreeItem()
        {
            if (_availableItems.Count <= 0)
            {
                FillPool(AppendLengthOnExhaustion);
            }

            var poolableItem = _availableItems[0];

            if (_availableItems.Contains(poolableItem))
            {
                _availableItems.Remove(poolableItem);
            }

            if (!_engagedItems.Contains(poolableItem))
            {
                _engagedItems.Add(poolableItem);
            }

            poolableItem.Engage();
            return poolableItem;
        }
        #endregion

        /// <summary>
        /// Fills the pool with the capacity equal to given pool size
        /// This should be called once the loading has been completed
        /// </summary>
        /// <param name="poolSize"></param>
        private void FillPool(int poolSize)
        {
            for (int i0 = 0; i0 < poolSize; i0++)
            {
                var newItem = Object.Instantiate(PrefabObject, _parentObject.transform).GetComponent<T>();
                newItem.Init();
                newItem.ItemExpired += () => ReturnToPool(ref newItem);
                newItem.Release();
                _availableItems.Add(newItem);
            }
        }

        /// <summary>
        /// Initialize the pool by caching prefab through addressable.
        /// </summary>
        /// <param name="poolSize">initial capacity of the pool</param>
        private void Initialize()
        {
            _engagedItems = new List<T>();
            _availableItems = new List<T>();
        }

        private void CachePrefab()
        {
            SpawnerUtility.InstantiateGameObject(PoolPrefabKey).Completed += OnPrefabLoadingComplete;
        }

        private void OnPrefabLoadingComplete(AsyncOperationHandle<GameObject> spawnHandler)
        {
            if (spawnHandler.Status == AsyncOperationStatus.Succeeded)
            {
                PrefabObject = spawnHandler.Result;
                var item = PrefabObject.GetComponent<IPoolableItem>();
                item.Init();
                item.Release();
                FillPool(_initialPoolSize);
            }
        }

        /// <summary>
        /// Cached prefab for the item to be spawned in the pool
        /// </summary>
        private GameObject PrefabObject { get; set; }
        
        /// <summary>
        /// Addressable key to the prefab of the pool
        /// </summary>
        private string PoolPrefabKey { get; set; }

        /// <summary>
        /// Return the item to the pool
        /// </summary>
        /// <param name="poolableItem"></param>
        protected void ReturnToPool(ref T poolableItem)
        {
            poolableItem.Release();
            _engagedItems.Remove(poolableItem);
            _availableItems.Add(poolableItem);
        }
    }
}

