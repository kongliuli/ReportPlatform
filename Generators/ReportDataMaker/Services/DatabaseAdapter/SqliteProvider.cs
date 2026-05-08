using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class SqliteProvider : IDatabaseProvider
{
    public DatabaseProvider ProviderType => DatabaseProvider.Sqlite;
    public string DisplayName => "SQLite";

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new SqliteConnection(connectionString);
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
        return new SqliteConnection(connectionString);
    }

    public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
    {
        var tables = new List<TableInfo>();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name, type FROM sqlite_master WHERE type IN ('table','view') AND name NOT LIKE 'sqlite_%'";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(new TableInfo
            {
                Schema = string.Empty,
                Name = reader.GetString(0),
                Type = reader.GetString(1)
            });
        }

        return tables;
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string connectionString, string tableName)
    {
        var columns = new List<ColumnInfo>();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({QuoteIdentifier(tableName)})";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString(1),
                DataType = reader.GetString(2),
                IsNullable = reader.GetInt32(3) == 0,
                IsPrimaryKey = reader.GetInt32(5) == 1,
                MaxLength = null
            });
        }

        return columns;
    }

    public async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string tableName)
    {
        var fks = new List<ForeignKeyInfo>();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA foreign_key_list({QuoteIdentifier(tableName)})";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            fks.Add(new ForeignKeyInfo
            {
                ColumnName = reader.GetString(3),
                ReferencedTable = reader.GetString(2),
                ReferencedColumn = reader.GetString(4)
            });
        }

        return fks;
    }

    public string BuildPagedQuery(string baseSql, int offset, int limit)
    {
        return $"{baseSql} LIMIT {limit} OFFSET {offset}";
    }

    public string QuoteIdentifier(string identifier) => $"\"{identifier}\"";

    public string GetParameterPrefix() => "@";
}
