using System.Collections.Generic;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.DatabaseAdapter;

/// <summary>数据库适配器配置，定义数据库连接和查询的配置信息</summary>
public class DatabaseAdapterConfig : AdapterConfigBase
{
    /// <summary>数据库提供者类型</summary>
    public DatabaseProvider Provider { get; set; }
    /// <summary>数据库连接字符串</summary>
    public string ConnectionString { get; set; } = string.Empty;
    /// <summary>查询配置</summary>
    public QueryConfig Query { get; set; } = new();
    /// <summary>数据库字段映射列表</summary>
    public List<DbFieldMapping> DbFieldMappings { get; set; } = new();
    /// <summary>查询参数列表</summary>
    public List<QueryParameter> Parameters { get; set; } = new();
    /// <summary>连接定义列表</summary>
    public List<JoinDefinition> Joins { get; set; } = new();
}

/// <summary>查询配置，定义SQL查询的构建方式</summary>
public class QueryConfig
{
    /// <summary>查询模式</summary>
    public QueryMode Mode { get; set; } = QueryMode.RawSql;
    /// <summary>原始SQL语句</summary>
    public string RawSql { get; set; } = string.Empty;
    /// <summary>主表名称</summary>
    public string PrimaryTable { get; set; } = string.Empty;
    /// <summary>已选择的列名列表</summary>
    public List<string> SelectedColumns { get; set; } = new();
    /// <summary>WHERE条件子句</summary>
    public string WhereClause { get; set; } = string.Empty;
    /// <summary>排序子句</summary>
    public string OrderBy { get; set; } = string.Empty;
}

/// <summary>查询模式枚举</summary>
public enum QueryMode
{
    /// <summary>原始SQL模式</summary>
    RawSql,
    /// <summary>可视化构建模式</summary>
    VisualBuilder
}

/// <summary>数据库字段映射，定义数据库列与模板数据路径的对应关系</summary>
public class DbFieldMapping
{
    /// <summary>数据库列名</summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>目标数据路径</summary>
    public string TargetDataPath { get; set; } = string.Empty;
    /// <summary>数据转换方式</summary>
    public string Transform { get; set; } = string.Empty;
}

/// <summary>查询参数，定义SQL查询中的参数信息</summary>
public class QueryParameter
{
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>参数显示标签</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>参数数据类型</summary>
    public FieldDataType DataType { get; set; } = FieldDataType.Text;
    /// <summary>参数默认值</summary>
    public string DefaultValue { get; set; } = string.Empty;
    /// <summary>参数来源</summary>
    public ParameterSource Source { get; set; } = ParameterSource.Manual;
}

/// <summary>参数来源枚举</summary>
public enum ParameterSource
{
    /// <summary>手动输入</summary>
    Manual,
    /// <summary>来自模板</summary>
    FromTemplate,
    /// <summary>来自上下文</summary>
    FromContext
}

/// <summary>连接定义，描述数据库表之间的JOIN关系</summary>
public class JoinDefinition
{
    /// <summary>左表名称</summary>
    public string LeftTable { get; set; } = string.Empty;
    /// <summary>左表列名</summary>
    public string LeftColumn { get; set; } = string.Empty;
    /// <summary>右表名称</summary>
    public string RightTable { get; set; } = string.Empty;
    /// <summary>右表列名</summary>
    public string RightColumn { get; set; } = string.Empty;
    /// <summary>连接类型</summary>
    public JoinType Type { get; set; } = JoinType.Inner;
}

/// <summary>连接类型枚举</summary>
public enum JoinType
{
    /// <summary>内连接</summary>
    Inner,
    /// <summary>左连接</summary>
    Left,
    /// <summary>右连接</summary>
    Right
}
