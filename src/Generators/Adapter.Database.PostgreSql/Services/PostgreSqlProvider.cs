using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;
using ReportDataMaker.Adapter.Database.Common.Services;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Adapter.Database.PostgreSql.Services;

/// <summary>PostgreSQL数据库提供者实现</summary>
public class PostgreSqlProvider : IDatabaseProvider
{
    /// <summary>数据库提供者类型</summary>
    public DatabaseProvider ProviderType => DatabaseProvider.PostgreSql;
    /// <summary>显示名称</summary>
    public string DisplayName => "PostgreSQL";

    /// <summary>异步测试数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>连接是否成功</returns>
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

    /// <summary>创建PostgreSQL数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接实例</returns>
    public DbConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }

    /// <summary>异步获取数据库表列表</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>表信息列表</returns>
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

    /// <summary>异步获取指定表的列信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>列信息列表</returns>
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

    /// <summary>异步获取指定表的外键信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>外键信息列表</returns>
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

    /// <summary>构建PostgreSQL分页查询语句</summary>
    /// <param name="baseSql">基础SQL语句</param>
    /// <param name="offset">偏移量</param>
    /// <param name="limit">限制行数</param>
    /// <returns>分页SQL语句</returns>
    public string BuildPagedQuery(string baseSql, int offset, int limit)
    {
        return $"{baseSql} LIMIT {limit} OFFSET {offset}";
    }

    /// <summary>引用PostgreSQL标识符</summary>
    /// <param name="identifier">标识符</param>
    /// <returns>引用后的标识符</returns>
    public string QuoteIdentifier(string identifier) => $"\"{identifier}\"";

    /// <summary>获取PostgreSQL参数前缀</summary>
    /// <returns>参数前缀字符</returns>
    public string GetParameterPrefix() => ":";
}
