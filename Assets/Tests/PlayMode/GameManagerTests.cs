using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Collections;
using Assets.Scripts.GameConstants;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace Tests.PlayMode
{
    public class GameManagerTests
    {
        private const string PrivateMethodSetPhysicsLayer = "SetPhysicsLayer";
        private GameManager _gameManager;
        private PrivateObject _privateObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            _gameManager.gameObject.SetActive(false);
            _privateObject = new PrivateObject(_gameManager);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameManager);
        }

        [Test]
        [TestCase(Layer.SpaceshipLayer)]
        [TestCase(Layer.UFOLayer)]
        [TestCase(Layer.PlayerBulletLayer)]
        [TestCase(Layer.AsteroidLayer)]
        [TestCase(Layer.EnemyBulletLayer)]
        public void _1_TestCollisionWithSelf(int layer)
        {
            _privateObject.Invoke(PrivateMethodSetPhysicsLayer);
            Assert.True(Physics2D.GetIgnoreLayerCollision(layer, layer));
        }

        [Test]
        [TestCase(Layer.SpaceshipLayer, Layer.PlayerBulletLayer)]
        [TestCase(Layer.UFOLayer, Layer.EnemyBulletLayer)]
        [TestCase(Layer.PlayerBulletLayer, Layer.EnemyBulletLayer)]
        public void _2_TestCollisionBetweenLayers(int layer1, int layer2)
        {
            _privateObject.Invoke(PrivateMethodSetPhysicsLayer);
            Assert.True(Physics2D.GetIgnoreLayerCollision(layer1, layer2));
        }

    }

}