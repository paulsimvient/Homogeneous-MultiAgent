using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSM
{

    [Serializable]
    public class GSMState : IInspectable, IDraggable
    {
        public const int UpdateTypeUpdate = 0;
        public const int UpdateTypeLateUpdate = 1;
        public const int UpdateTypeFixedUpdate = 2;

        #region State machine
        public string name;
        public int id;
        public bool hideWarningsInEditor;
        public bool hideWarningsInConsole;
        public int updateType = 0;
        public int callbackInvokationOrder = 0;
        public bool isTerminating;

        [SerializeField] internal Rect bounds;

        public GSMEvent onStateEntered;
        public GSMEvent onStateLeft;
        public GSMEvent onStateStay;
        public GSMEvent onStateSetActive;

        #endregion

        public Vector2 Move(Vector2 delta)
        {
            bounds = bounds.Move(delta);
            return bounds.position;
        }

        public void FindEventObjectReferences()
        {
            onStateEntered.FindCallbackObjectReferences();
            onStateLeft.FindCallbackObjectReferences();
            onStateStay.FindCallbackObjectReferences();
            onStateSetActive.FindCallbackObjectReferences();
        }

        #region equals and hashcode
        public override bool Equals(object obj)
        {
            return obj is GSMState state &&
                   id == state.id;
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<int>.Default.GetHashCode(id);
        }

        #endregion
    }
}
