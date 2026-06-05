using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using ReportDataMaker.Adapter.Database.Common.Services;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Adapter.Database.Sqlite.Services;

/// <summary>SQLite数据库提供者实现</summary>
public class SqliteProvider : IDatabaseProvider
{
    /// <summary>数据库提供者类型</summary>
    public DatabaseProvider ProviderType => DatabaseProvider.Sqlite;
    /// <summary>显示名称</summary>
    public string DisplayName => "SQLite";

    /// <summary>异步测试数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>连接是否成功</returns>
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

    /// <summary>创建SQLite数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接实例</returns>
    public DbConnection CreateConnection(string connectionString)
    {
        return new SqliteConnection(connectionString);
    }

    /// <summary>异步获取数据库表列表</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>表信息列表</returns>
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

    /// <summary>异步获取指定表的列信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>列信息列表</returns>
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

    /// <summary>异步获取指定表的外键信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>外键信息列表</returns>
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

    /// <summary>构建SQLite分页查询语句</summary>
    /// <param name="baseSql">基础SQL语句</param>
    /// <param name="offset">偏移量</param>
    /// <param name="limit">限制行数</param>
    /// <returns>分页SQL语句</returns>
    public string BuildPagedQuery(string baseSql, int offset, int limit)
    {
        return $"{baseSql} LIMIT {limit} OFFSET {offset}";
    }

    /// <summary>引用SQLite标识符</summary>
    /// <param name="identifier">标识符</param>
    /// <returns>引用后的标识符</returns>
    public string QuoteIdentifier(string identifier) => $"\"{identifier}\"";

    /// <summary>获取SQLite参数前缀</summary>
    /// <returns>参数前缀字符</returns>
    public string GetParameterPrefix() => "@";
}
