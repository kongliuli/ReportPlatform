namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 图表元素
/// </summary>
public class ChartElement : ExternalElementBase
{
    public string? ChartType { get; set; }
    
    public string? Title { get; set; }
    
    public string? DataSource { get; set; }
    
    public bool ShowLegend { get; set; } = true;
    
    public bool ShowGrid { get; set; } = true;
    
    public List<string>? Labels { get; set; }
    
    public List<ChartDataSeries>? Series { get; set; }
}

/// <summary>
/// 图表数据系列
/// </summary>
public class ChartDataSeries
{
    public string? Name { get; set; }
    
    public List<double> Values { get; set; } = new();
    
    public string? Color { get; set; }
}
