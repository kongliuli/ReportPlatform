using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReportDataMaker.Adapter.Database.Common.Services;

/// <summary>SQL构建器，根据查询配置和连接定义生成SQL语句</summary>
public class SqlBuilder
{
    private static readonly Regex OrderByPattern = new(@"^[a-zA-Z_][a-zA-Z0-9_]*(\s+(ASC|DESC))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>构建SQL查询语句</summary>
    /// <param name="query">查询配置</param>
    /// <param name="joins">连接定义列表</param>
    /// <param name="provider">数据库提供者</param>
    /// <returns>生成的SQL语句</returns>
    public string Build(QueryConfig query, List<JoinDefinition> joins, IDatabaseProvider provider)
    {
        var (sql, _) = BuildParameterizedQuery(query, joins, provider);
        return sql;
    }

    /// <summary>构建参数化SQL查询语句，返回SQL文本及参数列表</summary>
    /// <param name="query">查询配置</param>
    /// <param name="joins">连接定义列表</param>
    /// <param name="provider">数据库提供者</param>
    /// <returns>SQL语句和参数列表的元组</returns>
    public static (string sql, List<(string name, object value)> parameters) BuildParameterizedQuery(
        QueryConfig query, List<JoinDefinition> joins, IDatabaseProvider provider, List<QueryParameter>? queryParams = null)
    {
        if (query.Mode == QueryMode.RawSql)
            throw new NotSupportedException("RawSQL mode is disabled for security. Use parameterized query mode instead.");

        var parameters = new List<(string name, object value)>();
        var sb = new StringBuilder();
        sb.Append("SELECT ");

        if (query.SelectedColumns == null || query.SelectedColumns.Count == 0)
            sb.Append("*");
        else
            sb.Append(string.Join(", ", query.SelectedColumns.Select(c => provider.QuoteIdentifier(c))));

        sb.Append(" FROM ").Append(provider.QuoteIdentifier(query.PrimaryTable));

        if (joins != null)
        {
            foreach (var join in joins)
            {
                var joinKeyword = join.Type switch
                {
                    JoinType.Left => "LEFT JOIN",
                    JoinType.Right => "RIGHT JOIN",
                    _ => "INNER JOIN"
                };
                sb.Append($" {joinKeyword} {provider.QuoteIdentifier(join.RightTable)} ON ")
                  .Append($"{provider.QuoteIdentifier(join.LeftTable)}.{provider.QuoteIdentifier(join.LeftColumn)} = ")
                  .Append($"{provider.QuoteIdentifier(join.RightTable)}.{provider.QuoteIdentifier(join.RightColumn)}");
            }
        }

        if (!string.IsNullOrWhiteSpace(query.WhereClause))
        {
            sb.Append(" WHERE ").Append(query.WhereClause);
            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    parameters.Add((param.Name, (object?)param.DefaultValue ?? DBNull.Value));
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(query.OrderBy))
        {
            ValidateOrderBy(query.OrderBy);
            sb.Append(" ORDER BY ").Append(query.OrderBy);
        }

        return (sb.ToString(), parameters);
    }

    /// <summary>验证OrderBy子句是否符合白名单规则</summary>
    private static void ValidateOrderBy(string orderBy)
    {
        var clauses = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var clause in clauses)
        {
            var trimmed = clause.Trim();
            if (!OrderByPattern.IsMatch(trimmed))
                throw new ArgumentException("OrderBy contains invalid characters. Only column names with optional ASC/DESC are allowed.");
        }
    }
}
