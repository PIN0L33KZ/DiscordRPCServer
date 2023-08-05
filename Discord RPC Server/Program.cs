using System.Net.NetworkInformation;
using System.Reflection;
using DiscordRPC;
using System;

namespace Discord_RPC_Server
{
    internal class Program
    {
        private static DiscordRpcClient _client = new DiscordRpcClient("0");
        private static readonly Logger _logger = new Logger();

        private static void Main(string[] args)
        {
            PrintWelcomeScreen();

            //Handle arguments
            ArgumentHandler(args);

            while(true)
            {
                PrintUserInput();

                string userInput = _logger.ReadLine().ToLower().Trim();

                if(userInput == "") //Skip iteration if user input is empty.
                {
                    continue;
                }

                switch(userInput)
                {
                    case "setup":
                        Setup();
                        break;

                    case "start":
                        StartServer();
                        break;

                    case "stop":
                        StopServer();
                        break;

                    case "refresh":
                        Refresh();
                        break;

                    case "reset":
                        Reset();
                        break;

                    case "clear":
                        PrintWelcomeScreen();
                        break;

                    case "log":
                        SaveLog();
                        break;

                    case "clr":
                        PrintWelcomeScreen();
                        break;

                    case "exit":
                        Environment.Exit(0);
                        break;

                    case "help":
                        _logger.WriteLine(Properties.Resources.HelpMenu);
                        _logger.WriteLine();
                        break;

                    case "?":
                        _logger.WriteLine(Properties.Resources.HelpMenu);
                        _logger.WriteLine();
                        break;

                    default:
                        PrintError("Unknown command, use help or ? to get all valid commands.");
                        break;
                }
            }

        }

        private static void ArgumentHandler(string[] args)
        {
            if(args.Length == 0)
            {
                PrintInformation("No arguments detected.");
                return;
            }

            //The following code will only be executed if arguments where given.

            PrintInformation("Argument detected.");
            PrintInformation("Argument \"" + args[0] + "\" will be processed.");

            switch(args[0])
            {
                case "autostart":
                    PrintInformation($"According to your argument, the rpc server will start now.");
                    StartServer();
                    break;
                
                case "reset":
                    PrintInformation($"According to your argument, this application will clear all settings.");
                    Reset();
                    break;

                default:
                    PrintError($"Argument \"{args[0]}\" is invalid or unknown and won't be processed any further.");
                    break;
            }
        }

        private static void StartServer()
        {
            if(Properties.Settings.Default.ApplicationID == "" || Properties.Settings.Default.Details == "")
            {
                PrintError("You need to run \"setup\" before. You need to specify the application id and details at least.");
                return;
            }

            if(_client.IsInitialized)
            {
                PrintError("The server is alerady runnning.");
                return;
            }

            //Check if this machine is online.
            if(!MachineOnline())
            {
                PrintError("It seems like you're offline. Can't start the server under this circumstances.");
                return;
            }

            //This code will only be executed if the server is not running.
            _client = new DiscordRpcClient(Properties.Settings.Default.ApplicationID);

            if(Properties.Settings.Default.Timer)
            {
                //Calculate unix timestamp for timer.
                double unixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                Timestamps timeStamp = new Timestamps() { StartUnixMilliseconds = Convert.ToUInt64(unixTime) };

                _client.SetPresence
                (
                new RichPresence()
                {
                    Details = Properties.Settings.Default.Details,
                    State = Properties.Settings.Default.State,
                    Timestamps = timeStamp,

                    Assets = new Assets()
                    {
                        LargeImageKey = Properties.Settings.Default.LargeImageKey,
                        LargeImageText = Properties.Settings.Default.LargeImageText,

                        SmallImageKey = Properties.Settings.Default.SmallImageKey,
                        SmallImageText = Properties.Settings.Default.SmallImageText
                    }
                }
                );
            }
            else
            {
                _client.SetPresence
                (
                new RichPresence()
                {
                    Details = Properties.Settings.Default.Details,
                    State = Properties.Settings.Default.State,

                    Assets = new Assets()
                    {
                        LargeImageKey = Properties.Settings.Default.LargeImageKey,
                        LargeImageText = Properties.Settings.Default.LargeImageText,

                        SmallImageKey = Properties.Settings.Default.SmallImageKey,
                        SmallImageText = Properties.Settings.Default.SmallImageText
                    }
                }
                );
            }


            try
            {
                _ = _client.Initialize();

                PrintSuccess("Server started.");
                PrintInformation("Your richpresence should be visible trough this discord client now.");
                PrintInformation("If you're unable to see the richpresence, check if your discord client is running.");
            }
            catch(Exception e)
            {
                PrintError(e.Message);
            }
        }

        private static void StopServer()
        {
            if(_client.IsInitialized)
            {
                _client.Dispose();
                PrintSuccess("Server stopped.");
            }
            else
            {
                PrintError("Server is not running.");
            }
        }

        private static void Setup()
        {
            PrintInformation("Setup will start now.");
            PrintInformation("If you don't want to include specific things in your presence just let them empty.");

            while(true)
            {
                string userInput;

                #region Setup application id
                if(Properties.Settings.Default.ApplicationID == "")
                {
                    PrintQuestion("What's your application id? See https://discord.com/developers for further assistence.");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "")
                    {
                        PrintError("Your application id cannot be empty.");
                        continue;
                    }

                    Properties.Settings.Default.ApplicationID = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"Your application id is currently set to \"{Properties.Settings.Default.ApplicationID}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        Properties.Settings.Default.ApplicationID = "";
                        continue;
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup details
                if(Properties.Settings.Default.Details == "")
                {
                    PrintQuestion("What sould be the first text after the app title?");
                    PrintUserInput();

                    userInput = _logger.ReadLine().Trim();

                    Properties.Settings.Default.Details = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"Details are alerady set to \"{Properties.Settings.Default.Details}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        PrintQuestion("What sould be the first text after the app title?");
                        PrintUserInput();

                        userInput = _logger.ReadLine().Trim();

                        Properties.Settings.Default.Details = userInput;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup state
                if(Properties.Settings.Default.State == "")
                {
                    PrintQuestion("What sould be the second text after the app title?");
                    PrintUserInput();

                    userInput = _logger.ReadLine().Trim();

                    Properties.Settings.Default.State = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"State is are alerady set to \"{Properties.Settings.Default.State}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        PrintQuestion("What sould be the second text after the app title?");
                        PrintUserInput();

                        userInput = _logger.ReadLine().Trim();

                        Properties.Settings.Default.State = userInput;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup large image key
                if(Properties.Settings.Default.LargeImageKey == "")
                {
                    PrintQuestion("What sould the large image be?");
                    PrintUserInput();

                    userInput = _logger.ReadLine().Trim();

                    Properties.Settings.Default.LargeImageKey = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"The large image is are alerady set to \"{Properties.Settings.Default.LargeImageKey}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        PrintQuestion("What sould the large image be?");
                        PrintUserInput();

                        userInput = _logger.ReadLine().Trim();

                        Properties.Settings.Default.LargeImageKey = userInput;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup large image text
                if(Properties.Settings.Default.LargeImageText == "")
                {
                    PrintQuestion("What sould the text be, if you hover over the large image?");
                    PrintUserInput();

                    userInput = _logger.ReadLine().Trim();

                    Properties.Settings.Default.LargeImageText = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"The text is are alerady set to \"{Properties.Settings.Default.LargeImageText}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        PrintQuestion("What sould the text be, if you hover over the large image?");
                        PrintUserInput();

                        userInput = _logger.ReadLine().Trim();

                        Properties.Settings.Default.LargeImageText = userInput;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup small image key
                if(Properties.Settings.Default.SmallImageKey == "")
                {
                    PrintQuestion("What sould the small image be?");
                    PrintUserInput();

                    userInput = _logger.ReadLine().Trim();

                    Properties.Settings.Default.SmallImageKey = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"The small image is are alerady set to \"{Properties.Settings.Default.SmallImageKey}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        PrintQuestion("What sould the small image be?");
                        PrintUserInput();

                        userInput = _logger.ReadLine().Trim();

                        Properties.Settings.Default.SmallImageKey = userInput;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup small image text
                if(Properties.Settings.Default.SmallImageText == "")
                {
                    PrintQuestion("What sould the text be, if you hover over the small image?");
                    PrintUserInput();

                    userInput = _logger.ReadLine().Trim();

                    Properties.Settings.Default.SmallImageText = userInput;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    PrintInformation($"The text is are alerady set to \"{Properties.Settings.Default.SmallImageText}\".");
                    PrintQuestion("Do you wish to change it? (y/n)");
                    PrintUserInput();

                    userInput = _logger.ReadLine().ToLower().Trim();

                    if(userInput == "y")
                    {
                        PrintQuestion("What sould the text be, if you hover over the small image?");
                        PrintUserInput();

                        userInput = _logger.ReadLine().Trim();

                        Properties.Settings.Default.SmallImageText = userInput;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        PrintInformation("Skipped.");
                    }
                }
                #endregion

                #region Setup timer
                PrintQuestion("Do you wish a timer in your presence? (y/n)");
                PrintUserInput();

                userInput = _logger.ReadLine().Trim();

                if(userInput != "y")
                {
                    Properties.Settings.Default.Timer = false;
                    PrintInformation("Timer disabled.");
                }
                else
                {
                    Properties.Settings.Default.Timer = true;
                    PrintInformation("Timer enabled.");
                }

                Properties.Settings.Default.Save();
                #endregion

                PrintSuccess("Setup completed.");

                break;
            }
        }

        private static void Refresh()
        {
            if(_client.IsInitialized)
            {
                StopServer();
                StartServer();
            }
            else
            {
                PrintError("Server is not started yet.");
            }
        }

        private static void Reset()
        {
            PrintInformation("Attempting to reset RPC.");

            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            PrintSuccess("RPC reset successfully.");
        }

        private static void SaveLog()
        {
            try
            {
                System.Text.StringBuilder path = new System.Text.StringBuilder();
                _ = path.Append(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); //Get users documents directory
                _ = path.Append(@"\Discord RPC Server\"); //Add application name.

                //Create directory in case it does not exists.
                if(!System.IO.Directory.Exists(path.ToString()))
                {
                    _ = System.IO.Directory.CreateDirectory(path.ToString());
                    PrintInformation("Path \"" + path.ToString().Remove(path.Length - 1, 1) + "\" does not exist. It was created.");
                }

                _ = path.Append(DateTime.Now.ToShortDateString().Replace('/', '_')); //Add date and replace / with _
                _ = path.Append('-' + DateTime.Now.ToShortTimeString().Replace(':', '_')); //Add time and replace : with _
                _ = path.Append(".txt"); //Add file extension

                _logger.File = path.ToString();
                _logger.SaveLog();
                PrintSuccess("Log saved to: \"" + _logger.File + '"');
            }
            catch(Exception e)
            {
                PrintError("Saving faild.\n" + e.Message);
            }
        }

        private static bool MachineOnline()
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send("1.1.1.1", 3000);

            return reply.Status == IPStatus.Success;
        }

        private static void PrintInformation(string message)
        {
            _logger.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            _logger.Write("I");
            Console.ResetColor();
            _logger.WriteLine("] " + message);
        }

        private static void PrintQuestion(string message)
        {
            _logger.Write("[");
            Console.ForegroundColor = ConsoleColor.Blue;
            _logger.Write("Q");
            Console.ResetColor();
            _logger.WriteLine("] " + message);
        }

        private static void PrintSuccess(string message)
        {
            _logger.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            _logger.Write("S");
            Console.ResetColor();
            _logger.WriteLine("] " + message);
        }

        private static void PrintError(string message)
        {
            _logger.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            _logger.Write("E");
            Console.ResetColor();
            _logger.WriteLine("] " + message);
        }

        private static void PrintWelcomeScreen()
        {
            Console.Title = "Discord RPC Server";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            _logger.WriteLine("Discord RPC Server v." + Assembly.GetExecutingAssembly().GetName().Version);
            _logger.WriteLine("©PIN0L33KZ, visit https://www.pinoleekz.de for more information.");
            _logger.WriteLine("----------------------------------------------------------------");
            Console.ResetColor();
            _logger.WriteLine();
        }

        private static void PrintUserInput()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(System.Security.Principal.WindowsIdentity.GetCurrent().Name + "> ");
            Console.ResetColor();
        }
    }

    public class Logger
    {
        public string File { get; set; }
        private readonly System.Text.StringBuilder _log = new System.Text.StringBuilder();

        public void WriteLine()
        {
            Console.WriteLine();
            _ = _log.Append('\n');
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
            _ = _log.Append(value + '\n');
        }

        public void Write(string value)
        {
            Console.Write(value);
            _ = _log.Append(value);
        }

        public string ReadLine()
        {
            string userInput = Console.ReadLine();
            _ = _log.Append("[INPUT] " + userInput + '\n');
            return userInput;
        }

        public void SaveLog()
        {
            System.IO.File.WriteAllText(File, _log.ToString());
        }
    }
}