// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

//using System.IO;
//using System.Net;
using System.Threading;
using System.Threading.Tasks;

using TH_MTC_Data;
using TH_Global;
using TH_Global.Web;

namespace TH_MTC_Requests
{
    public class Stream
    {

        #region "Public"

        public Stream() { }

        public bool Verbose { get; set; }

        public bool InsureDelivery { get; set; }

        #region "Events"

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public event StreamResponse_Handler ResponseReceived;
        public event Error_Handler ResponseError;

        #endregion

        #region "Properties"

        //public Uri uri { get; set; }

        public string url { get; set; }

        //public int interval { get; set; }

        public int failureAttempts { get; set; }
        public int failureRetryInterval { get; set; }

        public int HttpTimeout { get; set; }

        public bool IsStarted = false;

        #endregion

        public void Start()
        {
            IsStarted = true;

            if (Started != null) Started();

            //stream_Start();

            //heartBeat_TIMER = new System.Timers.Timer();
            //heartBeat_TIMER.AutoReset = false;
            //heartBeat_TIMER.Elapsed += heartBeat_TIMER_Elapsed;

            //DoWork();
        }

        public void Stop()
        {
            //stop.Set();

            //if (heartBeat_TIMER != null) heartBeat_TIMER.Enabled = false;

            if (Stopped != null) Stopped();
        }

        #endregion

        #region "Methods"

        public void Run()
        {
            string response = HTTP.GetData(url);

            if (response != null)
            {
                if (ResponseReceived != null) ResponseReceived(response);
            }
            else
            {
                Error error = new Error();
                error.message = "Stream Connection Failed @ " + url;

                if (ResponseError != null) ResponseError(error);
            }
        }






        void Free()
        {
            stream_THREAD = null;

            stop.Close();
            stop = null;
        }

        #region "Worker"

        Thread stream_THREAD;

        ManualResetEvent stop = null;

        void stream_Start()
        {
            stop = new ManualResetEvent(false);

            if (stream_THREAD != null) stream_THREAD.Abort();

            stream_THREAD = new Thread(new ThreadStart(stream_Worker));
            stream_THREAD.Start();
        }

        void stream_Worker()
        {
            while (!stop.WaitOne(0, true))
            {
                //RequestReturn requestReturn = GetHttpRequest(uri);
                //if (requestReturn.success)
                //{
                //    if (ResponseReceived != null) ResponseReceived(requestReturn.result);

                //    if (interval > 0) Thread.Sleep(interval);
                //    else break;
                //}

                //string response = HTTP.GetData(url, InsureDelivery);

                //if (response != null)
                //{
                //    if (ResponseReceived != null) ResponseReceived(response);

                //    if (interval > 0) Thread.Sleep(interval);
                //    else stop.Set();
                //}
                //else if (interval < 1)
                //{
                //    Error error = new Error();
                //    error.message = "Stream Connection Failed";
                //    if (ResponseError != null) ResponseError(error);

                //    stop.Set();
                //}
                //else
                //{
                //    Error error = new Error();
                //    error.message = "Stream Connection Failed";
                //    if (ResponseError != null) ResponseError(error);

                //    Thread.Sleep(interval);
                //}
            } 
        }

        #endregion


        #region "OBSOLETE 11-25-15"

        //// Timer used for calling GetHttpRequest() at given interval
        //System.Timers.Timer heartBeat_TIMER;


        //CancellationTokenSource source = new CancellationTokenSource();
    

        //void heartBeat_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    DoWork();
        //}

        //void DoWork()
        //{
        //    //heartBeat_TIMER.Enabled = false;

        //    RequestReturn requestReturn = GetHttpRequest(uri);

        //    if (requestReturn.success)
        //    {
        //        tryCount = 0;

        //        if (interval > 0)
        //        {
        //            heartBeat_TIMER.Interval = interval;
        //            //if (IsStarted) heartBeat_TIMER.Enabled = true;
        //            //else if (Stopped != null) Stopped();
        //        }
        //        else
        //        {
        //            if (Stopped != null) Stopped();
        //        }

        //        if (ResponseReceived != null) ResponseReceived(requestReturn.result);
        //    }
        //    else
        //    {
        //        //if (IsStarted) heartBeat_TIMER.Enabled = true;
        //    }


        //    //else if (Stopped != null) Stopped();



        //    //if (IsStarted)
        //    //{
        //    //    heartBeat_TIMER.Enabled = false;

        //    //    RequestReturn requestReturn = GetHttpRequest(uri);

        //    //    if (requestReturn.success)
        //    //    {
        //    //        tryCount = 0;

        //    //        if (interval > 0)
        //    //        {
        //    //            heartBeat_TIMER.Interval = interval;
        //    //            if (IsStarted) heartBeat_TIMER.Enabled = true;
        //    //        }
        //    //        else
        //    //        {
        //    //            if (Stopped != null) Stopped();
        //    //        }

        //    //        if (ResponseReceived != null) ResponseReceived(requestReturn.result);
        //    //    }
        //    //    else if (Stopped != null) Stopped();
        //    //    //else
        //    //    //{
        //    //    //    //if (tryCount < Math.Max(3, failureAttempts))
        //    //    //    //{
        //    //    //    //    heartBeat_TIMER.Interval = Math.Max(1000, failureRetryInterval);
        //    //    //    //    if (IsStarted) heartBeat_TIMER.Enabled = true;
        //    //    //    //}
        //    //    //    //else
        //    //    //    //{
        //    //    //    tryCount = 0;

        //    //    //    // if interval > 0 then restart hearBeat with regular interval
        //    //    //    if (interval > 0)
        //    //    //    {
        //    //    //        heartBeat_TIMER.Interval = interval;
        //    //    //        if (IsStarted) heartBeat_TIMER.Enabled = true;
        //    //    //    }
        //    //    //    else
        //    //    //    {
        //    //    //        if (Stopped != null) Stopped();
        //    //    //    }

        //    //    //    // if still failed after failureAttempts is exceeded then raise event with no data
        //    //    //    if (ResponseReceived != null) ResponseReceived(null);

        //    //    //    //}
        //    //    //}
        //    //}
        //}

        #endregion





        class RequestReturn
        {
            public bool success { get; set; }
            public string result { get; set; }
        }

        int tryCount = 0;

        /// <summary>
        /// Gets HttpRequest Response as string
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>Response string</returns>
        //RequestReturn GetHttpRequest(Uri uri)
        //{
        //    RequestReturn Result = new RequestReturn();

        //    Result.success = false;

        //    if (uri != null)
        //    {
        //        try
        //        {
        //            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(uri);
        //            Request.KeepAlive = false;

        //            int timeout = 30000;
        //            if (HttpTimeout > 0) timeout = HttpTimeout;

        //            Request.Timeout = timeout;
        //            Request.ContinueTimeout = timeout;
        //            Request.ReadWriteTimeout = timeout;

        //            using (WebResponse Response = Request.GetResponse())
        //            {
        //                using (StreamReader Reader = new StreamReader(Response.GetResponseStream()))
        //                {
        //                    Result.result = Reader.ReadToEnd();
        //                    Result.success = true;
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            tryCount += 1;

        //            Error error = new Error();
        //            error.message = e.Message;

        //            if (ResponseError != null) ResponseError(error);

        //            if (Verbose) Console.WriteLine("TH_MTC_Requests.Streams.GetHttpRequest() : " + e.Message);
        //        }
        //    }

        //    return Result;
        //}

        #endregion

    }
}
