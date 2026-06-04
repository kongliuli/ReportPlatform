using System.Diagnostics;

namespace Xinglin.ReportEditor.Server.Middleware;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        var requestQuery = context.Request.QueryString.ToString();
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("[API] Request Started | Method: {Method} | Path: {Path}{Query} | IP: {ClientIp}",
            requestMethod, requestPath, requestQuery, clientIp);

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation("[API] Request Completed | Method: {Method} | Path: {Path}{Query} | StatusCode: {StatusCode} | Duration: {Duration}ms",
            requestMethod, requestPath, requestQuery, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}