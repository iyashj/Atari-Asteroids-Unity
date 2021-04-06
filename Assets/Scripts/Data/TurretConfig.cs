using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Specifies getter for Turret stats
    /// </summary>
    public interface ITurretConfig
    {
        float GetLaunchSpeed();
        float GetLifetime();
        float GetCooldown();
        string GetBulletKey();
    }

    [CreateAssetMenu(fileName = "New TurretData", menuName = "GameConfigs/TurretData")]
    public class TurretConfig : ScriptableObject, ITurretConfig
    {
        [SerializeField] private float _launchSpeed;
        [SerializeField] private float _lifetime;
        [SerializeField] private float _cooldown;
        [SerializeField] private string _bulletKey;

        public float GetLaunchSpeed() => _launchSpeed;
        public float GetLifetime() => _lifetime;
        public float GetCooldown() => _cooldown;
        public string GetBulletKey() => _bulletKey;
    }
}