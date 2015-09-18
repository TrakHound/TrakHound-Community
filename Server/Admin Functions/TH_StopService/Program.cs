using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceProcess;
using System.Security.Principal;

using TH_Global;

namespace TH_StopService
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceName = ApplicationNames.TrakHoundServer_ServiceName;
            int timeoutMilliseconds = 3000;

            StopService(serviceName, timeoutMilliseconds);
        }

        static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName, Environment.MachineName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine("Stop Service failed! : " + e.Message);
            }
            service.Close();
        }
    }
}
