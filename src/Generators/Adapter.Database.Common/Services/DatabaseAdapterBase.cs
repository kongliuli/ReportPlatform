using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Adapter.Database.Common.Services;

/// <summary>数据库适配器基类，提供数据库查询和数据映射功能</summary>
public class DatabaseAdapterBase
{
    private readonly IDatabaseProvider _provider;
    private readonly DatabaseAdapterConfig _config;
    private readonly ConnectionPoolManager? _poolManager;

    /// <summary>适配器显示名称</summary>
    public string AdapterName => _config.DisplayName;
    /// <summary>适配器类型</summary>
    public AdapterType Type => AdapterType.Database;
    /// <summary>目标数据路径列表</summary>
    public IReadOnlyList<string> TargetDataPaths => _config.DbFieldMappings.Select(m => m.TargetDataPath).Distinct().ToList().AsReadOnly();

    /// <summary>初始化数据库适配器</summary>
    /// <param name="provider">数据库提供者</param>
    /// <param name="config">数据库适配器配置</param>
    /// <param name="poolManager">连接池管理器（可选）</param>
    public DatabaseAdapterBase(IDatabaseProvider provider, DatabaseAdapterConfig config, ConnectionPoolManager? poolManager = null)
    {
        _provider = provider;
        _config = config;
        _poolManager = poolManager;
    }

    /// <summary>异步读取单行数据</summary>
    /// <returns>适配器结果</returns>
    public async Task<AdapterResult> ReadDataAsync()
    {
        try
        {
            var (sql, parameters) = SqlBuilder.BuildParameterizedQuery(_config.Query, _config.Joins, _provider, _config.Parameters);
            if (_poolManager != null)
            {
                await using var pooled = await _poolManager.AcquireAsync(_provider, _config.ConnectionString);
                return await ExecuteReadAsync(pooled.Connection, sql, parameters);
            }
            using var conn = _provider.CreateConnection(_config.ConnectionString);
            await conn.OpenAsync();
            return await ExecuteReadAsync(conn, sql, parameters);
        }
        catch (DbException ex)
        {
            return new AdapterResult { Success = false, ErrorMessage = $"数据库连接失败: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new AdapterResult { Success = false, ErrorMessage = $"查询执行失败: {ex.Message}" };
        }
    }

    /// <summary>异步读取批量数据</summary>
    /// <returns>适配器结果</returns>
    public async Task<AdapterResult> ReadBatchDataAsync()
    {
        try
        {
            var (sql, parameters) = SqlBuilder.BuildParameterizedQuery(_config.Query, _config.Joins, _provider, _config.Parameters);
            if (_poolManager != null)
            {
                await using var pooled = await _poolManager.AcquireAsync(_provider, _config.ConnectionString);
                return await ExecuteBatchReadAsync(pooled.Connection, sql, parameters);
            }
            using var conn = _provider.CreateConnection(_config.ConnectionString);
            await conn.OpenAsync();
            return await ExecuteBatchReadAsync(conn, sql, parameters);
        }
        catch (DbException ex)
        {
            return new AdapterResult { Success = false, ErrorMessage = $"数据库连接失败: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new AdapterResult { Success = false, ErrorMessage = $"查询执行失败: {ex.Message}" };
        }
    }

    /// <summary>异步校验配置有效性</summary>
    /// <returns>校验结果</returns>
    public Task<ValidationResult> ValidateConfigAsync()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(_config.ConnectionString))
            errors.Add("ConnectionString is required");
        if (_config.Query.Mode == QueryMode.VisualBuilder && string.IsNullOrWhiteSpace(_config.Query.PrimaryTable))
            errors.Add("PrimaryTable is required in VisualBuilder mode");

        return errors.Count > 0
            ? Task.FromResult(ValidationResult.Fail(errors.ToArray()))
            : Task.FromResult(ValidationResult.Success);
    }

    /// <summary>异步预览查询数据</summary>
    /// <param name="limit">预览行数限制</param>
    /// <returns>适配器结果</returns>
    public async Task<AdapterResult> PreviewAsync(int limit = 10)
    {
        try
        {
            var (sql, parameters) = SqlBuilder.BuildParameterizedQuery(_config.Query, _config.Joins, _provider, _config.Parameters);
            sql = _provider.BuildPagedQuery(sql, 0, limit);
            if (_poolManager != null)
            {
                await using var pooled = await _poolManager.AcquireAsync(_provider, _config.ConnectionString);
                return await ExecuteBatchReadAsync(pooled.Connection, sql, parameters);
            }
            using var conn = _provider.CreateConnection(_config.ConnectionString);
            await conn.OpenAsync();
            return await ExecuteBatchReadAsync(conn, sql, parameters);
        }
        catch (Exception ex)
        {
            return new AdapterResult { Success = false, ErrorMessage = $"预览失败: {ex.Message}" };
        }
    }

    private async Task<AdapterResult> ExecuteReadAsync(DbConnection conn, string sql, List<(string name, object value)> parameters)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameters(cmd, parameters);
        using var reader = await cmd.ExecuteReaderAsync();
        return MapResult(reader);
    }

    private async Task<AdapterResult> ExecuteBatchReadAsync(DbConnection conn, string sql, List<(string name, object value)> parameters)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameters(cmd, parameters);
        using var reader = await cmd.ExecuteReaderAsync();
        return MapBatchResult(reader);
    }

    private void AddParameters(DbCommand cmd, List<(string name, object value)> parameters)
    {
        if (parameters == null) return;
        var prefix = _provider.GetParameterPrefix();
        foreach (var (name, value) in parameters)
        {
            var dbParam = cmd.CreateParameter();
            dbParam.ParameterName = $"{prefix}{name.TrimStart(prefix[0])}";
            dbParam.Value = value;
            cmd.Parameters.Add(dbParam);
        }
    }

    private AdapterResult MapResult(DbDataReader reader)
    {
        var data = new Dictionary<string, object>();
        if (reader.Read())
        {
            foreach (var mapping in _config.DbFieldMappings)
            {
                var ordinal = FindOrdinal(reader, mapping.ColumnName);
                if (ordinal >= 0)
                {
                    var value = reader.GetValue(ordinal);
                    if (value != DBNull.Value)
                    {
                        data[mapping.TargetDataPath] = ApplyTransform(value, mapping.Transform);
                    }
                }
            }
        }
        return new AdapterResult { Success = true, Data = data };
    }

    private AdapterResult MapBatchResult(DbDataReader reader)
    {
        var batchData = new List<Dictionary<string, object>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            foreach (var mapping in _config.DbFieldMappings)
            {
                var ordinal = FindOrdinal(reader, mapping.ColumnName);
                if (ordinal >= 0)
                {
                    var value = reader.GetValue(ordinal);
                    if (value != DBNull.Value)
                    {
                        row[mapping.TargetDataPath] = ApplyTransform(value, mapping.Transform);
                    }
                }
            }
            batchData.Add(row);
        }
        return new AdapterResult { Success = true, BatchData = batchData };
    }

    private static int FindOrdinal(DbDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static object ApplyTransform(object value, string transform)
    {
        if (string.IsNullOrWhiteSpace(transform)) return value;
        return transform.Trim().ToUpperInvariant() switch
        {
            "TRIM()" => value.ToString()?.Trim() ?? value,
            "UPPER()" => value.ToString()?.ToUpper() ?? value,
            "LOWER()" => value.ToString()?.ToLower() ?? value,
            _ => value
        };
    }
}
