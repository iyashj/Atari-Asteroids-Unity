using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Spawners;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode
{
    public class TestReferenceMonoBehaviour : UnityEngine.MonoBehaviour
    {

    }
}

namespace Assets.Tests.PlayMode.Gameplay.Subsystems
{
    public class LevelHandlerTests
    {
        private const string privateMethodGetAsteroidCountForLevel = "GetAsteroidsCountForLevel";
        private const string privateFieldSpaceshipTransform = "_spaceshipTransform";
        private const string privateMethodAreAtSafeDistance = "AreAtSafeDistance";
        private const string privateFieldAsteroidRemainingInLevel = "_asteroidRemainingInLevel";

      

        private IAsteroidSpawner _asteroidSpawner;
        private IScreenHandler _screenHandler;
        private ILevelingConfig _levelingConfig;
        private UnityEngine.MonoBehaviour _monoBehaviour;

        private LevelHandler _levelHandler;
        private PrivateObject _privateObject;
        private Camera _camera;
        private Transform _spaceshipTransform;

        private Camera GetConfiguredCamera()
        {
            var cameraGameObject = new GameObject("Camera");
            cameraGameObject.transform.position = new Vector3(0, 0, -10);
            _camera = cameraGameObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 5;
            _camera.rect = new Rect(Vector2.zero, new Vector2(1, 1));
            _camera.pixelRect = new Rect(Vector2.zero, new Vector2(1920, 950));
            _camera.orthographicSize = 5;
            return _camera;
        }

        private void Construct()
        { 
            _camera = GetConfiguredCamera();
            _levelingConfig = TestConfigs.GetLevelingConfig();
            _asteroidSpawner = new AsteroidSpawner();
            _asteroidSpawner.Preload();
            _screenHandler = new ScreenHandler(_camera);
            _monoBehaviour = new GameObject("MonoRef").AddComponent<TestReferenceMonoBehaviour>();
            _spaceshipTransform = _camera.gameObject.transform;

            LevelHandlerConstructionData constructionData =
                new LevelHandlerConstructionData(_asteroidSpawner, _screenHandler, _levelingConfig, _monoBehaviour);

            _levelHandler = new LevelHandler(constructionData);
            _privateObject = new PrivateObject(_levelHandler);
        }

        [OneTimeSetUp]
        public void Setup()
        {
            TestCommons.ResetEventLibrary();
            Construct();
        }

        [UnityTest]
        [TestCase(1, ExpectedResult = null)]
        [TestCase(5, ExpectedResult = null)]
        public IEnumerator _1_TestSetupLevel(int levelIndex)
        {
            yield return new WaitWhile(() => !_asteroidSpawner.IsReady());
            var asteroidsBeforeSettingUpLevel = Object.FindObjectsOfType<Asteroid>();
            var asteroidCountsThatShouldBeSpawned = (int)_privateObject.Invoke(privateMethodGetAsteroidCountForLevel, levelIndex);
            _privateObject.SetField(privateFieldSpaceshipTransform, _spaceshipTransform);
            _levelHandler.Start(levelIndex);
            yield return new WaitForSeconds(_levelingConfig.GetWaitBeforeLevelSetup());
            var asteroidCountAfterSettingUpLevel = Object.FindObjectsOfType<Asteroid>();
            var spawnedAsteroidCount =
                asteroidCountAfterSettingUpLevel.Length - asteroidsBeforeSettingUpLevel.Length;
            Assert.True(asteroidCountsThatShouldBeSpawned == spawnedAsteroidCount,
                "asteroidsToBeSpawned " + asteroidCountsThatShouldBeSpawned + " spawnedAsteroidCount " +
                spawnedAsteroidCount);
        }

        [UnityTest]
        [TestCase(4, ExpectedResult = null)]
        [TestCase(1, ExpectedResult = null)]
        public IEnumerator _2_TestOnAsteroidDestroyedChangeAsteroidRemainingCount(int eventCount)
        {
            yield return new WaitWhile(() => !_asteroidSpawner.IsReady());
            var asteroidsInLevelBeforeEvent = (int) _privateObject.GetField(privateFieldAsteroidRemainingInLevel);
            for (int i = 0; i < eventCount; i++)
            {
                EventBus.GetInstance().Publish(new AsteroidExplodedPayload(1, Vector2.zero));
            }

            var asteroidsInLevelAfterEvent = (int) _privateObject.GetField(privateFieldAsteroidRemainingInLevel);
            Assert.True(asteroidsInLevelBeforeEvent - asteroidsInLevelAfterEvent == eventCount);
        }
        
        [UnityTest]
        public IEnumerator _3_TestOnSpaceshipLoaded()
        {
            GameObject spaceshipObject = new GameObject("spaceshipObject");
            spaceshipObject.transform.position = new Vector3(15,16,0);

            var loadedSpaceship = Substitute.For<ISpaceship>();
            loadedSpaceship.GetTransform().Returns(spaceshipObject.transform);

            yield return new WaitWhile(() => !_asteroidSpawner.IsReady());
            EventBus.GetInstance().Publish(new SpaceshipSpawnedPayload(loadedSpaceship));

            Assert.AreEqual(loadedSpaceship.GetTransform(), spaceshipObject.transform);
        }
    }
}
