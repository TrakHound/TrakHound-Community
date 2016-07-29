using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TrakHound.Logging;
using TrakHound.API.Users;
using System.Collections.Specialized;
using TrakHound.API;
using TrakHound.Tools.Web;
using Newtonsoft.Json;

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
                Uri apiHost = ApiConfiguration.ApiHost;

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

                    //string[] x = response.Split('(', ')');
                    //if (x != null && x.Length > 1)
                    //{
                    //    string error = x[1];

                    //    Logger.Log("Upload File Failed : Error " + error, LogLineType.Error);
                    //}
                    //else
                    //{
                    //    Logger.Log("Upload File Successful", LogLineType.Notification);

                    //    var uploadFileInfos = JSON.ToType<List<UploadFileInfo>>(response);
                    //    if (uploadFileInfos != null)
                    //    {
                    //        return uploadFileInfos.ToArray();
                    //    }
                    //}
                }
            }

            return null;
        }

    }
}
