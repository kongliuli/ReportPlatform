using System.Collections.Generic;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class DatabaseAdapterConfig : AdapterConfigBase
{
    public DatabaseProvider Provider { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public QueryConfig Query { get; set; } = new();
    public List<DbFieldMapping> DbFieldMappings { get; set; } = new();
    public List<QueryParameter> Parameters { get; set; } = new();
    public List<JoinDefinition> Joins { get; set; } = new();
}

public class QueryConfig
{
    public QueryMode Mode { get; set; } = QueryMode.RawSql;
    public string RawSql { get; set; } = string.Empty;
    public string PrimaryTable { get; set; } = string.Empty;
    public List<string> SelectedColumns { get; set; } = new();
    public string WhereClause { get; set; } = string.Empty;
    public string OrderBy { get; set; } = string.Empty;
}

public enum QueryMode { RawSql, VisualBuilder }

public class DbFieldMapping
{
    public string ColumnName { get; set; } = string.Empty;
    public string TargetDataPath { get; set; } = string.Empty;
    public string Transform { get; set; } = string.Empty;
}

public class QueryParameter
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldDataType DataType { get; set; } = FieldDataType.Text;
    public string DefaultValue { get; set; } = string.Empty;
    public ParameterSource Source { get; set; } = ParameterSource.Manual;
}

public enum ParameterSource { Manual, FromTemplate, FromContext }

public class JoinDefinition
{
    public string LeftTable { get; set; } = string.Empty;
    public string LeftColumn { get; set; } = string.Empty;
    public string RightTable { get; set; } = string.Empty;
    public string RightColumn { get; set; } = string.Empty;
    public JoinType Type { get; set; } = JoinType.Inner;
}

public enum JoinType { Inner, Left, Right }
