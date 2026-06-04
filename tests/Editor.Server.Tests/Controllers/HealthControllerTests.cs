using Microsoft.AspNetCore.Mvc;
using Xinglin.ReportEditor.Server.Controllers;

namespace Xinglin.ReportEditor.Server.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOk()
    {
        var controller = new HealthController();
        var result = controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var status = value?.GetType().GetProperty("status")?.GetValue(value)?.ToString();
        Assert.Equal("healthy", status);
    }
}

