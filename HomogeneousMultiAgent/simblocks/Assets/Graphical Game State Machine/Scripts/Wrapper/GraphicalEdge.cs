using System;
using System.Collections.Generic;
using static GSM.GraphicalStateMachine;

namespace GSM
{
    public class GraphicalEdge
    {
        /// <summary>
        /// State ID of origin state
        /// </summary>
        public int OriginID { get { return Original.originID; } }

        /// <summary>
        /// State ID of target state
        /// </summary>
        public int TargetID { get { return Original.targetID; } }

        /// <summary>
        /// Origin state
        /// </summary>
        public GraphicalState Origin { get { return Machine.GetStateByID(OriginID); } }

        /// <summary>
        /// Target state
        /// </summary>
        public GraphicalState Target { get { return Machine.GetStateByID(TargetID); } }

        /// <summary>
        /// Trigger. Sending this string as Trigger while origin state is active, this edge will be used
        /// </summary>
        public string Trigger { get { return Original.trigger; } }


        /// <summary>
        /// Machine this edge is currently in
        /// </summary>
        public GraphicalStateMachine Machine { get; private set; }


        public CallbackInvocationOrder CallbackInvocationOrder
        {
            get
            {
                switch (Original.callbackInvokationOrder)
                {
                    case GSMStateMachine.RuntimeBeforeFile:
                        return CallbackInvocationOrder.RuntimeBeforeFile;
                    case GSMStateMachine.FileBeforeRuntime:
                        return CallbackInvocationOrder.FileBeforeRuntime;
                    case GSMStateMachine.OnlyRuntime:
                        return CallbackInvocationOrder.OnlyRuntime;
                    case GSMStateMachine.OnlyFile:
                        return CallbackInvocationOrder.OnlyFile;
                    default:
                        return CallbackInvocationOrder.RuntimeBeforeFile;
                }
            }
        }


        internal GSMEdge Original;


        internal GraphicalEdge(GraphicalStateMachine machine, GSMEdge edge)
        {
            Machine = machine;
            Original = edge;
        }



        /// <summary>
        /// Getting called when edge is used to set a new active state.
        /// Called right after <see cref="GraphicalState.OnStateLeft"/> on old active state and before <see cref="GraphicalState.OnStateEntered"/> of new active state.
        /// </summary>
        /// <returns>True if the invocation was successful</returns>
        public bool OnEdgePassed()
        {
            switch (CallbackInvocationOrder)
            {
                case CallbackInvocationOrder.RuntimeBeforeFile:
                    bool s1 = InvokeRuntimeCallbacks();
                    bool s2 = Original.onEdgePassed.Invoke(Machine.original.errorOnFailedInvoke);
                    return s1 && s2;
                case CallbackInvocationOrder.FileBeforeRuntime:
                    s2 = Original.onEdgePassed.Invoke(Machine.original.errorOnFailedInvoke);
                    s1 = InvokeRuntimeCallbacks();
                    return s1 && s2;
                case CallbackInvocationOrder.OnlyFile:
                    return Original.onEdgePassed.Invoke(Machine.original.errorOnFailedInvoke);
                case CallbackInvocationOrder.OnlyRuntime:
                    return InvokeRuntimeCallbacks();
                default:
                    return false;
            }
        }

        #region Runtime callbacks

        private readonly List<RuntimeCallback> runtimeCallbacks = new List<RuntimeCallback>();


        /// <summary>
        /// Registers a new callback to event OnEdgePassed
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public void RegisterRuntimeCallback(RuntimeCallback callback)
        {
            if(callback.Type != RuntimeCallbackType.OnEdgePassed)
                throw new ArgumentException("Cannot register callbacks for event " + callback.Type.ToString() + " on an edge.");

            runtimeCallbacks.Add(callback);
        }


        /// <summary>
        /// Registers a new callback to event OnEdgePassed
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public void RegisterRuntimeCallback(RuntimeCallback.RuntimeEventDelegate callback)
        {
            runtimeCallbacks.Add(new RuntimeCallback(RuntimeCallbackType.OnEdgePassed, callback));
        }


        /// <summary>
        /// Get all registered callbacks
        /// </summary>
        /// <returns></returns>
        public List<RuntimeCallback> GetRuntimeCallbacks()
        {
            return runtimeCallbacks;
        }


        /// <summary>
        /// Invokes all registered runtime callback for event OnEdgePassed
        /// </summary>
        /// <returns></returns>
        public bool InvokeRuntimeCallbacks()
        {
            bool ret = true;
            foreach (var callback in runtimeCallbacks)
            {
                ret = callback.Invoke() && true;
            }
            return ret;
        }


        /// <summary>
        /// Removes all registered runtime callback for event OnEdgePassed
        /// </summary>
        public void ClearRuntimeCallbacks()
        {
            runtimeCallbacks.Clear();
        }

        #endregion
    }
}
