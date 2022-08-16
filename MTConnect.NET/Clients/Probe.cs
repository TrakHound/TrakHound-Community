// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MTConnect.Clients
{
    public class Probe
    {
        /// <summary>
        /// Create a new Probe Request Client
        /// </summary>
        public Probe() { }

        /// <summary>
        /// Create a new Probe Request Client
        /// </summary>
        /// <param name="baseUrl">The base URL for the Probe Request</param>
        public Probe(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Create a new Probe Request Client
        /// </summary>
        /// <param name="baseUrl">The base URL for the Probe Request</param>
        /// <param name="deviceName">The name of the requested device</param>
        public Probe(string baseUrl, string deviceName)
        {
            BaseUrl = baseUrl;
            DeviceName = deviceName;
        }

        /// <summary>
        /// The base URL for the Probe Request
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// (Optional) The name of the requested device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// User settable object sent with request and returned in Document on response
        /// </summary>
        public object UserObject { get; set; }

        /// <summary>
        /// Raised when an MTConnectError Document is received
        /// </summary>
        public event MTConnectErrorHandler Error;

        /// <summary>
        /// Raised when an Connection Error occurs
        /// </summary>
        public event ConnectionErrorHandler ConnectionError;

        /// <summary>
        /// Raised when an MTConnectDevices Document is received successfully
        /// </summary>
        public event MTConnectDevicesHandler Successful;


        /// <summary>
        /// Execute the Probe Request Synchronously
        /// </summary>
        public MTConnectDevices.Document Execute()
        {
            //// Create Uri
            //var uri = new Uri(BaseUrl);
            //uri = new Uri(uri, "probe");
            //if (DeviceName != null) uri = new Uri(uri, DeviceName);

            ////// Create HTTP Client and Request Data
            //var client = new HttpClient();
            //return ProcessResponse(client.GetStringAsync(uri).Result);

            try
            {
                // Create Uri
                //var uri = new Uri(BaseUrl);
                //uri = new Uri(uri, "probe");
                //if (DeviceName != null) uri = new Uri(uri, DeviceName);

                var url = BaseUrl;
                if (!string.IsNullOrEmpty(DeviceName)) url = CombineUrl(url, DeviceName);
                url = CombineUrl(url, "probe");

                //// Create HTTP Client and Request Data
                var client = new HttpClient();
                return ProcessResponse(client.GetStringAsync(url).Result);
            }
            catch (Exception ex) { }

            return null;
        }

        public static string CombineUrl(string baseUrl, string path)
        {
            if (baseUrl == null || baseUrl.Length == 0)
            {
                return baseUrl;
            }

            if (path.Length == 0)
            {
                return path;
            }

            baseUrl = baseUrl.TrimEnd('/', '\\');
            path = path.TrimStart('/', '\\');

            return $"{baseUrl}/{path}";
        }

        /// <summary>
        /// Execute the Probe Request Asynchronously
        /// </summary>
        public async Task ExecuteAsync()
        {
            // Create Uri
            var uri = new Uri(BaseUrl);
            uri = new Uri(uri, "probe");
            if (DeviceName != null) uri = new Uri(uri, DeviceName);

            //// Create HTTP Client and Request Data
            var client = new HttpClient();
            await client.GetStringAsync(uri).ContinueWith((o) =>
            {
                var doc = ProcessResponse(o.Result);
                if (doc != null) Successful?.Invoke(doc);
            });
            //return ProcessResponse(client.GetStringAsync(uri).Result);

            //// Create Uri
            //var uri = new Uri(BaseUrl);
            //if (DeviceName != null) uri = new Uri(uri, DeviceName);
            //uri = new Uri(uri, "probe");

            //// Create HTTP Client and Request Data
            //var client = new RestClient(uri);
            //var request = new RestRequest(Method.Get);
            //client.ExecuteAsync(request, AsyncCallback);
        }

        private MTConnectDevices.Document ProcessResponse(string response)
        {
            if (!string.IsNullOrEmpty(response))
            {
                string xml = response;

                // Process MTConnectStreams Document
                var doc = MTConnectDevices.Document.Create(xml);
                if (doc != null)
                {
                    doc.UserObject = UserObject;
                    return doc;
                }
                else
                {
                    // Process MTConnectError Document (if MTConnectDevices fails)
                    var errorDoc = MTConnectError.Document.Create(xml);
                    if (errorDoc != null)
                    {
                        errorDoc.UserObject = UserObject;
                        Error?.Invoke(errorDoc);
                    }

                }
            }

            return null;
        }

        //private MTConnectDevices.Document ProcessResponse(RestResponse response)
        //{
        //    if (response.ResponseStatus != ResponseStatus.Completed)
        //    {
        //        if (response.ErrorException != null) ConnectionError?.Invoke(response.ErrorException);
        //    }
        //    else if (!string.IsNullOrEmpty(response.Content))
        //    {
        //        string xml = response.Content;

        //        // Process MTConnectStreams Document
        //        var doc = MTConnectDevices.Document.Create(xml);
        //        if (doc != null)
        //        {
        //            doc.UserObject = UserObject;
        //            return doc;
        //        }
        //        else
        //        {
        //            // Process MTConnectError Document (if MTConnectDevices fails)
        //            var errorDoc = MTConnectError.Document.Create(xml);
        //            if (errorDoc != null)
        //            {
        //                errorDoc.UserObject = UserObject;
        //                Error?.Invoke(errorDoc);
        //            }

        //        }
        //    }

        //    return null;
        //}

        //private void AsyncCallback(IRestResponse response)
        //{
        //    var doc = ProcessResponse(response);
        //    if (doc != null) Successful?.Invoke(doc);
        //}
    }
}
