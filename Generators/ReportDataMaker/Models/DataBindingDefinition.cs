namespace ReportDataMaker.Models
{
    public class DataBindingDefinition
    {
        public string Id { get; set; }
        public string ElementId { get; set; }
        public string DataPath { get; set; }
        public string BindingType { get; set; }
        public string FormatString { get; set; }
        public string DefaultValue { get; set; }
    }
}
