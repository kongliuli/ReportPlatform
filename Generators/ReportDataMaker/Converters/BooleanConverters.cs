using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReportDataMaker.Converters;

/// <summary>布尔值到可见性转换器，true=Visible, false=Collapsed</summary>
public class Boolean2VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}

/// <summary>布尔值取反转换器</summary>
public class Boolean2BooleanReConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }
}
