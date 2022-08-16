// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Net;
using System.Net.Http;
using System.Linq;

namespace TrakHound.API.v2.Images
{
    public class DeviceImage
    {
        public static Image Download(string manufacturer)
        {
            //return Download(manufacturer, null);
            return null;
        }

        public static Image Download(string manufacturer, string model)
        {
            //return Download(manufacturer, model);
            return null;
        }

        //public static Image Download(string manufacturer, string model)
        //{
        //    var client = new RestClient("https://images.trakhound.com/");

        //    var request = new RestRequest("device-image", Method.GET);
        //    request.AddParameter("manufacturer", manufacturer);
        //    if (!string.IsNullOrEmpty(model)) request.AddParameter("model", model);

        //    var response = client.Execute(request);
        //    if (response != null)
        //    {
        //        var fileHeader = response.Headers.ToList().Find(o => o.Name == "Content-Disposition");
        //        if (fileHeader != null)
        //        {
        //            string val = fileHeader.Value.ToString();
        //            string s = "filename";
        //            int b = val.IndexOf(s);
        //            b += s.Length; // Add the length of the search term
        //            b += 1; // Add the '='
        //            b += 1; // Add the '"'

        //            int e = val.IndexOf('"', b); // Get index of trailing '"'
        //            string filename = val.Substring(b, e - b); // Create substring representing the filename

        //            if (!string.IsNullOrEmpty(filename))
        //            {
        //                var image = new Image();
        //                image.Filename = filename;
        //                image.Bytes = response.RawBytes;
        //                return image;
        //            }
        //        }
        //    }

        //    return null;
        //}

    }
}
