using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace TH_Global.Functions
{
    public static class Service_Functions
    {
        public static ServiceController[] GetRunningServices()
        {
            var result = new List<ServiceController>();

            ServiceController[] services = ServiceController.GetServices();
            foreach (var service in services) 
            {
                if (service.Status == ServiceControllerStatus.Running) result.Add(service);
            }

            return result.ToArray();
        }

        public static ServiceController GetServiceController(string serviceName)
        {
            ServiceController result = null;

            var services = ServiceController.GetServices();
            int index = services.ToList().FindIndex(x => x.ServiceName == serviceName);
            if (index >= 0) result = services[index];

            return result;
        }

        public static ServiceStatus GetServiceStatus(string serviceName)
        {
            var service = GetServiceController(serviceName);
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Running) return ServiceStatus.Running;
                if (service.Status == ServiceControllerStatus.Paused) return ServiceStatus.Paused;
                if (service.Status == ServiceControllerStatus.Stopped) return ServiceStatus.Stopped;
                return ServiceStatus.Changing;
            }
            return ServiceStatus.Unavailable;
        }

        public static bool IsServiceRunning(string serviceName)
        {
            var services = GetRunningServices();
            return services.ToList().Exists(x => x.ServiceName == serviceName);
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            var services = ServiceController.GetServices();
            return services.ToList().Exists(x => x.ServiceName == serviceName);
        }

    }

    public enum ServiceStatus
    {
        Unavailable, // Service Not Found
        Stopped,
        Changing,
        Paused,
        Running
    }
}
