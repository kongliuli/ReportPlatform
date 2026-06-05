using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReportDataMaker.Web;
using ReportDataMaker.Web.Services;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using ReportDataMaker.Adapter.Excel.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP client for API calls
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Adapter services
builder.Services.AddSingleton<AdapterRegistry>();
builder.Services.AddSingleton<IAdapterPlugin, ExcelAdapterPlugin>();

// Web-specific services
builder.Services.AddSingleton<ITemplateService, ApiTemplateService>();
builder.Services.AddSingleton<IPdfService, ApiPdfService>();

var host = builder.Build();

// Register plugins
var registry = host.Services.GetRequiredService<AdapterRegistry>();
foreach (var plugin in host.Services.GetServices<IAdapterPlugin>())
    registry.Register(plugin);

await host.RunAsync();
