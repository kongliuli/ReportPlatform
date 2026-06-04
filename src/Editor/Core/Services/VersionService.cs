using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.ReportEditor.Core.Data;

namespace Xinglin.ReportEditor.Core.Services;

public class VersionService : IVersionService
{
    private readonly TemplateDbContext _dbContext;

    public VersionService(TemplateDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TemplateVersionDto>> GetVersionsAsync(Guid templateId)
    {
        return await _dbContext.TemplateVersions
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new TemplateVersionDto
            {
                Id = v.Id,
                TemplateId = v.TemplateId,
                VersionNumber = v.VersionNumber,
                ChangeDescription = v.ChangeDescription,
                CreateTime = v.CreateTime,
                CreatedBy = v.CreatedBy
            })
            .ToListAsync();
    }

    public async Task<TemplateVersionDetailDto?> GetVersionAsync(Guid templateId, Guid versionId)
    {
        var entity = await _dbContext.TemplateVersions
            .FirstOrDefaultAsync(v => v.TemplateId == templateId && v.Id == versionId);

        if (entity == null) return null;

        return new TemplateVersionDetailDto
        {
            Id = entity.Id,
            TemplateId = entity.TemplateId,
            VersionNumber = entity.VersionNumber,
            ChangeDescription = entity.ChangeDescription,
            CreateTime = entity.CreateTime,
            CreatedBy = entity.CreatedBy,
            ContentJson = entity.ContentJson
        };
    }

    public async Task<TemplateVersionDto> RollbackAsync(Guid templateId, Guid versionId, string? createdBy)
    {
        var template = await _dbContext.Templates.FindAsync(templateId)
            ?? throw new KeyNotFoundException($"模板 {templateId} 不存在");

        var allVersions = await _dbContext.TemplateVersions
            .Where(v => v.TemplateId == templateId)
            .ToListAsync();

        var targetVersion = allVersions.FirstOrDefault(v => v.Id == versionId)
            ?? throw new KeyNotFoundException($"版本 {versionId} 不存在");

        var maxVersion = allVersions.Count > 0
            ? allVersions.Max(v => v.VersionNumber)
            : 0;

        var newVersion = new Data.TemplateVersionEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            VersionNumber = maxVersion + 1,
            ContentJson = targetVersion.ContentJson,
            ChangeDescription = $"回滚到版本 {targetVersion.VersionNumber}",
            CreateTime = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        template.ContentJson = targetVersion.ContentJson;
        template.Version += 1;
        template.UpdateTime = DateTime.UtcNow;

        _dbContext.TemplateVersions.Add(newVersion);
        _dbContext.Templates.Update(template);
        await _dbContext.SaveChangesAsync();

        return new TemplateVersionDto
        {
            Id = newVersion.Id,
            TemplateId = newVersion.TemplateId,
            VersionNumber = newVersion.VersionNumber,
            ChangeDescription = newVersion.ChangeDescription,
            CreateTime = newVersion.CreateTime,
            CreatedBy = newVersion.CreatedBy
        };
    }

    public async Task<VersionDiffResponse> DiffAsync(Guid templateId, Guid versionIdA, Guid versionIdB)
    {
        var versionA = await _dbContext.TemplateVersions
            .FirstOrDefaultAsync(v => v.TemplateId == templateId && v.Id == versionIdA)
            ?? throw new KeyNotFoundException($"版本 {versionIdA} 不存在");

        var versionB = await _dbContext.TemplateVersions
            .FirstOrDefaultAsync(v => v.TemplateId == templateId && v.Id == versionIdB)
            ?? throw new KeyNotFoundException($"版本 {versionIdB} 不存在");

        var jsonA = versionA.ContentJson;
        var jsonB = versionB.ContentJson;

        var diffs = new VersionDiffResponse
        {
            TemplateId = templateId,
            VersionIdA = versionIdA,
            VersionIdB = versionIdB
        };

        try
        {
            var objA = JObject.Parse(jsonA);
            var objB = JObject.Parse(jsonB);

            var elementsA = objA["elements"] as JArray ?? new JArray();
            var elementsB = objB["elements"] as JArray ?? new JArray();

            var dictA = elementsA.ToDictionary(e => e["id"]?.ToString() ?? "", e => e);
            var dictB = elementsB.ToDictionary(e => e["id"]?.ToString() ?? "", e => e);

            foreach (var kvp in dictB)
            {
                if (!dictA.ContainsKey(kvp.Key))
                {
                    diffs.ElementDiffs.Add(new ElementDiff
                    {
                        DiffType = "Added",
                        ElementId = kvp.Key,
                        ElementType = kvp.Value["$type"]?.ToString()?.Split('.').LastOrDefault()?.Replace("Element, Xinglin.Core", "") ?? "Unknown",
                        Description = $"新增元素"
                    });
                }
            }

            foreach (var kvp in dictA)
            {
                if (!dictB.ContainsKey(kvp.Key))
                {
                    diffs.ElementDiffs.Add(new ElementDiff
                    {
                        DiffType = "Removed",
                        ElementId = kvp.Key,
                        ElementType = kvp.Value["$type"]?.ToString()?.Split('.').LastOrDefault()?.Replace("Element, Xinglin.Core", "") ?? "Unknown",
                        Description = $"删除元素"
                    });
                }
                else
                {
                    var elA = kvp.Value.ToString(Formatting.None);
                    var elB = dictB[kvp.Key].ToString(Formatting.None);
                    if (elA != elB)
                    {
                        diffs.ElementDiffs.Add(new ElementDiff
                        {
                            DiffType = "Modified",
                            ElementId = kvp.Key,
                            ElementType = kvp.Value["$type"]?.ToString()?.Split('.').LastOrDefault()?.Replace("Element, Xinglin.Core", "") ?? "Unknown",
                            Description = $"修改元素"
                        });
                    }
                }
            }

            var pageProps = new[] { "pageWidth", "pageHeight", "marginLeft", "marginRight", "marginTop", "marginBottom", "orientation" };
            foreach (var prop in pageProps)
            {
                var valA = objA[prop]?.ToString();
                var valB = objB[prop]?.ToString();
                if (valA != valB)
                {
                    diffs.PropertyDiffs.Add(new PropertyDiff
                    {
                        PropertyName = prop,
                        OldValue = valA,
                        NewValue = valB
                    });
                }
            }
        }
        catch
        {
            diffs.PropertyDiffs.Add(new PropertyDiff
            {
                PropertyName = "contentJson",
                OldValue = "解析失败",
                NewValue = "解析失败"
            });
        }

        return diffs;
    }
}
