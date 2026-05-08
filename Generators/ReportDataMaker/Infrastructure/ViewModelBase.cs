using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReportDataMaker.Infrastructure;

/// <summary>视图模型基类，实现INotifyPropertyChanged接口提供属性变更通知</summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>属性变更事件</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>触发属性变更通知</summary>
    /// <param name="propertyName">属性名称</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>设置属性值并触发变更通知</summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="field">属性字段引用</param>
    /// <param name="value">新值</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>值是否发生变化</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
