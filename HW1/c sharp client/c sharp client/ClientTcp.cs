using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace c_sharp_client
{
    public class ClientTcp : BaseClient
    {
        private TcpClient client;

        public ClientTcp()
        {
            Port = 13000;
            AddressString = "127.0.0.1";
            IsStopAndWait = false;
        }

        public ClientTcp(string address, string port, string bytesToRead, string isStopAndWait)
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
                client = new TcpClient(AddressString, Port);

                StartCommunication();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        protected override void StartCommunication()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                NetworkStream stream = client.GetStream();
                FileStream fileHandle = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.tsv"));
                string stopWaitMessage = string.Empty, okResponse = "ok";
                byte[] data = new byte[BlockSize], response = new byte[BlockSize];
                int bytesRead, bytesCount;

                while ((bytesRead = fileHandle.Read(data, 0, BlockSize)) != 0)
                {
                    stream.Write(data, 0, bytesRead);
                    IncrementSent(bytesRead);
                    if (IsStopAndWait)
                    {
                        bytesCount = stream.Read(response, 0, response.Length);
                        stopWaitMessage = Encoding.ASCII.GetString(response, 0, bytesCount);
                        if (stopWaitMessage.Trim() != okResponse)
                        {
                            Console.WriteLine("There was an error while communicating with the client. No acknowledgement received");
                            break;
                        }
                    }
                }

                stream.Close();

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
                client.Close();
                Console.WriteLine("Protocol used was: TCP \n Number of messages sent: {0} \n Number of bytes sent: {1} \n Time spent: {2} ms", this.GetSentMessages(), this.GetSentBytes(), stopWatch.ElapsedMilliseconds);
            }
        }

        public override void SendStopSignal()
        {
            try
            {
                byte[] data = new byte[4];
                data = Encoding.ASCII.GetBytes("stop");
                client = new TcpClient(AddressString, Port);
                NetworkStream stream = client.GetStream(); ;
                stream.Write(data, 0, data.Length);
                if (IsStopAndWait)
                    _ = stream.Read(data, 0, data.Length);

                stream.Close();
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
                client.Close();
            }

        }
    }
}
