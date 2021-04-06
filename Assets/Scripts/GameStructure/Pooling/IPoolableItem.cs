using System;

using UnityEngine;

namespace Assets.Scripts.GameStructure.Pooling
{
    /// <summary>
    /// Interface that adds reposition for poolable items
    /// </summary>
    public interface IRepositionable
    {
        /// <summary>
        /// Set position for pooled items gamobject in world sapce
        /// </summary>
        /// <param name="position"></param>
        void SetPosition(Vector2 position);
    }

    /// <summary>
    /// Interface defining methods used in pooling the item
    /// </summary>
    public interface IPoolableItem
    {
        /// <summary>
        /// Callback on release/expiry of pooled item
        /// </summary>
        event Action ItemExpired;
        
        /// <summary>
        /// Initializing call for pooled item
        /// Called when item is created to be filled in the pool and not in use
        /// </summary>
        void Init();
        
        /// <summary>
        /// Method called on pooled item after its obtained from pool and is about to be used
        /// </summary>
        void Engage();
        
        /// <summary>
        /// Method called on pooled item after it has been used and is being sent back to the pool for recycling
        /// </summary>
        void Release();
    }
}