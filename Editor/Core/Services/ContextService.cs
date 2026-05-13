namespace Xinglin.WebReportEditor.Core.Services;

/// <summary>
/// 上下文参数填充服务，用于为模板提供动态上下文值（当前时间、用户等）
/// </summary>
public class ContextService
{
    /// <summary>
    /// 获取指定键的上下文值
    /// </summary>
    public object? GetContextValue(string dataPath)
    {
        return dataPath switch
        {
            "Context.DateTime.Now" => DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            "Context.DateTime.Date" => DateTime.Now.ToString("yyyy-MM-dd"),
            "Context.DateTime.Time" => DateTime.Now.ToString("HH:mm:ss"),
            "Context.DateTime.Year" => DateTime.Now.Year.ToString(),
            "Context.DateTime.Month" => DateTime.Now.Month.ToString(),
            "Context.DateTime.Day" => DateTime.Now.Day.ToString(),
            _ => null
        };
    }

    /// <summary>
    /// 获取所有内置上下文值
    /// </summary>
    public Dictionary<string, object> GetAllBuiltInValues()
    {
        return new Dictionary<string, object>
        {
            ["DateTime.Now"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            ["DateTime.Date"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["DateTime.Time"] = DateTime.Now.ToString("HH:mm:ss"),
            ["DateTime.Year"] = DateTime.Now.Year,
            ["DateTime.Month"] = DateTime.Now.Month,
            ["DateTime.Day"] = DateTime.Now.Day,
            ["System.UserName"] = Environment.UserName,
            ["System.MachineName"] = Environment.MachineName
        };
    }
}
