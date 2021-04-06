using System;
using System.Collections;
using Assets.Scripts.Gameplay.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayMode.Gameplay.MonoBehaviour
{
    public abstract class BaseMonoBehaviourTests
    {
        protected Func<bool> SceneLoadedPredicate;
        public Action<AsyncOperationHandle<GameObject>> _onGameObjectInstantiationComplete;
        public GameObject TargetGameObject { get; set; }
        public string ObjectAddressableKey { get; set; }
        public Vector2 ObjectStartPosition { get; set; }
        public bool SceneLoaded = false;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            SceneLoadedPredicate = () => SceneLoaded == false;
            Time.timeScale = 20;
            var handle = Addressables.InstantiateAsync(ObjectAddressableKey);
            handle.Completed += _onGameObjectInstantiationComplete;
        }

        public void InitializeTestParams(string objectAddressableKey, Vector2 objectStartPosition)
        {
            ObjectStartPosition = objectStartPosition;
            ObjectAddressableKey = objectAddressableKey;
            _onGameObjectInstantiationComplete += OnGameObjectInstantiationComplete;
        }
        public virtual void OnGameObjectInstantiationComplete(AsyncOperationHandle<GameObject> handle)
        {
            TargetGameObject = handle.Result;
            handle.Result.transform.position = ObjectStartPosition;
            SceneLoaded = true;
        }
       
        [OneTimeTearDown]
        public virtual void TearDown()
        {
            Time.timeScale = 1f;
            Physics2D.autoSimulation = true;
        }

        #region COMPONENTS_ATTACHED

        [UnityTest]
        public IEnumerator HasSpriteRenderer()
        {
            yield return new WaitWhile(SceneLoadedPredicate);

            var attachedSpriteRenderer = TargetGameObject.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(attachedSpriteRenderer, "has sprite renderer attached");
        }

        [UnityTest]
        public IEnumerator HasCollider()
        {
            yield return new WaitWhile(SceneLoadedPredicate);

            var attachedCollider = TargetGameObject.GetComponent<Collider2D>();
            Assert.IsNotNull(attachedCollider, "has collider attached");
        }

        [UnityTest]
        public IEnumerator HasRigidbody2D()
        {
            yield return new WaitWhile(SceneLoadedPredicate);

            var attachedRigidbody2D = TargetGameObject.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(attachedRigidbody2D, "has rigidbody2D attached");
        }

        [UnityTest]
        public IEnumerator HasSpaceObject()
        {
            yield return new WaitWhile(SceneLoadedPredicate);

            var attachedSpaceshipBehaviour = TargetGameObject.GetComponent<ScreenWrappableObject>();
            Assert.IsNotNull(attachedSpaceshipBehaviour, "has SpaceShipBehaviour attached");
        }

        #endregion

    }
}