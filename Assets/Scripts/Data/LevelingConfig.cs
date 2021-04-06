using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Specifies getter for Leveling stats
    /// </summary>
    public interface ILevelingConfig
    {
        int GetMinimumAsteroidsInALevel();
        float GetSafeDistanceFromPlayerShip();
        int GetAsteroidsIncreasedPerLevel();
        float GetWaitBeforeLevelSetup();
    }

    [CreateAssetMenu(fileName = "New LevelingData", menuName = "GameConfigs/Levelingdata")]
    public class LevelingConfig : ScriptableObject, ILevelingConfig
    {
        [SerializeField] private int _asteroidsIncreasedPerLevel;
        [SerializeField] private int _minimumAsteroidsInALevel;
        [SerializeField] private float _safeDistanceFromPlayerShip;
        [SerializeField] private float _waitBeforeLevelSetup;

        public int GetMinimumAsteroidsInALevel() => _minimumAsteroidsInALevel;
        public float GetSafeDistanceFromPlayerShip() => _safeDistanceFromPlayerShip;
        public int GetAsteroidsIncreasedPerLevel() => _asteroidsIncreasedPerLevel;
        public float GetWaitBeforeLevelSetup() => _waitBeforeLevelSetup;
    }
}