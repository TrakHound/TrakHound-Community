// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace TrakHound.Tools.Web
{
    public static class Download
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Image Image(string url)
        {
            Image result = null;

            if (!string.IsNullOrEmpty(url))
            {
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        byte[] data = webClient.DownloadData(url);

                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            result = System.Drawing.Image.FromStream(mem);
                        }
                    }
                    catch (Exception ex) { logger.Error(ex); }
                }
            }

            return result;
        }

    }
}
