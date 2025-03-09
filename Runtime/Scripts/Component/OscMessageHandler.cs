namespace OscCore
{
    public abstract class MessageHandlerBase
    {
        protected OscReceiver? m_Receiver;
        public OscReceiver? Receiver
        {
            get => m_Receiver;
            set => m_Receiver = value;
        }

        protected string m_Address = "/";
        public string Address => m_Address;
        
        protected OscActionPair? m_ActionPair;
        protected bool m_Registered;
        
        public void OnEnable()
        {
            if (m_Registered || string.IsNullOrEmpty(Address))
                return;

            if (m_Receiver != null && m_Receiver.Server != null)
            {
                m_ActionPair = new OscActionPair(ValueRead, InvokeEvent);
                m_Receiver.Server.TryAddMethodPair(Address, m_ActionPair);
                m_Registered = true;
            }
        }

        public void OnDisable()
        {
            m_Registered = false;
            if (m_ActionPair != null)
                m_Receiver?.Server?.RemoveMethodPair(Address, m_ActionPair);
        }
        
        public void OnValidate()
        {
            Utils.ValidateAddress(ref m_Address);
        }

        protected abstract void InvokeEvent();
    
        protected abstract void ValueRead(OscMessageValues values);

        // Empty update method here so the component gets an enable checkbox
        protected virtual void Update() { }
    }
    
    public abstract class OscMessageHandler<T> : MessageHandlerBase
    {
        public event Action<T?>? OnMessageReceived;
        
        protected T? m_Value;

        protected override void InvokeEvent()
            => OnMessageReceived?.Invoke(m_Value);
    }
}