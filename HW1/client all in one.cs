using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace c_sharp_client
{
    class Program
    {
        static string[] HandleUserInput()
        {
            string[] returnValues = new string[5];
            bool verificationPassed = false;

            Console.WriteLine("You need to input some values to continue the execution of the program.");
            Console.WriteLine("First enter the address you'd like to use:");
            returnValues[0] = Console.ReadLine();
            verificationPassed = IPAddress.TryParse(returnValues[0], out _);
            while(!verificationPassed)
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
                returnValues[2] = Console.ReadLine().ToLower() == "yes" ? "true" : "false";
                verificationPassed = returnValues[2] == "yes" || returnValues[2] == "no";
            }
            returnValues[2] = returnValues[2] == "yes" ? "true" : "false";

            Console.WriteLine("What is the size of the messages that you want to send?");
            returnValues[3] = Console.ReadLine();
            verificationPassed = uint.TryParse(returnValues[3], out _);
            while (!verificationPassed)
            {
                Console.WriteLine("Please enter a valid size value!");
                returnValues[3] = Console.ReadLine();
                verificationPassed = uint.TryParse(returnValues[3], out _);
            }

            Console.WriteLine("What type of connection do you want to use, TCP or UDP?");
            returnValues[4] = Console.ReadLine().ToLower();
            verificationPassed = returnValues[4] == "tcp" || returnValues[4] == "udp";
            while (!verificationPassed)
            {
                Console.WriteLine("Please choose one of the two: TCP or UDP!");
                returnValues[4] = Console.ReadLine().ToLower();
                verificationPassed = returnValues[4] == "tcp" || returnValues[4] == "udp";
            }

            return returnValues;
        }

        static Tuple<bool, bool> HandleCommunicationEnded()
        {
            bool verificationPassed;
            string executeAgain, useNewParameters;

            Console.WriteLine("Communication with the server ended. Do you want to execute again? Answer with yes or no.");
            executeAgain = Console.ReadLine().ToLower();
            verificationPassed = executeAgain == "yes" || executeAgain == "no";
            while (!verificationPassed)
            {
                Console.WriteLine("Please type yes or no!");
                executeAgain = Console.ReadLine().ToLower() == "yes" ? "true" : "false";
                verificationPassed = executeAgain == "yes" || executeAgain == "no";
            }
            executeAgain = executeAgain == "yes" ? "true" : "false";
            if(executeAgain == "false")
                return  new Tuple<bool, bool>(bool.Parse(executeAgain), false);

            Console.WriteLine("Do you want to change the parameters? You can only change the message size and the communication type.");
            Console.WriteLine("Answer with yes or no.");
            useNewParameters = Console.ReadLine().ToLower();
            verificationPassed = useNewParameters == "yes" || useNewParameters == "no";
            while (!verificationPassed)
            {
                Console.WriteLine("Please type yes or no!");
                useNewParameters = Console.ReadLine().ToLower();
                verificationPassed = useNewParameters == "yes" || useNewParameters == "no";
            }
            useNewParameters = useNewParameters == "yes" ? "true" : "false";

            return new Tuple<bool, bool>(bool.Parse(executeAgain), bool.Parse(useNewParameters));
        }

        static string[] HandleUserInputSecondary()
        {
            string[] returnValues = new string[2];
            bool verificationPassed;

            Console.WriteLine("What is the size of the messages that you want to send?");
            returnValues[0] = Console.ReadLine();
            verificationPassed = uint.TryParse(returnValues[0], out _);
            while (!verificationPassed)
            {
                Console.WriteLine("Please enter a valid size value!");
                returnValues[0] = Console.ReadLine();
                verificationPassed = uint.TryParse(returnValues[0], out _);
            }

            return returnValues;
        }

        static void Main(string[] args)
        {
            string[] userInput = args;
            if (userInput.Length != 5)
                userInput = HandleUserInput();

            Console.WriteLine("Received as input {0} {1} {2} {3} {4}", userInput[0], userInput[1], userInput[2], userInput[3], userInput[4]);
            string connectionType = userInput[4], blockSize = userInput[3], stopWait = userInput[2], port = userInput[1], address = userInput[0];

            while(true)
            {
                dynamic client = null;
                if (connectionType == "tcp")
                    client = new ClientTcp(address, port, blockSize, stopWait);
                else
                    client = new ClientUdp(address, port, blockSize, stopWait);

                client.Connect();

                Tuple<bool, bool> continueCommunication = HandleCommunicationEnded();
                if(continueCommunication.Item1)
                {
                    if(continueCommunication.Item2)
                    {
                        string[] newInput = HandleUserInputSecondary();
                        blockSize = newInput[0];
                    }
                }
                else
                {
                    client.SendStopSignal();
                    break;
                }
            }
        }
    }

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
                    if(IsStopAndWait)
                    {
                        bytesCount = client.ReceiveFrom(response, 0, BlockSize, SocketFlags.None, ref serverEndpoint);
                        responseMessage = Encoding.ASCII.GetString(response, 0, bytesCount);
                        if(responseMessage.Trim() != okResponse)
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
            catch(SocketException e)
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

                while((bytesRead = fileHandle.Read(data, 0, BlockSize)) != 0)
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