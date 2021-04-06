using System;
using System.Collections;
using UnityEngine;

using Assets.Scripts.GameStructure.Pooling;

namespace Assets.Scripts.Gameplay.MonoBehaviours
{
    /// <summary>
    /// Interface for Bullet. Adding parameterized launch method to poolable item
    /// </summary>
    public interface IBullet : IPoolableItem, IRepositionable
    {
        /// <summary>
        /// Launch bullet with specified params
        /// </summary>
        /// <param name="force">launch force</param>
        /// <param name="lifetime">lifetime</param>
        /// <param name="direction">direction</param>
        void Launch(float force, float lifetime, Vector3 direction);
    }

    /// <summary>
    /// Class representing Bullet GameObject.
    /// A Poolable item with an auto expiry based on lifetime.
    /// Pooling is controller by its owning turret.
    ///  </summary>
    public class Bullet : ScreenWrappableObject, IBullet
    {
        public event Action ItemExpired;

        #region API
        public void Launch(float force, float lifetime, Vector3 direction)
        {
            //Logger.Info($"Launched with force {force} lifetime {lifetime} direction {direction}");
            var appliedMovement = direction * force;
            ObjectRigibody2D.AddForce(appliedMovement);
            _expiryCoroutine = StartCoroutine(ExpireRoutine(lifetime));
        }
        public void Release()
        {
            //Logger.Info($"Release {this.gameObject.name}");
            ResetRigidbody();
            gameObject.SetActive(false);
        }
        public void SetPosition(Vector2 position)
        {
            //Logger.Info($"Set position for {this.gameObject.name} to {position}");
            transform.position = position;
        }
        public void Init()
        {
            //Logger.Info($"Init {this.gameObject.name}");
            Initialize();
        }
        public void Engage()
        {
            //Logger.Info($"Engage {this.gameObject.name}");
            ToggleTrigger(true);
            ToggleWrapping(true);
            gameObject.SetActive(true);
        }
        #endregion

        protected override void OnEnteringTrigger()
        {
            //Logger.Info($"{this.gameObject.name} entered trigger");

            base.OnEnteringTrigger();
            if (_expiryCoroutine != null)
            {
                StopCoroutine(_expiryCoroutine);
            }
            Expire();
        }

        private Coroutine _expiryCoroutine;
        private IEnumerator ExpireRoutine(float lifetime)
        {
            //Logger.Info($"{this.gameObject.name} started lifetime routine");

            yield return new WaitForSeconds(lifetime);
            Expire();
        }
        private void Expire()
        {
            //Logger.Info($"{this.gameObject.name} expired");
            ToggleTrigger(false);
            ToggleWrapping(false);
            ItemExpired?.Invoke();
        }
        private void ResetRigidbody()
        {
            //Logger.Info($"{this.gameObject.name} rigidbody reset");
            ObjectRigibody2D.velocity = new Vector2(0, 0);
        }
    }
}

