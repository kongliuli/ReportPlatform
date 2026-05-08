using System.Collections.Generic;

namespace ReportDataMaker.Models
{
    public class TableElement : ElementBase
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<TableCell> Cells { get; set; }
        public List<TableColumn> ColumnsConfig { get; set; }
        public List<double> ColumnWidths { get; set; }
        public List<double> RowHeights { get; set; }
        public string BorderColor { get; set; }
        public double BorderWidth { get; set; }
        public double CellSpacing { get; set; }
        public double CellPadding { get; set; }
        public string BackgroundColor { get; set; }
    }

    public class TableCell
    {
        public string Id { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string Content { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string FontWeight { get; set; }
        public string ForegroundColor { get; set; }
        public string BackgroundColor { get; set; }
        public string TextAlignment { get; set; }
        public string VerticalAlignment { get; set; }
        public string DataBindingPath { get; set; }
        public string FormatString { get; set; }
        public bool IsEditable { get; set; }
    }

    public class TableColumn
    {
        public int ColumnIndex { get; set; }
        public int Type { get; set; }
        public List<string> DropdownOptions { get; set; }
        public bool IsEditable { get; set; }
        public string DefaultValue { get; set; }
    }
}