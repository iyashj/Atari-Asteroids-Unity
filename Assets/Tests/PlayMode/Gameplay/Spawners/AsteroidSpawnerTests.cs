using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Spawners;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Assets.Tests.PlayMode.Gameplay.Subsystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode.Gameplay.Spawners
{
    public class AsteroidSpawnerTests
    {
        private const int asteroidsSpawnedOnExplosion = 2;
        private const string privateFieldAsteroidLevel = "_asteroidLevel";
        private IAsteroidSpawner asteroidSpawner;

        [OneTimeSetUp]
        public void OnetimeSetup()
        {

            SceneManager.LoadSceneAsync(TestCommons.TestScene).completed += operation =>
            {
                asteroidSpawner = new AsteroidSpawner();
                asteroidSpawner.Preload();
            };
        }

        [TearDown]
        public void TearDown()
        {
            var asteroidsAfterSpawnCall = Object.FindObjectsOfType<Asteroid>().ToList();
            var asteroidLength = asteroidsAfterSpawnCall.Count;
            for (int i = 0; i < asteroidLength; i++)
            {
                Object.DestroyImmediate(asteroidsAfterSpawnCall[i]);
            }
        }
        
        [UnityTest]
        [TestCase(2, 4,ExpectedResult = null)]
        [TestCase(1, 5,ExpectedResult = null)]
        [TestCase(0, 2,ExpectedResult = null)]
        public IEnumerator _1_TestSpawnAsteroid(int asteroidLevelToBeSpawned, int numberOfAsteroidsToBeSpawned)
        {
            yield return new WaitWhile(() => !asteroidSpawner.IsReady());
            
            var asteroidsBeforeSpawnCall = Object.FindObjectsOfType<Asteroid>().ToList();

            for (int i0 = 0; i0 < numberOfAsteroidsToBeSpawned; i0++)
            {
                asteroidSpawner.SpawnAsteroid(asteroidLevelToBeSpawned, Vector3.zero);
            }

            var asteroidsAfterSpawnCall = Object.FindObjectsOfType<Asteroid>().ToList();
            
            foreach (var asteroid in asteroidsBeforeSpawnCall)
            {
                asteroidsAfterSpawnCall.Remove(asteroid);
            }

            bool asteroidsOfExpectedLevelSpawned = true;
            foreach (var asteroid in asteroidsAfterSpawnCall)
            {
                int asteroidLevel = (int)new PrivateObject(asteroid).GetField(privateFieldAsteroidLevel);
                asteroidsOfExpectedLevelSpawned &= asteroidLevel == asteroidLevelToBeSpawned;
            }

            Assert.True(asteroidsAfterSpawnCall.Count == numberOfAsteroidsToBeSpawned && asteroidsOfExpectedLevelSpawned);
            
        }

        [UnityTest]
        [TestCase(2,1, ExpectedResult = null)]
        [TestCase(1, 0, ExpectedResult = null)]
        public IEnumerator _2_TestOnAsteroidExplosionAsteroidsSpawned(int originalAsteroidLevel, int expectedLevel)
        {
            yield return new WaitWhile(() => !asteroidSpawner.IsReady());

            var asteroidsBeforeEvent = Object.FindObjectsOfType<Asteroid>().ToList();
            EventBus.GetInstance().Publish(new AsteroidExplodedPayload(originalAsteroidLevel, Vector3.zero));
            var asteroidsAfterEvent = Object.FindObjectsOfType<Asteroid>().ToList();

            foreach (var asteroid in asteroidsBeforeEvent)
            {
                asteroidsAfterEvent.Remove(asteroid);
            }

            bool asteroidsOfExpectedLevelSpawned = true;
            foreach (var asteroid in asteroidsAfterEvent)
            {
                int asteroidLevel = (int) new PrivateObject(asteroid).GetField(privateFieldAsteroidLevel);
                asteroidsOfExpectedLevelSpawned &= asteroidLevel == expectedLevel;
            }

            Assert.True(asteroidsAfterEvent.Count == asteroidsSpawnedOnExplosion && asteroidsOfExpectedLevelSpawned);
        }

        [UnityTest]
        public IEnumerator _3_TestAsteroidNotGeneratedLevel0Asteroid()
        {
            yield return new WaitWhile(() => !asteroidSpawner.IsReady());

            var asteroidsBeforeEvent = Object.FindObjectsOfType<Asteroid>().ToList();
            EventBus.GetInstance().Publish(new AsteroidExplodedPayload(0, Vector3.zero));
            var asteroidsAfterEvent = Object.FindObjectsOfType<Asteroid>().ToList();
            Assert.True(asteroidsAfterEvent.Count == asteroidsBeforeEvent.Count);
        }
    }
}
