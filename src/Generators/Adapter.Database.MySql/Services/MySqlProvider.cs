using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;
using ReportDataMaker.Adapter.Database.Common.Services;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Adapter.Database.MySql.Services;

/// <summary>MySQL数据库提供者实现</summary>
public class MySqlProvider : IDatabaseProvider
{
    /// <summary>数据库提供者类型</summary>
    public DatabaseProvider ProviderType => DatabaseProvider.MySql;
    /// <summary>显示名称</summary>
    public string DisplayName => "MySQL";

    /// <summary>异步测试数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>连接是否成功</returns>
    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>创建MySQL数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接实例</returns>
    public DbConnection CreateConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }

    /// <summary>异步获取数据库表列表</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>表信息列表</returns>
    public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
    {
        var tables = new List<TableInfo>();
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SHOW FULL TABLES WHERE Table_type = 'BASE TABLE' OR Table_type = 'VIEW'";

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
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = $"SHOW COLUMNS FROM {QuoteIdentifier(tableName)}";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var field = reader.GetString(0);
            var type = reader.GetString(1);
            var isNullable = reader.GetString(2) == "YES";
            var key = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);

            int? maxLength = null;
            var parenIndex = type.IndexOf('(');
            if (parenIndex >= 0)
            {
                var closeParen = type.IndexOf(')', parenIndex);
                if (closeParen > parenIndex && int.TryParse(type.Substring(parenIndex + 1, closeParen - parenIndex - 1), out var len))
                    maxLength = len;
            }

            var baseType = parenIndex >= 0 ? type.Substring(0, parenIndex) : type;

            columns.Add(new ColumnInfo
            {
                Name = field,
                DataType = baseType,
                IsNullable = isNullable,
                IsPrimaryKey = key == "PRI",
                MaxLength = maxLength
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
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COLUMN_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = @tableName AND REFERENCED_TABLE_NAME IS NOT NULL";
        command.Parameters.Add(new MySqlParameter("@tableName", tableName));

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

    /// <summary>构建MySQL分页查询语句</summary>
    /// <param name="baseSql">基础SQL语句</param>
    /// <param name="offset">偏移量</param>
    /// <param name="limit">限制行数</param>
    /// <returns>分页SQL语句</returns>
    public string BuildPagedQuery(string baseSql, int offset, int limit)
    {
        return $"{baseSql} LIMIT {limit} OFFSET {offset}";
    }

    /// <summary>引用MySQL标识符</summary>
    /// <param name="identifier">标识符</param>
    /// <returns>引用后的标识符</returns>
    public string QuoteIdentifier(string identifier) => $"`{identifier}`";

    /// <summary>获取MySQL参数前缀</summary>
    /// <returns>参数前缀字符</returns>
    public string GetParameterPrefix() => "@";
}
