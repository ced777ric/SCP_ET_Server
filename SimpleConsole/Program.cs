﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;

namespace SCPET_Server
{
    class Program
    {
        public static int port = 0;
        public static bool portfound = true;
        public static TcpConsoleClient console;
        private static Thread inputthread;
        public static Process gameprocess;

        static void Main(string[] args)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            //look for a port that is not in use
            port = new Random().Next(50000, 60000);
            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    portfound = false;
                    Console.WriteLine("console port was already in use");
                    break;
                }
            }

            if (portfound)
            {
                Console.WriteLine("Loading");
                List<string> cmdargs = new List<string>();
                string command = string.Empty;



                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.WriteLine("Platform windows");
                    if (string.IsNullOrEmpty(GetArg("-gamelocation")))
                    {
                        Console.WriteLine("Starting game server...");

                        string file = "SCP_ET.exe";
                        Console.WriteLine(file);
                        command = file;
                        cmdargs.Add("-consoleport");
                        cmdargs.Add(port.ToString());
                        cmdargs.Add("-logfile");
                        cmdargs.Add("logs/SCP-ETServerLog-" + DateTime.UtcNow.Ticks + ".txt");
                    }
                    else
                    {
                        Console.WriteLine("Starting game server...");

                        string file = GetArg("-gamelocation") + "/SCP_ET.exe";
                        Console.WriteLine(file);
                        command = file;
                        cmdargs.Add("-consoleport");
                        cmdargs.Add(port.ToString());
                        cmdargs.Add("-logfile");
                        cmdargs.Add(GetArg("-gamelocation") + "/logs/SCP-ETServerLog-" + DateTime.UtcNow.Ticks + ".txt");
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine("Platform linux");
                    if (string.IsNullOrEmpty(GetArg("-gamelocation")))
                    {
                        Console.WriteLine("Starting game server...");

                        string file = "scp_et.x86_64";
                        Console.WriteLine(file);
                        command = file;
                        cmdargs.Add("-consoleport");
                        cmdargs.Add(port.ToString());
                        cmdargs.Add("-logfile");
                        cmdargs.Add("logs/SCP-ETServerLog-" + DateTime.UtcNow.Ticks + ".txt");
                    }
                    else
                    {
                        Console.WriteLine("Starting game server...");

                        string file = GetArg("-gamelocation") + "/scp_et.x86_64";
                        Console.WriteLine(file);
                        command = file;
                        cmdargs.Add("-consoleport");
                        cmdargs.Add(port.ToString());
                        cmdargs.Add("-logfile");
                        cmdargs.Add(GetArg("-gamelocation") + "/logs/SCP-ETServerLog-" + DateTime.UtcNow.Ticks + ".txt");
                    }
                }

                ProcessStartInfo info2 = new ProcessStartInfo(Path.GetFullPath(command), string.Join(' ', cmdargs));

                using (Process cmd = Process.Start(info2))
                {
                    console = new TcpConsoleClient();
                    console.ConnectToTcpServer();
                    AppDomain.CurrentDomain.DomainUnload += (s, e) => { cmd.Kill(); cmd.WaitForExit(); };
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => { cmd.Kill(); cmd.WaitForExit(); };
                    AppDomain.CurrentDomain.UnhandledException += (s, e) => { cmd.Kill(); cmd.WaitForExit(); };
                    listeninput();
                }
            }
        }

        public static void listeninput()
        {
            while (true)
            {
                string line = Console.ReadLine(); // Get string from user
                if (line == "exit") // Check string
                {
                    break;
                }

                Console.WriteLine(">" + line);
                console.SendMessage(line);
            }

            Console.WriteLine("Shutting down, killing server...");
            gameprocess.Kill();
        }

        public static string GetArg(string argname)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argname)
                {
                    if (args.Length > i + 1)
                    {
                        if (args[i + 1].StartsWith("-"))
                            return "true";
                        // else if (args[i + 1].EndsWith("\\"))
                        else
                            return args[i + 1];
                    }
                    else
                    {
                        return "true";
                    }
                }
            }

            return string.Empty;
        }

        public static ConsoleColor FromHex(string hex)
        {
            int argb = Int32.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);
            Color c = Color.FromArgb(argb);

            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit

            return (System.ConsoleColor) index;
        }
    }
}