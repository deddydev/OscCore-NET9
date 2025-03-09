namespace OscCore
{
    public class OscFloatMessageHandler : OscMessageHandler<float>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadFloatElement(0);
    }
}
