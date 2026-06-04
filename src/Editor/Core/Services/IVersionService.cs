using Xinglin.WebReportEditor.Contracts.DTOs;

namespace Xinglin.ReportEditor.Core.Services;

public interface IVersionService
{
    Task<List<TemplateVersionDto>> GetVersionsAsync(Guid templateId);
    Task<TemplateVersionDetailDto?> GetVersionAsync(Guid templateId, Guid versionId);
    Task<TemplateVersionDto> RollbackAsync(Guid templateId, Guid versionId, string? createdBy);
    Task<VersionDiffResponse> DiffAsync(Guid templateId, Guid versionIdA, Guid versionIdB);
}
