using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace c_sharp_client
{
    public abstract class BaseClient
    {
        public uint messagesSent, bytesSent;
        protected int m_blockSize;

        public int BlockSize
        {
            get
            {
                return m_blockSize;
            }
            set
            {
                if (value < 0 || value > 65535)
                {
                    m_blockSize = 1024;
                    Console.WriteLine("BlockSize was set to default value. If you want to change it, use a proper value between 0 and 65535");
                }
                else
                    m_blockSize = value;
            }
        }
        public int Port { get; protected set; }
        public string AddressString { get; protected set; }
        public string ConnectionType { get; protected set; }
        public IPAddress Address { get; protected set; }
        public bool IsStopAndWait { get; set; }

        public abstract void Connect();
        protected abstract void StartCommunication();
        protected void IncrementSent(int bytes)
        {
            messagesSent += 1;
            bytesSent += (uint)bytes;
        }

        public uint GetSentMessages() => messagesSent;
        public uint GetSentBytes() => bytesSent;

        public abstract void SendStopSignal();
    }
}
