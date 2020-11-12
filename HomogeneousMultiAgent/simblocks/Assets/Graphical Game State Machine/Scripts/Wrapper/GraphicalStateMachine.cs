using System.Collections.Generic;
using UnityEngine;

namespace GSM
{
    public class GraphicalStateMachine
    {
        private int previousStatesSize = 32;
        private readonly List<GraphicalState> m_States = new List<GraphicalState>();
        private readonly List<GraphicalEdge> m_Edges = new List<GraphicalEdge>();
        private readonly List<GraphicalState> previousStates = new List<GraphicalState>();


        /// <summary>
        /// Modify this value to specify the amount of states being saved in <see cref="previousStates"/>. Cannot be lower than 0 or higher than 1027.
        /// </summary>
        public int PreviousStatesSize {
            get { return previousStatesSize; }
            set { previousStatesSize = Mathf.Clamp(value, 0, 1027);
                CleanUpPreviousStates();
            }
        }


        /// <summary>
        /// Saves all states which were previously active. The very last one will be at index zero. The first on in the end
        /// Feel free to modify <see cref="PreviousStatesSize"/> for specifying the amount of states to save.
        /// </summary>
        public List<GraphicalState> PreviousStates { get { return previousStates; }
            private set {
                previousStates.Clear();
                previousStates.AddRange(value);
            } }


        /// <summary>
        /// Currently active state. Returns null if machine is not running
        /// </summary>
        public GraphicalState ActiveState {
            get { return GetStateByID(original.ActiveState != null ? original.ActiveState.id : -1); }
            internal set { original.ActiveState = original.GetState(value == null ? -1 : value.ID); }
        }

        /// <summary>
        /// State which will be active when the machine is started
        /// </summary>
        public GraphicalState StartState { get { return GetStateByID(original.StartState != null ? original.StartState.id : -1); } }

        /// <summary>
        /// List of states in the machine. You should not modify this by code
        /// </summary>
        public List<GraphicalState> States { get { return m_States; } }

        /// <summary>
        /// List of edges in the machine. You should not modify this by code
        /// </summary>
        public List<GraphicalEdge> Edges { get { return m_Edges; } }

        /// <summary>
        /// Checks if there is a tart state existing
        /// </summary>
        public bool HasStartState { get { return StartState != null; } }

        /// <summary>
        /// File name
        /// </summary>
        public string Name { get { return original.machineName; } }

        /// <summary>
        /// True if machine is active. Can only be possible while game is running
        /// </summary>
        public bool IsRunning { get { return original.isRunning; } }

        /// <summary>
        /// If true the start state will be the state which was active the last time, the machine was running.
        /// </summary>
        public bool SaveActiveState { get { return original.saveActiveState; } }

        /// <summary>
        /// If set to true, the machine will automatically start when the containing StateMachineProcessor awakes
        /// </summary>
        public bool StartOnAwake { get { return original.startMachineOnAwake; } set { original.startMachineOnAwake = value; } }

        /// <summary>
        /// Determines whether all runtime callbacks shall be removed when the machine stops
        /// </summary>
        public bool ClearRuntimeCallbacksOnStop { get { return original.clearRuntimeCallbacksOnStop; }  set { original.clearRuntimeCallbacksOnStop = value; } }

        /// <summary>
        /// The reason why the current active state was set active.
        /// </summary>
        public ActivationReason StateActivationReason { get; internal set; }


        /// <summary>
        /// List of all triggers in machine. There can be multiple triggers with same value
        /// </summary>
        public List<string> Triggers { get
            {
                List<string> ts = new List<string>();
                foreach (var edge in Edges)
                {
                    if (edge.Trigger != null)
                        ts.Add(edge.Trigger);
                }
                return ts;
            } }


        internal GSMStateMachine original;

        public GraphicalStateMachine(GSMStateMachine machine)
        {
            original = machine;

            foreach (var state in machine.states)
            {
                m_States.Add(new GraphicalState(this, state));
            }

            foreach (var edge in machine.edges)
            {
                m_Edges.Add(new GraphicalEdge(this, edge));
            }

            StateMachineProcessor.AddMachine(original.machineName, this);
        }



        /// <summary>
        /// Finds all edges having the given state as origin
        /// </summary>
        /// <param name="state">State to find outgoing edges to</param>
        /// <returns>List of edges</returns>
        public List<GraphicalEdge> GetOutgoingEdges(GraphicalState state)
        {
            var outgoingEdges = new List<GraphicalEdge>();
            foreach (var edge in m_Edges)
            {
                if (edge.OriginID == state.ID)
                    outgoingEdges.Add(edge);
            }
            return outgoingEdges;
        }



        /// <summary>
        /// Finds all edges having the given state as target
        /// </summary>
        /// <param name="state">State to find ingoing edges to</param>
        /// <returns>List of edges</returns>
        public List<GraphicalEdge> GetIngoingEdges(GraphicalState state)
        {
            var outgoingEdges = new List<GraphicalEdge>();
            foreach (var edge in m_Edges)
            {
                if (edge.TargetID == state.ID)
                    outgoingEdges.Add(edge);
            }
            return outgoingEdges;
        }



        /// <summary>
        /// Finds a state with the given id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>State with given id. May be null if no state with given id is existing</returns>
        public GraphicalState GetStateByID(int id)
        {
            foreach (var state in m_States)
            {
                if (state.ID == id)
                    return state;
            }
            return null;
        }



        /// <summary>
        /// Finds an edge with the given trigger. Returns the first one found if there are multiple
        /// May be null if there is no edge with that trigger
        /// </summary>
        /// <param name="trigger">Trigger</param>
        /// <returns>One edge with the given trigger</returns>
        public GraphicalEdge GetEdgeByTrigger(string trigger)
        {
            foreach (var edge in Edges)
            {
                if (edge.Trigger == trigger)
                    return edge;
            }
            return null;
        }



        /// <summary>
        /// Returns all edges having the given trigger. List may be empty if there is no such edge
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public List<GraphicalEdge> GetEdgesByTrigger(string trigger)
        {
            List<GraphicalEdge> edges = new List<GraphicalEdge>();
            foreach (var edge in Edges)
            {
                if (edge.Trigger == trigger)
                    edges.Add(edge);
            }
            return edges;
        }



        /// <summary>
        /// Finds a state with the given name. May be null if there is no such state
        /// </summary>
        /// <param name="name">State name</param>
        /// <returns></returns>
        public GraphicalState GetStateByName(string name)
        {
            foreach (var state in m_States)
            {
                if (state.Name == name)
                    return state;
            }
            return null;
        }


        /// <summary>
        /// Proofs for different problems such as:
        ///  - Not existing start state
        ///  - duplicate triggers
        ///  - absorbing states
        ///  - unreachable states
        /// </summary>
        /// <param name="results">Results of validation</param>
        /// <returns>true if machine is able to start</returns>
        public bool Validate(out ValidationResult results)
        {
            return original.Validate(out results);
        }



        /// <summary>
        /// Send a trigger to the machine.
        /// If the active state has an outgoing edge with the given trigger the edge will be used
        /// </summary>
        /// <param name="trigger">Trigger to send</param>
        /// <returns>True if there was a state change</returns>
        public bool SendTrigger(string trigger)
        {
            return SendTrigger(trigger, out _);
        }



        /// <summary>
        /// Send a trigger to the machine.
        /// If the active state has an outgoing edge with the given trigger the edge will be used
        /// </summary>
        /// <param name="trigger">Trigger to send</param>
        /// <param name="newState">Reference to the reached state</param>
        /// <returns>True if there was a state change</returns>
        public bool SendTrigger(string trigger, out GraphicalState newState)
        {
            if (!IsRunning)
            {
                newState = null;
                return false;
            }

            if (trigger == "")
            {
                newState = ActiveState;
                return false;
            }

            int prevID = ActiveState.ID;
            bool success = false;
            foreach (var edge in GetOutgoingEdges(ActiveState))
            {
                if (edge.Trigger == trigger)
                {
                    var target = edge.Target;
                    ActiveState.OnStateLeft();
                    edge.OnEdgePassed();
                    original.activeStateID = edge.TargetID;
                    bool isTerminating = target.IsTerminating;

                    if (!isTerminating)
                        target.OnStateEntered();
                    target.OnStateSetActive();
                    if (isTerminating)
                    {
                        Stop();
                        newState = null;
                        return true;
                    }

                    success = true;
                    break;
                }
            }

            newState = ActiveState;
            int newID = newState.ID;
            if(prevID != newID)
            {
                StateActivationReason = ActivationReason.Edge;
                previousStates.Insert(0, GetStateByID(prevID));
                CleanUpPreviousStates();
            }
            return success;

        }



        /// <summary>
        /// If there are too many states in <see cref="previousStates"/> remove some to match <see cref="PreviousStatesSize"/>
        /// </summary>
        public void CleanUpPreviousStates()
        {
            if (previousStates.Count > previousStatesSize)
            {
                int tooMuch = previousStates.Count - previousStatesSize;
                previousStates.RemoveRange(previousStates.Count - tooMuch, tooMuch);
            }
        }



        /// <summary>
        /// Sets the active state to a state with the given name
        /// </summary>
        /// <param name="name">Name of the state</param>
        public GraphicalState SetActiveState(string name)
        {
            if (!IsRunning)
                throw new System.ArgumentException("Cannot set active state while machine is not running!");

            var state = GetStateByName(name);
            ActiveState = state ?? throw new UnknownStateException();
            StateActivationReason = ActivationReason.SetByName;
            ActiveState.OnStateSetActive();
            return ActiveState;
        }



        /// <summary>
        /// Starts the machine. Sets running flag to true and activates the activeState
        /// </summary>
        public bool Start()
        {
            bool ret = original.Start();
            if (ret)
            {
                if (!ActiveState.OnStateSetActive() && !original.hideAllWarningsConsole)
                {
                    Debug.LogWarning("No OnStateSetActive() method found on start state \"" + ActiveState.Name + "\". Consider using this to initialize your state.");
                }
                StateActivationReason = ActivationReason.GameStarted;
            }
            return ret;
        }



        /// <summary>
        /// Stops the machine. Sets active state to null if you dont want to save
        /// Clears all runtime callbacks
        /// </summary>
        public void Stop()
        {
            Stop(true);
        }


        /// <summary>
        /// Stops the machine. Sets active state to null if you dont want to save.
        /// Use <paramref name="clearRuntimeCallbacks"/> to determine if the runtime callbacks should be cleared
        /// </summary>
        /// <param name="clearRuntimeCallbacks">If set to true, all callbacks will be deleted</param>
        public void Stop(bool clearRuntimeCallbacks)
        {
            PreviousStates = new List<GraphicalState>();
            StateActivationReason = ActivationReason.None;
            original.Stop();

            if (clearRuntimeCallbacks)
                ClearRuntimeCallbacks();
        }


        /// <summary>
        /// Removes all registered runtime callbacks from all edges and all states
        /// </summary>
        public void ClearRuntimeCallbacks()
        {
            foreach (var state in States)
            {
                state.ClearRuntimeCallbacks();
            }

            foreach (var edge in Edges)
            {
                edge.ClearRuntimeCallbacks();
            }
        }


        /// <summary>
        /// Reasons a state can be active
        /// </summary>
        public enum ActivationReason
        {
            /// <summary>
            /// No state active 
            /// </summary>
            None,

            /// <summary>
            /// When game was started
            /// </summary>
            GameStarted,

            /// <summary>
            /// When the state was set active by using an edge
            /// </summary>
            Edge,

            /// <summary>
            /// When the state was set active by setting it by its name
            /// </summary>
            SetByName
        }


        /// <summary>
        /// Type of Update function
        /// </summary>
        public enum StateStayUpdateType
        {
            LateUpdate, Update, FixedUpdate, None
        }


        /// <summary>
        /// Event types for states and edge
        /// </summary>
        public enum RuntimeCallbackType
        {
            OnStateStay, OnStateLeft, OnStateSetActive, OnStateEntered, OnEdgePassed
        }


        /// <summary>
        /// Used to determine the order in which the callbacks get invoked
        /// </summary>
        public enum CallbackInvocationOrder
        {
            RuntimeBeforeFile, FileBeforeRuntime, OnlyFile, OnlyRuntime
        }
    }
}
