using Assets.Scripts.GameStructure.Pooling;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.MonoBehaviours
{
    /// <summary>
    /// Interface for VfxItem. Adding Play Method
    /// </summary>
    public interface IVfxItem : IPoolableItem, IRepositionable
    {
        /// <summary>
        /// Called to play particle system attached on this item
        /// </summary>
        void Play();
    }

    /// <summary>
    /// Class representing Vfx GameObject.
    /// A Poolable item with an auto expiry based on lifetime.
    ///  </summary>
    public class VfxItem : MonoBehaviour, IVfxItem
    {
        private ParticleSystem particleSystem;

        public event Action ItemExpired;
        public void Init()
        {
            particleSystem = GetComponent<ParticleSystem>();
            particleSystem.Stop();
            gameObject.SetActive(false);
        }

        public void Engage()
        {
            gameObject.SetActive(true);
        }

        public void Release()
        {
            particleSystem.Stop();
            gameObject.SetActive(false);
        }

        public void Play()
        {
            particleSystem.Play();
        }

        public void SetPosition(Vector2 position)
        {
            gameObject.transform.position = position;
        }
    }
}