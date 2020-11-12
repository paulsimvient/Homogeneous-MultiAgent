using System;

namespace GSM
{
    public class StateAlreadyExistsException : Exception
    {
        public StateAlreadyExistsException(GSMState state) : base("State " + state.name + " already exists in the statemachine") { }
    }
}
