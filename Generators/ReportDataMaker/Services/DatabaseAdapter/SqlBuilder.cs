using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class SqlBuilder
{
    public string Build(QueryConfig query, List<JoinDefinition> joins, IDatabaseProvider provider)
    {
        if (query.Mode == QueryMode.RawSql)
            return query.RawSql;

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
            sb.Append(" WHERE ").Append(query.WhereClause);

        if (!string.IsNullOrWhiteSpace(query.OrderBy))
            sb.Append(" ORDER BY ").Append(query.OrderBy);

        return sb.ToString();
    }
}
