using System;
using System.Collections;
using Assets.Scripts.Gameplay.MonoBehaviours;
using Assets.Scripts.Gameplay.Subsystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

namespace Assets.Tests.PlayMode.Gameplay.Subsystems
{
    public class ScreenWrapperTests
    {
        private ScreenWrapper _screenWrapper;
        private IScreenHandler _screenHandler;

        private const string privateMethodCanWrapFromLeftToRight = "IsOutFromLeftEdge";
        private const string privateMethodCanWrapFromRightToLeft = "IsOutFromRightEdge";
        private const string privateMethodCanWrapFromTopToBottom = "IsOutFromTopEdge";
        private const string privateMethodCanWrapFromBottomToTop = "IsOutFromBottomEdge";
        private const string privateMethodWrap = "Wrap";

        private float outerLimitX = 0;
        private float outerLimitY = 0;
        private Vector2 bottomLeftCorner = Vector2.zero;
        private Vector2 topRightCorner = Vector2.zero;

        private GameObject _wrappableObject;
        private ScreenWrappableObject _screenWrappableObject;
        private PrivateObject _privateObject;

        private Camera GetConfiguredCamera()
        {
            var cameraGameObject = new GameObject("Camera");
            cameraGameObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraGameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.rect = new Rect(Vector2.zero, new Vector2(1, 1));
            camera.pixelRect = new Rect(Vector2.zero, new Vector2(1920, 950));
            camera.orthographicSize = 5;
            return camera;
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(TestCommons.TestScene);
            TestCommons.ResetEventLibrary();

            _screenHandler = new ScreenHandler(GetConfiguredCamera());
            _screenWrapper = new ScreenWrapper(_screenHandler);

            _privateObject = new PrivateObject(_screenWrapper);

            _wrappableObject = new GameObject("wrap");
            _wrappableObject.AddComponent<Rigidbody2D>();
            _wrappableObject.AddComponent<BoxCollider2D>();
            _wrappableObject.AddComponent<SpriteRenderer>();

            _screenWrappableObject = _wrappableObject.AddComponent<Asteroid>();
            _screenWrappableObject.Initialize();

            var bounds = _screenWrappableObject.GetObjectBounds();
            outerLimitX = bounds.extents.x;
            outerLimitY = bounds.extents.x;

            bottomLeftCorner = _screenHandler.GetViewportToWorldPoint(new Vector2(0, 0));
            topRightCorner = _screenHandler.GetViewportToWorldPoint(new Vector2(1, 1));
        }

        [Test]
        public void _1_WrapFromLeftToRight()
        {
            // Arrange for left to right wrap
            _screenWrappableObject.transform.position = new Vector3(bottomLeftCorner.x - outerLimitX, 0);

            bool shouldBeWrapped = IsOutFromLeftEdge(ref bottomLeftCorner.x, ref outerLimitX);
            Wrap();
            bool hasNotBeenWrapped = IsOutFromLeftEdge(ref bottomLeftCorner.x, ref outerLimitX);

            Assert.False(hasNotBeenWrapped);
        }

        [Test]
        public void _2_WrapFromRightToLeft()
        {
            // Arrange for right to left wrap
            _screenWrappableObject.transform.position = new Vector3(topRightCorner.x + outerLimitX, 0);
            bool shouldBeWrapped = IsOutFromRightEdge(ref topRightCorner.x, ref outerLimitX);
            Wrap();
            bool hasNotBeenWrapped = IsOutFromRightEdge(ref topRightCorner.x, ref outerLimitX);

            Assert.False(hasNotBeenWrapped);
        }

        [Test]
        public void _3_WrapFromTopToBottom()
        {
            // Arrange for top to bottom wrap
            _screenWrappableObject.transform.position = new Vector3(0, bottomLeftCorner.y - outerLimitY);

            bool shouldBeWrapped = IsOutFromTopEdge(ref bottomLeftCorner.y, ref outerLimitY);
            Wrap();
            bool hasNotBeenWrapped = IsOutFromTopEdge(ref bottomLeftCorner.y, ref outerLimitY);

            Assert.False(hasNotBeenWrapped);
        }

        [Test]
        public void _4_WrapFromBottomToTop()
        {
            // Arrange for bottom to top wrap
            _screenWrappableObject.transform.position = new Vector3(0, topRightCorner.y + outerLimitY);

            bool shouldBeWrapped = IsOutFromBottomEdge(ref topRightCorner.y, ref outerLimitY);
            Wrap();
            bool hasNotBeenWrapped = IsOutFromBottomEdge(ref topRightCorner.y, ref outerLimitY);

            Assert.False(hasNotBeenWrapped);
        }

        private void Wrap()
        {
            _privateObject.Invoke(privateMethodWrap, _screenWrappableObject.GetObjectBounds(), _screenWrappableObject.GetObjectTransform());
        }
        private bool IsOutFromLeftEdge(ref float directionalX, ref float boundsLimit)
        {
            return (bool)_privateObject.Invoke(privateMethodCanWrapFromLeftToRight, _screenWrappableObject.GetObjectTransform(),
                directionalX,
                boundsLimit);
        }
        private bool IsOutFromRightEdge(ref float directionalX, ref float boundsLimit)
        {
            return (bool)_privateObject.Invoke(privateMethodCanWrapFromRightToLeft, _screenWrappableObject.GetObjectTransform(),
                directionalX,
                boundsLimit);
        }
        private bool IsOutFromTopEdge(ref float directionalX, ref float boundsLimit)
        {
            return (bool)_privateObject.Invoke(privateMethodCanWrapFromTopToBottom, _screenWrappableObject.GetObjectTransform(),
                directionalX,
                boundsLimit);
        }
        private bool IsOutFromBottomEdge(ref float directionalX, ref float boundsLimit)
        {
            return (bool)_privateObject.Invoke(privateMethodCanWrapFromBottomToTop, _screenWrappableObject.GetObjectTransform(),
                directionalX,
                boundsLimit);
        }

    }
}
