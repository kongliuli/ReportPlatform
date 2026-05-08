namespace Xinglin.ReportEditor.Contracts.Enums;

/// <summary>
/// 数据适配器类型
/// </summary>
public enum AdapterType
{
    /// <summary>
    /// 上下文配置适配器，自动执行
    /// </summary>
    Context,

    /// <summary>
    /// Excel 导入适配器
    /// </summary>
    Excel,

    /// <summary>
    /// 数据库查询适配器
    /// </summary>
    Database,

    /// <summary>
    /// API 调用适配器
    /// </summary>
    Api
}

/// <summary>
/// 数据库提供者类型
/// </summary>
public enum DatabaseProvider
{
    SqlServer,
    MySql,
    Sqlite,
    PostgreSql
}
