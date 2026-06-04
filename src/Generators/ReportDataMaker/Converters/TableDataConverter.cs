using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ReportDataMaker.Converters;

[ValueConversion(typeof(List<List<string>>), typeof(DataTable))]
public class TableDataConverter : IValueConverter, IMultiValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not List<List<string>> cellData || cellData.Count == 0)
            return new DataTable();

        var headerRows = parameter is int h ? h : (parameter is string s && int.TryParse(s, out var parsed) ? parsed : 0);
        return BuildDataTable(cellData, headerRows);
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DataViewToList(value);
    }

    object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return new DataTable();

        var cellData = values[0] as List<List<string>>;
        var headerRows = values[1] is int h ? h : 0;

        if (cellData == null || cellData.Count == 0)
            return new DataTable();

        return BuildDataTable(cellData, headerRows);
    }

    object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { DataViewToList(value), Binding.DoNothing };
    }

    private static DataTable BuildDataTable(List<List<string>> cellData, int headerRows)
    {
        int maxCols = cellData.Max(row => row.Count);
        if (maxCols == 0) return new DataTable();

        var table = new DataTable();

        for (int c = 0; c < maxCols; c++)
        {
            string colName;
            if (headerRows > 0 && cellData.Count > 0 && c < cellData[0].Count && !string.IsNullOrEmpty(cellData[0][c]))
                colName = cellData[0][c];
            else
                colName = $"列{c + 1}";
            table.Columns.Add(colName, typeof(string));
        }

        int startRow = headerRows;
        for (int r = startRow; r < cellData.Count; r++)
        {
            var row = cellData[r];
            var dataRow = table.NewRow();
            for (int c = 0; c < row.Count && c < maxCols; c++)
                dataRow[c] = row[c] ?? string.Empty;
            table.Rows.Add(dataRow);
        }

        return table;
    }

    private static object DataViewToList(object value)
    {
        if (value is not DataView dataView || dataView.Table == null)
            return value;

        var result = new List<List<string>>();
        foreach (DataRowView rowView in dataView)
        {
            var row = new List<string>();
            foreach (var item in rowView.Row.ItemArray)
                row.Add(item?.ToString() ?? string.Empty);
            result.Add(row);
        }
        return result;
    }
}
