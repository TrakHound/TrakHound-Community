using System;
using System.Data;

namespace TH_History
{
    public class GraphInfo
    {
        public string Name { get; set; }

        public DataTable Table { get; set; }

        public string CategoryColumn { get; set; }
        public string DataColumn { get; set; }

        public string SortColumn { get; set; }

        public string SortBegin { get; set; }
        public string SortEnd { get; set; }
    }
}
