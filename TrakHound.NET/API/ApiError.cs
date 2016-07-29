using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static class ApiError
    {

        public static bool ProcessResponse(string response, string label = null)
        {
            if (!string.IsNullOrEmpty(response))
            {
                string[] x = response.Split('(', ')');
                if (x != null && x.Length > 1)
                {
                    Logger.Log(label + " : Error : " + response, LogLineType.Warning);
                    return false;
                }
                else
                {
                    Logger.Log(label + " Successful", LogLineType.Notification);
                    return true;
                }
            }

            return false;          
        }

        public static bool ProcessResponse(HTTP.ResponseInfo responseInfo, string label = null)
        {
            if (responseInfo != null && responseInfo.Body != null)
            {
                string s = Encoding.ASCII.GetString(responseInfo.Body);
                return ProcessResponse(s, label);
            }

            return false;
        }

    }
}
