using System.Collections;
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
    public class UFOSpawnerTests
    {
        private IUFOSpawner _ufoSpawner;
        private PrivateObject _privateObject;
        private const string spaceshipTransform = "_spaceshipTransform";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadSceneAsync(TestCommons.TestScene).completed += operation =>
            {
                Time.timeScale = 100f;
                TestCommons.ResetEventLibrary();
                var ufoObject = new GameObject("ufo");
                var monoBehaviourReference = ufoObject.AddComponent<TestReferenceMonoBehaviour>();
                var rect = new Rect(Vector3.zero, new Vector2(1, 1));
                _ufoSpawner = new UFOSpawner(rect, monoBehaviourReference);
                _ufoSpawner.Preload();
                _privateObject = new PrivateObject(_ufoSpawner);
                _privateObject.SetField(spaceshipTransform, ufoObject.transform);
            };
        }

        [UnityTest]
        public IEnumerator _1_TestSpawn()
        {
            yield return new WaitWhile(() => !_ufoSpawner.IsReady());
            var ufosActiveBeforeSpawnCall = Object.FindObjectsOfType<UFO>();
            _ufoSpawner.Spawn();
            yield return IsUFOGettingEnabledAfterRoutine(ufosActiveBeforeSpawnCall);
        }

        [UnityTest]
        public IEnumerator _2_TestOnUFOExplosionEvent()
        {
            yield return new WaitWhile(() => !_ufoSpawner.IsReady());
            var ufosActiveBeforeSpawnCall = Object.FindObjectsOfType<UFO>();
            EventBus.GetInstance().Publish(new UfoExplodedPayload(1, new Vector2()));
            yield return IsUFOGettingEnabledAfterRoutine(ufosActiveBeforeSpawnCall);
        }

        private IEnumerator IsUFOGettingEnabledAfterRoutine(UFO[] ufosActiveBeforeSpawnCall)
        {
            yield return new WaitForSeconds(Scripts.GameConstants.UFO.DelayBetweenSubsequentSpawns + 0.1f);
            var ufosActiveAfterSpawnCall = Object.FindObjectsOfType<UFO>();
            Assert.True(ufosActiveAfterSpawnCall.Length - ufosActiveBeforeSpawnCall.Length == 1);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Time.timeScale = 1;
        }
        
    }
}
