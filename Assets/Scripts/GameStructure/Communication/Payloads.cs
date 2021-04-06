using UnityEngine;

using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;


namespace Assets.Scripts.GameStructure.Communication
{
    // Defines readonly Payload struct(s) to be used with EventBus
    namespace Payloads
    {
        public readonly struct PlayAgainPayload : IBasePayload
        {
            public PlayAgainPayload(string eventName)
            {
                EventName = eventName;
            }
            public string EventName { get; }
        }
        public readonly struct BasePayload : IBasePayload
        {
            public BasePayload(string eventName)
            {
                EventName = eventName;
            }

            public string EventName { get; }
        }
        public readonly struct WrapperTogglePayload : IBasePayload
        {
            public WrapperTogglePayload(IScreenWrappable screenWrappable, bool toggleStatus)
            {
                EventName = GameConstants.EventKey.ToggleScreenWrapping;
                ToggleStatus = toggleStatus;
                ScreenWrappable = screenWrappable;
            }

            public IScreenWrappable ScreenWrappable { get; }
            public string EventName { get; }
            public bool ToggleStatus { get; }
        }
        public readonly struct AsteroidExplodedPayload : IBasePayload
        {
            public string EventName { get; }
            public int Level { get; }
            public Vector2 Position { get; }

            public AsteroidExplodedPayload(int level, Vector2 position)
            {
                EventName = GameConstants.EventKey.AsteroidExploded;
                Level = level;
                Position = position;
            }
        }
        public readonly struct HyperspacePayload : IBasePayload
        {
            public string EventName { get; }

            public HyperspacePayload(string eventName)
            {
                EventName = eventName;
            }
        }
        public readonly struct SpaceshipExplodedPayload : IBasePayload
        {
            public SpaceshipExplodedPayload(Vector2 position)
            {
                EventName = Scripts.GameConstants.EventKey.SpaceshipExploded;
                Position = position;
            }

            public string EventName { get; }
            public Vector2 Position { get; }
        }
        public readonly struct UfoExplodedPayload : IBasePayload
        {
            public UfoExplodedPayload(int level, Vector2 position)
            {
                EventName = GameConstants.EventKey.UFOExploded;
                Level = level;
                Position = position;
            }

            public Vector2 Position { get; }
            public string EventName { get; }
            public int Level { get; }
        }
        public readonly struct SpaceshipSpawnedPayload : IBasePayload
        {
            public SpaceshipSpawnedPayload(ISpaceship spawnedSpaceshipBehaviour)
            {
                EventName = GameConstants.EventKey.SpaceshipSpawned;
                spaceshipBehaviour = spawnedSpaceshipBehaviour;
            }

            public ISpaceship spaceshipBehaviour { get; }

            public string EventName { get; }
        }
        public readonly struct UpdateScorePayload : IBasePayload
        {
            public UpdateScorePayload(int updatedScore)
            {
                EventName = GameConstants.EventKey.ChangeScores;
                UpdatedScore = updatedScore;
            }

            public int UpdatedScore { get; }

            public string EventName { get; }
        }
        public readonly struct UpdateLivesPayload : IBasePayload
        {
            public UpdateLivesPayload(int updatedLives)
            {
                EventName = GameConstants.EventKey.ChangeLives;
                UpdatedLives = updatedLives;
            }

            public string EventName { get; }
            public int UpdatedLives { get; }
        }
    }
}

