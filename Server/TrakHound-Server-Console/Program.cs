// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Xml;

using TrakHound_Server_Core;

namespace TrakHound_Server_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            Server server = new Server();
            server.Start();

            Console.ReadLine();

        }
    }
}
