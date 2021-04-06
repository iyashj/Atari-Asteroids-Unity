using System.Runtime.CompilerServices;
using TMPro;

using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Class to update UI based on gameplay events
    /// </summary>
    public class UIHandler
    {
        private readonly TextMeshProUGUI _textScore;
        private readonly TextMeshProUGUI _textLives;
        private readonly TextMeshProUGUI _textGameOver;

        #region CONSTRUCTOR

        public UIHandler(TextMeshProUGUI textScore, TextMeshProUGUI textLives, TextMeshProUGUI textGameOver)
        {
            Logger.Info($"constructed ui handler");

            _textScore = textScore;
            _textLives = textLives;
            _textGameOver = textGameOver;

            SubscribeToGameEvents();
        }
        ~UIHandler()
        {
            UnsubscribeFromGameEvents();
        }

        #endregion

        private void SetScoreText(string score)
        {
            _textScore.text = score;
        }
        private void SetLivesText(string lives)
        {
            _textLives.text = lives;
        }

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.ChangeLives, OnRemainingLivesUpdate);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.ChangeScores, OnScoreUpdate);
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.GameOver, OnGameOver);
        }
        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.ChangeLives, OnRemainingLivesUpdate);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.ChangeScores, OnScoreUpdate);
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.GameOver, OnGameOver);
        }
        private void OnScoreUpdate(IBasePayload basePayload)
        {
            UpdateScorePayload updateScorePayLoad = (UpdateScorePayload)basePayload;
            SetScoreText(updateScorePayLoad.UpdatedScore.ToString());
        }
        private void OnRemainingLivesUpdate(IBasePayload basePayload)
        {
            UpdateLivesPayload updateLivesPayload = (UpdateLivesPayload)basePayload;
            SetLivesText(updateLivesPayload.UpdatedLives.ToString());
        }
        private void OnGameOver(IBasePayload basePayload)
        {
            _textGameOver.gameObject.SetActive(true);
        }
    }
}