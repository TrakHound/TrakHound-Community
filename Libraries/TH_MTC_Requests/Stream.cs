// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.IO;
using System.Net;

using TH_MTC_Data;
using TH_Global;

namespace TH_MTC_Requests
{
    public class Stream
    {

        #region "Public"

        public Stream() { }

        #region "Events"

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public event StreamResponse_Handler ResponseReceived;

        #endregion

        #region "Properties"

        public Uri uri { get; set; }

        public int interval { get; set; }

        public int failureAttempts { get; set; }
        public int failureRetryInterval { get; set; }

        public int HttpTimeout { get; set; }

        public bool IsStarted = false;

        #endregion

        public void Start()
        {
            IsStarted = true;

            if (Started != null) Started();

            heartBeat_TIMER = new System.Timers.Timer();
            heartBeat_TIMER.Elapsed += heartBeat_TIMER_Elapsed;

            DoWork();
        }

        public void Stop()
        {
            IsStarted = false;

            if (heartBeat_TIMER != null) heartBeat_TIMER.Enabled = false;

            if (Stopped != null) Stopped();
        }

        #endregion

        #region "Methods"

        // Timer used for calling GetHttpRequest() at given interval
        System.Timers.Timer heartBeat_TIMER;

        void heartBeat_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DoWork();
        }

        void DoWork()
        {
            heartBeat_TIMER.Enabled = false;

            RequestReturn requestReturn = GetHttpRequest(uri);

            if (requestReturn.success)
            {
                tryCount = 0;

                if (interval > 0)
                {
                    heartBeat_TIMER.Interval = interval;
                    if (IsStarted) heartBeat_TIMER.Enabled = true;
                }
                else
                {
                    if (Stopped != null) Stopped();
                }

                if (ResponseReceived != null) ResponseReceived(requestReturn.result);
            }
            else
            {
                if (tryCount < Math.Max(3, failureAttempts))
                {
                    heartBeat_TIMER.Interval = Math.Max(1000, failureRetryInterval);
                    if (IsStarted) heartBeat_TIMER.Enabled = true;
                }
                else
                {
                    tryCount = 0;

                    // if interval > 0 then restart hearBeat with regular interval
                    if (interval > 0)
                    {
                        heartBeat_TIMER.Interval = interval;
                        if (IsStarted) heartBeat_TIMER.Enabled = true;
                    }
                    else
                    {
                        if (Stopped != null) Stopped();
                    }

                    // if still failed after failureAttempts is exceeded then raise event with no data
                    if (ResponseReceived != null) ResponseReceived(null);

                }
            }
        }

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
        RequestReturn GetHttpRequest(Uri uri)
        {
            RequestReturn Result = new RequestReturn();

            Result.success = false;

            if (uri != null)
            {
                try
                {
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(uri);
                    Request.KeepAlive = false;
                    Request.Timeout = Math.Max(30000, HttpTimeout);
                    Request.ContinueTimeout = Math.Max(30000, HttpTimeout);
                    Request.ReadWriteTimeout = Math.Max(30000, HttpTimeout);

                    using (WebResponse Response = Request.GetResponse())
                    {
                        using (StreamReader Reader = new StreamReader(Response.GetResponseStream()))
                        {
                            Result.result = Reader.ReadToEnd();
                            Result.success = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    tryCount += 1;
                    Logger.Log(e.Message);
                }
            }

            return Result;
        }

        #endregion

    }
}
