using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class SqlServerProvider : IDatabaseProvider
{
    public DatabaseProvider ProviderType => DatabaseProvider.SqlServer;
    public string DisplayName => "SQL Server";

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public DbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
    {
        var tables = new List<TableInfo>();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW'";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(new TableInfo
            {
                Schema = reader.GetString(0),
                Name = reader.GetString(1),
                Type = reader.GetString(2)
            });
        }

        return tables;
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string connectionString, string tableName)
    {
        var columns = new List<ColumnInfo>();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var pkColumns = new HashSet<string>();
        using (var pkCommand = connection.CreateCommand())
        {
            pkCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME=@tableName AND CONSTRAINT_NAME LIKE 'PK_%'";
            pkCommand.Parameters.Add(new SqlParameter("@tableName", tableName));

            using var pkReader = await pkCommand.ExecuteReaderAsync();
            while (await pkReader.ReadAsync())
            {
                pkColumns.Add(pkReader.GetString(0));
            }
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@tableName";
        command.Parameters.Add(new SqlParameter("@tableName", tableName));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var colName = reader.GetString(0);
            columns.Add(new ColumnInfo
            {
                Name = colName,
                DataType = reader.GetString(1),
                IsNullable = reader.GetString(2) == "YES",
                IsPrimaryKey = pkColumns.Contains(colName),
                MaxLength = reader.IsDBNull(3) ? null : reader.GetInt32(3)
            });
        }

        return columns;
    }

    public async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string tableName)
    {
        var fks = new List<ForeignKeyInfo>();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT fkc.COLUMN_NAME, pk.TABLE_NAME AS REFERENCED_TABLE, fkc.REFERENCED_COLUMN_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fkc ON rc.CONSTRAINT_NAME = fkc.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
WHERE fkc.TABLE_NAME = @tableName";
        command.Parameters.Add(new SqlParameter("@tableName", tableName));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            fks.Add(new ForeignKeyInfo
            {
                ColumnName = reader.GetString(0),
                ReferencedTable = reader.GetString(1),
                ReferencedColumn = reader.GetString(2)
            });
        }

        return fks;
    }

    public string BuildPagedQuery(string baseSql, int offset, int limit)
    {
        var trimmed = baseSql.TrimEnd();
        if (!trimmed.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
            trimmed += " ORDER BY (SELECT 0)";

        return $"{trimmed} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
    }

    public string QuoteIdentifier(string identifier) => $"[{identifier}]";

    public string GetParameterPrefix() => "@";
}
