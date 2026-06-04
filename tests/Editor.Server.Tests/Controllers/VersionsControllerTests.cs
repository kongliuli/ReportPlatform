using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.ReportEditor.Core.Services;
using Xinglin.ReportEditor.Server.Controllers;

namespace Xinglin.ReportEditor.Server.Tests.Controllers;

public class VersionsControllerTests
{
    private readonly Mock<IVersionService> _serviceMock;
    private readonly Mock<ILogger<VersionsController>> _loggerMock;
    private readonly VersionsController _controller;

    public VersionsControllerTests()
    {
        _serviceMock = new Mock<IVersionService>();
        _loggerMock = new Mock<ILogger<VersionsController>>();
        _controller = new VersionsController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetVersions_ReturnsList()
    {
        var templateId = Guid.NewGuid();
        var expected = new List<TemplateVersionDto>
        {
            new() { Id = Guid.NewGuid(), VersionNumber = 1, ChangeDescription = "v1" },
            new() { Id = Guid.NewGuid(), VersionNumber = 2, ChangeDescription = "v2" }
        };
        _serviceMock.Setup(x => x.GetVersionsAsync(templateId)).ReturnsAsync(expected);

        var result = await _controller.GetVersions(templateId);

        var apiResult = Assert.IsType<ApiResponse<List<TemplateVersionDto>>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.Equal(2, apiResult.Data.Count);
    }

    [Fact]
    public async Task GetVersion_Existing_ReturnsVersion()
    {
        var templateId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var expected = new TemplateVersionDetailDto { Id = versionId, VersionNumber = 1 };
        _serviceMock.Setup(x => x.GetVersionAsync(templateId, versionId)).ReturnsAsync(expected);

        var result = await _controller.GetVersion(templateId, versionId);

        var apiResult = Assert.IsType<ApiResponse<TemplateVersionDetailDto>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.Equal(1, apiResult.Data.VersionNumber);
    }

    [Fact]
    public async Task GetVersion_NonExisting_Returns404()
    {
        var templateId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        _serviceMock.Setup(x => x.GetVersionAsync(templateId, versionId)).ReturnsAsync((TemplateVersionDetailDto?)null);

        var result = await _controller.GetVersion(templateId, versionId);

        var apiResult = Assert.IsType<ApiResponse<TemplateVersionDetailDto>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(404, apiResult.Code);
    }

    [Fact]
    public async Task Rollback_ExistingVersion_ReturnsOk()
    {
        var templateId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var resultDto = new TemplateVersionDto { Id = Guid.NewGuid(), VersionNumber = 3 };
        _serviceMock.Setup(x => x.RollbackAsync(templateId, versionId, null)).ReturnsAsync(resultDto);

        var result = await _controller.Rollback(templateId, versionId, null);

        var apiResult = Assert.IsType<ApiResponse<TemplateVersionDto>>(result);
        Assert.Equal(200, apiResult.Code);
    }

    [Fact]
    public async Task Rollback_NonExistingVersion_Returns404()
    {
        var templateId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        _serviceMock.Setup(x => x.RollbackAsync(templateId, versionId, null))
            .ThrowsAsync(new KeyNotFoundException("Version not found"));

        var result = await _controller.Rollback(templateId, versionId, null);

        var apiResult = Assert.IsType<ApiResponse<TemplateVersionDto>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(404, apiResult.Code);
    }
}

