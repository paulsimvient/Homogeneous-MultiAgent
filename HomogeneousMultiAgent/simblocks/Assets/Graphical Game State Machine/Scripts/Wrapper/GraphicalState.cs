using System;
using System.Collections.Generic;
using static GSM.GraphicalStateMachine;

namespace GSM
{
    public class GraphicalState
    {
        /// <summary>
        /// Name of the state
        /// </summary>
        public string Name { get { return Original.name; } }

        /// <summary>
        /// Unique ID of the state
        /// </summary>
        public int ID { get { return Original.id; } }

        /// <summary>
        /// All edges having this state as origin
        /// </summary>
        public List<GraphicalEdge> OutgoingEdges { get { return Machine.GetOutgoingEdges(this); } }

        /// <summary>
        /// All edges having this state as target
        /// </summary>
        public List<GraphicalEdge> IngoingEdges { get { return Machine.GetIngoingEdges(this); } }

        /// <summary>
        /// Number of outgoing edges
        /// </summary>
        public int Outgrade { get { return OutgoingEdges.Count; } }

        /// <summary>
        /// Number of ingoing edges
        /// </summary>
        public int Ingrade { get { return IngoingEdges.Count; } }

        /// <summary>
        /// True if this is the currently active state
        /// </summary>
        public bool IsActive { get { return this == Machine.ActiveState; } }

        /// <summary>
        /// If set to true, the machine will stop if the state is set activated.
        /// </summary>
        public bool IsTerminating { get { return Original.isTerminating; } }


        /// <summary>
        /// Use this variable to determine in which typ of update-method the OnStateStay()-event should be triggered.
        /// It can be set in the machine editor
        /// </summary>
        public StateStayUpdateType UpdateType
        {
            get
            {
                switch(Original.updateType)
                {
                    case GSMState.UpdateTypeUpdate:
                        return StateStayUpdateType.Update;
                    case GSMState.UpdateTypeLateUpdate:
                        return StateStayUpdateType.LateUpdate;
                    case GSMState.UpdateTypeFixedUpdate:
                        return StateStayUpdateType.FixedUpdate;
                    default:
                        return StateStayUpdateType.None;
                }
            }
        }



        /// <summary>
        /// Determines the order to invoke the callbacks.
        /// Can be set in machine editor
        /// </summary>
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



        /// <summary>
        /// Machine this state is in
        /// </summary>
        public GraphicalStateMachine Machine { get; private set; }



        internal GSMState Original;

        internal GraphicalState(GraphicalStateMachine machine, GSMState state)
        {
            Machine = machine;
            Original = state;
        }


        /// <summary>
        /// Checks if the given edge targets to this state
        /// </summary>
        /// <param name="edge">Edge to proof</param>
        /// <returns></returns>
        public bool HasEdgeIngoing(GraphicalEdge edge)
        {
            return IngoingEdges.Contains(edge);
        }

        /// <summary>
        /// Checks if the given edge has this state as origin
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool HasEdgeOutgoing(GraphicalEdge edge)
        {
            return OutgoingEdges.Contains(edge);
        }




        /// <summary>
        /// Method getting called each Update() call when state is active.
        /// Also invokes runtime events
        /// Use <see cref="CallbackInvocationOrder"/> to determine the order to invoke callbacks
        /// </summary>
        /// <returns>True if the invocation was successful</returns>
        public bool OnStateStay()
        {
            switch (CallbackInvocationOrder)
            {
                case CallbackInvocationOrder.RuntimeBeforeFile:
                    bool s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateStay);
                    bool s2 = Original.onStateStay.Invoke(Machine.original.errorOnFailedInvoke);
                    return s1 && s2;
                case CallbackInvocationOrder.FileBeforeRuntime:
                    s2 = Original.onStateStay.Invoke(Machine.original.errorOnFailedInvoke);
                    s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateStay);
                    return s1 && s2;
                case CallbackInvocationOrder.OnlyFile:
                    return Original.onStateStay.Invoke(Machine.original.errorOnFailedInvoke);
                case CallbackInvocationOrder.OnlyRuntime:
                    return InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateStay);
                default:
                    return false;
            }
        }




        /// <summary>
        /// Method getting called when state is active and is going to be left.
        /// Also invokes runtime events
        /// Use <see cref="CallbackInvocationOrder"/> to determine the order to invoke callbacks
        /// Getting called before <see cref="GraphicalEdge.OnEdgePassed"/>
        /// </summary>
        /// <returns>True if the invocation was successful</returns>
        public bool OnStateLeft()
        {
            switch (CallbackInvocationOrder)
            {
                case CallbackInvocationOrder.RuntimeBeforeFile:
                    bool s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateLeft);
                    bool s2 = Original.onStateLeft.Invoke(Machine.original.errorOnFailedInvoke);
                    return s1 && s2;
                case CallbackInvocationOrder.FileBeforeRuntime:
                    s2 = Original.onStateLeft.Invoke(Machine.original.errorOnFailedInvoke);
                    s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateLeft);
                    return s1 && s2;
                case CallbackInvocationOrder.OnlyFile:
                    return Original.onStateLeft.Invoke(Machine.original.errorOnFailedInvoke);
                case CallbackInvocationOrder.OnlyRuntime:
                    return InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateLeft);
                default:
                    return false;
            }
        }



        /// <summary>
        /// Method getting called when state is entered by an edge. Getting called right after <see cref="GraphicalEdge.OnEdgePassed"/>      
        /// Also invokes runtime events
        /// Use <see cref="CallbackInvocationOrder"/> to determine the order to invoke callbacks
        /// </summary>
        /// <returns>True if the invocation was successful</returns>
        public bool OnStateEntered()
        {
            switch (CallbackInvocationOrder)
            {
                case CallbackInvocationOrder.RuntimeBeforeFile:
                    bool s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateEntered);
                    bool s2 = Original.onStateEntered.Invoke(Machine.original.errorOnFailedInvoke);
                    return s1 && s2;
                case CallbackInvocationOrder.FileBeforeRuntime:
                    s2 = Original.onStateEntered.Invoke(Machine.original.errorOnFailedInvoke);
                    s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateEntered);
                    return s1 && s2;
                case CallbackInvocationOrder.OnlyFile:
                    return Original.onStateEntered.Invoke(Machine.original.errorOnFailedInvoke);
                case CallbackInvocationOrder.OnlyRuntime:
                    return InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateEntered);
                default:
                    return false;
            }
        }



        /// <summary>
        /// Method getting called when state is set active. Getting called right after <see cref="OnStateEntered"/>. Also called when setting active by script or as start state.
        /// Also invokes runtime events
        /// Use <see cref="CallbackInvocationOrder"/> to determine the order to invoke callbacks
        /// </summary>
        /// <returns>True if the invocation was successful</returns>
        public bool OnStateSetActive()
        {
            switch (CallbackInvocationOrder)
            {
                case CallbackInvocationOrder.RuntimeBeforeFile:
                    bool s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateSetActive);
                    bool s2 = Original.onStateSetActive.Invoke(Machine.original.errorOnFailedInvoke);
                    return s1 && s2;
                case CallbackInvocationOrder.FileBeforeRuntime:
                    s2 = Original.onStateSetActive.Invoke(Machine.original.errorOnFailedInvoke);
                    s1 = InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateSetActive);
                    return s1 && s2;
                case CallbackInvocationOrder.OnlyFile:
                    return Original.onStateSetActive.Invoke(Machine.original.errorOnFailedInvoke);
                case CallbackInvocationOrder.OnlyRuntime:
                    return InvokeRuntimeCallbacks(RuntimeCallbackType.OnStateSetActive);
                default:
                    return false;
            }
        }


        /// <summary>
        /// Finds all states which have an edge connected with the origin state. Uses only outgoing edges if you leave <paramref name="alsoIngoingEdges"/> false.
        /// </summary>
        /// <param name="alsoIngoingEdges">Set this to true if you also want neighboured states which target the origin state</param>
        /// <returns>List if connected states</returns>
        public List<GraphicalState> GetNeighbours(bool alsoIngoingEdges)
        {
            List<GraphicalState> states = new List<GraphicalState>();
            foreach (var state in Machine.original.GetNeighbours(Machine.original.GetState(ID), alsoIngoingEdges))
            {
                states.Add(Machine.GetStateByID(state.id));
            }
            return states;
        }


        /// <summary>
        /// Finds all states which are reachable by an edge.
        /// </summary>
        /// <seealso cref="GetNeighbours(bool)"/>
        /// <returns>List of neighboured states</returns>
        public List<GraphicalState> GetNeighbours()
        {
            return GetNeighbours(false);
        }






        #region Runtime Events
        private readonly List<RuntimeCallback> runtimeCallbacksOnStateEntered = new List<RuntimeCallback>();
        private readonly List<RuntimeCallback> runtimeCallbacksOnStateSetActive = new List<RuntimeCallback>();
        private readonly List<RuntimeCallback> runtimeCallbacksOnStateStay = new List<RuntimeCallback>();
        private readonly List<RuntimeCallback> runtimeCallbacksOnStateLeft = new List<RuntimeCallback>();


        /// <summary>
        /// Returns all RuntimeCallbacks of this state
        /// </summary>
        /// <returns></returns>
        public List<RuntimeCallback> GetRuntimeCallbacks()
        {
            List<RuntimeCallback> events = new List<RuntimeCallback>();
            events.AddRange(runtimeCallbacksOnStateEntered);
            events.AddRange(runtimeCallbacksOnStateSetActive);
            events.AddRange(runtimeCallbacksOnStateStay);
            events.AddRange(runtimeCallbacksOnStateLeft);
            return events;
        }

        /// <summary>
        /// Returns a list of all RuntimeCallbacks which are invoked at the given event type
        /// </summary>
        /// <param name="type">Type which determines when the callback should be called</param>
        /// <returns>List of RuntimeCallbacks</returns>
        public List<RuntimeCallback> GetRuntimeCallbacks(RuntimeCallbackType type)
        {
            switch (type)
            {
                case RuntimeCallbackType.OnStateStay:
                    return runtimeCallbacksOnStateStay;
                case RuntimeCallbackType.OnStateLeft:
                    return runtimeCallbacksOnStateLeft;
                case RuntimeCallbackType.OnStateSetActive:
                    return runtimeCallbacksOnStateSetActive;
                case RuntimeCallbackType.OnStateEntered:
                    return runtimeCallbacksOnStateEntered;
                case RuntimeCallbackType.OnEdgePassed:
                    throw new ArgumentException("Cannot get callbacks for event " + type.ToString() + ".");
                default:
                    throw new ArgumentException("Illegal RuntimeCallbackType");
            }
        }


        /// <summary>
        /// Registeres a new runtime callback
        /// You can only use types OnStateEntered, OnStateLeft, OnStateStay and OnStateSetActive here.
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public void RegisterRuntimeCallback(RuntimeCallback callback)
        {
            switch (callback.Type)
            {
                case RuntimeCallbackType.OnStateStay:
                    runtimeCallbacksOnStateStay.Add(callback);
                    break;
                case RuntimeCallbackType.OnStateLeft:
                    runtimeCallbacksOnStateLeft.Add(callback);
                    break;
                case RuntimeCallbackType.OnStateSetActive:
                    runtimeCallbacksOnStateSetActive.Add(callback);
                    break;
                case RuntimeCallbackType.OnStateEntered:
                    runtimeCallbacksOnStateEntered.Add(callback);
                    break;
                case RuntimeCallbackType.OnEdgePassed:
                    throw new ArgumentException("Cannot register callbacks for event " + callback.Type.ToString() + " on a state.");
                default:
                    throw new ArgumentException("Illegal RuntimeCallbackType");
            }
        }


        /// <summary>
        /// Registeres a new runtime callback
        /// You can only use types OnStateEntered, OnStateLeft, OnStateStay and OnStateSetActive here.
        /// </summary>
        /// <param name="type">Type of the callback</param>
        /// <param name="callback">Callback to register</param>
        public void RegisterRuntimeCallback(RuntimeCallbackType type, RuntimeCallback.RuntimeEventDelegate callback)
        {
            RegisterRuntimeCallback(new RuntimeCallback(type, callback));
        }


        /// <summary>
        /// Invokes all registered runtime callbacks for the given callback type.
        /// You can only use types OnStateEntered, OnStateLeft, OnStateStay and OnStateSetActive here.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool InvokeRuntimeCallbacks(RuntimeCallbackType type)
        {
            bool ret = true;
            foreach (var callback in GetRuntimeCallbacks(type))
            {
                ret = callback.Invoke() && true;
            }
            return ret;
        }


        /// <summary>
        /// Removes all registered runtime callbacks for all four events
        /// </summary>
        public void ClearRuntimeCallbacks()
        {
            runtimeCallbacksOnStateEntered.Clear();
            runtimeCallbacksOnStateStay.Clear();
            runtimeCallbacksOnStateLeft.Clear();
            runtimeCallbacksOnStateSetActive.Clear();
        }

        #endregion





        public override string ToString()
        {
            return Name + " (ID: " + ID + ")";
        }
    }



}
