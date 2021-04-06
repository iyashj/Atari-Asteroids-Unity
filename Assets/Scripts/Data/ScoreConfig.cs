using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Specifies getter for Score stats
    /// </summary>
    public interface IScoreConfig
    {
        int GetAsteroidValue(int asteroidLevel);
        int GetUFOValue(int ufoLevel);
    }

    [CreateAssetMenu(fileName = "New ScoringData", menuName = "GameConfigs/ScoringData")]
    public class ScoreConfig : ScriptableObject, IScoreConfig
    {
        [SerializeField] private int[] UFO;
        [SerializeField] private int[] Asteroids;

        public int GetAsteroidValue(int asteroidLevel) => Asteroids[asteroidLevel];
        public int GetUFOValue(int ufoLevel) => UFO[ufoLevel];
    }
}