using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

using Assets.Scripts.GameStructure.Pooling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using Assert = NUnit.Framework.Assert;
using Assets.Tests.PlayMode.Gameplay.Subsystems;

namespace Assets.Tests.PlayMode.GameStructure.Pooling
{
    public class BaseObjectPoolTests
    {
        private readonly string privateFieldPrefabObject = "PrefabObject";
        private readonly string privateFieldAvailableItems = "_availableItems";

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            SceneManager.LoadScene(TestCommons.TestScene);
        }

        [UnityTest]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel0, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel1, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel2, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.PlayerBullet, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.EnemyBullet, ExpectedResult = null)]
        public IEnumerator _1_TestIsSpawnable(string prefabKey)
        {
            GameObject spawnedObject = null;
            var handle = SpawnerUtility.InstantiateGameObject(prefabKey);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                spawnedObject = handle.Result;
            }

            Assert.NotNull(spawnedObject);
        }

        [UnityTest]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel0, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel1, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel2, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.PlayerBullet, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.EnemyBullet, 5, ExpectedResult = null)]
        public IEnumerator _2_TestPrefabObject(string objectKey, int initialPoolSize)
        {
            BaseObjectPool<IPoolableItem> baseObjectPool =
                new BaseObjectPool<IPoolableItem>(objectKey, initialPoolSize);
            yield return new WaitWhile(() => !baseObjectPool.IsReady());
            
            PrivateObject privateObject = new PrivateObject(baseObjectPool);
            GameObject loadedPrefabObject = (GameObject) privateObject.GetFieldOrProperty(privateFieldPrefabObject);
            Assert.NotNull(loadedPrefabObject);
        }

        [UnityTest]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel0,5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel1, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel2, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.PlayerBullet, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.EnemyBullet, 5, ExpectedResult = null)]
        public IEnumerator _3_TestInitializeAndFillPoll(string objectKey, int initialPoolSize)
        {
            BaseObjectPool<IPoolableItem> baseObjectPool =
                new BaseObjectPool<IPoolableItem>(objectKey, initialPoolSize);
           yield return  new WaitWhile(() => !baseObjectPool.IsReady());
           
           PrivateObject privateObject = new PrivateObject(baseObjectPool);
           List<IPoolableItem> availableItems = (List<IPoolableItem>) privateObject.GetField(privateFieldAvailableItems);
           var availableItemsIsFilled = availableItems.Count > 0;
           Assert.True(availableItemsIsFilled);
        }

        [UnityTest]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel0, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel1, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel2, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.PlayerBullet, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.EnemyBullet, 5, ExpectedResult = null)]
        public IEnumerator _4_TestCanGetFreeItem(string objectKey, int initialPoolSize)
        {
            BaseObjectPool<IPoolableItem> baseObjectPool =
                new BaseObjectPool<IPoolableItem>(objectKey, initialPoolSize);
            yield return new WaitWhile(() => !baseObjectPool.IsReady());

            var freeItem = baseObjectPool.GetFreeItem();
            Assert.NotNull(freeItem);
        }

        [UnityTest]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel0, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel1, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.AsteroidLevel2, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.PlayerBullet, 5, ExpectedResult = null)]
        [TestCase(Scripts.GameConstants.Prefabs.EnemyBullet, 5, ExpectedResult = null)]
        public IEnumerator _5_TestCanGetFreeItemEvenAfterExhaust(string objectKey, int initialPoolSize)
        {
            BaseObjectPool<IPoolableItem> baseObjectPool =
                new BaseObjectPool<IPoolableItem>(objectKey, initialPoolSize);
            yield return new WaitWhile(() => !baseObjectPool.IsReady());

            // exhausting all possibilities
            for (int i0 = 0; i0 < initialPoolSize; i0++)
            {
                baseObjectPool.GetFreeItem();
            }

            // get another free item by exhaust
            // testing append function
            var freeItem = baseObjectPool.GetFreeItem();

            Assert.NotNull(freeItem);
        }

    }
}
