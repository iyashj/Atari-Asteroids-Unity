using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Specifies getter for UFO stats
    /// </summary>
    public interface IUFOConfig
    {
        float GetMinAngle();
        float GetMaxAngle();
        float GetMovementForce();
    }

    [CreateAssetMenu(fileName = "New UFOData", menuName = "GameConfigs/UFOData")]
    public class UFOConfig : ScriptableObject, IUFOConfig
    {
        [SerializeField] private float _minAngle;
        [SerializeField] private float _maxAngle;
        [SerializeField] private float _movementForce;

        public float GetMinAngle() => _minAngle;
        public float GetMaxAngle() => _maxAngle;
        public float GetMovementForce() => _movementForce;
    }

}