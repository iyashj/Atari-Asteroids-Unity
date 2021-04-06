using UnityEngine;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Specifies implementable controls on spaceship.
    /// As well as methods to check and toggle controller.
    /// </summary>
    public interface ISpaceshipControllable
    {
        bool IsEnabled();
        void ToggleController(bool toggleStatus);

        bool IsShooting();
        bool IsTriggeringHyperspace();
        float GetTurnAxis();
        float GetForwardAxis();
    }

    /// <summary>
    /// Implements spaceship controls binding them to keyboard controls
    /// </summary>
    public class SpaceshipController : ISpaceshipControllable
    {
        private bool _isEnabled = false;
        private readonly string horizonalAxis = "Horizontal";
        private readonly string verticalAxis = "Vertical";

        #region CONSTRUCTOR

        public SpaceshipController()
        {
            Logger.Info($"Constructed spaceship controller");
            _isEnabled = true;
        }


        #endregion

        #region API
        public bool IsEnabled()
        {
            return _isEnabled;
        }
        public void ToggleController(bool toggleStatus)
        {
            Logger.Info($"toggled spaceship controller with status {toggleStatus}");

            _isEnabled = toggleStatus;
        }
        
        public bool IsShooting()
        {
            return Input.GetKeyDown(KeyCode.LeftControl);
        }
        public bool IsTriggeringHyperspace()
        {
            return Input.GetKeyDown(KeyCode.RightControl);
        }
        public float GetTurnAxis()
        {
            return Input.GetAxis(horizonalAxis);
        }
        public float GetForwardAxis()
        {
            var axis = Input.GetAxis(verticalAxis);
            return Mathf.Clamp01(axis);
        }
        #endregion
    }
}


