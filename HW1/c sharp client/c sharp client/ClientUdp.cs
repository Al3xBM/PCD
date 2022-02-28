using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace c_sharp_client
{
    class ClientUdp : BaseClient
    {
        private Socket client;
        private EndPoint serverEndpoint;

        public ClientUdp()
        {
            Port = 13000;
            AddressString = "127.0.0.1";
            BlockSize = 1024;
        }

        public ClientUdp(string address, string port, string bytesToRead, string isStopAndWait)
        {
            Port = int.Parse(port);
            AddressString = address;
            BlockSize = int.Parse(bytesToRead);
            IsStopAndWait = bool.Parse(isStopAndWait);
        }

        public override void Connect()
        {
            try
            {
                Address = IPAddress.Parse(AddressString);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                serverEndpoint = new IPEndPoint(Address, Port);

                StartCommunication();
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

        protected override void StartCommunication()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                FileStream fileHandle = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.tsv"));
                string responseMessage = string.Empty, okResponse = "ok";
                byte[] data = new byte[BlockSize], response = new byte[BlockSize];
                int bytesRead, bytesCount;

                while ((bytesRead = fileHandle.Read(data, 0, BlockSize)) != 0)
                {
                    client.SendTo(data, 0, bytesRead, SocketFlags.None, serverEndpoint);
                    IncrementSent(bytesRead);
                    if (IsStopAndWait)
                    {
                        bytesCount = client.ReceiveFrom(response, 0, BlockSize, SocketFlags.None, ref serverEndpoint);
                        responseMessage = Encoding.ASCII.GetString(response, 0, bytesCount);
                        if (responseMessage.Trim() != okResponse)
                        {
                            Console.Write("There was an error while communicating with the server. No acknowledgement received");
                            break;
                        }
                    }
                }

                data = Encoding.ASCII.GetBytes("print");
                client.SendTo(data, 0, data.Length, SocketFlags.None, serverEndpoint);
                if (IsStopAndWait)
                    _ = client.ReceiveFrom(data, 0, BlockSize, SocketFlags.None, ref serverEndpoint);
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
                Console.WriteLine("Done!");
                stopWatch.Stop();
                Console.WriteLine("Protocol used was: UDP \n Number of messages sent: {0} \n Number of bytes sent: {1} \n Time spent: {2}", this.GetSentMessages(), this.GetSentBytes(), stopWatch.ElapsedMilliseconds);
            }
        }

        public override void SendStopSignal()
        {
            byte[] data = Encoding.ASCII.GetBytes("stop");
            client.SendTo(data, 0, data.Length, SocketFlags.None, serverEndpoint);
            if (IsStopAndWait)
                _ = client.ReceiveFrom(data, 0, data.Length, SocketFlags.None, ref serverEndpoint);

            client.Close();
        }
    }
}
