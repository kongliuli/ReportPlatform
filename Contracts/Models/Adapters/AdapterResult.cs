using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

public class AdapterResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public List<Dictionary<string, object>> BatchData { get; set; } = new();
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public static ValidationResult Success => new() { IsValid = true };
    public static ValidationResult Fail(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}
