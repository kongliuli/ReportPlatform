using System.Collections.Generic;

namespace ReportDataMaker.Models
{
    public class ReportDataContext
    {
        public Dictionary<string, object> Patient { get; set; }
        public Dictionary<string, object> Report { get; set; }
        public Dictionary<string, object> Doctor { get; set; }
        public Dictionary<string, object> Hospital { get; set; }
        public List<Dictionary<string, object>> Items { get; set; }
    }
}
