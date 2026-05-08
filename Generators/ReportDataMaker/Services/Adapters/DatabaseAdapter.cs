using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ReportDataMaker.Models;
using IDataAdapter = ReportDataMaker.Models.IDataAdapter;

namespace ReportDataMaker.Services.Adapters
{
    public class DatabaseAdapter : IDataAdapter
    {
        private readonly DbAdapterConfig _config;
        private readonly List<string> _targetPaths;
        private readonly DbProviderFactory _providerFactory;

        public string AdapterName => "DatabaseAdapter";

        public IReadOnlyList<string> TargetDataPaths => _targetPaths.AsReadOnly();

        public DatabaseAdapter(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException($"Database adapter config file not found: {configFilePath}");

            var jsonContent = File.ReadAllText(configFilePath);
            _config = JsonConvert.DeserializeObject<DbAdapterConfig>(jsonContent);

            if (_config?.Mappings == null || _config.Mappings.Count == 0)
                throw new InvalidOperationException("Database adapter config contains no mappings.");

            _targetPaths = _config.Mappings.Select(m => m.DataPath).Distinct().ToList();

            _providerFactory = ResolveProviderFactory(_config.Provider);
        }

        public Dictionary<string, object> ReadData(IReadOnlyDictionary<string, object> parameters)
        {
            var result = new Dictionary<string, object>();

            try
            {
                using var connection = _providerFactory.CreateConnection();
                connection.ConnectionString = _config.ConnectionString;
                connection.Open();

                var queryGroups = _config.Mappings
                    .GroupBy(m => new { m.Table, m.Where })
                    .ToList();

                foreach (var group in queryGroups)
                {
                    var columns = group.Select(m => m.Column).ToList();
                    var where = group.Key.Where ?? "1=1";
                    var table = group.Key.Table;

                    var columnList = string.Join(", ", columns.Select(c => $"\"{c}\""));
                    var sql = $"SELECT {columnList} FROM \"{table}\" WHERE {where}";

                    using var command = _providerFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = sql;

                    FillParameters(command, parameters);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            var value = reader.GetValue(i);
                            if (value != DBNull.Value)
                            {
                                var mapping = group.First(m => m.Column == columns[i]);
                                result[mapping.DataPath] = value;
                            }
                        }
                    }
                }
            }
            catch (DbException ex)
            {
                throw new DataAdapterException(
                    $"数据库连接失败: {ex.Message}\n请检查数据库配置或回退到手动录入模式。", ex);
            }

            return result;
        }

        private void FillParameters(DbCommand command, IReadOnlyDictionary<string, object> parameters)
        {
            foreach (var kvp in parameters)
            {
                var param = _providerFactory.CreateParameter();
                param.ParameterName = $"@{kvp.Key}";
                param.Value = kvp.Value ?? DBNull.Value;
                command.Parameters.Add(param);
            }
        }

        private static DbProviderFactory ResolveProviderFactory(string provider)
        {
            return provider?.ToLowerInvariant() switch
            {
                "sqlserver" => Microsoft.Data.SqlClient.SqlClientFactory.Instance,
                "mysql" => MySqlConnector.MySqlConnectorFactory.Instance,
                "sqlite" => Microsoft.Data.Sqlite.SqliteFactory.Instance,
                _ => throw new NotSupportedException(
                    $"Unsupported database provider: '{provider}'. Supported: SqlServer, MySql, SQLite.")
            };
        }
    }

    public class DataAdapterException : Exception
    {
        public DataAdapterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
