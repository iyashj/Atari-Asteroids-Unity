using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using EventLibraryDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Action<Assets.Scripts.GameStructure.Communication.EventSystem.IBasePayload>>>;
namespace Assets.Tests.PlayMode.Gameplay.Subsystems
{
    public class TestCommons
    {
        public const string TestScene = "TestScene";
        private const string privateFieldEventLibrary = "_eventLibrary";

        public static void ResetEventLibrary()
        {
            EventBus.GetInstance();
            PrivateObject privateObject = new PrivateObject(EventBus.GetInstance());
            privateObject.SetField(privateFieldEventLibrary, new EventLibraryDictionary());
        }
        public static void SetPhysicsCollisionRules()
        {
                Physics2D.autoSimulation = false;
                Physics2D.IgnoreLayerCollision(Layer.SpaceshipLayer, Layer.SpaceshipLayer);
                Physics2D.IgnoreLayerCollision(Layer.UFOLayer, Layer.UFOLayer);
                Physics2D.IgnoreLayerCollision(Layer.PlayerBulletLayer, Layer.PlayerBulletLayer);
                Physics2D.IgnoreLayerCollision(Layer.AsteroidLayer, Layer.AsteroidLayer);
                Physics2D.IgnoreLayerCollision(Layer.EnemyBulletLayer, Layer.EnemyBulletLayer);

                Physics2D.IgnoreLayerCollision(Layer.SpaceshipLayer, Layer.PlayerBulletLayer);
                Physics2D.IgnoreLayerCollision(Layer.UFOLayer, Layer.EnemyBulletLayer);
                Physics2D.IgnoreLayerCollision(Layer.PlayerBulletLayer, Layer.EnemyBulletLayer);
        }
    }

    public class UIHandlerTests
    {
        private readonly string privateMethodSetLivesText = "SetLivesText";
        private readonly string privateMethodSetScoreText = "SetScoreText";

        private UIHandler _uiHandler;
        private PrivateObject _privateObject;
        private TextMeshProUGUI textScore;
        private TextMeshProUGUI textLives;
        private TextMeshProUGUI textGameOver;

        [OneTimeSetUp]
        public void Setup()
        {
            SceneManager.LoadScene(TestCommons.TestScene);
            TestCommons.ResetEventLibrary();

            textScore = new GameObject("score").AddComponent<TextMeshProUGUI>();
            textLives = new GameObject("lives").AddComponent<TextMeshProUGUI>();
            textGameOver = new GameObject("gameover").AddComponent<TextMeshProUGUI>();
            _uiHandler = new UIHandler(textScore, textLives, textGameOver);
            _privateObject = new PrivateObject(_uiHandler);
        }

        [Test]
        public void _1_TestSettingTextAfterConstruction()
        {
            var sampleLives = "2";
            var sampleScore = "1550";

            _privateObject.Invoke(privateMethodSetLivesText, sampleLives);
            _privateObject.Invoke(privateMethodSetScoreText, sampleScore);

            var isLivesTextSetCorrectly = Equals(textLives.text, sampleLives);
            var isScoreTextSetCorrectly = Equals(textScore.text, sampleScore);

            Assert.True(isLivesTextSetCorrectly && isScoreTextSetCorrectly);
        }
        [Test]
        public void _2_TestScoreUpdateEvent()
        {
            var sampleScore = 998945;
            EventBus.GetInstance().Publish(new UpdateScorePayload(sampleScore));
            var isLivesTextSetCorrectly = Equals(int.Parse(textScore.text), sampleScore);
            Assert.True(isLivesTextSetCorrectly);
        }
        [Test]
        public void _3_TestLivesUpdateEvent()
        {
            var sampleLives = 2;
            EventBus.GetInstance().Publish(new UpdateLivesPayload(sampleLives));
            var isScoreTextSetCorrectly = Equals(int.Parse(textLives.text), sampleLives);
            Assert.True(isScoreTextSetCorrectly);
        }
        [Test]
        public void _4_TestGameOverEvent()
        {
            EventBus.GetInstance().Publish(new BasePayload(Scripts.GameConstants.EventKey.GameOver));
            Assert.True(textGameOver.gameObject.activeSelf);
        }

    }
}
