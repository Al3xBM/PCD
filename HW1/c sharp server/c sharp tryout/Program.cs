using System;
using System.Net;

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
                BaseServer server = null;
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
}
