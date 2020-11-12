using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace GSM
{
    [Serializable]
    public class GSMEdge : IInspectable
    {
        public GSMEvent onEdgePassed;
        public int originID = -1;
        public int targetID = -1;
        public string trigger = "";
        public int callbackInvokationOrder = 0;

        public void FindEventObjectReferences()
        {
            onEdgePassed.FindCallbackObjectReferences();
        }

        public bool IsSelfLoop
        {
            get
            {
                return originID == targetID && originID != -1;
            }
        }

        #region equals and hashcode
        public override bool Equals(object obj)
        {
            return obj is GSMEdge edge &&
                   originID == edge.originID &&
                   targetID == edge.targetID;
        }

        public override int GetHashCode()
        {
            var hashCode = -2119527993;
            hashCode = hashCode * -1521134295 + originID.GetHashCode();
            hashCode = hashCode * -1521134295 + targetID.GetHashCode();
            return hashCode;
        }
        #endregion
    }
}
