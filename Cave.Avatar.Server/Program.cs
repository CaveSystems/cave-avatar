﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cave.Console;
using Cave.Web;
using Cave.Web.Avatar;

namespace Cave.Avatar.Server
{
    class ConsoleTraceListener : TraceListener
    {
        public override void Write(string message) => SystemConsole.Write(message);
        public override void WriteLine(string message) => SystemConsole.WriteLine(message);
    }

    class Program
    {
        int webPort = 8080;
        string avatarPath = @"Images";

        ManualResetEvent exit = new ManualResetEvent(false);

        [WebPage(Paths = "close")]
        public void Close(WebData webData)
        {
            exit.Set();
        }

        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            new Program().Run();
        }

        void Run()
        {
            avatarPath = Path.GetFullPath(avatarPath);
            var avatarInterface = new AvatarInterface(avatarPath);
            var webServer = new WebServer();
            webServer.EnableExplain = true;
            webServer.Register(this);
            webServer.Register(avatarInterface);
            webServer.Listen(webPort);
            exit.WaitOne();
            // Wait for shut down
            Thread.Sleep(500);
        }
    }
}
