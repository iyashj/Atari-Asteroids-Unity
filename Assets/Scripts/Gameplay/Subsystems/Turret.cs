using System.Collections;
using UnityEngine;

using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Assets.Scripts.GameStructure.Pooling;

using BulletPool = Assets.Scripts.GameStructure.Pooling.BaseObjectPool<Assets.Scripts.Gameplay.MonoBehaviours.IBullet>;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Methods publicly accessible for SpaceshipTurret
    /// </summary>
    public interface ITurret
    {
        void Shoot(Vector3 spawnPosition, Vector3 direction);
        void Toggle(bool status);
    }

    /// <summary>
    /// Turret attached to player and enemy spaceship to fire bullets.
    /// </summary>
    public class Turret : ITurret
    {
        public readonly IBaseObjectPool<IBullet> BulletPool;
        private readonly float _bulletFireCooldownTime;
        private readonly float _bulletLaunchSpeed;
        private readonly float _bulletLifeTime;
        private readonly MonoBehaviour _coroutineReferenceMonoBehaviour;

        private bool _isUnderCooldown;

        #region CONSTRUCTOR
        public Turret(ITurretConfig turretConfig, MonoBehaviour coroutineReferenceMonoBehaviour)
        {
            Logger.Info($"Constructed turret for {coroutineReferenceMonoBehaviour.name}");

            _coroutineReferenceMonoBehaviour = coroutineReferenceMonoBehaviour;
            _bulletFireCooldownTime = turretConfig.GetCooldown();
            _bulletLaunchSpeed = turretConfig.GetLaunchSpeed();
            _bulletLifeTime = turretConfig.GetLifetime();
            BulletPool = new BulletPool(turretConfig.GetBulletKey(), GameConstants.InitialPoolSize.Bullet);
            _isUnderCooldown = false;
        }
        #endregion

        #region API
        public void Shoot(Vector3 spawnPosition, Vector3 direction)
        {
            if (CanShoot())
            {
                var obtainedItem = BulletPool.GetFreeItem();
                obtainedItem.SetPosition(spawnPosition);
                obtainedItem.Launch(_bulletLaunchSpeed, _bulletLifeTime, direction);
                _coroutineReferenceMonoBehaviour.StartCoroutine(CooldownRoutine(_bulletFireCooldownTime));
                PublishShootEvent();
            }
        }

        public void Toggle(bool status)
        {
            _isUnderCooldown = !status;
        }
        #endregion

        private void PublishShootEvent()
        {
            EventBus.GetInstance().Publish(new BasePayload(GameConstants.EventKey.Shoot));
        }

        private bool CanShoot()
        {
            bool bulletPoolHasBeenInitialized = BulletPool.IsReady();
            bool cooldownComplete = !_isUnderCooldown;
            return cooldownComplete && bulletPoolHasBeenInitialized;
        }

        private IEnumerator CooldownRoutine(float cooldown)
        {
            _isUnderCooldown = true;
            yield return new WaitForSeconds(cooldown);
            _isUnderCooldown = false;
        }

    }
}