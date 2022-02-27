using System;
using System.Net;

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
                useNewParameters = Console.ReadLine().ToLower() == "yes" ? "true" : "false";
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
                BaseClient client = null;
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
}
