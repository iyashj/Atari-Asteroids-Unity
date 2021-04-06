using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Pooling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using Object = UnityEngine.Object;

namespace Assets.Tests.PlayMode.Gameplay.Subsystems
{
    public class TestConfigs
    {
        private const float cooldown = 0.2f;
        private const float launchSpeed = 700f;
        private const float lifetime = 0.5f;
        private const string bulletKey = "PlayerBullet";
        private const string enemyBulletKey = "EnemyBullet";

        public static ITurretConfig GetTurretConfig()
        {
            var substitutedTurret = Substitute.For<ITurretConfig>();
            substitutedTurret.GetCooldown().Returns(cooldown);
            substitutedTurret.GetLaunchSpeed().Returns(launchSpeed);
            substitutedTurret.GetLifetime().Returns(lifetime);
            substitutedTurret.GetBulletKey().Returns(bulletKey);
            return substitutedTurret;
        }

        private const float maxAngle = 30;
        private const float minAngle = -30;
        private const float movementForce = 1000f;

        public static IUFOConfig GetUFOConfig()
        {
            var substitutedUFO = Substitute.For<IUFOConfig>();
            substitutedUFO.GetMaxAngle().Returns(maxAngle);
            substitutedUFO.GetMinAngle().Returns(minAngle);
            substitutedUFO.GetMovementForce().Returns(movementForce);
            return substitutedUFO;
        }

        private const float SpaceshipMovementForce = 300;
        private const float SpaceshipTurnTorque = -1200;
        private const float SpaceshipWaitBeforeRespawn = 2;
        private const float SpaceshipWaitForSafeSpawnDelay = 0.5f;
        private const float SpaceshipWaitForHyperspace = 2;

        public static ISpaceshipConfig GetSpaceshipConfig()
        {
            var substitutedSpaceshipConfig = Substitute.For<ISpaceshipConfig>();
            
            substitutedSpaceshipConfig.GetMovementForce().Returns(SpaceshipMovementForce);
            substitutedSpaceshipConfig.GetTurnTorque().Returns(SpaceshipTurnTorque);
            substitutedSpaceshipConfig.GetWaitBeforeRespawn().Returns(SpaceshipWaitBeforeRespawn);
            substitutedSpaceshipConfig.GetWaitBeforeRespawn().Returns(SpaceshipWaitForSafeSpawnDelay);
            substitutedSpaceshipConfig.GetWaitForHyperspace().Returns(SpaceshipWaitForHyperspace);
            return substitutedSpaceshipConfig;
        }

        private const float AsteroidMinForce = 50;
        private const float AsteroidMaxForce = 75;
        private const float AsteroidMaxTorque = 50;
        private const float AsteroidMinTorque = 25;

        public static IAsteroidConfig GetAsteroidConfig()
        {
            var asteroidConfig = Substitute.For<IAsteroidConfig>();
            asteroidConfig.GetMinForce().Returns(AsteroidMinForce);
            asteroidConfig.GetMaxForce().Returns(AsteroidMaxForce);
            asteroidConfig.GetMaxTorque().Returns(AsteroidMaxTorque);
            asteroidConfig.GetMinTorque().Returns(AsteroidMinTorque);
            return asteroidConfig;
        }

        private const float safeDistance = 2.5f;
        private const int minAsteroidInLevel = 4;
        private const int asteroidsPerLevel = 2;
        private const float waitBeforeLevelSetup = 3;

        public static ILevelingConfig GetLevelingConfig()
        {
            ILevelingConfig levelConfig = Substitute.For<ILevelingConfig>();
            levelConfig.GetSafeDistanceFromPlayerShip().Returns(safeDistance);
            levelConfig.GetAsteroidsIncreasedPerLevel().Returns(asteroidsPerLevel);
            levelConfig.GetMinimumAsteroidsInALevel().Returns(minAsteroidInLevel);
            levelConfig.GetWaitBeforeLevelSetup().Returns(waitBeforeLevelSetup);
            return levelConfig;
        }

    }


    public class TurretTests
    {
        private readonly string privateFieldAvailableItems = "_availableItems";

        private Turret _turret;

        private TestReferenceMonoBehaviour monoBehaviour;

        [SetUp]
        public void Setup()
        {
            
            SceneManager.LoadScene(TestCommons.TestScene);
            TestCommons.ResetEventLibrary();
            //_turret = GetTurret();
        }

        [UnityTest]
        public IEnumerator _1_TestTurretBulletPoolIsInitializedInConstructor()
        {
            monoBehaviour = new GameObject("testTurret").AddComponent<TestReferenceMonoBehaviour>();
            _turret = new Turret(TestConfigs.GetTurretConfig(), monoBehaviour);
            yield return new WaitWhile(() => !_turret.BulletPool.IsReady());
            var freeItem = _turret.BulletPool.GetFreeItem();
            Assert.NotNull(_turret.BulletPool.GetFreeItem());
        }

        [UnityTest]
        public IEnumerator _2_TestCanShootAfterTurretSetup()
        {
            monoBehaviour = new GameObject("testTurret").AddComponent<TestReferenceMonoBehaviour>();
            _turret = new Turret(TestConfigs.GetTurretConfig(), monoBehaviour);
            yield return new WaitWhile(() => !_turret.BulletPool.IsReady());
            var privateObject = new PrivateObject(_turret);
            var canShoot = (bool) privateObject.Invoke("CanShoot");
            Assert.True(canShoot);
        }

        [UnityTest]
        public IEnumerator _3_TestShoot()
        {
            monoBehaviour = new GameObject("testTurret").AddComponent<TestReferenceMonoBehaviour>();
            _turret = new Turret(TestConfigs.GetTurretConfig(), monoBehaviour);
            yield return new WaitWhile(() => !_turret.BulletPool.IsReady());

            var allObjectsOfTypeBulletBeforeFire = Object.FindObjectsOfType<Bullet>();
            _turret.Shoot(Vector3.zero, Vector3.up);
            var allObjectsOfTypeBulletAfterFire = Object.FindObjectsOfType<Bullet>();

            var increaseInNumberOfBulletsOnFire =
                allObjectsOfTypeBulletAfterFire.Length - allObjectsOfTypeBulletBeforeFire.Length;

            Assert.True(increaseInNumberOfBulletsOnFire == 1);
        }

        [UnityTest]
        [TestCase(2, ExpectedResult = null)]
        [TestCase(10, ExpectedResult = null)]
        [TestCase(6, ExpectedResult = null)]
        public IEnumerator _4_TestCanNotShootBulletsWithoutWait(int consecutiveShoot)
        {
            monoBehaviour = new GameObject("testTurret").AddComponent<TestReferenceMonoBehaviour>();
            _turret = new Turret(TestConfigs.GetTurretConfig(), monoBehaviour);
            yield return new WaitWhile(() => !_turret.BulletPool.IsReady());
            _turret.Shoot(Vector3.zero, Vector3.up);
            var allObjectsOfTypeBulletBeforeFiringRally = Object.FindObjectsOfType<Bullet>();
            
            for (int i0 = 0; i0 < consecutiveShoot; i0++)
            {
                _turret.Shoot(Vector3.zero, Vector3.up);
            }

            var allObjectsOfTypeBulletAfterFire = Object.FindObjectsOfType<Bullet>();
            var bulletsFired =
                allObjectsOfTypeBulletAfterFire.Length - allObjectsOfTypeBulletBeforeFiringRally.Length;

            Assert.True(bulletsFired == 0);
        }

        [UnityTest]
        public IEnumerator _5_TestCanShootAfterWait()
        {
            monoBehaviour = new GameObject("testTurret").AddComponent<TestReferenceMonoBehaviour>();
            var turretConfig = TestConfigs.GetTurretConfig();
            _turret = new Turret(turretConfig, monoBehaviour);
            yield return new WaitWhile(() => !_turret.BulletPool.IsReady());
            _turret.Shoot(Vector3.zero, Vector3.up);
            var allObjectsOfTypeBulletBeforeFire = Object.FindObjectsOfType<Bullet>();
            yield return new WaitForSeconds(turretConfig.GetCooldown() + 0.1f);
            _turret.Shoot(Vector3.zero, Vector3.up);
            var allObjectsOfTypeBulletAfterFire = Object.FindObjectsOfType<Bullet>();

            var bulletsFired =
                allObjectsOfTypeBulletAfterFire.Length - allObjectsOfTypeBulletBeforeFire.Length;

            Assert.True(bulletsFired == 1);
        }
    }
}
