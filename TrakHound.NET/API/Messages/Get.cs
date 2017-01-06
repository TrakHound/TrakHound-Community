// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Messages
    {
        public static List<MessageInfo> Get(UserConfiguration userConfig)
        {
            return Get(userConfig, null);
        }

        public static List<MessageInfo> Get(UserConfiguration userConfig, List<string> messageIds)
        {

            Uri apiHost = ApiConfiguration.AuthenticationApiHost;

            string url = new Uri(apiHost, "messages/get/index.php").ToString();

            string format = "{0}?token={1}&sender_id={2}{3}";

            string token = userConfig.SessionToken;
            string senderId = UserManagement.SenderId.Get();
            string devices = "";

            // List Message IDs to Get
            // If no Messages are listed then ALL Messages are retrieved
            if (messageIds != null)
            {
                string json = JSON.FromList<string>(messageIds);
                if (!string.IsNullOrEmpty(json))
                {
                    devices = "messages=" + json;
                }
            }

            url = string.Format(format, url, token, senderId, devices);

            var httpInfo = new HTTP.HTTPInfo();
            httpInfo.Url = url;
            httpInfo.MaxAttempts = 1;

            string response = HTTP.GET(httpInfo);
            if (response != null)
            {
                bool success = ApiError.ProcessResponse(response, "Get Messages");
                if (success)
                {
                    var messageInfos = JSON.ToType<List<MessageInfo>>(response);
                    if (messageInfos != null)
                    {
                        return messageInfos;
                    }
                }
            }

            return null;
        }
    }
}
