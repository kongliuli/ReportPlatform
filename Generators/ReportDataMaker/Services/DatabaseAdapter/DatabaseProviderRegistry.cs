using System;
using System.Collections.Generic;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class DatabaseProviderRegistry
{
    private readonly Dictionary<DatabaseProvider, IDatabaseProvider> _providers = new();

    public void Register(IDatabaseProvider provider)
    {
        _providers[provider.ProviderType] = provider;
    }

    public IDatabaseProvider Get(DatabaseProvider providerType)
    {
        if (_providers.TryGetValue(providerType, out var provider))
            return provider;
        throw new NotSupportedException($"Database provider '{providerType}' is not registered.");
    }

    public IReadOnlyList<IDatabaseProvider> GetAll() => _providers.Values.ToList().AsReadOnly();

    public static DatabaseProviderRegistry CreateDefault()
    {
        var registry = new DatabaseProviderRegistry();
        registry.Register(new SqlServerProvider());
        registry.Register(new MySqlProvider());
        registry.Register(new SqliteProvider());
        registry.Register(new PostgreSqlProvider());
        return registry;
    }
}
