using System.Net.Http.Json;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Web.Services;

/// <summary>基于API的模板服务实现</summary>
public class ApiTemplateService : ITemplateService
{
    private readonly HttpClient _http;

    public ApiTemplateService(HttpClient http) => _http = http;

    public async Task<TemplateDefinition?> GetLatestTemplateAsync()
    {
        var response = await _http.GetFromJsonAsync<TemplateDefinition>("api/templates/latest");
        return response;
    }

    public async Task<TemplateDefinition?> GetTemplateAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<TemplateDefinition>($"api/templates/{id}");
        return response;
    }

    public async Task<List<TemplateDefinition>> GetTemplatesAsync()
    {
        var response = await _http.GetFromJsonAsync<List<TemplateDefinition>>("api/templates");
        return response ?? new();
    }
}
