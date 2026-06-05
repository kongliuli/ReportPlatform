using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Requests;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.ReportEditor.Core.Services;
using Xinglin.ReportEditor.Server.Controllers;

namespace Xinglin.ReportEditor.Server.Tests.Controllers;

public class TemplatesControllerTests
{
    private readonly Mock<ITemplateService> _serviceMock;
    private readonly Mock<ILogger<TemplatesController>> _loggerMock;
    private readonly TemplatesController _controller;

    public TemplatesControllerTests()
    {
        _serviceMock = new Mock<ITemplateService>();
        _loggerMock = new Mock<ILogger<TemplatesController>>();
        _controller = new TemplatesController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTemplates_ReturnsPagedResult()
    {
        var filter = new TemplateFilterRequest { Page = 1, PageSize = 10 };
        var expected = new PagedResponse<TemplateDto>
        {
            Items = new List<TemplateDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Template 1", Type = "Report" },
                new() { Id = Guid.NewGuid(), Name = "Template 2", Type = "Report" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        _serviceMock.Setup(x => x.GetTemplatesAsync(filter)).ReturnsAsync(expected);

        var result = await _controller.GetTemplates(filter);

        var apiResult = Assert.IsType<ApiResponse<PagedResponse<TemplateDto>>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.NotNull(apiResult.Data);
        Assert.Equal(2, apiResult.Data!.TotalCount);
    }

    [Fact]
    public async Task GetTemplate_ExistingId_ReturnsTemplate()
    {
        var id = Guid.NewGuid();
        var expected = new TemplateDetailDto { Id = id, Name = "Test Template" };
        _serviceMock.Setup(x => x.GetTemplateAsync(id)).ReturnsAsync(expected);

        var result = await _controller.GetTemplate(id);

        var apiResult = Assert.IsType<ApiResponse<TemplateDetailDto>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.NotNull(apiResult.Data);
        Assert.Equal("Test Template", apiResult.Data!.Name);
    }

    [Fact]
    public async Task GetTemplate_NonExistingId_Returns404()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.GetTemplateAsync(id)).ReturnsAsync((TemplateDetailDto?)null);

        var result = await _controller.GetTemplate(id);

        var apiResult = Assert.IsType<ApiResponse<TemplateDetailDto>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(404, apiResult.Code);
    }

    [Fact]
    public async Task CreateTemplate_ReturnsCreatedTemplate()
    {
        var request = new CreateTemplateRequest { Name = "New Template", Type = "Report" };
        var expected = new TemplateDetailDto { Id = Guid.NewGuid(), Name = "New Template" };
        _serviceMock.Setup(x => x.CreateTemplateAsync(request)).ReturnsAsync(expected);

        var result = await _controller.CreateTemplate(request);

        var apiResult = Assert.IsType<ApiResponse<TemplateDetailDto>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.NotNull(apiResult.Data);
        Assert.Equal("New Template", apiResult.Data!.Name);
    }

    [Fact]
    public async Task UpdateTemplate_ExistingId_ReturnsUpdated()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTemplateRequest { Name = "Updated" };
        var expected = new TemplateDetailDto { Id = id, Name = "Updated" };
        _serviceMock.Setup(x => x.UpdateTemplateAsync(id, request)).ReturnsAsync(expected);

        var result = await _controller.UpdateTemplate(id, request);

        var apiResult = Assert.IsType<ApiResponse<TemplateDetailDto>>(result);
        Assert.Equal(200, apiResult.Code);
    }

    [Fact]
    public async Task UpdateTemplate_NonExistingId_Returns404()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTemplateRequest { Name = "Updated" };
        _serviceMock.Setup(x => x.UpdateTemplateAsync(id, request)).ReturnsAsync((TemplateDetailDto?)null);

        var result = await _controller.UpdateTemplate(id, request);

        var apiResult = Assert.IsType<ApiResponse<TemplateDetailDto>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(404, apiResult.Code);
    }

    [Fact]
    public async Task DeleteTemplate_ExistingId_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.DeleteTemplateAsync(id)).ReturnsAsync(true);

        var result = await _controller.DeleteTemplate(id);

        var apiResult = Assert.IsType<ApiResponse<bool>>(result);
        Assert.Equal(200, apiResult.Code);
    }

    [Fact]
    public async Task DeleteTemplate_NonExistingId_Returns404()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.DeleteTemplateAsync(id)).ReturnsAsync(false);

        var result = await _controller.DeleteTemplate(id);

        var apiResult = Assert.IsType<ApiResponse<bool>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(404, apiResult.Code);
    }
}

