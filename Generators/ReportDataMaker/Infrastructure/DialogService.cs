using System.Windows;
using Microsoft.Win32;

namespace ReportDataMaker.Infrastructure;

public class DialogService : IDialogService
{
    public string? OpenFile(string filter, string title)
    {
        var dialog = new OpenFileDialog { Filter = filter, Title = title };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFile(string filter, string title, string? defaultName = null)
    {
        var dialog = new SaveFileDialog { Filter = filter, Title = title, FileName = defaultName ?? string.Empty };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool Confirm(string message, string title)
    {
        return System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public void ShowInfo(string message, string title)
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title)
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowSuccess(string message, string title = "成功")
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
