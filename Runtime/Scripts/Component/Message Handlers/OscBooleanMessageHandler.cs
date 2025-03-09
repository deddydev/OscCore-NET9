namespace OscCore
{
    public class OscBooleanMessageHandler : OscMessageHandler<bool>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadBooleanElement(0);
    }
}
