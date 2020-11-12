using System;

namespace GSM
{
    public class UnknownEdgeException : Exception
    {
        public UnknownEdgeException(string msg) : base(msg) { }
    }
}
