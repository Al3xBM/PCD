using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_sharp_server
{
    public class UdpServer : BaseServer
    {
        private UdpClient server;

        public UdpServer()
        {
            Port = 13000;
            AddressString = "127.0.0.1";
            Address = IPAddress.Parse(AddressString);
            IsStopAndWait = false;

            CreateServer();
        }

        public UdpServer(string address, string port, string isStopAndWait)
        {
            Port = int.Parse(port);
            AddressString = address;
            Address = IPAddress.Parse(AddressString);
            IsStopAndWait = bool.Parse(isStopAndWait);

            CreateServer();
        }

        protected override void CreateServer()
        {
            try
            {
                server = new UdpClient(Port);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        public override void Communicate()
        {
            try
            {
                IPEndPoint recFrom = new IPEndPoint(IPAddress.Any, Port);
                string data = string.Empty, okResponse = "ok";
                byte[] bytes = new byte[65535], okResponseBytes = Encoding.ASCII.GetBytes(okResponse);

                Console.WriteLine("Waiting for connection...");

                while (true)
                {
                    bytes = server.Receive(ref recFrom);
                    IncrementRead(bytes.Count(x => x != '\0'));
                    if (IsStopAndWait)
                        server.Send(okResponseBytes, okResponseBytes.Length, recFrom);

                    data = Encoding.ASCII.GetString(bytes, 0, bytes.Count());

                    if (data.Trim().ToLower() == "stop")
                        break;

                    if (data.Trim().ToLower() == "print")
                    {
                        Console.WriteLine("Protocol used was: UDP \n Number of messages read: {0} \n Number of bytes read: {1}", this.GetReadMessages(), this.GetReadBytes());
                        ResetCounters();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                Console.WriteLine("Protocol used was: UDP \n Number of messages read: {0} \n Number of bytes read: {1}", this.GetReadMessages(), this.GetReadBytes());
                server.Close();
                Console.WriteLine("Done!");
            }
        }
    }
}
