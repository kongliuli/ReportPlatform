using System.Collections.Generic;

namespace ReportDataMaker.Models;

public class TableElement
{
    public string? Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<TableCell> Cells { get; set; } = new();
    public int HeaderRows { get; set; } = 1;
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
    public bool IsEditable { get; set; }
}
