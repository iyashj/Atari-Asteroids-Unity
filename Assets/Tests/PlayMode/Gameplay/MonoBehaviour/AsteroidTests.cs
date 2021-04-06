using System.Collections;
using Assets.Scripts.Data;
using Assets.Scripts.GameConstants;
using NUnit.Framework;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Tests.PlayMode.Gameplay.Subsystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode.Gameplay.MonoBehaviour
{
    public class AsteroidTests : BaseSpaceObjectTests
    {
        private Asteroid _asteroidObject;
        private PrivateObject _asteroidPrivateObject;
        private Rigidbody2D _rb2d;

        private const string PrivateMethodLaunch = "Launch";
        private const string PrivateMethodGetRandomTorque = "GetRandomTorque";
        private const string PrivateMethodGetRandomForce = "GetRandomForce";
        private const string PrivateMethodGetRandomDirection = "GetRandomDirection";
        private const string PrivateFieldAsteroidConfig = "_asteroidConfig";

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            TestCommons.ResetEventLibrary();
            TestCommons.SetPhysicsCollisionRules();
         
            base.OneTimeSetup();
            LoadPrefab(Scripts.GameConstants.Prefabs.AsteroidLevel0);
        }

        [UnityTest]
        public IEnumerator _5_HasAsteroidMonoBehaviour()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _asteroidObject = baseObject.GetComponent<Asteroid>();
            Assert.NotNull(_asteroidObject, "does not have Asteroid attached");
        }

        [UnityTest]
        [TestCase(0.3f,  ExpectedResult = null)]
        public IEnumerator _6_HasMovedAfterLaunch(float simulationTimeStep)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _asteroidObject = baseObject.GetComponent<Asteroid>();
            _asteroidObject.Initialize();
            var originalPosition = baseObject.transform.position;
            
            PhysicsPrerequisite();
            
            Physics2D.Simulate(simulationTimeStep);
            var newPosition = baseObject.transform.position;
            Assert.AreNotEqual(originalPosition, newPosition,
                "newPosition: " + newPosition + " originalPosition: " + originalPosition);
        }

        [UnityTest]
        [TestCase(0.8f, ExpectedResult = null)]
        [TestCase(3f, ExpectedResult = null)]
        public IEnumerator _7_HasRotatedAfterLaunch(float simulationTimeStep)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _asteroidObject = baseObject.GetComponent<Asteroid>();
            _asteroidObject.Initialize();
            var originalRotation = baseObject.transform.rotation;
            
            PhysicsPrerequisite();
            
            Physics2D.Simulate(simulationTimeStep);
            var newRotation = baseObject.transform.rotation;
            Assert.AreNotEqual(originalRotation, newRotation,
                "newRotation: " + newRotation + " originalRotation: " + originalRotation);
        }

        [UnityTest]
        [TestCase(0.1f, 0.5f, ExpectedResult = null)]
        [TestCase(1.1f, 2f, ExpectedResult = null)]
        public IEnumerator _8_HasSameDirection(float firstCheck, float secondCheck)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _asteroidObject = baseObject.GetComponent<Asteroid>();
            _asteroidObject.Initialize();
            PhysicsPrerequisite();

            Physics2D.Simulate(firstCheck);
            var originalDirection = _rb2d.velocity.normalized;
            Physics2D.Simulate(secondCheck);
            var newDirection = _rb2d.velocity.normalized;

            var comparer = new FloatEqualityComparer(10e-1f);
            Assert.IsTrue(originalDirection == newDirection);
        }

        [UnityTest]
        [TestCase(Layer.AsteroidLayer, ExpectedResult = null)]
        public IEnumerator _9_HasCorrectCollisionLayer(int expectedLayer)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            
            Assert.True(GetLayer() == expectedLayer);
        }

        private void CacheFields()
        {
            var asteroidObject = baseObject.GetComponent<Asteroid>();
            _asteroidPrivateObject = new PrivateObject(asteroidObject);
            _asteroidPrivateObject.SetField(PrivateFieldAsteroidConfig, TestConfigs.GetAsteroidConfig());
            _rb2d = baseObject.GetComponent<Rigidbody2D>();
        }
        

        private void PhysicsPrerequisite()
        {
            CacheFields();
            Launch();
            ResetSpaceObject();
            Time.timeScale = 100;
            Physics2D.autoSimulation = false;
        }

        private void Launch()
        {
            _asteroidPrivateObject.Invoke(PrivateMethodLaunch, GetRandomForce(), GetRandomTorque(), GetRandomDirection());
        }
        private float GetRandomTorque()
        {
            return (float)(_asteroidPrivateObject.Invoke(PrivateMethodGetRandomTorque));
        }
        private float GetRandomForce()
        {
            return (float)(_asteroidPrivateObject.Invoke(PrivateMethodGetRandomForce));
        }
        private Vector3 GetRandomDirection()
        {
            return (Vector2) (_asteroidPrivateObject.Invoke(PrivateMethodGetRandomDirection));
        }

    }
}
