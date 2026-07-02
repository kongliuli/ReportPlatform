using Xinglin.Medical.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace Xinglin.Medical.Adapters;

public sealed class FakePatientDataAdapter : IDataAdapter
{
    public string AdapterId => "medical.fake-patient";
    public string AdapterName => "Medical fake patient";
    public AdapterType Type => AdapterType.Context;

    public IReadOnlyList<string> TargetDataPaths { get; } =
    [
        "Patient.PatientId",
        "Patient.PatientName",
        "Patient.Gender",
        "Patient.Age"
    ];

    public PatientInfo Patient { get; set; } = new()
    {
        PatientId = "P0001",
        PatientName = "张三",
        Gender = "男",
        Age = 35
    };

    public Task<AdapterResult> ReadDataAsync() => Task.FromResult(new AdapterResult
    {
        Success = true,
        Data = new Dictionary<string, object>
        {
            ["Patient.PatientId"] = Patient.PatientId,
            ["Patient.PatientName"] = Patient.PatientName,
            ["Patient.Gender"] = Patient.Gender,
            ["Patient.Age"] = Patient.Age ?? 0
        }
    });

    public async Task<AdapterResult> ReadBatchDataAsync()
    {
        var result = await ReadDataAsync();
        result.BatchData.Add(result.Data);
        return result;
    }

    public Task<ValidationResult> ValidateConfigAsync() => Task.FromResult(ValidationResult.Success);
}
