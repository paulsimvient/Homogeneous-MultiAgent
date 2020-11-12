using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSM
{



    public class StateMachineProcessor : MonoBehaviour
    {
        internal static Dictionary<string, GraphicalStateMachine> machines = new Dictionary<string, GraphicalStateMachine>();


        /// <summary>
        /// Finds a machine by its name and returns it
        /// </summary>
        /// <param name="name">Name of the machine to find</param>
        /// <returns>Machine with the given name. Null if machine is not found</returns>
        public static GraphicalStateMachine GetMachine(string name)
        {
            if (machines.ContainsKey(name))
            {
                return machines[name];
            }
            return null;
        }


        internal static bool AddMachine(string name, GraphicalStateMachine machine)
        {
            if (machines.ContainsKey(name))
                return false;

            machines.Add(name, machine);
            return true;
        }

        /// <summary>
        /// Checks if a machine is running
        /// </summary>
        /// <param name="name">Name of the machine, you want to proof running</param>
        /// <returns>True if the given machine is running</returns>
        public static bool IsMachineRunning(string name)
        {
            if (machines.ContainsKey(name))
                return machines[name].IsRunning;
            return false;
        }


        /// <summary>
        /// Returns all currentyl running machines.
        /// </summary>
        public static List<GraphicalStateMachine> RunningMachines
        {
            get
            {
                List<GraphicalStateMachine> machines = new List<GraphicalStateMachine>();
                foreach(var machine in StateMachineProcessor.machines.Values)
                {
                    if (machine.IsRunning)
                        machines.Add(machine);
                }
                return machines;
            }
        }

#pragma warning disable 0649
        [SerializeField] internal GSMStateMachine stateMachine;
#pragma warning restore 0649


        [HideInInspector] public GraphicalStateMachine Machine { get; private set; }


        /// <summary>
        /// Currently active state.
        /// May be null if machine is not running
        /// </summary>
        public GraphicalState ActiveState { get { return Machine.ActiveState; } }

        /// <summary>
        /// Wraps <see cref="GraphicalStateMachine.IsRunning"/>
        /// </summary>
        public bool IsRunning { get { return Machine == null ? false : Machine.IsRunning; } }


#pragma warning disable 0051


        void Awake()
        {

            //If two or more state machine processors refer to the same machine, dont start again but get reference
            bool isMachineExisting = machines.ContainsKey(stateMachine.machineName);
            if(isMachineExisting)
            {
                Machine = GetMachine(stateMachine.machineName);
                return;
            }



            Machine = new GraphicalStateMachine(stateMachine);
            if (!stateMachine.startMachineOnAwake)
                return;

            if (stateMachine == null)
            {
                Debug.LogError("There is no machine attached to this behaviour");
                return;
            }
            bool startMachine = Machine.Validate(out ValidationResult results);

            results.PrintResults();
            if (!startMachine)
                Debug.LogWarning("Machine \"" + stateMachine.machineName + "\" could not be started");

            if (startMachine)
            {
                if (Machine.SaveActiveState)
                    Machine.StateActivationReason = GraphicalStateMachine.ActivationReason.GameStarted;
                StartMachine();
            }
        }

        void OnDisable()
        {
            StopMachine();
        }


        void Update()
        {
            if (Machine != null && Machine.IsRunning)
            {
                var state = Machine.ActiveState;
                if (state.UpdateType == GraphicalStateMachine.StateStayUpdateType.Update)
                {
                    Machine.ActiveState.OnStateStay();
                }
            }
        }

        void FixedUpdate()
        {
            if (Machine != null && Machine.IsRunning)
            {
                var state = Machine.ActiveState;
                if (state.UpdateType == GraphicalStateMachine.StateStayUpdateType.FixedUpdate)
                {
                    Machine.ActiveState.OnStateStay();
                }
            }
        }

        void LateUpdate()
        {
            if (Machine != null && Machine.IsRunning)
            {
                var state = Machine.ActiveState;
                if(state.UpdateType == GraphicalStateMachine.StateStayUpdateType.LateUpdate)
                {
                    Machine.ActiveState.OnStateStay();
                }
            }
        }


#pragma warning restore 0051

        /// <summary>
        /// Send a trigger to the machine.
        /// If the active state has an outgoing edge with the given trigger the edge will be used
        /// </summary>
        /// <param name="trigger">Trigger to send</param>
        /// <returns>True if there was a state change</returns>
        public bool SendTrigger(string trigger)
        {
            if (Machine != null)
            {
                return Machine.SendTrigger(trigger); ;
            }
            Debug.LogWarning("No machine is set. Cannot send trigger");
            return false;
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
            if (Machine != null)
                return Machine.SendTrigger(trigger, out newState);
            newState = null;
            Debug.LogWarning("No machine is set. Cannot send trigger");
            return false;
        }



        /// <summary>
        /// Sends a trigger but does not return any result.
        /// Can be used for Button-events
        /// </summary>
        /// <param name="trigger"></param>
        public void SendTriggerBlind(string trigger)
        {
            Machine.SendTrigger(trigger);
        }



        /// <summary>
        /// Sends a trigger to the machine after a given delay
        /// If the active state has an outgoing edge with the given trigger the edge will be used.
        /// If the active states from another source while within the delay, the trigger will be sent anyways
        /// </summary>
        /// <param name="trigger">Trigger to send</param>
        /// <param name="sec">Delay time in seconds</param>
        public void SendTriggerDelayed(string trigger, float sec)
        {
            StartCoroutine(TriggerDelayed(trigger, sec));
        }

        private IEnumerator TriggerDelayed(string trigger, float sec)
        {
            yield return new WaitForSeconds(sec);
                SendTrigger(trigger);
        }


        /// <summary>
        /// Sets the currently active state. Make sure to have unique state names before calling this method.
        /// </summary>
        /// <exception cref="UnknownStateException">If the state was not found</exception>
        /// <param name="name">Name of the state</param>
        /// <returns>State which was set active</returns>
        public GraphicalState SetActiveState(string name)
        {
            if (Machine != null)
                return Machine.SetActiveState(name);
            Debug.LogWarning("No machine is set. Cannot set active state");
            return null;
        }



        /// <summary>
        /// Starts the machine
        /// </summary>
        public bool StartMachine()
        {
            return Machine.Start();
        }


        /// <summary>
        /// Stops the machine
        /// </summary>
        public void StopMachine()
        {
            Machine?.Stop();
        }

    }

}