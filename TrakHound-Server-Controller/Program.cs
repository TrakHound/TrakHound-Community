// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Forms;

namespace TrakHound_Server_Controller
{
    class Program
    {
        static void Main(string[] args)
        {
             var controller = new Controller();

            Application.Run(controller);

            Environment.ExitCode = 0;
        }
    }
}
