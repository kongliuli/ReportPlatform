namespace Xinglin.ReportEditor.Core.Services;

public class ContextService
{
    private readonly TimeProvider _timeProvider;

    public ContextService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public object? GetContextValue(string dataPath)
    {
        var now = _timeProvider.GetLocalNow().DateTime;
        return dataPath switch
        {
            "Context.DateTime.Now" => now.ToString("yyyy-MM-dd HH:mm"),
            "Context.DateTime.Date" => now.ToString("yyyy-MM-dd"),
            "Context.DateTime.Time" => now.ToString("HH:mm:ss"),
            "Context.DateTime.Year" => now.Year.ToString(),
            "Context.DateTime.Month" => now.Month.ToString(),
            "Context.DateTime.Day" => now.Day.ToString(),
            _ => null
        };
    }

    public Dictionary<string, object> GetAllBuiltInValues()
    {
        var now = _timeProvider.GetLocalNow().DateTime;
        return new Dictionary<string, object>
        {
            ["DateTime.Now"] = now.ToString("yyyy-MM-dd HH:mm"),
            ["DateTime.Date"] = now.ToString("yyyy-MM-dd"),
            ["DateTime.Time"] = now.ToString("HH:mm:ss"),
            ["DateTime.Year"] = now.Year,
            ["DateTime.Month"] = now.Month,
            ["DateTime.Day"] = now.Day
            // S7: 移除 System.UserName 和 System.MachineName，避免暴露服务器敏感信息
        };
    }
}
