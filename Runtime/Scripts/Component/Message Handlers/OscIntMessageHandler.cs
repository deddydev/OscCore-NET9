namespace OscCore
{
    public class OscIntMessageHandler : OscMessageHandler<int>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadIntElement(0);
    }
}
