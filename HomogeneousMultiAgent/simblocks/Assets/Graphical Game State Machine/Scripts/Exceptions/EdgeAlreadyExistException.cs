using System;

namespace GSM {
    public class EdgeAlreadyExistException : Exception
    {
        public EdgeAlreadyExistException(GSMEdge _) : base("Edge already exists") { }
    }
}
