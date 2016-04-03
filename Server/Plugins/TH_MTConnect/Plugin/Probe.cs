
using TH_Configuration;
using TH_Global;
using TH_Plugins;

using TH_MTConnect.Components;

namespace TH_MTConnect.Plugin
{
    public partial class MTConnect
    {

        private ReturnData GetProbe(AgentConfiguration config)
        {
            ReturnData result = null;

            string address = config.IP_Address;
            int port = config.Port;
            string deviceName = config.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            string url = HTTP.GetUrl(address, port, deviceName) + "probe";

            result = Requests.Get(url, proxy);
            if (result != null)
            {
                Logger.Log("Probe Successful : " + url);
            }
            else
            {
                Logger.Log("Probe Error : " + url);
            }

            return result;
        }

        private void SendProbeData(ReturnData returnData, Configuration config)
        {
            if (returnData != null)
            {
                var data = new EventData();
                data.Id = "MTConnect_Probe";
                data.Data01 = config;
                data.Data02 = returnData;

                if (SendData != null) SendData(data);
            }
        }

    }
}
