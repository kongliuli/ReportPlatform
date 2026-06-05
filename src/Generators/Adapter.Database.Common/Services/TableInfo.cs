using System.Collections.Generic;

namespace ReportDataMaker.Adapter.Database.Common.Services;

/// <summary>表信息，描述数据库表的基本信息</summary>
public class TableInfo
{
    /// <summary>架构名称</summary>
    public string Schema { get; set; } = string.Empty;
    /// <summary>表名</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>表类型</summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>列信息，描述数据库表列的详细信息</summary>
public class ColumnInfo
{
    /// <summary>列名</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>数据类型</summary>
    public string DataType { get; set; } = string.Empty;
    /// <summary>是否允许为空</summary>
    public bool IsNullable { get; set; }
    /// <summary>是否为主键</summary>
    public bool IsPrimaryKey { get; set; }
    /// <summary>最大长度</summary>
    public int? MaxLength { get; set; }
}

/// <summary>外键信息，描述数据库表之间的外键关系</summary>
public class ForeignKeyInfo
{
    /// <summary>外键列名</summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>引用的表名</summary>
    public string ReferencedTable { get; set; } = string.Empty;
    /// <summary>引用的列名</summary>
    public string ReferencedColumn { get; set; } = string.Empty;
}
