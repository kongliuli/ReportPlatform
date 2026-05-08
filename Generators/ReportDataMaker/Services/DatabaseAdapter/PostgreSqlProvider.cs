using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class PostgreSqlProvider : IDatabaseProvider
{
    public DatabaseProvider ProviderType => DatabaseProvider.PostgreSql;
    public string DisplayName => "PostgreSQL";

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
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
        return new NpgsqlConnection(connectionString);
    }

    public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
    {
        var tables = new List<TableInfo>();
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT table_schema, table_name, table_type FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema')";

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
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var pkColumns = new HashSet<string>();
        using (var pkCommand = connection.CreateCommand())
        {
            pkCommand.CommandText = @"SELECT kcu.column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
WHERE tc.constraint_type = 'PRIMARY KEY' AND kcu.table_name = :tableName";
            pkCommand.Parameters.Add(new NpgsqlParameter(":tableName", tableName));

            using var pkReader = await pkCommand.ExecuteReaderAsync();
            while (await pkReader.ReadAsync())
            {
                pkColumns.Add(pkReader.GetString(0));
            }
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT column_name, data_type, is_nullable, character_maximum_length FROM information_schema.columns WHERE table_name = :tableName";
        command.Parameters.Add(new NpgsqlParameter(":tableName", tableName));

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
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT kcu.column_name, ccu.table_name AS referenced_table, kcu.column_name AS referenced_column
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY' AND kcu.table_name = :tableName";
        command.Parameters.Add(new NpgsqlParameter(":tableName", tableName));

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
        return $"{baseSql} LIMIT {limit} OFFSET {offset}";
    }

    public string QuoteIdentifier(string identifier) => $"\"{identifier}\"";

    public string GetParameterPrefix() => ":";
}
