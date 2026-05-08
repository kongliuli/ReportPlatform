using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Services;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;
using ReportDataMaker.ViewModels;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker;

/// <summary>应用程序入口类，负责依赖注入配置和启动</summary>
public partial class App : Application
{
    /// <summary>全局服务提供者实例</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>应用程序启动时初始化依赖注入和主窗口</summary>
    /// <param name="e">启动事件参数</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddSingleton<IDialogService, DialogService>();

        services.AddSingleton<ITemplateLoaderService, TemplateLoaderService>();
        services.AddSingleton<ITemplatePreviewService, TemplatePreviewService>();
        services.AddSingleton<IDataBindingService, DataBindingService>();
        services.AddSingleton<AdapterConfigStore>();
        services.AddSingleton<ExcelAdapterFactory>();
        services.AddSingleton<DatabaseProviderRegistry>(sp => DatabaseProviderRegistry.CreateDefault());
        services.AddSingleton<DatabaseAdapterFactory>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<TemplateLoadViewModel>();
        services.AddTransient<SidePanelViewModel>();

        Services = services.BuildServiceProvider();
        ServiceLocator.Initialize(services);

        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}
