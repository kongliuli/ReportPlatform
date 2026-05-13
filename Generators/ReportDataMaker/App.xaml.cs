using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Services;
using ReportDataMaker.Services.ContextAdapter;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;
using ReportDataMaker.Services.PdfExport;
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
        FileLogger.Initialize();
        FileLogger.Instance.WriteLine("[LOG] ===== App.OnStartup 开始 =====");
        FileLogger.Instance.WriteLine($"[LOG] 基准目录: {AppDomain.CurrentDomain.BaseDirectory}");

        base.OnStartup(e);

        FileLogger.Instance.WriteLine("[LOG] 开始注册 DI 服务...");
        var services = new ServiceCollection();

        services.AddSingleton<IDialogService, DialogService>();

        services.AddSingleton<ITemplateLoaderService, TemplateLoaderService>();
        services.AddSingleton<ITemplatePreviewService, TemplatePreviewService>();
        services.AddSingleton<IDataBindingService, DataBindingService>();
        services.AddSingleton<AdapterConfigStore>();
        services.AddSingleton<ExcelAdapterFactory>();
        services.AddSingleton<DatabaseProviderRegistry>(sp => DatabaseProviderRegistry.CreateDefault());
        services.AddSingleton<ConnectionPoolManager>();
        services.AddSingleton<DatabaseAdapterFactory>();
        services.AddSingleton<ContextProfileStore>();
        services.AddSingleton<ContextAdapterService>();
        services.AddSingleton<ContextAdapterFactory>();

        services.AddSingleton<IPdfExportService, PdfExportService>();
        services.AddSingleton<BatchExportService>();
        services.AddSingleton<ExportHistoryStore>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<TemplateLoadViewModel>();
        services.AddTransient<SidePanelViewModel>();

        FileLogger.Instance.WriteLine("[LOG] DI 注册完成，开始 BuildServiceProvider...");
        Services = services.BuildServiceProvider();
        FileLogger.Instance.WriteLine("[LOG] BuildServiceProvider 完成");

        FileLogger.Instance.WriteLine("[LOG] 执行 TestTemplateLoading...");
        TestTemplateLoading();
        FileLogger.Instance.WriteLine("[LOG] TestTemplateLoading 完成");

        FileLogger.Instance.WriteLine("[LOG] 创建 MainWindow...");
        MainViewModel mainVm;
        try
        {
            mainVm = Services.GetRequiredService<MainViewModel>();
            FileLogger.Instance.WriteLine("[LOG] MainViewModel 创建成功");
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] MainViewModel 创建失败: {ex.GetType().Name} - {ex.Message}");
            FileLogger.Instance.WriteLine($"[LOG] 堆栈: {ex.StackTrace}");
            throw;
        }

        var mainWindow = new MainWindow
        {
            DataContext = mainVm
        };
        FileLogger.Instance.WriteLine("[LOG] 调用 mainWindow.Show()...");
        mainWindow.Show();
        FileLogger.Instance.WriteLine("[LOG] ===== App.OnStartup 完成 =====");
    }

    private void TestTemplateLoading()
    {
        FileLogger.Instance.WriteLine("[LOG] ===== TestTemplateLoading =====");
        try
        {
            FileLogger.Instance.WriteLine("[LOG] 获取 ITemplateLoaderService...");
            var templateLoader = Services.GetRequiredService<ITemplateLoaderService>();
            var testPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "住院病历.json");
            FileLogger.Instance.WriteLine($"[LOG] 测试路径: {testPath}");
            
            if (File.Exists(testPath))
            {
                FileLogger.Instance.WriteLine("[LOG] 文件存在，开始加载...");
                var template = templateLoader.LoadFromFile(testPath);
                
                FileLogger.Instance.WriteLine($"[LOG] 加载成功！模板名称: {template.Name}, 元素数量: {template.Elements.Count}");
                FileLogger.Instance.WriteLine($"[LOG] 页面尺寸: {template.PageWidth}x{template.PageHeight}, DataBindings: {template.DataBindings.Count}");
                
                int dataPathCount = 0;
                for (int i = 0; i < template.Elements.Count; i++)
                {
                    var el = template.Elements[i];
                    var typeName = el.GetType().Name;
                    var dataPath = el.DataPath;
                    var label = el.Label ?? "(空)";
                    FileLogger.Instance.WriteLine($"[LOG]   元素[{i}]: {typeName} DataPath='{dataPath}' Label='{label}' Group={el.Group} DefaultValue='{el.DefaultValue}'");
                    if (!string.IsNullOrEmpty(dataPath)) dataPathCount++;
                }
                FileLogger.Instance.WriteLine($"[LOG] 有 DataPath 的元素: {dataPathCount}");
                
                // 额外检查：可录入元素数量
                var entryCount = template.Elements.Count(e => !string.IsNullOrEmpty(e.DataPath) || e.Group == ElementGroup.Editable);
                FileLogger.Instance.WriteLine($"[LOG] DataEntry 分组或 DataPath 非空的元素数: {entryCount}");
            }
            else
            {
                FileLogger.Instance.WriteLine($"[LOG] 文件不存在: {testPath}");
                FileLogger.Instance.WriteLine("[LOG] 列出 Templates 目录内容:");
                var templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
                if (Directory.Exists(templateDir))
                {
                    foreach (var f in Directory.GetFiles(templateDir))
                        FileLogger.Instance.WriteLine($"[LOG]   - {Path.GetFileName(f)}");
                }
                else
                {
                    FileLogger.Instance.WriteLine($"[LOG]   Templates 目录不存在: {templateDir}");
                }
            }
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] 加载失败: {ex.GetType().Name} - {ex.Message}");
            FileLogger.Instance.WriteLine($"[LOG] 堆栈: {ex.StackTrace}");
            if (ex.InnerException != null)
                FileLogger.Instance.WriteLine($"[LOG] 内部异常: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
        }
        FileLogger.Instance.WriteLine("[LOG] ===== TestTemplateLoading 结束 =====");
    }
    
}
