using BlobHandles;
using System.Text;

namespace OscCore.Demo
{
    public class MonitorToDebugText 
    {
        const int k_LineCount = 9;
        const int k_LastIndex = k_LineCount - 1;
        static readonly StringBuilder k_StringBuilder = new();
        
        public OscReceiver Receiver { get; }

        public event Action<string>? RecentValueText;

        private int m_ReplaceLineIndex;
        private bool m_Dirty;

        private readonly string[] m_ReceivedAsString = new string[k_LineCount];

        public MonitorToDebugText(OscReceiver receiver, out string localIp, out int port)
        {
            Receiver = receiver;
            localIp = Utils.GetLocalIpAddress();
            port = receiver.Port;
            receiver.Server?.AddMonitorCallback(Monitor);
        }

        public bool Update(out string output)
        {
            output = string.Empty;
            if (!m_Dirty)
                return false;

            output = BuildMultiLine();
            m_Dirty = false;
            return true;
        }

        private void Monitor(BlobString address, OscMessageValues values)
        {
            m_Dirty = true;

            if (m_ReplaceLineIndex == k_LastIndex)
            {
                for (int i = 0; i < k_LastIndex; i++)
                {
                    m_ReceivedAsString[i] = m_ReceivedAsString[i + 1];
                }
            }

            m_ReceivedAsString[m_ReplaceLineIndex] = Utils.MonitorMessageToString(address, values);
            
            if (m_ReplaceLineIndex < k_LastIndex) 
                m_ReplaceLineIndex++;
        }

        private string BuildMultiLine()
        {
            k_StringBuilder.Clear();
            for (int i = 0; i <= m_ReplaceLineIndex; i++)
            {
                k_StringBuilder.AppendLine(m_ReceivedAsString[i]);
                k_StringBuilder.AppendLine();
            }

            return k_StringBuilder.ToString();
        }
    }
}