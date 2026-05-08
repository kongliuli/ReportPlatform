using System.Collections.Generic;

namespace ReportDataMaker.Models;

public class ReportDataContext
{
    public Dictionary<string, object> Patient { get; set; } = new();
    public Dictionary<string, object> Report { get; set; } = new();
    public Dictionary<string, object> Doctor { get; set; } = new();
    public Dictionary<string, object> Hospital { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public List<Dictionary<string, object>> Items { get; set; } = new();
}
