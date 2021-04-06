using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode.Gameplay.Subsystems
{
    public class GameStatsHandlerTests
    {
        private readonly int asteroid1Value = 10;
        private readonly int asteroid2Value = 10;
        private readonly int asteroid3Value = 10;

        private readonly int ufo1Value = 10;
        private readonly int ufo2Value = 10;

        private readonly int maxLives = 3;
        
        private GameStatsHandler _gameStatsHandler;
        private PrivateObject _gameStatPrivateObject;

        private IScoreConfig GetScoreConfig()
        {
            var scoreConfig = Substitute.For<IScoreConfig>();
            scoreConfig.GetAsteroidValue(0).Returns(asteroid1Value);
            scoreConfig.GetAsteroidValue(1).Returns(asteroid2Value);
            scoreConfig.GetAsteroidValue(2).Returns(asteroid3Value);

            scoreConfig.GetUFOValue(0).Returns(ufo1Value);
            scoreConfig.GetUFOValue(1).Returns(ufo2Value);
            
            return scoreConfig;
        }


        private int GetCurrentScore()
        {
            return (int)_gameStatPrivateObject.GetField("_score");
        }

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            SceneManager.LoadScene(TestCommons.TestScene);
            TestCommons.ResetEventLibrary();
            
            _gameStatsHandler = new GameStatsHandler(maxLives, GetScoreConfig());
            _gameStatPrivateObject = new PrivateObject(_gameStatsHandler);
        }

        [Test]
        public void _1_TestHasSetDefaultStatUponConstruction()
        {
            var lives = _gameStatsHandler.GetLives();
            var score = GetCurrentScore();

            var defaultLivesSetCorrectly = lives == maxLives;
            var defaultScoreSetCorrectly = score == 0;

            Assert.True(defaultScoreSetCorrectly && defaultLivesSetCorrectly);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(1)]
        public void _2_TestUpdateScoreOnAsteroidExplodedEvent(int asteroidLevel)
        {
            int scoreBeforeEvent = GetCurrentScore();
            EventBus.GetInstance().Publish(new AsteroidExplodedPayload(asteroidLevel, Vector2.zero));
            int scoreAfterEvent = GetCurrentScore();
            Assert.True(scoreAfterEvent - scoreBeforeEvent == GetScoreConfig().GetAsteroidValue(asteroidLevel));
        }

        [Test]
        public void _2_TestUpdateScoreOnSpaceshipExplodedEvent()
        {
            int livesBeforeEvent = _gameStatsHandler.GetLives();
            EventBus.GetInstance().Publish(new SpaceshipExplodedPayload(new Vector2()));
            int livesAfterEvent = _gameStatsHandler.GetLives();
            Assert.True(livesBeforeEvent - livesAfterEvent == 1);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void _2_TestUpdateScoreOnUFOExplodedEvent(int ufoLevel)
        {
            int scoreBeforeEvent = GetCurrentScore();
            EventBus.GetInstance().Publish(new UfoExplodedPayload(ufoLevel, new Vector2()));
            int scoreAfterEvent = GetCurrentScore();
            Assert.True(scoreAfterEvent - scoreBeforeEvent == GetScoreConfig().GetAsteroidValue(ufoLevel));
        }

    }
}
