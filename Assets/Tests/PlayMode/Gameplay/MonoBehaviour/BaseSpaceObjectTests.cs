using System;
using System.Collections;
using Assets.Scripts.GameConstants;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Tests.PlayMode.Gameplay.Subsystems;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayMode.Gameplay.MonoBehaviour
{
    public abstract class BaseSpaceObjectTests
    {
        protected Func<bool> _testInitializationPredicate;
        private bool _bIsInitialized;
        protected ScreenWrappableObject baseObject;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
//            SceneManager.LoadScene("TestScene");
            TestCommons.SetPhysicsCollisionRules();
            _testInitializationPredicate = () => _bIsInitialized == false;
        }
        protected virtual void LoadPrefab(string prefabKey)
        {
            SpawnerUtility.InstantiateGameObject(prefabKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    baseObject = handle.Result.GetComponent<ScreenWrappableObject>();
                    _bIsInitialized = true;
                }
            };
        }

        [UnityTest]
        public IEnumerator _1_HasSpriteRenderer()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var attachedSpriteRenderer = baseObject.GetComponent<SpriteRenderer>();
            Assert.NotNull(attachedSpriteRenderer, "does not have SpriteRenderer attached");
        }

        [UnityTest]
        public IEnumerator _2_HasCollider2D()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var attachedCollider2D = baseObject.GetComponent<Collider2D>();
            Assert.NotNull(attachedCollider2D, "does not have Collider2D attached");
        }

        [UnityTest]
        public IEnumerator _3_HasRigidbody2D()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var hasRigidbody2D = baseObject.GetComponent<Rigidbody2D>();
            Assert.NotNull(hasRigidbody2D, "does not have Rigidbody2D attached");
        }

        [UnityTest]
        public IEnumerator _4_HasSpaceObjectAttached()
        {
            yield return new WaitWhile(_testInitializationPredicate);
            var attachedSpaceObject = baseObject.GetComponent<ScreenWrappableObject>();
            Assert.NotNull(attachedSpaceObject, "does not have SpaceObject attached");
        }

        protected void ResetSpaceObject()
        {
            baseObject.GetObjectTransform().position = Vector3.zero;
            var rb2d = baseObject.GetComponent<Rigidbody2D>();
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = 0;
            rb2d.rotation = 0;
        }

        protected int GetLayer()
        {
            return baseObject.gameObject.layer;
        }
    }
}