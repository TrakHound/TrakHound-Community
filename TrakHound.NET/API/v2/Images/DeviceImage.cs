using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RestSharp;

namespace TrakHound.API.v2.Images
{
    public class DeviceImage
    {
        public static Image Download(string manufacturer)
        {
            return Download(manufacturer, null);
        }

        public static Image Download(string manufacturer, string model)
        {
            var client = new RestClient("http://dev.trakhound.com/");

            var request = new RestRequest("api/images/device_images/download", Method.GET);
            request.AddParameter("manufacturer", manufacturer);
            if (!string.IsNullOrEmpty(model)) request.AddParameter("model", model);

            var response = client.Execute(request);
            if (response != null)
            {
                var fileHeader = response.Headers.ToList().Find(o => o.Name == "Content-Disposition");
                if (fileHeader != null)
                {
                    string val = fileHeader.Value.ToString();
                    string s = "filename";
                    int b = val.IndexOf(s);
                    b += s.Length; // Add the length of the search term
                    b += 1; // Add the '='
                    b += 1; // Add the '"'

                    int e = val.IndexOf('"', b); // Get index of trailing '"'
                    string filename = val.Substring(b, e - b); // Create substring representing the filename

                    if (!string.IsNullOrEmpty(filename))
                    {
                        var image = new Image();
                        image.Filename = filename;
                        image.Bytes = response.RawBytes;
                        return image;
                    }
                }
            }

            return null;
        }

    }
}
