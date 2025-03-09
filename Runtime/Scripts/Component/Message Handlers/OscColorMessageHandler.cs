using System.Drawing;

namespace OscCore
{
    public class OscColorMessageHandler : OscMessageHandler<Color32>
    {
        protected override void ValueRead(OscMessageValues values)
            => m_Value = values.ReadColor32Element(0);
    }
}
