using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace GSM
{
    [Serializable]
    public partial class GSMStateMachine : ScriptableObject
    {

        public const int RuntimeBeforeFile = 0;
        public const int FileBeforeRuntime = 1;
        public const int OnlyFile = 2;
        public const int OnlyRuntime = 3;

        [SerializeField] internal List<GSMEdge> edges = new List<GSMEdge>();
        [SerializeField] internal List<GSMState> states = new List<GSMState>();
        [SerializeField] private int startStateID = -1;
        [SerializeField] internal string machineName;
        [SerializeField] public bool saveActiveState;
        [SerializeField] public bool hideAllWarningsEditor;
        [SerializeField] public bool hideAllWarningsConsole;
        [SerializeField] public bool errorOnFailedInvoke;

        [SerializeField] internal int activeStateID = -1;

        /// <summary>
        /// If set to true, the machine will automatically be started on awake.
        /// </summary>
        [SerializeField] public bool startMachineOnAwake = true;

        /// <summary>
        /// Determines whether the runtime callbacks shall be removed when the machine is stopped
        /// </summary>
        [SerializeField] public bool clearRuntimeCallbacksOnStop = true;

        public GSMState ActiveState {
            get { return activeStateID == -1 ? null : GetState(activeStateID); }
            set { activeStateID = value != null && ContainsState(value) ? value.id : -1 ; GUI.changed = true; }
        }
        public int StateCount { get { return states.Count; } }
        public GSMState StartState { get { return startStateID == -1 ? null : GetState(startStateID); } }
        public bool HasStartState { get { return startStateID != -1; } }
        [NonSerialized] public bool isRunning = false;



        #region Runtime

        /// <summary>
        /// Starts the machine. Sets running flag to true and activates the activeState.
        /// Invokes OnStateSetActive on active state
        /// </summary>
        public bool Start()
        {
            if (isRunning)
                throw new MachineAlreadyRunningException(this);

            isRunning = true;
            if (!saveActiveState || activeStateID == -1)
                activeStateID = startStateID;
            GUI.changed = true;

            FindObjectReferences();

            return true;
        }


        private void FindObjectReferences()
        {
            foreach (var state in states)
            {
                state.FindEventObjectReferences();
            }

            foreach (var edge in edges)
            {
                edge.FindEventObjectReferences();
            }
        }


        /// <summary>
        /// Stops the machine. Sets active state to null if you dont want to save
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            if (!saveActiveState || ActiveState != null && ActiveState.isTerminating)
                activeStateID = -1;
        }



        /// <summary>
        /// Finds all states which have an edge connected with the origin state. Uses only outgoing edges if you leave <paramref name="alsoIngoingEdges"/> false.
        /// </summary>
        /// <param name="origin">State to find neighbours to</param>
        /// <param name="alsoIngoingEdges">Set this to true if you also want neighboured states which target the origin state</param>
        /// <returns>List if connected states</returns>
        public List<GSMState> GetNeighbours(GSMState origin, bool alsoIngoingEdges)
        {
            List<GSMState> states = new List<GSMState>();
            foreach (var edge in GetOutgoingEdges(origin))
            {
                states.Add(GetState(edge.targetID));
            }

            if(alsoIngoingEdges)
                foreach (var edge in GetIngoingEdges(origin))
                {
                    states.Add(GetState(edge.originID));
                }

            return states;
        }

        #endregion

        #region Edge insertion, deletion and management
        /// <summary>
        /// Checks if the given edge exists
        /// </summary>
        /// <param name="edge">Edge to proof</param>
        /// <returns></returns>
        public bool ContainsEdge(GSMEdge edge)
        {
            return edges.Contains(edge);
        }




        /// <summary>
        /// If there is a cycle within origin and target state of the edge return the other edge of the circle
        /// </summary>
        /// <param name="edge">Edge to find the complement to</param>
        /// <returns>Complement edge</returns>
        public GSMEdge GetComplementEdge(GSMEdge edge)
        {
            var origin = GetState(edge.originID);
            var target = GetState(edge.targetID);
            foreach (var o in GetOutgoingEdges(target))
            {
                if (o.targetID == edge.originID)
                    return o;
            }

            foreach (var i in GetIngoingEdges(origin))
            {
                if (i.originID == edge.targetID)
                    return i;
            }
            return null;
        }



        /// <summary>
        /// Checks if there is a complement edge 
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool HasComplementEdge(GSMEdge edge)
        {
            return GetComplementEdge(edge) != null;
        }



        /// <summary>
        /// Deletes the given edge from the machine
        /// </summary>
        /// <param name="edge">Edge to remove</param>
        /// <exception cref="UnknownEdgeException">If the edge does not exist in the machine</exception>
        public void DeleteEdge(GSMEdge edge)
        {
            if (!edges.Contains(edge))
                throw new UnknownEdgeException("Tried to remove an edge which does not exist");

            edges.Remove(edge);
            GUI.changed = true;
        }



        /// <summary>
        /// Finds all edges which have the given state as origin. List may be empty
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>List with all edges having the given state as origin</returns>
        public List<GSMEdge> GetOutgoingEdges(GSMState state)
        {
            int id = state.id;
            List<GSMEdge> edges = new List<GSMEdge>();
            foreach (var edge in this.edges)
            {
                if (edge.originID == id)
                    edges.Add(edge);
            }
            return edges;
        }



        /// <summary>
        /// Finds all edges targeting the given state. List may be empty
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>List of edges targeting the given state</returns>
        public List<GSMEdge> GetIngoingEdges(GSMState state)
        {
            int id = state.id;
            List<GSMEdge> edges = new List<GSMEdge>();
            foreach (var edge in this.edges)
            {
                if (edge.targetID == id)
                    edges.Add(edge);
            }
            return edges;
        }


        /// <summary>
        /// Creates and inserts a new edge from state origin to state target. Both states need to exist in the statemachine first.
        /// </summary>
        /// <param name="origin">Origin state</param>
        /// <param name="target">Target state</param>
        /// <param name="onEdgePassed">Event getting called when edge is passed</param>
        /// <param name="insertStatesIfNotExisting">Inserts state if they are not existing in the machine</param>
        /// <exception cref="NullReferenceException">if one of the states is null</exception>
        /// <exception cref="UnknownStateException">if one of the states is not existing in the machine and <paramref name="insertStatesIfNotExisting"/> is false</exception>
        /// <returns></returns>
        public GSMEdge InsertEdgeBetween(GSMState origin, GSMState target, GSMEvent onEdgePassed = null, bool insertStatesIfNotExisting = false)
        {

            //all states which are not existing in this machine are automatically inserted
            if (insertStatesIfNotExisting)
            {
                if (!ContainsState(origin))
                    InsertState(origin);

                if (!ContainsState(target))
                    InsertState(target);
            }
            else
            {
                if (!ContainsState(origin))
                    throw new UnknownStateException(origin);

                if (!ContainsState(target))
                    throw new UnknownStateException(target);
            }

            return InsertEdgeBetween(origin.id, target.id, onEdgePassed);
            
        }

        /// <summary>
        /// <see cref="InsertEdgeBetween(GSMState, GSMState, UnityEvent, bool)"/>
        /// </summary>
        /// <param name="originID">Index of the origin state</param>
        /// <param name="targetID">Index of the target state</param>
        /// <param name="onEdgePassed">Event getting called when edge is passed</param>       
        /// <param name="insertStatesIfNotExisting">Inserts state if they are not existing in the machine</param>
        /// <exception cref="NullReferenceException">if one of the states is null</exception>
        /// <exception cref="UnknownStateException">if one of the states is not existing in the machine and <paramref name="insertStatesIfNotExisting"/> is false</exception>
        /// <exception cref="ArgumentException">if nodes with given indexes do not exist</exception>
        /// <returns>Inserted edge</returns>
        public GSMEdge InsertEdgeBetween(int originID, int targetID, GSMEvent onEdgePassed = null)
        {
            var target = GetState(targetID);
            var origin = GetState(originID);
            if (origin == null)
                throw new ArgumentException("There is no node with index " + originID);

            if (target == null)
                throw new ArgumentException("There is no node with index " + targetID);

            GSMEdge edge = new GSMEdge()
            {
                originID = originID,
                targetID = targetID,
                onEdgePassed = onEdgePassed,
                trigger = "goto "+target.name
            };

            /*if(originID == targetID)
            {
                Debug.LogWarning("Self-loops are not allowed.");
                return null;
            }*/

            if(ContainsEdge(edge))
            {
                Debug.LogWarning("Tried to insert edge which is already existing.");
                return null;
            }

            EditorUtility.SetDirty(this);
            edges.Add(edge);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return edge;
        }
        #endregion

        #region State insertion, deletion and management

        /// <summary>
        /// Finds the state with the given id and returns it
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>State with the given id</returns>
        public GSMState GetState(int id)
        {
            foreach (var state in states)
            {
                if (state.id == id)
                    return state;
            }
            return null;
        }



        /// <summary>
        /// Inserts the given state
        /// </summary>
        /// <param name="state">State to insert</param>
        /// <exception cref="StateAlreadyExistsException">if the given state already exists</exception>
        public GSMState InsertState(GSMState state)
        {

            EditorUtility.SetDirty(this);
            if (ContainsState(state))
                throw new StateAlreadyExistsException(state);

            state.id = FindUniqueID(state, state.GetHashCode());
            states.Add(state);

            if(!HasStartState)
            {
                SetStartState(state);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return state;
        }


        /// <summary>
        /// Deletes the given state from this machine. The state will be existing but there will be no connection in this machine
        /// </summary>
        /// <param name="state">The state to delete</param>
        /// <returns>True if deleting was successfull</returns>
        public bool DeleteState(GSMState state)
        {
            if (state == null)
            {
                Debug.LogWarning("Tried to delete a null state");
                return false;
            }

            if (!ContainsState(state))
            {
                Debug.LogWarning("Tried to delete a state which does not exist in this state machine");
                return false;
            }

            EditorUtility.SetDirty(this);
            foreach (var edge in GetOutgoingEdges(state)) edges.Remove(edge);
            foreach (var edge in GetIngoingEdges(state)) edges.Remove(edge);




            bool success = states.Remove(state);

            //if is start state
            if(startStateID == state.id)
            {
                if (states.Count == 0)
                {
                    Debug.Log("The start state was deleted. Make sure to mark a new state as start state.");
                    startStateID = -1;
                }
                else
                    SetStartState(states[0]);
            }

            state.id = -1;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GUI.changed = true;
            return success;
        }


        /// <summary>
        /// Creates a state you can insert into the machine without any conflict
        /// </summary>
        /// <param name="insertAfter">Automatically inserts the created state if set to true</param>
        /// <returns>The created state</returns>
        public GSMState CreateUniqueState(bool insertAfter = false)
        {
            int index = 0;
            string name;
            do
            {
                name = "New State " + index++;
            } while (ContainsStateName(name));

            GSMState state = new GSMState() { name = name };
            if (insertAfter)
                InsertState(state);
            return state;

        }



        /// <summary>
        /// Sets the given state as start state
        /// </summary>
        /// <param name="state">State to start with</param>
        public void SetStartState(GSMState state)
        {
            startStateID = state == null ? -1 : state.id;
        }



        private int FindUniqueID(GSMState state, int lastID)
        {
            if (ContainsStateID(lastID))
                return FindUniqueID(state, lastID + 1);
            return lastID;
        }

        #endregion

        #region Contains
        /// <summary>
        /// Checks if the given state is already existing in the machine
        /// </summary>
        /// <param name="state">State to proof</param>
        public bool ContainsState(GSMState state)
        {
            return states.Contains(state);
        }


        /// <summary>
        /// Checks if any state in this machine has this name
        /// </summary>
        /// <param name="name">Name to proof</param>
        /// <returns></returns>
        public bool ContainsStateName(string name)
        {
            foreach (var state in states)
            {
                if (state.name == name)
                    return true;
            }
            return false;
        }



        /// <summary>
        /// Checks if there is a state with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsStateID(int id)
        {
            foreach (var state in states)
            {
                if (state.id == id)
                    return true;
            }
            return false;
        }

        #endregion

        #region Validation
        internal bool Validate(out ValidationResult results)
        {
            results = new ValidationResult();
            bool ret = true;

            // Proof for startstate
            if(!HasStartState)
            {
                results.AddResult(ValidationResult.MISSING_START_STATE, "Your machine does not have a start state!", ValidationResult.WarnLevel.Fatal);
                ret = false;
            }

            // Search unreachable or absorbing states
            if (!hideAllWarningsEditor) foreach (var state in states)
                {
                    if (state.hideWarningsInConsole)
                        continue;

                    if (GetIngoingEdges(state).Count == 0 && startStateID != state.id)
                    {
                        results.AddResult(ValidationResult.UNREACHABLE_STATE, "State \"" + state.name + "\" is unreachable", ValidationResult.WarnLevel.Warn);
                    }

                    if (GetOutgoingEdges(state).Count == 0 && !state.isTerminating)
                    {
                        results.AddResult(ValidationResult.ABSORBING_STATE, "State \"" + state.name + "\" has no outgoing edges and therefore cannot be left", ValidationResult.WarnLevel.Warn);
                    }

                    if (state.isTerminating && GetOutgoingEdges(state).Count > 0)
                    {
                        results.AddResult(ValidationResult.UNNECESSARY_EDGE, "State \"" + state.name + "\" is terminating but has an outgoing edge", ValidationResult.WarnLevel.Warn);
                    }
                }



            // Search edges without trigger
            if (!hideAllWarningsEditor) foreach (var edge in edges)
            {
                if(edge.trigger == "" || edge.trigger == null)
                {
                    GSMState origin = GetState(edge.originID);
                    GSMState target = GetState(edge.targetID);
                    results.AddResult(ValidationResult.EMPTY_TRIGGER, "Edge between \"" + origin.name + "\" and \"" + target.name + 
                        "\" does not have a trigger. Edge cannot be passed", ValidationResult.WarnLevel.Warn);
                }
            }


            // Search for duplicate states
            HashSet<string> stateNames = new HashSet<string>();
            if (!hideAllWarningsEditor) foreach (var state in states)
            {
                if(stateNames.Contains(state.name))
                {
                    results.AddResult(ValidationResult.DUPLICATE_STATE_NAME, "There are more than one state with name \"" + state.name + ".", ValidationResult.WarnLevel.Warn);
                }
                stateNames.Add(state.name);
            }


            // Search for duplicate edges
            foreach (var state in states)
            {
                HashSet<string> edgeTriggers = new HashSet<string>();
                foreach (var edge in GetOutgoingEdges(state))
                {
                    if (edgeTriggers.Contains(edge.trigger))
                    {
                        results.AddResult(ValidationResult.DUPLICATE_TRIGGER, "State \""+state.name+"\" has multiple outgoing edges with trigger \""+edge.trigger+"\".", ValidationResult.WarnLevel.Fatal);
                        ret = false;
                    }
                    edgeTriggers.Add(edge.trigger);
                }
            }


            return ret;
        }


        public static implicit operator GraphicalStateMachine(GSMStateMachine original)
        {
            foreach (var machine in StateMachineProcessor.machines)
            {
                if (machine.Value.original == original)
                    return machine.Value;
            }
            return null;
        }




        #endregion
    }
}
