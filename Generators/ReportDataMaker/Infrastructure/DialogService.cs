using System.Windows;
using Microsoft.Win32;

namespace ReportDataMaker.Infrastructure;

/// <summary>对话框服务实现，提供文件选择和消息提示功能</summary>
public class DialogService : IDialogService
{
    /// <summary>打开文件选择对话框</summary>
    /// <param name="filter">文件筛选器</param>
    /// <param name="title">对话框标题</param>
    /// <returns>选中的文件路径，未选择返回null</returns>
    public string? OpenFile(string filter, string title)
    {
        var dialog = new OpenFileDialog { Filter = filter, Title = title };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <summary>打开文件保存对话框</summary>
    /// <param name="filter">文件筛选器</param>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultName">默认文件名</param>
    /// <returns>保存的文件路径，未选择返回null</returns>
    public string? SaveFile(string filter, string title, string? defaultName = null)
    {
        var dialog = new SaveFileDialog { Filter = filter, Title = title, FileName = defaultName ?? string.Empty };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <summary>显示确认对话框</summary>
    /// <param name="message">确认消息</param>
    /// <param name="title">对话框标题</param>
    /// <returns>用户是否确认</returns>
    public bool Confirm(string message, string title)
    {
        return System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    /// <summary>显示信息提示对话框</summary>
    /// <param name="message">信息内容</param>
    /// <param name="title">对话框标题</param>
    public void ShowInfo(string message, string title)
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>显示错误提示对话框</summary>
    /// <param name="message">错误信息</param>
    /// <param name="title">对话框标题</param>
    public void ShowError(string message, string title)
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>显示成功提示对话框</summary>
    /// <param name="message">成功信息</param>
    /// <param name="title">对话框标题</param>
    public void ShowSuccess(string message, string title = "成功")
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
