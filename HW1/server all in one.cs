using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace c_sharp_server
{
    class Program
    {
        static string[] HandleUserInput()
        {
            string[] returnValues = new string[4];
            bool verificationPassed = false;

            Console.WriteLine("You need to input some values to continue the execution of the program.");
            Console.WriteLine("First enter the address you'd like to use:");
            returnValues[0] = Console.ReadLine();
            verificationPassed = IPAddress.TryParse(returnValues[0], out _);
            while (!verificationPassed)
            {
                Console.WriteLine("Please enter a valid address!");
                returnValues[0] = Console.ReadLine();
                verificationPassed = IPAddress.TryParse(returnValues[0], out _);
            }

            Console.WriteLine("What port do you want to use?");
            returnValues[1] = Console.ReadLine();
            verificationPassed = int.TryParse(returnValues[1], out _);
            while (!verificationPassed)
            {
                Console.WriteLine("Please enter a valid port number!");
                returnValues[1] = Console.ReadLine();
                verificationPassed = int.TryParse(returnValues[1], out _);
            }

            Console.WriteLine("Do you want the communication to be stop-and-wait? Answer with yes or no.");
            returnValues[2] = Console.ReadLine().ToLower();
            verificationPassed = returnValues[2] == "yes" || returnValues[2] == "no";
            while (!verificationPassed)
            {
                Console.WriteLine("Please type yes or no!");
                returnValues[2] = Console.ReadLine().ToLower();
                verificationPassed = returnValues[2] == "yes" || returnValues[2] == "no";
            }
            returnValues[2] = returnValues[2] == "yes" ? "true" : "false";

            Console.WriteLine("What type of connection do you want to use, TCP or UDP?");
            returnValues[3] = Console.ReadLine().ToLower();
            verificationPassed = returnValues[3] == "tcp" || returnValues[3] == "udp";
            while (!verificationPassed)
            {
                Console.WriteLine("Please choose one of the two: TCP or UDP!");
                returnValues[3] = Console.ReadLine().ToLower();
                verificationPassed = returnValues[3] == "tcp" || returnValues[3] == "udp";
            }

            return returnValues;
        }

        static bool HandleCommunicationEnded()
        {
            bool verificationPassed;
            string executeAgain;

            Console.WriteLine("Communication with the client ended. Do you want to execute again? Answer with yes or no.");
            executeAgain = Console.ReadLine().ToLower();
            verificationPassed = executeAgain == "yes" || executeAgain == "no";
            while (!verificationPassed)
            {
                Console.WriteLine("Please type yes or no!");
                executeAgain = Console.ReadLine().ToLower();
                verificationPassed = executeAgain == "yes" || executeAgain == "no";
            }
            executeAgain = executeAgain == "yes" ? "true" : "false";

            return bool.Parse(executeAgain);
        }

        static void Main(string[] args)
        {
            string[] userInput = args;
            if (userInput.Length != 4)
                userInput = HandleUserInput();

            Console.WriteLine("Received as input {0} {1} {2} {3}", userInput[0], userInput[1], userInput[2], userInput[3]);
            string connectionType = userInput[3], stopWait = userInput[2], port = userInput[1], address = userInput[0];

            while (true)
            {
                dynamic server = null;
                if (connectionType == "tcp")
                    server = new TcpServer(address, port, stopWait);
                else
                    server = new UdpServer(address, port, stopWait);

                server.Communicate();

                bool continueCommunication = HandleCommunicationEnded();
                if (!continueCommunication)
                    break;
            }
        }
    }

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

        protected void ResetCounters()
        {
            messagesRead = 0;
            bytesRead = 0;
        }
    }

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
                string data = string.Empty, okResponse = "ok";
                byte[] bytes = new byte[65535], okResponseBytes = Encoding.ASCII.GetBytes(okResponse);
                int bytesCount;
                bool endComm = false;

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    ResetCounters();

                    while ((bytesCount = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        IncrementRead(bytesCount);
                        if (IsStopAndWait)
                            stream.Write(okResponseBytes, 0, okResponseBytes.Length);

                        data = Encoding.ASCII.GetString(bytes, 0, bytesCount);
                        if (data.Trim().ToLower() == "stop")
                        {
                            endComm = true;
                            break;
                        }
                    }

                    Console.WriteLine("Protocol used was: TCP \n Number of messages read: {0} \n Number of bytes read: {1}", this.GetReadMessages(), this.GetReadBytes());
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