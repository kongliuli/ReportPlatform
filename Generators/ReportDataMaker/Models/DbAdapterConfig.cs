using System.Collections.Generic;

namespace ReportDataMaker.Models
{
    public class DbAdapterConfig
    {
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public List<DbMapping> Mappings { get; set; }
    }

    public class DbMapping
    {
        public string DataPath { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public string Where { get; set; }
    }
}
