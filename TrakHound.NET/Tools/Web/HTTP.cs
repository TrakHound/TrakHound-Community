// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace TrakHound.Tools.Web
{
    public static class HTTP
    {
        const int CONNECTION_ATTEMPTS = 3;

        const int TIMEOUT = 10000;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public class HeaderData
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }

        public class PostContentData
        {
            public PostContentData(string name, string value)
            {
                Name = name;
                Value = HttpUtility.UrlEncode(value);
            }

            public PostContentData(string name, string value, string contentType)
            {
                Name = name;
                Value = HttpUtility.UrlEncode(value);
                ContentType = contentType;
            }

            public string Name { get; set; }
            public string Value { get; set; }
            public string ContentType { get; set; }

            public static PostContentData[] FromNamedValueCollection(NameValueCollection nvc)
            {
                var result = new List<PostContentData>();

                try
                {
                    if (nvc != null && nvc.Count > 0 && nvc.HasKeys())
                    {
                        var keys = nvc.AllKeys.ToList();

                        foreach (var key in keys)
                        {
                            var vals = "";
                            foreach (var val in nvc.GetValues(key)) vals += val;

                            result.Add(new PostContentData(key, vals));
                        }
                    }
                }
                catch (Exception ex) { }

                return result.ToArray();
            }
        }

        public class FileContentData
        {
            public FileContentData(string id, string filePath, string contentType)
            {
                Id = id;
                FilePath = filePath;
                ContentType = contentType;
            }

            public string Id { get; set; }
            public string FilePath { get; set; }
            public string ContentType { get; set; }
        }

        public class HTTPInfo
        {
            public HTTPInfo()
            {
                Init();
            }

            public HTTPInfo(string url)
            {
                Init();
                Url = url;
            }

            private void Init()
            {
                Url = "";
                Headers = null;
                UserAgent = null;
                Timeout = 5000;
                MaxAttempts = 3;
                GetResponse = true;
            }

            public string Url { get; set; }
            public PostContentData[] ContentData;
            public FileContentData[] FileData { get; set; }
            public HeaderData[] Headers { get; set; }
            public string UserAgent { get; set; }
            public NetworkCredential Credential { get; set; }
            public ProxySettings ProxySettings { get; set; }
            public int Timeout { get; set; }
            public int MaxAttempts { get; set; }
           
            public bool GetResponse { get; set; }
        }

        #region "POST"

        public static string POST(HTTPInfo info)
        {
            return SendData("POST", info.Url, info.ContentData, info.FileData, info.Headers, info.UserAgent, info.Credential, info.ProxySettings, info.Timeout, info.MaxAttempts, info.GetResponse);
        }

        public static string POST(string url, NameValueCollection postValues)
        {
            return SendData("POST", url, PostContentData.FromNamedValueCollection(postValues));
        }

        public static string POST(string url, PostContentData[] postContentData)
        {
            return SendData("POST", url, postContentData);
        }

        public static string POST(string url, PostContentData[] postContentData, HeaderData[] headers)
        {
            return SendData("POST", url, postContentData, null, headers);
        }

        public static string POST(string url, PostContentData[] postContentData, HeaderData[] headers, string userAgent)
        {
            return SendData("POST", url, postContentData, null, headers, userAgent);
        }

        public static string POST(string url, PostContentData[] postContentData, HeaderData[] headers, string userAgent, NetworkCredential credential)
        {
            return SendData("POST", url, postContentData, null, headers, userAgent, credential);
        }

        #endregion

        #region "PUT"

        public static string PUT(string url)
        {
            return SendData("PUT", url);
        }

        public static string PUT(string url, NameValueCollection postValues)
        {
            return SendData("PUT", url, PostContentData.FromNamedValueCollection(postValues));
        }

        public static string PUT(string url, PostContentData[] postContentData, HeaderData[] headers)
        {
            return SendData("PUT", url, postContentData, null, headers);
        }

        public static string PUT(string url, PostContentData[] postContentData, HeaderData[] headers, string userAgent)
        {
            return SendData("PUT", url, postContentData, null, headers, userAgent);
        }

        public static string PUT(string url, PostContentData[] postContentData, HeaderData[] headers, string userAgent, NetworkCredential credential)
        {
            return SendData("PUT", url, postContentData, null, headers, userAgent, credential);
        }
        
        #endregion

        #region "GET"

        public static string GET(HTTPInfo info)
        {
            return SendData("GET", info.Url, null, null, info.Headers, info.UserAgent, info.Credential, info.ProxySettings, info.Timeout, info.MaxAttempts, info.GetResponse);
        }

        public static string GET(string url)
        {
            return SendData("GET", url);
        }

        public static ResponseInfo GET(string url, bool returnBytes)
        {
            return SendData(returnBytes, "GET", url);
        }

        public static string GET(string url, HeaderData[] headers)
        {
            return SendData("GET", url, null, null, headers);
        }

        public static string GET(string url, HeaderData[] headers, string userAgent)
        {
            return SendData("GET", url, null, null, headers, userAgent);
        }

        public static string GET(string url, HeaderData[] headers, string userAgent, NetworkCredential credential)
        {
            return SendData("GET", url, null, null, headers, userAgent, credential);
        }

        #endregion

        public class ReponseHeaderInfo
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class ResponseInfo
        {
            public ReponseHeaderInfo[] Headers { get; set; }
            public byte[] Body { get; set; }
        }

        private static string SendData(
           string method,
           string url,
           PostContentData[] postDatas = null,
           FileContentData[] fileDatas = null,
           HeaderData[] headers = null,
           string userAgent = null,
           NetworkCredential credential = null,
           ProxySettings proxySettings = null,
           int timeout = TIMEOUT,
           int maxAttempts = CONNECTION_ATTEMPTS,
           bool getResponse = true)
        {
            var response = SendData(false, method, url, postDatas, fileDatas, headers, userAgent, credential, proxySettings, timeout, maxAttempts, getResponse);
            if (response != null && response.Body != null) return Encoding.ASCII.GetString(response.Body);
            else return null;
        }

        private static ResponseInfo SendData(
            bool returnBytes,
            string method,
            string url,
            PostContentData[] postDatas = null,
            FileContentData[] fileDatas = null,
            HeaderData[] headers = null,
            string userAgent = null,
            NetworkCredential credential = null,
            ProxySettings proxySettings = null,
            int timeout = TIMEOUT,
            int maxAttempts = CONNECTION_ATTEMPTS,
            bool getResponse = true
            )
        {
            ResponseInfo result = null;

            int attempts = 0;
            bool success = false;
            string message = null;

            // Try to send data for number of connectionAttempts
            while (attempts < maxAttempts && !success)
            {
                attempts += 1;

                try
                {
                    // Create HTTP request and define Header info
                    var request = (HttpWebRequest)WebRequest.Create(url);

                    string boundary = String_Functions.RandomString(10);

                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;
                    if (method == "POST") request.ContentType = "multipart/form-data; boundary=" + boundary;
                    else request.ContentType = "application/x-www-form-urlencoded";


                    // Set the Method
                    request.Method = method;

                    // Set the UserAgent
                    if (userAgent != null) request.UserAgent = userAgent;
                    else request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                    // Add Header data to request stream (if present)
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers[header.Id] = header.Text;
                        }
                    }

                    // set NetworkCredentials
                    if (credential != null)
                    {
                        request.Credentials = credential;
                        request.PreAuthenticate = true;
                    }

                    // Get Default System Proxy (Windows Internet Settings -> Proxy Settings)
                    var proxy = WebRequest.GetSystemWebProxy();

                    // Get Custom Proxy Settings from Argument (overwrite default proxy settings)
                    if (proxySettings != null)
                    {
                        if (proxySettings.Address != null && proxySettings.Port > 0)
                        {
                            var customProxy = new WebProxy(proxySettings.Address, proxySettings.Port);
                            customProxy.BypassProxyOnLocal = false;
                            proxy = customProxy;
                        }
                    }

                    request.Proxy = proxy;

                    var bytes = new List<byte>();

                    // Add Post Name/Value Pairs
                    if (postDatas != null)
                    {
                        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                        foreach (var postData in postDatas)
                        {
                            string formitem = string.Format(formdataTemplate, postData.Name, postData.Value);

                            bytes.AddRange(GetBytes("\r\n--" + boundary + "\r\n"));
                            bytes.AddRange(GetBytes(formitem));
                        }
                    }

                    // Add File data
                    if (fileDatas != null)
                    {
                        bytes.AddRange(GetFileContents(fileDatas, boundary));
                    }

                    if (bytes.Count > 0)
                    {
                        // Write Trailer Boundary
                        string trailer = "\r\n--" + boundary + "--\r\n";
                        bytes.AddRange(GetBytes(trailer));

                        var byteArray = bytes.ToArray();

                        // Write Data to Request Stream
                        request.ContentLength = byteArray.Length;

                        using (var requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(byteArray, 0, byteArray.Length);
                        }
                    }

                    // Get Response Message from HTTP Request
                    if (getResponse)
                    {
                        result = new ResponseInfo();

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            // Get HTTP Response Body
                            using (var responseStream = response.GetResponseStream())
                            using (var memStream = new MemoryStream())
                            {
                                byte[] buffer = new byte[10240];

                                int read;
                                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    memStream.Write(buffer, 0, read);
                                }

                                result.Body = memStream.ToArray();

                                success = true;
                            }

                            var responseHeaders = new List<ReponseHeaderInfo>();

                            // Get HTTP Response Headers
                            foreach (var key in response.Headers.AllKeys)
                            {
                                var header = new ReponseHeaderInfo();
                                header.Key = key;
                                header.Value = response.Headers.Get(key);

                                responseHeaders.Add(header);
                            }

                            result.Headers = responseHeaders.ToArray();
                        }
                    }
                    else success = true;
                }
                catch (WebException wex) { message = wex.Message; }
                catch (Exception ex) { message = ex.Message; }

                if (!success) System.Threading.Thread.Sleep(500);
            }

            if (!success) logger.Info("Send :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

            return result;
        }

        /// <summary>
        /// Get bytes for HTTP file Content
        /// </summary>
        private static byte[] GetFileContents(FileContentData[] fileDatas, string boundary)
        {
            var result = new List<byte>();

            foreach (var fileData in fileDatas)
            {
                if (File.Exists(fileData.FilePath))
                {
                    var file = new FileInfo(fileData.FilePath);

                    // Write Header Boundary
                    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                    string header = string.Format(headerTemplate, fileData.Id, fileData.FilePath, fileData.ContentType);

                    result.AddRange(GetBytes("\r\n--" + boundary + "\r\n"));
                    result.AddRange(GetBytes(header));

                    // Write File Contents
                    using (var fileStream = new FileStream(fileData.FilePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[file.Length];

                        using (var memStream = new MemoryStream())
                        {
                            int read;
                            while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memStream.Write(buffer, 0, read);
                            }

                            result.AddRange(memStream.ToArray());
                        }
                    }
                }
            }

            return result.ToArray();
        }

        private static byte[] GetBytes(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }
        
        public static string GetPostValue(string body, string parameterName)
        {
            string response = null;

            int i = body.IndexOf(Environment.NewLine, 1);
            if (i >= 0)
            {
                string boundary = body.Substring(0, i);

                i = body.IndexOf(Environment.NewLine, i + 1);
                if (i >= 0)
                {
                    string contentHeader = body.Substring(0, i);
                    string n = "name=\"";
                    int x = contentHeader.IndexOf(n);
                    if (x >= 0)
                    {
                        x += n.Length;

                        int l = contentHeader.IndexOf("\"", x + 1);
                        if (l >= 0)
                        {
                            string name = contentHeader.Substring(x, l - x).Trim();

                            if (name == parameterName)
                            {
                                x = body.IndexOf(boundary, i + 1);
                                if (x >= 0)
                                {
                                    string value = body.Substring(i, x - i).Trim();

                                    response = HttpUtility.UrlDecode(value);
                                }
                            }
                        }
                    }
                }
            }

            return response;
        }

    }
}

