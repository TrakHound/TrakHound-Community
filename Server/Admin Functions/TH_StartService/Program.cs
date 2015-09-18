using System;

using System.ServiceProcess;
using System.Security.Principal;

using TH_Global;

namespace TH_StartService
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceName = ApplicationNames.TrakHoundServer_ServiceName;
            int timeoutMilliseconds = 3000;

            StartService(serviceName, timeoutMilliseconds);
        }

        static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName, Environment.MachineName);

            try
            {
                if (service.Status != ServiceControllerStatus.Running)
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Start Service failed! : " + serviceName + " : " + service.MachineName + " : " + e.Message);
            }

            service.Close();
        }
    }
}
