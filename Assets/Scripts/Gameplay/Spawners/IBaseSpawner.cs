using Assets.Scripts.GameStructure.Communication.EventSystem;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Spawners
{
    /// <summary>
    /// Interface providing base features used in spawner.
    /// </summary>
    public interface IBaseSpawner
    {
        /// <summary>
        /// Starts preloading the object to be spawned and sets the flag once loading is complete
        /// </summary>
        void Preload();
        bool IsReady();
    }

    /// <summary>
    /// Interface implemented by classes that subscribes to GameOver element
    /// </summary>
    public interface IGameOverObserver
    {
        /// <summary>
        /// Release all spawned objects on game over for reset
        /// </summary>
        /// <param name="basePayload"></param>
        void OnGameOver(IBasePayload basePayload);
    }
    
}