// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Files
    {

        public class UploadFileInfo
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public static UploadFileInfo[] Upload(UserConfiguration userConfig, HTTP.FileContentData fileContentData)
        {
            var fileContentDatas = new HTTP.FileContentData[1];
            fileContentDatas[0] = fileContentData;

            return Upload(userConfig, fileContentDatas);
        }

        public static UploadFileInfo[] Upload(UserConfiguration userConfig, HTTP.FileContentData[] fileContentDatas)
        {
            if (userConfig != null)
            {
                Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                string url = new Uri(apiHost, "files/upload/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["token"] = userConfig.SessionToken;
                postDatas["sender_id"] = UserManagement.SenderId.Get();

                var httpInfo = new HTTP.HTTPInfo();
                httpInfo.Url = url;
                httpInfo.ContentData = HTTP.PostContentData.FromNamedValueCollection(postDatas);
                httpInfo.FileData = fileContentDatas;

                string response = HTTP.POST(httpInfo);
                if (response != null)
                {
                    bool success = ApiError.ProcessResponse(response, "Upload File");
                    if (success)
                    {
                        var uploadFileInfos = JSON.ToType<List<UploadFileInfo>>(response);
                        if (uploadFileInfos != null)
                        {
                            return uploadFileInfos.ToArray();
                        }
                    }
                }
            }

            return null;
        }

    }
}
