using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_sharp_server
{
    public class TcpServer : BaseServer
    {
        private TcpListener server = null;

        public TcpServer() 
        {
            Port = 13000;
            AddressString = "127.0.0.1";
            Address = IPAddress.Parse(AddressString);
            IsStopAndWait = false;
            
            CreateServer();
        }

        public TcpServer(string address, string port, string isStopAndWait)
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
                server = new TcpListener(Address, Port);
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
                server.Start();
                string data = string.Empty, stopWaitMessage = string.Empty, okResponse = "ok";
                byte[] bytes = new byte[65535], okResponseBytes = Encoding.ASCII.GetBytes(okResponse);
                int bytesCount;
                bool endComm = false;

                while(true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    while ((bytesCount = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        IncrementRead(bytes.Length);
                        if (IsStopAndWait)
                            stream.Write(okResponseBytes, 0, okResponseBytes.Length); 

                        data = Encoding.ASCII.GetString(bytes, 0, bytesCount);
                        if (data.Trim().ToLower() == "stop")
                        {
                            endComm = true;
                            break;
                        }

                        /* data = data.ToUpper();
                        byte[] response = Encoding.ASCII.GetBytes(data);

                        stream.Write(response, 0, response.Length);
                        IncrementSent(response.Length);
                        if (IsStopAndWait)
                        {
                            bytesCount = stream.Read(okResponseBytes, 0, okResponseBytes.Length);
                            IncrementRead(okResponseBytes.Length);
                            stopWaitMessage = Encoding.ASCII.GetString(okResponseBytes, 0, okResponseBytes.Length);
                            if (stopWaitMessage.Trim() != okResponse)
                            {
                                Console.Write("There was an error while communicating with the client. No acknowledgement received");
                                break;
                            }
                        }*/
                    }

                    Console.WriteLine("Protocol used was: TCP \n Number of messages read: {1} \n Number of bytes read: {2}", this.GetReadMessages(), this.GetReadBytes());
                    client.Close();

                    if (endComm)
                        break;
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
                server.Stop();
                Console.WriteLine("Done!");
            }
        }
    }
}
