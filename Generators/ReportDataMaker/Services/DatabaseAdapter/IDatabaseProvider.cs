using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

public interface IDatabaseProvider
{
    DatabaseProvider ProviderType { get; }
    string DisplayName { get; }

    Task<bool> TestConnectionAsync(string connectionString);
    DbConnection CreateConnection(string connectionString);
    Task<List<TableInfo>> GetTablesAsync(string connectionString);
    Task<List<ColumnInfo>> GetColumnsAsync(string connectionString, string tableName);
    Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string tableName);
    string BuildPagedQuery(string baseSql, int offset, int limit);
    string QuoteIdentifier(string identifier);
    string GetParameterPrefix();
}
