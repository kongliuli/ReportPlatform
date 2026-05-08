using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>适配器读取结果</summary>
public class AdapterResult
{
    /// <summary>是否成功</summary>
    public bool Success { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>单条数据</summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>批量数据</summary>
    public List<Dictionary<string, object>> BatchData { get; set; } = new();
}

/// <summary>验证结果</summary>
public class ValidationResult
{
    /// <summary>是否验证通过</summary>
    public bool IsValid { get; set; }

    /// <summary>错误信息列表</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>警告信息列表</summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>验证成功的静态实例</summary>
    public static ValidationResult Success => new() { IsValid = true };

    /// <summary>创建验证失败的结果</summary>
    /// <param name="errors">错误信息数组</param>
    /// <returns>验证失败的结果</returns>
    public static ValidationResult Fail(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}
