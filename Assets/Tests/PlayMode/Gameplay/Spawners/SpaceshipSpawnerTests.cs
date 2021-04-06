using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Spawners;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayMode.Gameplay.Spawners
{
    public class SpaceshipSpawnerTests
    {
        private ISpaceshipSpawner _spaceshipSpawner;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _spaceshipSpawner = new SpaceshipSpawner();
            _spaceshipSpawner.Preload();
        }

        [UnityTest]
        public IEnumerator _1_Spawn()
        {
            yield return new WaitWhile(() => !_spaceshipSpawner.IsReady());
            var spaceshipsBeforeSpawn = Object.FindObjectsOfType<Spaceship>();
            _spaceshipSpawner.Spawn();
            var spaceshipsAfterSpawn = Object.FindObjectsOfType<Spaceship>();
            Assert.True(spaceshipsAfterSpawn.Length - spaceshipsBeforeSpawn.Length == 1);
        }
    }
}
