using ReportDataMaker.Services.ContextAdapter;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;

namespace ReportDataMaker.Services;

/// <summary>聚合适配器相关服务，减少构造器参数</summary>
public class AdapterServices
{
    public ExcelAdapterFactory ExcelFactory { get; }
    public DatabaseAdapterFactory DbFactory { get; }
    public ContextAdapterFactory ContextFactory { get; }
    public AdapterConfigStore ConfigStore { get; }

    public AdapterServices(ExcelAdapterFactory excelFactory, DatabaseAdapterFactory dbFactory, ContextAdapterFactory contextFactory, AdapterConfigStore configStore)
    {
        ExcelFactory = excelFactory;
        DbFactory = dbFactory;
        ContextFactory = contextFactory;
        ConfigStore = configStore;
    }
}
