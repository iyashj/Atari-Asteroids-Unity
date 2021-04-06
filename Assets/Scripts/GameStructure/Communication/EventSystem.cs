using System;
using System.Collections.Generic;

namespace Assets.Scripts.GameStructure.Communication
{
    // Defines EventSystem used to have implement Observer Pattern.
    // for communication among gameplay classes.
    // This keeps the coupling weak.
    namespace EventSystem
    {
        public interface IBasePayload
        {
            string EventName { get; }
        }

        public class EventBus
        {
            private readonly Dictionary<string, List<Action<IBasePayload>>> _eventLibrary;

            #region CONSTRUCTOR
            public EventBus()
            {
                _eventLibrary = new Dictionary<string, List<Action<IBasePayload>>>();
            }
            #endregion

            #region LAZY_SINGLETON

            private static EventBus _instance;
            public static EventBus GetInstance()
            {
                if (_instance == null)
                {
                    Logger.Info($"lazyily instantiated event bus");
                    _instance = new EventBus();
                }

                return _instance;
            }

            #endregion

            #region API

            public void Subscribe(string eventName, Action<IBasePayload> callback)
            {
                if (!_eventLibrary.ContainsKey(eventName))
                {
                    _eventLibrary.Add(eventName, new List<Action<IBasePayload>>());
                }
                _eventLibrary[eventName].Add(callback);
            }
            public void Unsubscribe(string eventName, Action<IBasePayload> callback)
            {
                if (!_eventLibrary.ContainsKey(eventName))
                {
                    return;
                }
                _eventLibrary[eventName].Remove(callback);
            }
            public void Publish(IBasePayload payload)
            {
                var eventName = payload.EventName;
                if (!_eventLibrary.ContainsKey(eventName))
                {
                    return;
                }
                foreach (var callback in _eventLibrary[eventName])
                {
                    callback.Invoke(payload);
                }
            }

            #endregion
        }
    }
}


