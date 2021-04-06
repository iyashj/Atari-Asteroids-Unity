using System.Collections;
using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Tests.PlayMode.Gameplay.Subsystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode.Gameplay.MonoBehaviour
{
    public class SpaceshipTests : BaseSpaceObjectTests
    {
        private Spaceship spaceship;
        private PrivateObject _privateObjectSpaceship;
        private const string PrivateSpaceshipDead = "DelayedRespawn";
        private const string PrivateFieldSpaceshipConfig = "_spaceshipConfig";
        private const string PrivateMethodInitializeController = "InitializeController";

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            TestCommons.ResetEventLibrary();
            base.OneTimeSetup();
            LoadPrefab("Spaceship");
        }

        [UnityTest]
        public IEnumerator _5_HasSpaceshipMonoBehaviour()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var attachedSpaceshipMonoBehaviour = baseObject.GetComponent<Spaceship>();
            Assert.NotNull(attachedSpaceshipMonoBehaviour, "does not have Spaceship attached");
        }

        [UnityTest]
        public IEnumerator _6_OnTriggerEnterRespawn()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            CacheFields();
            var spaceshipConfig = TestConfigs.GetSpaceshipConfig();
            var spriteRenderer = spaceship.GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = false;

            SpaceshipDependencyData injectionData =
                new SpaceshipDependencyData(null, new GameStatsHandler(3, new ScoreConfig()), 2.5f);
            spaceship.Inject(injectionData);

            _privateObjectSpaceship.Invoke(PrivateMethodInitializeController);
            _privateObjectSpaceship.SetField(PrivateFieldSpaceshipConfig, spaceshipConfig);
            new PrivateObject(baseObject).Invoke(PrivateSpaceshipDead);
            
            yield return new WaitForSeconds(spaceshipConfig.GetWaitBeforeRespawn() + 0.5f);
            Assert.True(spriteRenderer.enabled);
        }

        [UnityTest]
        [TestCase(Scripts.GameConstants.Layer.SpaceshipLayer, ExpectedResult = null)]
        public IEnumerator _7_HasCorrectCollisionLayer(int expectedLayer)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            Assert.True(GetLayer() == expectedLayer);
        }

        private void CacheFields()
        {
            spaceship = baseObject.GetComponent<Spaceship>();
            spaceship.Initialize();
            _privateObjectSpaceship = new PrivateObject(spaceship);
        }
    }
}
