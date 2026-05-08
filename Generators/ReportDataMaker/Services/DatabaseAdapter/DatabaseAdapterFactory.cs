using System.Collections.Generic;
using System.Threading.Tasks;
using ReportDataMaker.Models;
using ReportDataMaker.Services.ExcelAdapter;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class DatabaseAdapterFactory
{
    private readonly DatabaseProviderRegistry _providerRegistry;
    private readonly TemplateFlattenService _flattener = new();

    public DatabaseAdapterFactory(DatabaseProviderRegistry providerRegistry)
    {
        _providerRegistry = providerRegistry;
    }

    public AdapterType Type => AdapterType.Database;

    public DatabaseProviderRegistry ProviderRegistry => _providerRegistry;

    public TemplateFieldSchema FlattenTemplate(ExternalTemplateDefinition template)
        => _flattener.Flatten(template);

    public IDatabaseProvider GetProvider(DatabaseProvider providerType)
        => _providerRegistry.Get(providerType);

    public IReadOnlyList<IDatabaseProvider> GetAllProviders()
        => _providerRegistry.GetAll();

    public DatabaseAdapterBase CreateAdapter(DatabaseAdapterConfig config)
    {
        var provider = _providerRegistry.Get(config.Provider);
        return new DatabaseAdapterBase(provider, config);
    }

    public async Task<bool> TestConnectionAsync(DatabaseProvider providerType, string connectionString)
    {
        var provider = _providerRegistry.Get(providerType);
        return await provider.TestConnectionAsync(connectionString);
    }

    public async Task<List<TableInfo>> GetTablesAsync(DatabaseProvider providerType, string connectionString)
    {
        var provider = _providerRegistry.Get(providerType);
        return await provider.GetTablesAsync(connectionString);
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(DatabaseProvider providerType, string connectionString, string tableName)
    {
        var provider = _providerRegistry.Get(providerType);
        return await provider.GetColumnsAsync(connectionString, tableName);
    }

    public async Task<AdapterResult> ExecuteQueryAsync(DatabaseAdapterConfig config)
    {
        var adapter = CreateAdapter(config);
        return await adapter.ReadDataAsync();
    }

    public async Task<AdapterResult> PreviewAsync(DatabaseAdapterConfig config, int limit = 10)
    {
        var adapter = CreateAdapter(config);
        return await adapter.PreviewAsync(limit);
    }

    public async Task<ValidationResult> ValidateConfigAsync(DatabaseAdapterConfig config)
    {
        var adapter = CreateAdapter(config);
        return await adapter.ValidateConfigAsync();
    }
}
