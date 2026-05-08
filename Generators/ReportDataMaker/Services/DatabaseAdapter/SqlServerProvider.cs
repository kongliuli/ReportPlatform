using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

/// <summary>SQL Server数据库提供者实现</summary>
public class SqlServerProvider : IDatabaseProvider
{
    /// <summary>数据库提供者类型</summary>
    public DatabaseProvider ProviderType => DatabaseProvider.SqlServer;
    /// <summary>显示名称</summary>
    public string DisplayName => "SQL Server";

    /// <summary>异步测试数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>连接是否成功</returns>
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

    /// <summary>创建SQL Server数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接实例</returns>
    public DbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    /// <summary>异步获取数据库表列表</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>表信息列表</returns>
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

    /// <summary>异步获取指定表的列信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>列信息列表</returns>
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

    /// <summary>异步获取指定表的外键信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>外键信息列表</returns>
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

    /// <summary>构建SQL Server分页查询语句</summary>
    /// <param name="baseSql">基础SQL语句</param>
    /// <param name="offset">偏移量</param>
    /// <param name="limit">限制行数</param>
    /// <returns>分页SQL语句</returns>
    public string BuildPagedQuery(string baseSql, int offset, int limit)
    {
        var trimmed = baseSql.TrimEnd();
        if (!trimmed.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
            trimmed += " ORDER BY (SELECT 0)";

        return $"{trimmed} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
    }

    /// <summary>引用SQL Server标识符</summary>
    /// <param name="identifier">标识符</param>
    /// <returns>引用后的标识符</returns>
    public string QuoteIdentifier(string identifier) => $"[{identifier}]";

    /// <summary>获取SQL Server参数前缀</summary>
    /// <returns>参数前缀字符</returns>
    public string GetParameterPrefix() => "@";
}
