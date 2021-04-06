using UnityEngine;

namespace Assets.Scripts.Gameplay.Subsystems
{
    /// <summary>
    /// Specifies getter for fields required for wrapping.
    /// </summary>
    public interface IScreenWrappable
    {
        /// <summary>
        /// Get bounds of the object based on their sprite renderer.
        /// </summary>
        /// <returns>bounds of the object</returns>
        Bounds GetObjectBounds();
        Transform GetObjectTransform();
    }
}

