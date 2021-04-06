using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Specifies getter for Asteroid stats
    /// </summary>
    public interface IAsteroidConfig
    {
        float GetMaxForce();
        float GetMinForce();
        float GetMaxTorque();
        float GetMinTorque();
    }

    [CreateAssetMenu(fileName = "New AsteroidData", menuName = "GameConfigs/AsteroidData")]
    public class AsteroidConfig : ScriptableObject, IAsteroidConfig
    {
        [SerializeField] private float _maxForce;
        [SerializeField] private float _minForce;
        [SerializeField] private float _maxTorque;
        [SerializeField] private float _minTorque;

        public float GetMaxForce() => _maxForce;
        public float GetMinForce() => _minForce;
        public float GetMaxTorque() => _maxTorque;
        public float GetMinTorque() => _minTorque;
    }
}