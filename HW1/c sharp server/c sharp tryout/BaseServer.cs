using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace c_sharp_server
{
    public abstract class BaseServer
    {
        public uint messagesRead, bytesRead;

        public int Port { get; protected set; }
        public string AddressString { get; protected set; }
        public IPAddress Address { get; protected set; }
        public bool IsStopAndWait { get; protected set; }

        protected abstract void CreateServer();

        public abstract void Communicate();
        
        protected void IncrementRead(int bytes)
        {
            messagesRead += 1;
            bytesRead += (uint)bytes;
        }

        public uint GetReadMessages() => messagesRead;

        public uint GetReadBytes() => bytesRead;
    }
}
