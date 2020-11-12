namespace GSM
{
    public class RuntimeCallback
    {
        public delegate void RuntimeEventDelegate();

        /// <summary>
        /// Callback which is going to be invoked when the event is triggered.
        /// </summary>
        public RuntimeEventDelegate onEventCalled;

        /// <summary>
        /// Type which determines when the callback is going to be invoked
        /// </summary>
        public GraphicalStateMachine.RuntimeCallbackType Type { get; private set; }


        /// <summary>
        /// Creates new <see cref="RuntimeCallback"/>
        /// </summary>
        /// <param name="type">Type of callback. Use this to determine when to call the event</param>
        /// <param name="callback">Callback to invoke</param>
        public RuntimeCallback(GraphicalStateMachine.RuntimeCallbackType type, RuntimeEventDelegate callback)
        {
            this.Type = type;
            onEventCalled = callback;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        public bool Invoke()
        {
            if(onEventCalled != null)
            {
                onEventCalled.Invoke();
                return true;
            }
            return false;
        }
    }
}
