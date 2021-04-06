using System.Linq;
using NUnit.Framework;

using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = NUnit.Framework.Assert;
using EventLibraryDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Action<Assets.Scripts.GameStructure.Communication.EventSystem.IBasePayload>>>;
using Assets.Tests.PlayMode.Gameplay.Subsystems;

namespace Assets.Tests.PlayMode.GameStructure.Communication
{
    public class EventBusTests
    {
        private int _receivedCount = 0;
        private const int SendMessagesCount = 1000;

        private readonly string keyBasePayload = "test_payload";
        private readonly string keyTestSubscribe = "test-subscribe";
        private readonly string privateFieldEventLibrary = "_eventLibrary";

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            SceneManager.LoadScene(TestCommons.TestScene);
        }

        private void ResetEventLibrary()
        {
            EventBus.GetInstance();
            PrivateObject privateObject = new PrivateObject(EventBus.GetInstance());
            privateObject.SetField(privateFieldEventLibrary, new EventLibraryDictionary());
        }

        [Test]
        public void _1_TestLazySingleton()
        {
            Assert.NotNull(EventBus.GetInstance());
        }
        
        [Test]
        public void _2_TestSubscribe()
        {
            EventBus.GetInstance().Subscribe(keyTestSubscribe, null);
            PrivateObject privateObject = new PrivateObject(EventBus.GetInstance());
            EventLibraryDictionary eventLibrary =
                (EventLibraryDictionary) privateObject.GetField(privateFieldEventLibrary);
            var contains = eventLibrary.Keys.Contains(keyTestSubscribe);
            Assert.True(contains);
            EventBus.GetInstance().Unsubscribe(keyTestSubscribe, null);
        }

        [Test]
        public void _3_TestNonSubscribed()
        {
            ResetEventLibrary();
            PrivateObject privateObject = new PrivateObject(EventBus.GetInstance());
            EventLibraryDictionary eventLibrary =
                (EventLibraryDictionary)privateObject.GetField(privateFieldEventLibrary);
            var contains = eventLibrary.Keys.Contains(keyTestSubscribe);
            Assert.False(contains);
        }

        [Test]
        public void _4_TestPublish()
        {
            var isPublished = false;
            EventBus.GetInstance().Subscribe(keyTestSubscribe, payload => isPublished = true);
            EventBus.GetInstance().Publish(new BasePayload(keyTestSubscribe));
            Assert.True(isPublished);
            EventBus.GetInstance().Unsubscribe(keyTestSubscribe, payload => isPublished = true);
        }

        [Test]
        public void _5_TestUnsubsribe()
        {
            var isPublished = false;
            EventBus.GetInstance().Subscribe(keyTestSubscribe, payload => isPublished = true);
            EventBus.GetInstance().Unsubscribe(keyTestSubscribe, payload => isPublished = true);
            EventBus.GetInstance().Publish(new BasePayload(keyTestSubscribe)); 
            Assert.True(isPublished);
        }

        [Test]
        public void _6_TestPayload()
        {
            EventBus.GetInstance().Subscribe(keyBasePayload, OnTestCallback);
            TestLoadLoop();
        }
        private void OnTestCallback(IBasePayload basePayload)
        {
            _receivedCount++;
        }
        private void TestLoadLoop()
        {
            _receivedCount = 0;
            for (int i0 = 0; i0 < SendMessagesCount; i0++)
            {
                EventBus.GetInstance().Publish(new BasePayload(keyBasePayload));
            }

            Assert.AreEqual(SendMessagesCount, _receivedCount);
            EventBus.GetInstance().Unsubscribe(keyBasePayload, OnTestCallback);
        }

        [Test]
        public void _7_TestParameterizedPayload()
        {
            EventBus.GetInstance().Subscribe(Scripts.GameConstants.EventKey.UFOExploded, OnParameterizedTestCallback);
            ParameterizedLoadLoop();
        }
        private void OnParameterizedTestCallback(IBasePayload basePayload)
        {
            UfoExplodedPayload ufoExplodedPayload = (UfoExplodedPayload) basePayload;
            _receivedCount += ufoExplodedPayload.Level;
        }
        private void ParameterizedLoadLoop()
        {
            _receivedCount = 0;
            for (int i0 = 0; i0 < SendMessagesCount; i0++)
            {
                EventBus.GetInstance().Publish(new UfoExplodedPayload(1, new Vector2()));
            }

            Assert.AreEqual(SendMessagesCount, _receivedCount);
            EventBus.GetInstance().Unsubscribe(Scripts.GameConstants.EventKey.UFOExploded, OnParameterizedTestCallback);
        }
    }
}
