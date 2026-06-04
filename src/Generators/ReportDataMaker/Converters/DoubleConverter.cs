using System;
using System.Globalization;
using System.Windows.Data;

namespace ReportDataMaker.Converters;

/// <summary>双精度数值转换器，支持乘以参数值</summary>
public class DoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue && double.TryParse(parameter?.ToString(), out var multiplier))
        {
            return doubleValue * multiplier;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue && double.TryParse(parameter?.ToString(), out var multiplier) && multiplier != 0)
        {
            return doubleValue / multiplier;
        }
        return value;
    }
}