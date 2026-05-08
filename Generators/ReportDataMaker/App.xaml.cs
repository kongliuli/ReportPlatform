using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Services;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;
using ReportDataMaker.ViewModels;

namespace ReportDataMaker;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

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

        var mainWindow = new Views.MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}
