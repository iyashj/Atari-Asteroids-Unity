using System;

using UnityEngine;

using Assets.Scripts.Data;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Assets.Scripts.GameStructure.Pooling;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Gameplay.MonoBehaviours
{
    /// <summary>
    /// Interface for Asteroid. Adding launch method to poolable item
    /// </summary>
    public interface IAsteroid : IPoolableItem, IRepositionable
    {
        void Launch();
    }

    /// <summary>
    /// Class representing Asteroid GameObject.
    /// A Poolable item. Pooling is controller by Asteroid Spawner.
    /// Publishes its explosion event
    ///  </summary>
    public class Asteroid : ScreenWrappableObject, IAsteroid
    {
        #region SERIALIZED_FIELDS

        [SerializeField] private int _asteroidLevel = 2;
        [SerializeField] private AsteroidConfig asteroidConfig;

        #endregion

        #region DEPENDENCIES

        private IAsteroidConfig _asteroidConfig;

        #endregion

        #region API
        public override void Initialize()
        {
            //Logger.Info($"initialized {this.gameObject.name}");
            base.Initialize();
            CacheConfigsToDependencies();
        }
        public void Init()
        {
            //Logger.Info($"Init {this.gameObject.name}");
            Initialize();
        }
        public void Engage()
        {
            //Logger.Info($"Engaged {this.gameObject.name}");

            ToggleWrapping(true);
            ToggleTrigger(true);
            gameObject.SetActive(true);
        }
        public void Release()
        {
            //Logger.Info($"Release {this.gameObject.name}");

            ResetRigibody();
            gameObject.SetActive(false);
        }
        public void Launch()
        {
            //Logger.Info($"Launch {this.gameObject.name}");

            Launch(GetRandomForce(), GetRandomTorque(), GetRandomDirection());
        }
        public void SetPosition(Vector2 position)
        {
            //Logger.Info($"Position set to {position}");

            transform.position = position;
        }
        #endregion

        public event Action ItemExpired;

        protected override void OnEnteringTrigger()
        {
            //Logger.Info($"asteroid entered trigger");
            base.OnEnteringTrigger();
            PublishAsteroidExplosionEvent();
            ItemExpired?.Invoke();
        }
        private void PublishAsteroidExplosionEvent()
        {
            //Logger.Info($"published this asteroid explosion event with payload {_asteroidLevel} at {transform.position}");
            EventBus.GetInstance().Publish(new AsteroidExplodedPayload(_asteroidLevel, transform.position));
        }

        private void CacheConfigsToDependencies()
        {
            //Logger.Info($"cached config for asteroid");
            _asteroidConfig = asteroidConfig;
        }
        
        private Vector2 GetRandomDirection()
        {
            return Random.insideUnitCircle;
        }
        private float GetRandomTorque()
        {
            return Random.Range(_asteroidConfig.GetMinTorque(), _asteroidConfig.GetMaxTorque());
        }
        private float GetRandomForce()
        {
           return Random.Range(_asteroidConfig.GetMinForce(), _asteroidConfig.GetMaxForce());
        }

        private void Launch(float force, float torque, Vector3 direction)
        {
            //Logger.Info($"launched with force {force} torque {torque} and direction {direction}");
            
            var movementForce = direction * force;
            ObjectRigibody2D.AddForce(movementForce);

            var turnTorque = torque;
            ObjectRigibody2D.AddTorque(turnTorque);
        }
        private void ResetRigibody()
        {
            //Logger.Info($"reset rigidbody");
            ObjectRigibody2D.velocity = Vector2.zero;
        }
    }
}

