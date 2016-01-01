// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TrakHound_Server_Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                { 
                    new Service1() 
                };
            ServiceBase.Run(ServicesToRun);
            //Environment.ExitCode = 0;     
        }

        //static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    Console.WriteLine("TrakHound Server :: Unhandled Exception :: " + e.ExceptionObject.ToString());
        //    Environment.ExitCode = 12;            
        //}
    }
}
