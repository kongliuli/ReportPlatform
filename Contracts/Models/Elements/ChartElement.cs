namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>图表元素</summary>
public class ChartElement : ExternalElementBase
{
    /// <summary>图表类型</summary>
    public string? ChartType { get; set; }

    /// <summary>图表标题</summary>
    public string? Title { get; set; }

    /// <summary>数据源路径</summary>
    public string? DataSource { get; set; }

    /// <summary>是否显示图例</summary>
    public bool ShowLegend { get; set; } = true;

    /// <summary>是否显示网格</summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>标签列表</summary>
    public List<string>? Labels { get; set; }

    /// <summary>数据系列列表</summary>
    public List<ChartDataSeries>? Series { get; set; }
}

/// <summary>图表数据系列</summary>
public class ChartDataSeries
{
    /// <summary>系列名称</summary>
    public string? Name { get; set; }

    /// <summary>系列数据值列表</summary>
    public List<double> Values { get; set; } = new();

    /// <summary>系列颜色</summary>
    public string? Color { get; set; }
}
