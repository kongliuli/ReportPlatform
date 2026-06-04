using System.Diagnostics;
using System.Web;

namespace Xinglin.ReportEditor.Server.Middleware;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    // B7: 敏感参数列表，在日志中脱敏
    private static readonly HashSet<string> SensitiveParams = new(StringComparer.OrdinalIgnoreCase)
    {
        "token", "refreshtoken", "password", "secret", "key", "authorization"
    };

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
        var requestQuery = SanitizeQueryString(context.Request.QueryString);
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("[API] Request Started | Method: {Method} | Path: {Path}{Query} | IP: {ClientIp}",
            requestMethod, requestPath, requestQuery, clientIp);

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation("[API] Request Completed | Method: {Method} | Path: {Path}{Query} | StatusCode: {StatusCode} | Duration: {Duration}ms",
            requestMethod, requestPath, requestQuery, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }

    private static string SanitizeQueryString(QueryString queryString)
    {
        if (!queryString.HasValue) return string.Empty;

        var query = HttpUtility.ParseQueryString(queryString.Value!);
        foreach (string? key in query.AllKeys)
        {
            if (key != null && SensitiveParams.Contains(key))
                query[key] = "***";
        }
        var sanitized = query.ToString();
        return string.IsNullOrEmpty(sanitized) ? string.Empty : "?" + sanitized;
    }
}