using System.Collections.Generic;

using UnityEngine;

using Assets.Scripts.GameStructure.Communication.EventSystem;
using Assets.Scripts.GameStructure.Communication.Payloads;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Methods publicly accessible for IScreenWrapper.
    /// </summary>
    public interface IScreenWrapper
    {
        /// <summary>
        /// Visit all registered screen wrappable objects to perform a screen wrap.
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Class using visitor pattern to screen wrap all screen wrappable objects.
    /// </summary>
    public class ScreenWrapper : IScreenWrapper
    {
        private readonly List<IScreenWrappable> _observers;
        private readonly IScreenHandler _screenHandler;

        #region CONSTRUCTOR
        public ScreenWrapper(IScreenHandler screenHandler)
        {
            Logger.Info($"constructed screen wrapper");

            _screenHandler = screenHandler;
            _observers = new List<IScreenWrappable>();
            SubscribeToGameEvents();
        }
        ~ScreenWrapper()
        {
            UnsubscribeFromGameEvents();
        }
        #endregion

        #region API
        public void Update()
        {
            foreach (var observer in _observers)
            {
                var bounds = observer.GetObjectBounds();
                var transform = observer.GetObjectTransform();
                Wrap(ref bounds, ref transform);
            }
        }
        #endregion

        private void SubscribeToGameEvents()
        {
            EventBus.GetInstance().Subscribe(GameConstants.EventKey.ToggleScreenWrapping, ToggleScreenWrapperEvent);
        }
        private void UnsubscribeFromGameEvents()
        {
            EventBus.GetInstance().Unsubscribe(GameConstants.EventKey.ToggleScreenWrapping, ToggleScreenWrapperEvent);
        }
        private void ToggleScreenWrapperEvent(IBasePayload basePayload)
        {
            WrapperTogglePayload wrappableTogglePayload = (WrapperTogglePayload)basePayload;

            if (wrappableTogglePayload.ToggleStatus)
            {
                Register(wrappableTogglePayload.ScreenWrappable);
            }
            else
            {
                Unregister(wrappableTogglePayload.ScreenWrappable);
            }
        }

        private void Unregister(IScreenWrappable wrappableObject)
        {
            //Logger.Info($"Unregistered {wrappableObject} for screen wrapping");
            _observers.Remove(wrappableObject);
        }
        private void Register(IScreenWrappable wrappableObject)
        {
            //Logger.Info($"registered {wrappableObject} for screen wrapping");
            _observers.Add(wrappableObject);
        }

        /// <summary>
        /// Wrapping operation performed during the visit on registered objects based on their bounds and transform.
        /// </summary>
        /// <param name="bounds">bounds of the object being wrapped.</param>
        /// <param name="observerObjectTransform">transform of the object being wrapped.</param>
        private void Wrap(ref Bounds bounds, ref Transform observerObjectTransform)
        {
            var outerLimitX = bounds.extents.x;
            var outerLimitY = bounds.extents.x;

            var bottomLeftCorner = _screenHandler.GetViewportToWorldPoint(new Vector2(0, 0));
            var topRightCorner = _screenHandler.GetViewportToWorldPoint(new Vector2(1, 1));

            Vector2 wrappedPosition = observerObjectTransform.transform.position;

            if (IsOutFromLeftEdge(ref observerObjectTransform, ref bottomLeftCorner.x, ref outerLimitX))
            {
                wrappedPosition.x = topRightCorner.x;
            }
            if (IsOutFromRightEdge(ref observerObjectTransform, ref topRightCorner.x, ref outerLimitX))
            {
                wrappedPosition.x = bottomLeftCorner.x;
            }
            if (IsOutFromTopEdge(ref observerObjectTransform, ref bottomLeftCorner.y, ref outerLimitY))
            {
                wrappedPosition.y = topRightCorner.y;
            }
            if (IsOutFromBottomEdge(ref observerObjectTransform, ref topRightCorner.y, ref outerLimitY))
            {
                wrappedPosition.y = bottomLeftCorner.y;
            }

            observerObjectTransform.position = wrappedPosition;
        }

        private bool IsOutFromLeftEdge(ref Transform observerObjectTransform, ref float leftEdgeX, ref float leftLimit)
        {
            return observerObjectTransform.position.x <= leftEdgeX - leftLimit;
        }
        private bool IsOutFromRightEdge(ref Transform observerObjectTransform, ref float rightEdgeX, ref float rightLimit)
        {
            return observerObjectTransform.position.x >= rightEdgeX + rightLimit;
        }
        private bool IsOutFromTopEdge(ref Transform observerObjectTransform, ref float topEdgeY, ref float topLimit)
        {
            return observerObjectTransform.position.y <= topEdgeY - topLimit;
        }
        private bool IsOutFromBottomEdge(ref Transform observerObjectTransform, ref float bottomEdgeY, ref float bottomLimit)
        {
            return observerObjectTransform.position.y >= bottomEdgeY + bottomLimit;
        }
    }
}


