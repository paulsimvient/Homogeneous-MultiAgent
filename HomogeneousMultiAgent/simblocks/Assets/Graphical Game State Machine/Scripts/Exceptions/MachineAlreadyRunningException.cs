using System;

namespace GSM
{
    public class MachineAlreadyRunningException : Exception
    {

        public MachineAlreadyRunningException(GSMStateMachine machine) : base("Could not start machine \""+machine.machineName + "\". Machine already running.")
        {

        }

    }
}
