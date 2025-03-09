namespace OscCore
{
    public class OscInt64MessageHandler : OscMessageHandler<long>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadInt64Element(0);
    }
}
