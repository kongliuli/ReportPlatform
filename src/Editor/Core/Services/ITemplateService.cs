using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Requests;
using Xinglin.WebReportEditor.Contracts.Responses;

namespace Xinglin.ReportEditor.Core.Services;

public interface ITemplateService
{
    Task<PagedResponse<TemplateDto>> GetTemplatesAsync(TemplateFilterRequest filter);
    Task<TemplateDetailDto?> GetTemplateAsync(Guid id);
    Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateRequest request);
    Task<TemplateDetailDto?> UpdateTemplateAsync(Guid id, UpdateTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid id);
}
