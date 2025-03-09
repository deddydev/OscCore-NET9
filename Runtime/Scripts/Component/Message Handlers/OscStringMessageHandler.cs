namespace OscCore
{
    public class OscStringMessageHandler : OscMessageHandler<string>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadStringElement(0);
    }
}
