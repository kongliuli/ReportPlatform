using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.WebReportEditor.Core.Services;
using Xinglin.WebReportEditor.Server.Controllers;

namespace Xinglin.WebReportEditor.Server.Tests.Controllers;

public class PreviewControllerTests
{
    private readonly Mock<IPdfRenderService> _pdfMock;
    private readonly Mock<ILogger<PreviewController>> _loggerMock;
    private readonly PreviewController _controller;

    public PreviewControllerTests()
    {
        _pdfMock = new Mock<IPdfRenderService>();
        _loggerMock = new Mock<ILogger<PreviewController>>();
        _controller = new PreviewController(_pdfMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPreviewImage_ExistingTemplate_ReturnsBase64()
    {
        var id = Guid.NewGuid();
        _pdfMock.Setup(x => x.RenderToImageAsync(id)).ReturnsAsync("base64imagestring");

        var result = await _controller.GetPreviewImage(id);

        var apiResult = Assert.IsType<ApiResponse<string>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.Equal("base64imagestring", apiResult.Data);
    }

    [Fact]
    public async Task GetPreviewImage_NonExistingTemplate_Returns404()
    {
        var id = Guid.NewGuid();
        _pdfMock.Setup(x => x.RenderToImageAsync(id))
            .ThrowsAsync(new KeyNotFoundException("Template not found"));

        var result = await _controller.GetPreviewImage(id);

        var apiResult = Assert.IsType<ApiResponse<string>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(404, apiResult.Code);
    }

    [Fact]
    public async Task GetPdf_ExistingTemplate_ReturnsFile()
    {
        var id = Guid.NewGuid();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _pdfMock.Setup(x => x.RenderToPdfAsync(id)).ReturnsAsync(pdfBytes);

        var result = await _controller.GetPdf(id);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal(pdfBytes.Length, fileResult.FileContents.Length);
    }

    [Fact]
    public async Task GetPdf_NoContent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _pdfMock.Setup(x => x.RenderToPdfAsync(id)).ReturnsAsync(Array.Empty<byte>());

        var result = await _controller.GetPdf(id);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}

