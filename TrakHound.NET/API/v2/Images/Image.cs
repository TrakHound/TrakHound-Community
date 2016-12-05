using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrakHound.API.v2.Images
{
    public class Image
    {
        public string Filename { get; set; }
        public byte[] Bytes { get; set; }
    }
}
