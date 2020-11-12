using System;

namespace GSM
{
    public class UnknownStateException : Exception
    {
        public UnknownStateException() : base("Tried to create an edge from or to a node which is not in the statetmachine. Insert node first")
        {

        }

        public UnknownStateException(string msg) : base(msg) { }

        public UnknownStateException(GSMState node) : base("Node " + node.name + " is not in the statemachine. Insert node first")
        {

        }
    }
}
