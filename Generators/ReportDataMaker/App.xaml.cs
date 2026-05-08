using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ReportDataMaker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动应用程序时发生错误: {ex.Message}\n\n{ex.StackTrace}", "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show($"未处理的异常: {ex?.Message ?? "未知错误"}\n\n{ex?.StackTrace ?? ""}", "运行时错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

