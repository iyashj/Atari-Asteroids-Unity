using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Specifies getter for Spaceship stats
    /// </summary>
    public interface ISpaceshipConfig
    {
        float GetMaxVelocity();
        float GetMovementForce();
        float GetTurnTorque();
        float GetWaitBeforeRespawn();
        float GetWaitForSafeSpawnDelay();
        float GetWaitForHyperspace();
    }

    [CreateAssetMenu(fileName = "New SpaceshipData", menuName = "GameConfigs/SpaceshipData")]

    public class SpaceshipConfig : ScriptableObject, ISpaceshipConfig
    {
        [SerializeField] private float _maxVelocity;
        [SerializeField] private float _movementForce;
        [SerializeField] private float _turnTorque;
        [SerializeField] private float _waitBeforeRespawn;
        [SerializeField] private float _waitForSafeSpawnDelay;
        [SerializeField] private float _waitForHyperspace;

        public float GetMaxVelocity() => _maxVelocity;
        public float GetMovementForce() => _movementForce;
        public float GetTurnTorque() => _turnTorque;
        public float GetWaitBeforeRespawn() => _waitBeforeRespawn;
        public float GetWaitForSafeSpawnDelay() => _waitForSafeSpawnDelay;
        public float GetWaitForHyperspace() => _waitForHyperspace;
    }
}