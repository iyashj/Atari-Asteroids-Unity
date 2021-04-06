using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

using Assets.Scripts.Gameplay.Subsystems;
using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.MonoBehaviours
{
    /// <summary>
    /// Base Class for all screen wrappable objects in the game.
    /// Publishes toggling of its screen wrapping ability.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class ScreenWrappableObject : MonoBehaviour, IScreenWrappable
    {
        protected Rigidbody2D ObjectRigibody2D;
        protected SpriteRenderer ObjectSpriteRenderer;
        protected Collider2D ObjectCollider2D;

        private Bounds _objectBounds;
        private Transform _objectTransform;

        #region UNITY_FUNCTIONS
        protected void OnTriggerEnter2D()
        {
            if (HasValidTrigger())
            {
                OnEnteringTrigger();
            }
        }
        #endregion

        #region API
        /// <summary>
        /// Should be called explicity on spawning/engaging this object
        /// Initialized Components, Enables Trigger and Wrapping
        /// </summary>
        public virtual void Initialize()
        {
            InitializeAttachedComponents();
            ToggleTrigger(true);
            ToggleWrapping(true);
        }
        public Bounds GetObjectBounds() => _objectBounds;
        public Transform GetObjectTransform() => _objectTransform;
        #endregion

        protected virtual void OnEnteringTrigger()
        {
            ToggleTrigger(false);
            ToggleWrapping(false);
        }
        protected void ToggleWrapping(bool toggleStatus)
        {
            PublishToggleScreenWrapEvent(toggleStatus);
        }
        protected void ToggleTrigger(bool triggerStatus)
        {
            _hasValidTrigger = triggerStatus;
        }

        private void InitializeAttachedComponents()
        {
            ObjectCollider2D = GetComponent<Collider2D>();
            ObjectSpriteRenderer = GetComponent<SpriteRenderer>();
            ObjectRigibody2D = GetComponent<Rigidbody2D>();
            _objectBounds = ObjectSpriteRenderer.bounds;
            _objectTransform = transform;
        }
        private void PublishToggleScreenWrapEvent(bool toggleStatus)
        {
            EventBus.GetInstance().Publish(new WrapperTogglePayload(this, toggleStatus));
        }

        // For TriggerEnter2D
        // Using OnTriggerEnter2D can be triggered multiple times. Unity says its by Design.
        // These fields keep track of whether the trigger has already been invalidated. 
        // And if it has been invalidated should not be considered for triggering again
        //
        private bool _hasValidTrigger = false;
        private bool HasValidTrigger()
        {
            return _hasValidTrigger;
        }
    }
}