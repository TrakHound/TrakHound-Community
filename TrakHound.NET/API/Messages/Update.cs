using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Messages
    {
        public static bool Update(UserConfiguration userConfig, List<MessageInfo> messageInfos)
        {

            string json = JSON.FromList<MessageInfo>(messageInfos);
            if (!string.IsNullOrEmpty(json))
            {
                Uri apiHost = ApiConfiguration.ApiHost;

                string url = new Uri(apiHost, "messages/update/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["token"] = userConfig.SessionToken;
                postDatas["sender_id"] = UserManagement.SenderId.Get();
                postDatas["messages"] = json;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    return ApiError.ProcessResponse(response, "Remove Messages");
                }
            }

            return false;
        }
    }
}
