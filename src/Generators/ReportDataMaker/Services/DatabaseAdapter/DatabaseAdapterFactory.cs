using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class DatabaseAdapterFactory
{
    public DatabaseAdapterService Create(TemplateDefinition template)
    {
        return new DatabaseAdapterService();
    }
}
