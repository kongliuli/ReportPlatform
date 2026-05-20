using Microsoft.EntityFrameworkCore;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Requests;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.WebReportEditor.Core.Data;
using Xinglin.WebReportEditor.Core.SharedInterfaces;

namespace Xinglin.WebReportEditor.Core.Services;

public class TemplateService : ITemplateService
{
    private readonly TemplateDbContext _dbContext;
    private readonly IJsonTemplateSerializer _serializer;

    public TemplateService(TemplateDbContext dbContext, IJsonTemplateSerializer serializer)
    {
        _dbContext = dbContext;
        _serializer = serializer;
    }

    public async Task<PagedResponse<TemplateDto>> GetTemplatesAsync(TemplateFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;
        if (filter.PageSize > 100) filter.PageSize = 100;

        var query = _dbContext.Templates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(t => t.Name.Contains(filter.Name));

        if (!string.IsNullOrWhiteSpace(filter.Type))
            query = query.Where(t => t.Type == filter.Type);

        if (!string.IsNullOrWhiteSpace(filter.HospitalId))
            query = query.Where(t => t.HospitalId == filter.HospitalId);

        if (filter.IsPublished.HasValue)
            query = query.Where(t => t.IsPublished == filter.IsPublished.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.UpdateTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return new PagedResponse<TemplateDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<TemplateDetailDto?> GetTemplateAsync(Guid id)
    {
        var entity = await _dbContext.Templates.FindAsync(id);
        if (entity == null) return null;

        return MapToDetailDto(entity);
    }

    public async Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateRequest request)
    {
        var entity = new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Version = 1,
            ContentJson = request.ContentJson,
            HospitalId = request.HospitalId,
            IsDefault = false,
            IsPublished = false,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        var version = new TemplateVersionEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = entity.Id,
            VersionNumber = 1,
            ContentJson = request.ContentJson,
            ChangeDescription = "初始版本",
            CreateTime = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _dbContext.Templates.Add(entity);
        _dbContext.TemplateVersions.Add(version);
        await _dbContext.SaveChangesAsync();

        return MapToDetailDto(entity);
    }

    public async Task<TemplateDetailDto?> UpdateTemplateAsync(Guid id, UpdateTemplateRequest request)
    {
        var entity = await _dbContext.Templates.FindAsync(id);
        if (entity == null) return null;

        if (request.Name != null) entity.Name = request.Name;
        if (request.Type != null) entity.Type = request.Type;
        if (request.IsPublished.HasValue) entity.IsPublished = request.IsPublished.Value;

        var newContentJson = request.ContentJson ?? entity.ContentJson;

        if (request.ContentJson != null)
        {
            entity.ContentJson = request.ContentJson;
            entity.Version += 1;

            var maxVersion = await _dbContext.TemplateVersions
                .Where(v => v.TemplateId == id)
                .MaxAsync(v => (int?)v.VersionNumber) ?? 0;

            var version = new TemplateVersionEntity
            {
                Id = Guid.NewGuid(),
                TemplateId = id,
                VersionNumber = maxVersion + 1,
                ContentJson = request.ContentJson,
                ChangeDescription = request.ChangeDescription ?? $"更新至版本 {maxVersion + 1}",
                CreateTime = DateTime.UtcNow,
                CreatedBy = entity.CreatedBy
            };

            _dbContext.TemplateVersions.Add(version);
        }

        entity.UpdateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return MapToDetailDto(entity);
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        var entity = await _dbContext.Templates.FindAsync(id);
        if (entity == null) return false;

        _dbContext.Templates.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static TemplateDto MapToDto(TemplateEntity entity)
    {
        return new TemplateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Version = entity.Version,
            HospitalId = entity.HospitalId,
            IsDefault = entity.IsDefault,
            IsPublished = entity.IsPublished,
            CreateTime = entity.CreateTime,
            UpdateTime = entity.UpdateTime,
            CreatedBy = entity.CreatedBy
        };
    }

    private static TemplateDetailDto MapToDetailDto(TemplateEntity entity)
    {
        return new TemplateDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Version = entity.Version,
            HospitalId = entity.HospitalId,
            IsDefault = entity.IsDefault,
            IsPublished = entity.IsPublished,
            CreateTime = entity.CreateTime,
            UpdateTime = entity.UpdateTime,
            CreatedBy = entity.CreatedBy,
            ContentJson = entity.ContentJson
        };
    }
}
