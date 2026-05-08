using System.Collections.Generic;
using System.Threading.Tasks;
using ReportDataMaker.Models;
using ReportDataMaker.Services.ExcelAdapter;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.DatabaseAdapter;

/// <summary>数据库适配器工厂，提供数据库连接、查询和配置管理功能</summary>
public class DatabaseAdapterFactory
{
    private readonly DatabaseProviderRegistry _providerRegistry;
    private readonly TemplateFlattenService _flattener = new();

    /// <summary>初始化数据库适配器工厂</summary>
    /// <param name="providerRegistry">数据库提供者注册表</param>
    public DatabaseAdapterFactory(DatabaseProviderRegistry providerRegistry)
    {
        _providerRegistry = providerRegistry;
    }

    /// <summary>适配器类型</summary>
    public AdapterType Type => AdapterType.Database;

    /// <summary>数据库提供者注册表</summary>
    public DatabaseProviderRegistry ProviderRegistry => _providerRegistry;

    /// <summary>将外部模板定义扁平化为字段模式</summary>
    /// <param name="template">外部模板定义</param>
    /// <returns>模板字段模式</returns>
    public TemplateFieldSchema FlattenTemplate(ExternalTemplateDefinition template)
        => _flattener.Flatten(template);

    /// <summary>获取指定类型的数据库提供者</summary>
    /// <param name="providerType">数据库提供者类型</param>
    /// <returns>数据库提供者实例</returns>
    public IDatabaseProvider GetProvider(DatabaseProvider providerType)
        => _providerRegistry.Get(providerType);

    /// <summary>获取所有已注册的数据库提供者</summary>
    /// <returns>数据库提供者只读列表</returns>
    public IReadOnlyList<IDatabaseProvider> GetAllProviders()
        => _providerRegistry.GetAll();

    /// <summary>根据配置创建数据库适配器</summary>
    /// <param name="config">数据库适配器配置</param>
    /// <returns>数据库适配器实例</returns>
    public DatabaseAdapterBase CreateAdapter(DatabaseAdapterConfig config)
    {
        var provider = _providerRegistry.Get(config.Provider);
        return new DatabaseAdapterBase(provider, config);
    }

    /// <summary>异步测试数据库连接</summary>
    /// <param name="providerType">数据库提供者类型</param>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>连接是否成功</returns>
    public async Task<bool> TestConnectionAsync(DatabaseProvider providerType, string connectionString)
    {
        var provider = _providerRegistry.Get(providerType);
        return await provider.TestConnectionAsync(connectionString);
    }

    /// <summary>异步获取数据库表列表</summary>
    /// <param name="providerType">数据库提供者类型</param>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>表信息列表</returns>
    public async Task<List<TableInfo>> GetTablesAsync(DatabaseProvider providerType, string connectionString)
    {
        var provider = _providerRegistry.Get(providerType);
        return await provider.GetTablesAsync(connectionString);
    }

    /// <summary>异步获取指定表的列信息</summary>
    /// <param name="providerType">数据库提供者类型</param>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>列信息列表</returns>
    public async Task<List<ColumnInfo>> GetColumnsAsync(DatabaseProvider providerType, string connectionString, string tableName)
    {
        var provider = _providerRegistry.Get(providerType);
        return await provider.GetColumnsAsync(connectionString, tableName);
    }

    /// <summary>异步执行查询并返回结果</summary>
    /// <param name="config">数据库适配器配置</param>
    /// <returns>适配器结果</returns>
    public async Task<AdapterResult> ExecuteQueryAsync(DatabaseAdapterConfig config)
    {
        var adapter = CreateAdapter(config);
        return await adapter.ReadDataAsync();
    }

    /// <summary>异步预览查询结果</summary>
    /// <param name="config">数据库适配器配置</param>
    /// <param name="limit">预览行数限制</param>
    /// <returns>适配器结果</returns>
    public async Task<AdapterResult> PreviewAsync(DatabaseAdapterConfig config, int limit = 10)
    {
        var adapter = CreateAdapter(config);
        return await adapter.PreviewAsync(limit);
    }

    /// <summary>异步校验数据库适配器配置</summary>
    /// <param name="config">数据库适配器配置</param>
    /// <returns>校验结果</returns>
    public async Task<ValidationResult> ValidateConfigAsync(DatabaseAdapterConfig config)
    {
        var adapter = CreateAdapter(config);
        return await adapter.ValidateConfigAsync();
    }
}
