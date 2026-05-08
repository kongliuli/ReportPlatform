using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class DatabaseAdapterBase
{
    private readonly IDatabaseProvider _provider;
    private readonly DatabaseAdapterConfig _config;
    private readonly SqlBuilder _sqlBuilder = new();

    public string AdapterName => _config.DisplayName;
    public AdapterType Type => AdapterType.Database;
    public IReadOnlyList<string> TargetDataPaths => _config.DbFieldMappings.Select(m => m.TargetDataPath).Distinct().ToList().AsReadOnly();

    public DatabaseAdapterBase(IDatabaseProvider provider, DatabaseAdapterConfig config)
    {
        _provider = provider;
        _config = config;
    }

    public async Task<AdapterResult> ReadDataAsync()
    {
        try
        {
            using var conn = _provider.CreateConnection(_config.ConnectionString);
            await conn.OpenAsync();

            var sql = _sqlBuilder.Build(_config.Query, _config.Joins, _provider);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            ApplyParameters(cmd);

            using var reader = await cmd.ExecuteReaderAsync();
            return MapResult(reader);
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

    public async Task<AdapterResult> ReadBatchDataAsync()
    {
        try
        {
            using var conn = _provider.CreateConnection(_config.ConnectionString);
            await conn.OpenAsync();

            var sql = _sqlBuilder.Build(_config.Query, _config.Joins, _provider);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            ApplyParameters(cmd);

            using var reader = await cmd.ExecuteReaderAsync();
            return MapBatchResult(reader);
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

    public Task<ValidationResult> ValidateConfigAsync()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(_config.ConnectionString))
            errors.Add("ConnectionString is required");
        if (_config.Query.Mode == QueryMode.RawSql && string.IsNullOrWhiteSpace(_config.Query.RawSql))
            errors.Add("RawSql is required in RawSql mode");
        if (_config.Query.Mode == QueryMode.VisualBuilder && string.IsNullOrWhiteSpace(_config.Query.PrimaryTable))
            errors.Add("PrimaryTable is required in VisualBuilder mode");

        return errors.Count > 0
            ? Task.FromResult(ValidationResult.Fail(errors.ToArray()))
            : Task.FromResult(ValidationResult.Success);
    }

    public async Task<AdapterResult> PreviewAsync(int limit = 10)
    {
        try
        {
            using var conn = _provider.CreateConnection(_config.ConnectionString);
            await conn.OpenAsync();

            var sql = _sqlBuilder.Build(_config.Query, _config.Joins, _provider);
            sql = _provider.BuildPagedQuery(sql, 0, limit);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            ApplyParameters(cmd);

            using var reader = await cmd.ExecuteReaderAsync();
            return MapBatchResult(reader);
        }
        catch (Exception ex)
        {
            return new AdapterResult { Success = false, ErrorMessage = $"预览失败: {ex.Message}" };
        }
    }

    private void ApplyParameters(DbCommand cmd)
    {
        if (_config.Parameters == null) return;
        foreach (var param in _config.Parameters)
        {
            var dbParam = cmd.CreateParameter();
            dbParam.ParameterName = $"{_provider.GetParameterPrefix()}{param.Name.TrimStart(_provider.GetParameterPrefix()[0])}";
            dbParam.Value = (object?)param.DefaultValue ?? DBNull.Value;
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
