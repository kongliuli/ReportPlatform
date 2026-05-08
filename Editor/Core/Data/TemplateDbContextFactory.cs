using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Xinglin.WebReportEditor.Core.Data;

public class TemplateDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
    public TemplateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TemplateDbContext>();
        var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "data");
        Directory.CreateDirectory(dataDir);
        optionsBuilder.UseSqlite($"Data Source={Path.Combine(dataDir, "xinglin_webreport.db")}");

        return new TemplateDbContext(optionsBuilder.Options);
    }
}
