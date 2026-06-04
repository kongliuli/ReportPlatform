using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReportDataMaker.Converters;

/// <summary>布尔值到可见性转换器，true转换为Visible，false转换为Collapsed</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>将布尔值转换为可见性</summary>
    /// <param name="value">源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>可见性值</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }
    /// <summary>将可见性转换回布尔值</summary>
    /// <param name="value">源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>布尔值</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}

/// <summary>反向布尔值到可见性转换器，true转换为Collapsed，false转换为Visible</summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>将布尔值反向转换为可见性</summary>
    /// <param name="value">源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>可见性值</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }
    /// <summary>将可见性反向转换回布尔值</summary>
    /// <param name="value">源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>布尔值</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not Visibility.Visible;
    }
}

/// <summary>布尔值到宽度转换器，根据布尔值选择不同的宽度值</summary>
public class BoolToWidthConverter : IValueConverter
{
    /// <summary>将布尔值转换为宽度</summary>
    /// <param name="value">源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数，格式为"真值宽度,假值宽度"</param>
    /// <param name="culture">区域信息</param>
    /// <returns>宽度值</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string widths)
        {
            var parts = widths.Split(',');
            if (parts.Length == 2 && double.TryParse(parts[boolValue ? 0 : 1], out var w))
                return w;
        }
        return 240.0;
    }
    /// <summary>不支持反向转换</summary>
    /// <param name="value">源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域信息</param>
    /// <returns>不支持</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
