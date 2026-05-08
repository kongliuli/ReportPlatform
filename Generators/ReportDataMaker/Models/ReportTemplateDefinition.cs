using System.Collections.Generic;

namespace ReportDataMaker.Models
{
    public class ReportTemplateDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public string HospitalId { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public double MarginLeft { get; set; }
        public double MarginRight { get; set; }
        public double MarginTop { get; set; }
        public double MarginBottom { get; set; }
        public string Orientation { get; set; }
        public string BackgroundColor { get; set; }
        public List<ElementBase> Elements { get; set; }
    }
}