using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Bugs
    {
        public static bool Send(UserConfiguration userConfig, List<BugInfo> bugInfos)
        {
            string json = JSON.FromList<BugInfo>(bugInfos);
            if (!string.IsNullOrEmpty(json))
            {
                Uri apiHost = ApiConfiguration.ApiHost;

                string url = new Uri(apiHost, "bugs/send/index.php").ToString();

                var postDatas = new NameValueCollection();
                if (userConfig != null)
                {
                    postDatas["token"] = userConfig.SessionToken;
                    postDatas["sender_id"] = UserManagement.SenderId.Get();
                }

                postDatas["bugs"] = json;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    return ApiError.ProcessResponse(response, "Send Bug Report");
                }
            }

            return false;
        }
    }
}
