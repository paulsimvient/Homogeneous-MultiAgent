using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSM
{
    [Serializable]
    public class GSMEvent
    {
        [SerializeField] internal List<GSMCallback> callbacks = new List<GSMCallback>();
       

        internal void SwapCallbacks(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= callbacks.Count)
                return;
            if (toIndex < 0 || toIndex >= callbacks.Count)
                return;

            var swap = callbacks[toIndex];
            callbacks[toIndex] = callbacks[fromIndex];
            callbacks[fromIndex] = swap;
        }

        public bool Invoke(bool error)
        {
            bool ret = true;
            foreach (var callback in callbacks)
            {
                ret = callback.Invoke(error) && ret;
            }
            return ret;
        }

        public GSMEvent Clone()
        {
            GSMEvent evt = new GSMEvent
            {
                callbacks = new List<GSMCallback>()
            };
            foreach (var c in callbacks)
            {
                evt.callbacks.Add(c.Clone());
            }
            return evt;
        }

        public GSMCallback FindFirstUsefulCallback()
        {
            foreach (var callback in callbacks)
            {
                if (callback.objectName != "" && callback.methodName != "")
                    return callback;
            }
            return null;
        }

        public void FindCallbackObjectReferences()
        {
            foreach (var callback in callbacks)
            {
                callback.FindObjectReferences();
            }
        }
    }
}
