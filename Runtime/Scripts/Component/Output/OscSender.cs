using System.Net;

namespace OscCore
{
    public class OscSender
    {
        private string m_IpAddress = "127.0.0.1";
        private int m_Port = 7000;
        private OscClient? m_Client;

        /// <summary>The IP address to send to</summary>
        public string IpAddress
        {
            get => m_IpAddress;
            set
            {
                if (IPAddress.TryParse(value, out var ip))
                {
                    m_IpAddress = value;
                    Reset();
                }
            }
        }

        /// <summary>The port on the remote IP to send to</summary>
        public int Port
        {
            get => m_Port;
            set
            {
                m_Port = value.ClampPort();
                Reset();
            }
        }

        /// <summary>
        /// Handles serializing and sending messages.  Use methods on this to send messages to the endpoint.
        /// </summary>
        public OscClient? Client => m_Client ??= new OscClient(m_IpAddress, m_Port);

        public void Reset()
            => m_Client = null;
    }
}

