using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xinglin.ReportEditor.Core.Data;
using Xinglin.ReportEditor.Core.Services;
using Xinglin.ReportEditor.Core.SharedInterfaces;

namespace Xinglin.ReportEditor.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbProvider = configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";

        services.AddDbContext<TemplateDbContext>(options =>
        {
            if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
            }
            else
            {
                var dbPath = ResolveSqlitePath(configuration.GetConnectionString("SqliteConnection"));
                options.UseSqlite($"Data Source={dbPath}");
            }
        });

        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVersionService, VersionService>();
        services.AddScoped<IPdfRenderService, PdfRenderService>();
        services.AddScoped<IJsonTemplateSerializer, JsonTemplateSerializer>();
        services.AddScoped<IDataBindingEngine, DataBindingEngine>();
#pragma warning disable CS0618
        services.AddScoped<IPdfSharpTemplateRenderer, PdfTemplateRenderer>();
#pragma warning restore CS0618

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ContextService>();

        return services;
    }

    private static string ResolveSqlitePath(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "xinglin_webreport.db";

        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var dataSourcePart = parts.FirstOrDefault(p => p.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));

        if (dataSourcePart == null)
            return "xinglin_webreport.db";

        var dbFileName = dataSourcePart.Substring("Data Source=".Length).Trim();

        // Absolute path: use as-is (e.g. /app/data/xxx.db in Docker)
        if (Path.IsPathRooted(dbFileName))
            return dbFileName;

        // Relative path with directory components: resolve relative to working directory
        // (e.g. ./data/xxx.db → /app/data/xxx.db in Docker)
        if (dbFileName.Contains(Path.DirectorySeparatorChar) || dbFileName.Contains(Path.AltDirectorySeparatorChar))
            return Path.GetFullPath(dbFileName);

        // Bare filename: resolve relative to solution's data/ directory (local dev)
        var rootDir = FindSolutionRoot();
        var dataDir = Path.Combine(rootDir, "data");
        Directory.CreateDirectory(dataDir);

        return Path.Combine(dataDir, dbFileName);
    }

    private static string FindSolutionRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Xinglin.WebReportEditor.sln")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return AppContext.BaseDirectory;
    }
}
