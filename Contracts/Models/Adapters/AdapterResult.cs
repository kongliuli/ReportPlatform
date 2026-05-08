using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>
/// 适配器结果
/// </summary>
public class AdapterResult
{
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public Dictionary<string, object> Data { get; set; } = new();
    
    public List<Dictionary<string, object>> BatchData { get; set; } = new();
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    
    public List<string> Errors { get; set; } = new();
    
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Success => new() { IsValid = true };
    
    public static ValidationResult Fail(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}
