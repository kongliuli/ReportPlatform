using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.DatabaseAdapter;

/// <summary>数据库提供者接口，定义数据库操作的契约</summary>
public interface IDatabaseProvider
{
    /// <summary>数据库提供者类型</summary>
    DatabaseProvider ProviderType { get; }
    /// <summary>显示名称</summary>
    string DisplayName { get; }

    /// <summary>异步测试数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>连接是否成功</returns>
    Task<bool> TestConnectionAsync(string connectionString);
    /// <summary>创建数据库连接</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接实例</returns>
    DbConnection CreateConnection(string connectionString);
    /// <summary>异步获取数据库表列表</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>表信息列表</returns>
    Task<List<TableInfo>> GetTablesAsync(string connectionString);
    /// <summary>异步获取指定表的列信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>列信息列表</returns>
    Task<List<ColumnInfo>> GetColumnsAsync(string connectionString, string tableName);
    /// <summary>异步获取指定表的外键信息</summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="tableName">表名</param>
    /// <returns>外键信息列表</returns>
    Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string tableName);
    /// <summary>构建分页查询语句</summary>
    /// <param name="baseSql">基础SQL语句</param>
    /// <param name="offset">偏移量</param>
    /// <param name="limit">限制行数</param>
    /// <returns>分页SQL语句</returns>
    string BuildPagedQuery(string baseSql, int offset, int limit);
    /// <summary>引用标识符</summary>
    /// <param name="identifier">标识符</param>
    /// <returns>引用后的标识符</returns>
    string QuoteIdentifier(string identifier);
    /// <summary>获取参数前缀</summary>
    /// <returns>参数前缀字符</returns>
    string GetParameterPrefix();
}
