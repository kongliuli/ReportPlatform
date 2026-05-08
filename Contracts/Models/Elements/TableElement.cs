using Newtonsoft.Json;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>表格元素</summary>
public class TableElement : ExternalElementBase
{
    /// <summary>行数</summary>
    public int Rows { get; set; } = 3;

    /// <summary>列数</summary>
    public int Cols { get; set; } = 4;

    /// <summary>单元格列表</summary>
    public List<TableCell> Cells { get; set; } = new();

    /// <summary>表头行数</summary>
    public int HeaderRows { get; set; } = 1;

    /// <summary>边框颜色</summary>
    public string? BorderColor { get; set; }

    /// <summary>边框宽度</summary>
    public double BorderWidth { get; set; } = 1;

    /// <summary>是否显示网格线</summary>
    public bool GridLines { get; set; } = true;
}

/// <summary>表格单元格</summary>
public class TableCell
{
    /// <summary>所在行索引</summary>
    public int Row { get; set; }

    /// <summary>所在列索引</summary>
    public int Col { get; set; }

    /// <summary>跨行数</summary>
    public int RowSpan { get; set; } = 1;

    /// <summary>跨列数</summary>
    public int ColSpan { get; set; } = 1;

    /// <summary>单元格文本</summary>
    public string? Text { get; set; }

    /// <summary>数据绑定路径</summary>
    public string? DataPath { get; set; }

    /// <summary>水平对齐方式</summary>
    public string? Align { get; set; }

    /// <summary>垂直对齐方式</summary>
    public string? VerticalAlign { get; set; }

    /// <summary>背景颜色</summary>
    public string? BackgroundColor { get; set; }

    /// <summary>是否可编辑</summary>
    public bool IsEditable { get; set; }

    /// <summary>输入类型</summary>
    public string? InputType { get; set; }

    /// <summary>选项列表</summary>
    public List<string>? Options { get; set; }
}
