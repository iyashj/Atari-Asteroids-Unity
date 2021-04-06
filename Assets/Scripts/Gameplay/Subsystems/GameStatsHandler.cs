using System.Threading.Tasks;
using Assets.Scripts.Data;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Methods publicly accessible for GameStatsHandler.
    /// </summary>
    public interface IGameStatsHandler
    {
        int GetLives();
    }
    /// <summary>
    /// Class to maintain game stats based on game events
    /// </summary>
    public class GameStatsHandler : IGameStatsHandler
    {
        private int _score;
        private int _lives;

        #region DEPENDENCIES

        private readonly IScoreConfig _scoreConfig;

        #endregion

        #region API
        public int GetLives() => _lives;
        #endregion

        #region CONSTRUCTOR
        public GameStatsHandler(int maxLives, IScoreConfig scoreConfig)
        {
            Logger.Info($"constructed gamestats handler");
            _scoreConfig = scoreConfig;
            SubscribeToGameEvents();
            SetDefaultStats(maxLives);
        }
        ~GameStatsHandler()
        {
            UnsubscribeFromGameEvents();
        }
        #endregion

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.AsteroidExploded, OnAsteroidExploded);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.UFOExploded, OnUFOExploded);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.SpaceshipExploded, OnSpaceshipExploded);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.PlayAgainPayload, OnGameOver);
        }

        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.AsteroidExploded, OnAsteroidExploded);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.UFOExploded, OnUFOExploded);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.SpaceshipExploded, OnSpaceshipExploded);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.PlayAgainPayload, OnGameOver);

        }
        private void OnAsteroidExploded(IBasePayload basePayload)
        {
            AsteroidExplodedPayload explodedPayload = (AsteroidExplodedPayload)basePayload;
            IncreaseScore(_scoreConfig.GetAsteroidValue(explodedPayload.Level));
        }
        private void OnSpaceshipExploded(IBasePayload basePayload)
        {
            SpaceshipExplodedPayload explodedPayload = (SpaceshipExplodedPayload)basePayload;
            ChangeLivesCount(-1);
            if (HasLostAllLives())
            {
                DelayedGameEnd();
            }
        }

        private async void DelayedGameEnd()
        {
            await Task.Delay(3000);
            EventBus.GetInstance().Publish(new BasePayload(GameConstants.EventKey.GameOver));
        }

        private void OnGameOver(IBasePayload basePayload)
        {
            SetLives(3);
            SetScore(0);
        }

        private void OnUFOExploded(IBasePayload basePayload)
        {
            UfoExplodedPayload ufoExplodedPayload = (UfoExplodedPayload)basePayload;
            IncreaseScore(_scoreConfig.GetUFOValue(ufoExplodedPayload.Level));
        }

        private void PublishScoreUpdateEvent(int score)
        {
            Logger.Info($"publishing score update with score {score}");
            EventBus.GetInstance().Publish(new UpdateScorePayload(_score));
        }
        private void PublishLivesUpdateEvent(int lives)
        {
            Logger.Info($"publishing lives update with lives {lives}");
            EventBus.GetInstance().Publish(new UpdateLivesPayload(_lives));
        }

        private bool HasLostAllLives() => _lives == 0;
        private void IncreaseScore(int score)
        {
            _score += score;
            SetScore(_score);
        }
        private void ChangeLivesCount(int changedLivesAmount)
        {
            _lives += changedLivesAmount;
           SetLives(_lives);
        }
        
        private void SetDefaultStats(int maxLives)
        {
            SetScore(0);
            SetLives(maxLives);
        }
        private void SetScore(int score)
        {
            _score = score;
            PublishScoreUpdateEvent(_score);
        }
        private void SetLives(int lives)
        {
            _lives = lives;
            PublishLivesUpdateEvent(_lives);
        }
    }
}