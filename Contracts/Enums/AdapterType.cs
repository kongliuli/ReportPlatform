namespace Xinglin.ReportEditor.Contracts.Enums;

/// <summary>数据适配器类型枚举</summary>
public enum AdapterType
{
    /// <summary>上下文数据适配器</summary>
    Context,

    /// <summary>Excel数据适配器</summary>
    Excel,

    /// <summary>数据库数据适配器</summary>
    Database,

    /// <summary>API数据适配器</summary>
    Api
}

/// <summary>数据库提供程序枚举</summary>
public enum DatabaseProvider
{
    /// <summary>SQL Server数据库</summary>
    SqlServer,

    /// <summary>MySQL数据库</summary>
    MySql,

    /// <summary>SQLite数据库</summary>
    Sqlite,

    /// <summary>PostgreSQL数据库</summary>
    PostgreSql
}
