using System.Net.Http.Json;

namespace ReportDataMaker.Web.Services;

/// <summary>基于API的PDF服务实现</summary>
public class ApiPdfService : IPdfService
{
    private readonly HttpClient _http;

    public ApiPdfService(HttpClient http) => _http = http;

    public async Task<byte[]?> RenderPdfAsync(Guid templateId, Dictionary<string, object> data)
    {
        var response = await _http.PostAsJsonAsync($"api/preview/{templateId}", data);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsByteArrayAsync();
        return null;
    }
}
