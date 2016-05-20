using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.Net;
using System.IO;

namespace TH_Global.Web
{
    public static class Download
    {

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
                    catch (Exception ex) { Logger.Log("Exception : " + ex.Message); }
                }
            }

            return result;
        }

    }
}
