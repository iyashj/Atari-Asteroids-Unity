using System.Collections;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Assets.Tests.PlayMode.Gameplay.Subsystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using UFO = Assets.Scripts.Gameplay.MonoBehaviours.UFO;

namespace Assets.Tests.PlayMode.Gameplay.MonoBehaviour
{
    public class UFOTests : BaseSpaceObjectTests
    {

        private PrivateObject _ufoPrivateObject;
        private UFO _ufoObject;
        private Rigidbody2D _rb2d;
        private const string privateFieldIsShipEnabled = "_bIsShipEnabled";

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            TestCommons.ResetEventLibrary();
            base.OneTimeSetup();
            LoadPrefab("UFOLarge");
        }

        [UnityTest]
        public IEnumerator _5_HasUFOMonoBehaviour()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var attachedUFOMonoBehaviour = baseObject.GetComponent<UFO>();
            Assert.NotNull(attachedUFOMonoBehaviour, "does not have UFO attached");
        }

        [UnityTest]
        [TestCase(0.5f, ExpectedResult = null)]
        public IEnumerator _7_TestHasMovedAfterLaunch(float simulationTimeStep)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            Time.timeScale = 100;
            CacheFields();
            baseObject.Initialize();
            _ufoObject.Enable();
            
            var originalPosition = baseObject.transform.position;
            
            Physics2D.autoSimulation = false;
            Physics2D.Simulate(simulationTimeStep);
            
            var newPosition = baseObject.transform.position;
            Assert.AreNotEqual(originalPosition, newPosition,
                "newPosition: " + newPosition + " originalPosition: " + originalPosition);
        }

        [UnityTest]
        [TestCase(false, ExpectedResult = null)]
        [TestCase(true, ExpectedResult = null)]
        public IEnumerator _8_TestToggleShipRenderer(bool expectedShipRenderingStatus)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var _privateBaseObject = new PrivateObject(baseObject);
            baseObject.Initialize();
            CacheFields();
            _ufoObject.Enable();
            _ufoObject.ToggleShipRenderer(expectedShipRenderingStatus);
            
            var isShipToggled = _rb2d.velocity == Vector2.zero;
            isShipToggled &=
                _ufoObject.GetComponent<SpriteRenderer>().enabled == expectedShipRenderingStatus;
            isShipToggled &= _ufoObject.GetComponent<Collider2D>().enabled == expectedShipRenderingStatus;
            isShipToggled &= ((bool)_ufoPrivateObject.GetField(privateFieldIsShipEnabled)) == expectedShipRenderingStatus;
            isShipToggled &= _ufoObject.gameObject.activeSelf == expectedShipRenderingStatus;

            Assert.True(isShipToggled);
        }

        [UnityTest]
        [TestCase(Layer.UFOLayer, ExpectedResult = null)]
        public IEnumerator _9_TestHasCorrectCollisionLayer(int expectedLayer)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            Assert.True(GetLayer() == expectedLayer);
        }

        private void CacheFields()
        {
            _ufoObject = baseObject.GetComponent<UFO>();
            _ufoPrivateObject = new PrivateObject(_ufoObject);
            _rb2d = baseObject.GetComponent<Rigidbody2D>();
        }

    }
}
