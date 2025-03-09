using System.Net;
using System.Net.Sockets;

namespace OscCore
{
    sealed class OscSocket : IDisposable
    {
        readonly Socket m_Socket;
        readonly Thread m_Thread;
        bool m_Disposed;
        bool m_Started;

        public int Port { get; }
        public OscServer? Server { get; set; }
        
        public OscSocket(int port)
        {
            Port = port;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = int.MaxValue };
            m_Thread = new Thread(Serve);
        }

        public void Start()
        {
            // make sure redundant calls don't do anything after the first
            if (m_Started)
                return;
            
            m_Disposed = false;
            if (!m_Socket.IsBound)
                m_Socket.Bind(new IPEndPoint(IPAddress.Any, Port));

            m_Thread.Start();
            m_Started = true;
        }
        
        private void Serve()
        {
            var buffer = Server?.Parser?.Buffer;
            if (buffer == null)
                return;

            var socket = m_Socket;
            
            while (!m_Disposed)
            {
                try
                {
                    // it's probably better to let Receive() block the thread than test socket.Available > 0 constantly
                    int receivedByteCount = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    if (receivedByteCount == 0)
                        continue;

                    Server?.ParseBuffer(receivedByteCount);

                    //Profiler.EndSample();
                }
                // a read timeout can result in a socket exception, should just be ok to ignore
                catch (SocketException) { }
                catch (ThreadAbortException) {}
                catch (Exception)
                {
                    //if (!m_Disposed)
                    //    Debug.LogException(e); 
                    break;
                }
            }
            
            //Profiler.EndThreadProfiling();
        }

        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Socket.Close();
            m_Socket.Dispose();
            m_Disposed = true;
        }
    }
}