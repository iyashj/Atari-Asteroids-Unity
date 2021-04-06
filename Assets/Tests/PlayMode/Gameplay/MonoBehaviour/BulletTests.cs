using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Tests.PlayMode.Gameplay.Subsystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode.Gameplay.MonoBehaviour
{
    public class BulletTests : BaseSpaceObjectTests
    {
        private PrivateObject _bulletPrivateObject;
        private Rigidbody2D _rb2d;
        private Bullet _bulletObject;

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            TestCommons.ResetEventLibrary();
            TestCommons.SetPhysicsCollisionRules();

            base.OneTimeSetup();
            LoadPrefab(Scripts.GameConstants.Prefabs.PlayerBullet);
        }

        [UnityTest]
        public IEnumerator _5_HasBulletMonoBehaviour()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var attachedBulletMonoBehaviour = baseObject.GetComponent<Bullet>();
            Assert.NotNull(attachedBulletMonoBehaviour, "does not have Bullet attached");
        }

        [UnityTest]
        [TestCase(0.3f, ExpectedResult = null)]
        public IEnumerator _6_HasMovedAfterLaunch(float simulationTimeStep)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _bulletObject = baseObject.GetComponent<Bullet>();
            _bulletObject.Initialize();
            var originalPosition = baseObject.transform.position;

            PhysicsPrerequisite();

            Physics2D.Simulate(simulationTimeStep);
            var newPosition = baseObject.transform.position;
            Assert.AreNotEqual(originalPosition, newPosition,
                "newPosition: " + newPosition + " originalPosition: " + originalPosition);
        }

        [UnityTest]
        [TestCase(0.1f, 0.5f, ExpectedResult = null)]
        [TestCase(1.1f, 2f, ExpectedResult = null)]
        public IEnumerator _7_HasSameDirection(float firstCheck, float secondCheck)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _bulletObject = baseObject.GetComponent<Bullet>();
            _bulletObject.Initialize();
            PhysicsPrerequisite();

            Physics2D.Simulate(firstCheck);
            var originalDirection = _rb2d.velocity.normalized;
            Physics2D.Simulate(secondCheck);
            var newDirection = _rb2d.velocity.normalized;

            var comparer = new FloatEqualityComparer(10e-1f);
            Assert.IsTrue(originalDirection == newDirection);
        }

        [UnityTest]
        [TestCase(Layer.PlayerBulletLayer, ExpectedResult = null)]
        public IEnumerator _8_HasCorrectCollisionLayer(int expectedLayer)
        {
            yield return new WaitWhile(_testInitializationPredicate);
            Assert.True(GetLayer() == expectedLayer);
        }

        [UnityTest]
        public IEnumerator _9_ExpiresAfterLifetime()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            _bulletObject = baseObject.GetComponent<Bullet>();
            _bulletObject.Initialize();
            
            var expiryCalled = false;
            _bulletObject.ItemExpired += () => expiryCalled = true; 
            PhysicsPrerequisite();
            
            yield return new WaitForSeconds(bulletLifetime + 0.3f);
            Assert.True(expiryCalled);
        }

        private void CacheFields()
        {
            _bulletObject = baseObject.GetComponent<Bullet>();
            _bulletPrivateObject = new PrivateObject(_bulletObject);
            _rb2d = baseObject.GetComponent<Rigidbody2D>();
        }

        private const float bulletLaunchSpeed = 700f;
        private const float bulletLifetime = 0.12f;
        private void PhysicsPrerequisite()
        {
            CacheFields();
            _bulletObject.Launch(bulletLaunchSpeed, bulletLifetime, Vector3.up);
            ResetSpaceObject();
            Time.timeScale = 100;
            Physics2D.autoSimulation = false;
        }

        

    }
}
