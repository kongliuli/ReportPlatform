using System;
using System.Collections.Generic;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Adapter.Database.Common.Services;

/// <summary>数据库提供者注册表，管理所有已注册的数据库提供者</summary>
public class DatabaseProviderRegistry
{
    private readonly Dictionary<DatabaseProvider, IDatabaseProvider> _providers = new();

    /// <summary>注册数据库提供者</summary>
    /// <param name="provider">数据库提供者实例</param>
    public void Register(IDatabaseProvider provider)
    {
        _providers[provider.ProviderType] = provider;
    }

    /// <summary>获取指定类型的数据库提供者</summary>
    /// <param name="providerType">数据库提供者类型</param>
    /// <returns>数据库提供者实例</returns>
    public IDatabaseProvider Get(DatabaseProvider providerType)
    {
        if (_providers.TryGetValue(providerType, out var provider))
            return provider;
        throw new NotSupportedException($"Database provider '{providerType}' is not registered.");
    }

    /// <summary>获取所有已注册的数据库提供者</summary>
    /// <returns>数据库提供者只读列表</returns>
    public IReadOnlyList<IDatabaseProvider> GetAll() => _providers.Values.ToList().AsReadOnly();
}
