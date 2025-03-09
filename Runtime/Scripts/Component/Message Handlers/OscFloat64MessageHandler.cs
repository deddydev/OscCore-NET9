namespace OscCore
{
    public class OscFloat64MessageHandler : OscMessageHandler<double>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadFloat64Element(0);
    }
}
