using Newtonsoft.Json;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class TableElement : ExternalElementBase
{
    public int Rows { get; set; } = 3;
    
    public int Cols { get; set; } = 4;
    
    public List<TableCell> Cells { get; set; } = new();
    
    public int HeaderRows { get; set; } = 1;
    
    public string? BorderColor { get; set; }
    
    public double BorderWidth { get; set; } = 1;
    
    public bool GridLines { get; set; } = true;
}

public class TableCell
{
    public int Row { get; set; }
    
    public int Col { get; set; }
    
    public int RowSpan { get; set; } = 1;
    
    public int ColSpan { get; set; } = 1;
    
    public string? Text { get; set; }
    
    public string? DataPath { get; set; }
    
    public string? Align { get; set; }
    
    public string? VerticalAlign { get; set; }
    
    public string? BackgroundColor { get; set; }
    
    public bool IsEditable { get; set; }
    
    public string? InputType { get; set; }
    
    public List<string>? Options { get; set; }
}
