// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace TH_Global.Web
{
    public static class HTTP
    {
        const int CONNECTION_ATTEMPTS = 3;

        const int TIMEOUT = 10000;

        public static bool UploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            bool result = false;

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Timeout = TIMEOUT;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;

            int attempts = 0;
            bool success = false;

            while (attempts < CONNECTION_ATTEMPTS && !success)
            {
                attempts += 1;

                try
                {
                    wresp = wr.GetResponse();
                    Stream stream2 = wresp.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    Logger.Log(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));

                    result = true;

                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Log("Error uploading file : " + ex.Message);
                    if (wresp != null)
                    {
                        wresp.Close();
                        wresp = null;
                    }
                }
                finally
                {
                    wr = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Encode a string used in Post Data. 
        /// This was typically done using WebUtility.HtmlEncode but this is not available
        /// in .NET 4.0 Client Profile which is required to work in XP
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncodePostString(string str)
        {
            string result = str;
            result = result.Replace("!", "%21");
            result = result.Replace("#", "%23");
            result = result.Replace("$", "%24");
            result = result.Replace("&", "%26");
            result = result.Replace("'", "%27");
            //result = result.Replace("(", "%28");
            //result = result.Replace(")", "%29");
            result = result.Replace("*", "%2a");
            result = result.Replace("+", "%2b");
            result = result.Replace(",", "%2c");
            result = result.Replace("/", "%2f");
            result = result.Replace(":", "%3a");
            result = result.Replace(";", "%3b");
            result = result.Replace("=", "%3d");
            result = result.Replace("?", "%3f");
            result = result.Replace("@", "%40");
            result = result.Replace("[", "%5b");
            result = result.Replace("]", "%5d");
            result = result.Replace(" ", "+");

            return result;
        }


        public static string Send(string url, NameValueCollection postData = null)
        {
            string result = null;

            var attempts = 0;
            var success = false;
            string message = null;

            // Try to send data for number of connectionAttempts
            while (attempts < CONNECTION_ATTEMPTS && !success)
            {
                attempts += 1;

                try
                {
                    byte[] postBytes = new byte[0];

                    if (postData != null) postBytes = CreatePostBytes(postData);

                    // Create HTTP request and define Header info
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = TIMEOUT;
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = postBytes.Length;
                    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                    // Add POST data to request stream
                    Stream postStream = request.GetRequestStream();
                    postStream.Write(postBytes, 0, postBytes.Length);
                    postStream.Flush();
                    postStream.Close();

                    // Get HTTP resonse and return as string
                    GetHTTPResponse(request);

                    success = true;
                }
                catch (WebException wex) { message = wex.Message; }
                catch (Exception ex) { message = ex.Message; }

                if (!success) System.Threading.Thread.Sleep(1000);
            }

            if (!success) Logger.Log("Send :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

            return result;
        }

        /// <summary>
        /// Create byte array that contains Post Data
        /// </summary>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public static byte[] CreatePostBytes(NameValueCollection nvc)
        {
            var postData = new StringBuilder();
            for (var x = 0; x <= nvc.AllKeys.Length - 1; x++)
            {
                string key = nvc.AllKeys[x];
                var vals = "";
                foreach (var value in nvc.GetValues(key))
                {
                    vals += value;
                }
                postData.Append(EncodePostString(key));
                postData.Append("=");
                postData.Append(EncodePostString(vals));

                // If not the last data item then add '&'
                if (x < nvc.AllKeys.Length) postData.Append("&");
            }

            // Convert POST data to byte array
            var ascii = new ASCIIEncoding();
            return ascii.GetBytes(postData.ToString());
        }

        /// <summary>
        /// HTTP resonse and return as string
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static string GetHTTPResponse(HttpWebRequest request)
        {
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var s = response.GetResponseStream())
            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// Send Data to URL with POST Data using HTTP
        /// </summary>
        /// <param name="url"></param>
        /// <param name="nvc"></param>
        /// <returns></returns>
        //public static string SendData(string url, NameValueCollection nvc)
        //{

        //    string result = null;

        //    int attempts = 0;
        //    bool success = false;
        //    string message = null;

        //    // Try to send data for number of connectionAttempts
        //    while (attempts < CONNECTION_ATTEMPTS && !success)
        //    {
        //        attempts += 1;

        //        try
        //        {
        //            // Create POST data string
        //            StringBuilder postData = new StringBuilder();
        //            for (int x = 0; x <= nvc.AllKeys.Length - 1; x++)
        //            {
        //                string key = nvc.AllKeys[x];
        //                string vals = "";
        //                foreach (string value in nvc.GetValues(key))
        //                {
        //                    vals += value;
        //                }
        //                postData.Append(EncodePostString(key));
        //                postData.Append("=");
        //                postData.Append(EncodePostString(vals));

        //                if (x < nvc.AllKeys.Length) postData.Append("&");
        //            }

        //            // Convert POST data to byte array
        //            ASCIIEncoding ascii = new ASCIIEncoding();
        //            byte[] postBytes = ascii.GetBytes(postData.ToString());

        //            // Create HTTP request and define Header info
        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //            request.Timeout = TIMEOUT;
        //            request.Method = "POST";
        //            request.ContentType = "application/x-www-form-urlencoded";
        //            request.ContentLength = postBytes.Length;
        //            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        //            // Add POST data to request stream
        //            Stream postStream = request.GetRequestStream();
        //            postStream.Write(postBytes, 0, postBytes.Length);
        //            postStream.Flush();
        //            postStream.Close();

        //            // Get HTTP resonse and return as string
        //            using (var response = (HttpWebResponse)request.GetResponse())
        //            using (var s = response.GetResponseStream())
        //            using (var reader = new StreamReader(s))
        //            {
        //                result = reader.ReadToEnd();
        //                success = true;
        //            }
        //        }
        //        catch (WebException wex) { message = wex.Message; }
        //        catch (Exception ex) { message = ex.Message; }

        //        if (!success) System.Threading.Thread.Sleep(1000);
        //    }

        //    if (!success) Logger.Log("Send :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

        //    return result;
        //}

        public class HeaderData
        {
            public string Id { get; set; }
            public string Text { get; set; }
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
                Data = null;
                Headers = null;
                UserAgent = null;
                Timeout = 5000;
                MaxAttempts = 3;
                GetResponse = true;
            }

            public string Url { get; set; }
            public byte[] Data { get; set; }
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
            return SendData("POST", info.Url, info.Data, info.Headers, info.UserAgent, info.Credential, info.ProxySettings, info.Timeout, info.MaxAttempts, info.GetResponse);
        }

        public static string POST(string url, NameValueCollection postValues)
        {
            var postBytes = CreatePostBytes(postValues);

            return SendData("POST", url, postBytes);
        }

        public static string POST(string url, byte[] postBytes)
        {
            return SendData("POST", url, postBytes);
        }

        public static string POST(string url, byte[] postBytes, HeaderData[] headers)
        {
            return SendData("POST", url, postBytes, headers);
        }

        public static string POST(string url, byte[] postBytes, HeaderData[] headers, string userAgent)
        {
            return SendData("POST", url, postBytes, headers, userAgent);
        }

        public static string POST(string url, byte[] postBytes, HeaderData[] headers, string userAgent, NetworkCredential credential)
        {
            return SendData("POST", url, postBytes, headers, userAgent, credential);
        }

        #endregion

        #region "PUT"

        public static string PUT(string url)
        {
            return SendData("PUT", url);
        }

        public static string PUT(string url, NameValueCollection postValues)
        {
            var postBytes = CreatePostBytes(postValues);

            return SendData("PUT", url, postBytes);
        }

        public static string PUT(string url, byte[] postBytes)
        {
            return SendData("PUT", url, postBytes);
        }

        public static string PUT(string url, byte[] postBytes, HeaderData[] headers)
        {
            return SendData("PUT", url, postBytes, headers);
        }

        public static string PUT(string url, byte[] postBytes, HeaderData[] headers, string userAgent)
        {
            return SendData("PUT", url, postBytes, headers, userAgent);
        }

        public static string PUT(string url, byte[] postBytes, HeaderData[] headers, string userAgent, NetworkCredential credential)
        {
            return SendData("PUT", url, postBytes, headers, userAgent, credential);
        }

        #endregion

        #region "GET"

        public static string GET(HTTPInfo info)
        {
            return SendData("GET", info.Url, info.Data, info.Headers, info.UserAgent, info.Credential, info.ProxySettings, info.Timeout, info.MaxAttempts, info.GetResponse);
        }

        public static string GET(string url)
        {
            return SendData("GET", url);
        }

        public static string GET(string url, HeaderData[] headers)
        {
            return SendData("GET", url, null, headers);
        }

        public static string GET(string url, HeaderData[] headers, string userAgent)
        {
            return SendData("GET", url, null, headers, userAgent);
        }

        public static string GET(string url, byte[] postBytes, HeaderData[] headers, string userAgent, NetworkCredential credential)
        {
            return SendData("GET", url, postBytes, headers, userAgent, credential);
        }

        #endregion


        private static string SendData(string method, string url,
            byte[] sendBytes = null, HeaderData[] headers = null,
            string userAgent = null, NetworkCredential credential = null,
            ProxySettings proxySettings = null,
            int timeout = TIMEOUT, int maxAttempts = CONNECTION_ATTEMPTS,
            bool getResponse = true)
        {
            string result = null;

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

                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;
                    request.ContentType = "application/x-www-form-urlencoded";
                    //request.ContentType = "application/json";

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

                    // Add POST data to request stream
                    if (sendBytes != null)
                    {
                        request.ContentLength = sendBytes.Length;

                        using (Stream postStream = request.GetRequestStream())
                        {
                            postStream.WriteTimeout = timeout;
                            postStream.Write(sendBytes, 0, sendBytes.Length);
                        }
                    }
                    else request.ContentLength = 0;

                    if (getResponse)
                    {
                        // Get HTTP resonse and return as string
                        using (var response = (HttpWebResponse)request.GetResponse())
                        using (var s = response.GetResponseStream())
                        using (var reader = new StreamReader(s))
                        {
                            result = reader.ReadToEnd();
                            success = true;
                        }
                    }
                    else success = true;
                }
                catch (WebException wex) { message = wex.Message; }
                catch (Exception ex) { message = ex.Message; }

                if (!success) System.Threading.Thread.Sleep(500);
            }

            if (!success) Logger.Log("Send :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

            return result;
        }

        





        //public static string SendData(string url, NameValueCollection nvc, bool insureDelivery = false)
        //{

        //    string result = null;

        //    int attempts = 0;
        //    bool success = false;
        //    string message = null;

        //    // Try to send & receive data for number of connectionAttempts or infinitely if insureDelivery is set
        //    while ((attempts < connectionAttempts || insureDelivery) && !success)
        //    {
        //        attempts += 1;

        //        try
        //        {
        //            // Create POST data string
        //            StringBuilder postData = new StringBuilder();
        //            for (int x = 0; x <= nvc.AllKeys.Length - 1; x++)
        //            {
        //                string key = nvc.AllKeys[x];
        //                string vals = "";
        //                foreach (string value in nvc.GetValues(key))
        //                {
        //                    vals += value;
        //                }
        //                postData.Append(HttpUtility.UrlEncode(key));
        //                postData.Append("=");
        //                postData.Append(HttpUtility.UrlEncode(vals));

        //                if (x < nvc.AllKeys.Length) postData.Append("&");
        //            }

        //            // Convert POST data to byte array
        //            ASCIIEncoding ascii = new ASCIIEncoding();
        //            byte[] postBytes = ascii.GetBytes(postData.ToString());

        //            // Create HTTP request and define Header info
        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //            request.Timeout = timeout;
        //            request.Method = "POST";
        //            request.ContentType = "application/x-www-form-urlencoded";
        //            request.ContentLength = postBytes.Length;
        //            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        //            // Add POST data to request stream
        //            Stream postStream = request.GetRequestStream();
        //            postStream.Write(postBytes, 0, postBytes.Length);
        //            postStream.Flush();
        //            postStream.Close();

        //            // Get HTTP resonse and return as string
        //            using (var response = (HttpWebResponse)request.GetResponse())
        //            using (var s = response.GetResponseStream())
        //            using (var reader = new StreamReader(s))
        //            {
        //                result = reader.ReadToEnd();
        //                success = true;
        //            }
        //        }
        //        catch (WebException wex) { message = wex.Message; }
        //        catch (Exception ex) { message = ex.Message; }

        //        if (!success) System.Threading.Thread.Sleep(1000);
        //    }

        //    if (!success) Logger.Log("Send :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

        //    return result;
        //}

        public static string GetData(string url)
        {

            string result = null;

            int attempts = 0;
            bool success = false;
            string message = null;

            // Try to receive data for number of connectionAttempts
            while (attempts < CONNECTION_ATTEMPTS && !success)
            {
                attempts += 1;

                try
                {
                    // Create HTTP request and define Header info
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = TIMEOUT;

                    // Get HTTP resonse and return as string
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var s = response.GetResponseStream())
                    using (var reader = new StreamReader(s))
                    {
                        result = reader.ReadToEnd();
                        success = true;
                    }
                }
                catch (WebException wex) { message = wex.Message; }
                catch (Exception ex) { message = ex.Message; }

                if (!success) System.Threading.Thread.Sleep(1000);
            }

            //if (!success) Logger.Log("Get :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

            return result;
        }

        //public static string GetData(string url, bool insureDelivery = false)
        //{

        //    string result = null;

        //    int attempts = 0;
        //    bool success = false;
        //    string message = null;

        //    // Try to send & receive data for number of connectionAttempts or infinitely if insureDelivery is set
        //    while ((attempts < connectionAttempts || insureDelivery) && !success)
        //    {
        //        attempts += 1;

        //        try
        //        {
        //            // Create HTTP request and define Header info
        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //            request.Timeout = timeout;

        //            // Get HTTP resonse and return as string
        //            using (var response = (HttpWebResponse)request.GetResponse())
        //            using (var s = response.GetResponseStream())
        //            using (var reader = new StreamReader(s))
        //            {
        //                result = reader.ReadToEnd();
        //                success = true;
        //            }
        //        }
        //        catch (WebException wex) { message = wex.Message; }
        //        catch (Exception ex) { message = ex.Message; }

        //        if (!success) System.Threading.Thread.Sleep(1000);
        //    }

        //    if (!success) Logger.Log("Get :: " + attempts.ToString() + " Attempts :: URL = " + url + " :: " + message);

        //    return result;
        //}
    }
}

