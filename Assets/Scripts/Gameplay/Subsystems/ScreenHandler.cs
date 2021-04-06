using UnityEngine;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Methods publicly accessible for IScreenHandler.
    /// </summary>
    public interface IScreenHandler
    {
        /// <summary>
        /// Get a random point on screen at a safe distance from input target position.
        /// </summary>
        /// <param name="targetPosition">input position from which distance is to be calculated.</param>
        /// <param name="safeDistance">Minimum distance the random point to be qualified as safe.</param>
        /// <returns></returns>
        Vector2 GetRandomPointAtSafeDistanceFromTarget(ref Vector2 targetPosition, ref float safeDistance);
        /// <summary>
        /// Translates viewport point to world point.
        /// </summary>
        /// <param name="viewportPoint">input viewport point</param>
        /// <returns>output world point</returns>     
        Vector2 GetViewportToWorldPoint(Vector2 viewportPoint);
        
        Rect GetScreenRect();
        Vector2 GetRandomPointOnScreen();
    }

    /// <summary>
    /// Class to handle screen interactions.
    /// </summary>
    public class ScreenHandler : IScreenHandler
    {
        private readonly Camera _camera;
        private readonly Rect _screenRect;

        #region CONSTRUCTOR
        public ScreenHandler(Camera camera)
        {
            Logger.Info($"constructed screen handler");

            _camera = camera;
            _screenRect = InitializeScreenRect();
        }

        #endregion
        
        #region API
        public Vector2 GetRandomPointAtSafeDistanceFromTarget(ref Vector2 targetPosition, ref float safeDistance)
        {
            var randomPointOnScreen = GetRandomPointOnScreen();
            while (!AreAtSafeDistance(randomPointOnScreen, targetPosition, safeDistance))
            {
                randomPointOnScreen = GetRandomPointOnScreen();
            }

            return randomPointOnScreen;
        }
        public Rect GetScreenRect() => _screenRect;
        public Vector2 GetViewportToWorldPoint(Vector2 viewportPoint) => _camera.ViewportToWorldPoint(viewportPoint);
        public Vector2 GetRandomPointOnScreen()
        {
            return _camera.ViewportToWorldPoint(new Vector2(Random.value, Random.value));
        }
        #endregion

        private Rect InitializeScreenRect()
        {
            var screenMin = Vector2.zero;
            var screenMax = Vector2.one;
            Vector2 worldMin = GetWorldPointFromViewport(screenMin);
            Vector2 worldMax = GetWorldPointFromViewport(screenMax);
            return Rect.MinMaxRect(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
        }
        
        private Vector2 GetWorldPointFromViewport(Vector3 viewportPoint)
        {
            return _camera.ViewportToWorldPoint(viewportPoint);
        }
        private bool AreAtSafeDistance(Vector2 point1, Vector2 point2, float safeDistance)
        {
            var distanceBetweenGivenPoints = Vector2.Distance(point1, point2);
            return distanceBetweenGivenPoints > safeDistance;
        }
    }
}

