using Xinglin.Medical.Adapters;
using Xinglin.Medical.Binding;
using Xinglin.Medical.Formatting;
using Xinglin.Medical.Licensing;
using Xinglin.Medical.Rules;
using System.Text.Json;

namespace Xinglin.WebReportEditor.Server.Tests.Medical;

public class MedicalCoreTests
{
    [Fact]
    public void ConditionEngine_UsesHighestPriorityMatchingRule()
    {
        var rules = new[]
        {
            new ConditionRule { DataPath = "Result.Wbc", Operator = ConditionOperator.GreaterThan, Value = "10", Color = "#ff9900", Mark = "H", Priority = 1 },
            new ConditionRule { DataPath = "Result.Wbc", Operator = ConditionOperator.GreaterThan, Value = "20", Color = "#ff0000", Mark = "HH", Priority = 10 }
        };

        var display = new ConditionEngine().Evaluate(rules, new Dictionary<string, object?> { ["Result.Wbc"] = "24" });

        Assert.Equal("#ff0000", display.Color);
        Assert.Equal("HH", display.Mark);
    }

    [Theory]
    [InlineData("umol/L", "μmol/L")]
    [InlineData("10^9/L", "10⁹/L")]
    [InlineData("alpha", "α")]
    public void MedicalUnitFormatter_FormatsCommonMedicalUnits(string input, string expected)
    {
        Assert.Equal(expected, MedicalUnitFormatter.FormatUnit(input));
    }

    [Fact]
    public void MedicalBindingPathCatalog_ContainsPatientAndFooterPaths()
    {
        var paths = MedicalBindingPathCatalog.GetLabReportPaths();

        Assert.Contains("Patient.PatientName", paths);
        Assert.Contains("Footer.Technician", paths);
        Assert.True(MedicalBindingPathCatalog.IsValidLabReportPath("Patient.PatientName"));
        Assert.False(MedicalBindingPathCatalog.IsValidLabReportPath("Missing.Path"));
    }

    [Fact]
    public async Task FakePatientDataAdapter_ProducesPlatformDataPaths()
    {
        var result = await new FakePatientDataAdapter().ReadDataAsync();

        Assert.True(result.Success);
        Assert.Equal("张三", result.Data["Patient.PatientName"]);
        Assert.Contains("Patient.Age", result.Data.Keys);
    }

    [Fact]
    public void LocalMachineCode_GeneratesValidStableShape()
    {
        var code = LocalMachineCode.Generate();

        Assert.True(LocalMachineCode.IsValid(code));
        Assert.Equal(64, code.Length);
    }

    [Fact]
    public void MedicalTemplate_IsPresentAndUsesMedicalBindingPaths()
    {
        var templatePath = FindRepoFile("Medical", "Templates", "lab-report.template.json");
        using var document = JsonDocument.Parse(File.ReadAllText(templatePath));
        var root = document.RootElement;

        Assert.Equal("MedicalLabReport", root.GetProperty("type").GetString());
        var elements = root.GetProperty("elements").EnumerateArray().ToArray();
        Assert.Contains(elements, e => e.TryGetProperty("dataPath", out var path) && path.GetString() == "Patient.PatientName");
        Assert.Contains(elements, e => e.TryGetProperty("dataPath", out var path) && path.GetString() == "Footer.Technician");
    }

    [Fact]
    public void LegacyMedicalAssets_AreAvailableForBranchDevelopment()
    {
        Assert.True(Directory.Exists(FindRepoPath("Medical", "LegacyTemplates", "xinglin")));
        Assert.True(Directory.Exists(FindRepoPath("Medical", "LegacyTemplates", "xinglin-core")));
        Assert.True(Directory.Exists(FindRepoPath("Medical", "LegacyConfigs", "xinlingMain")));
        Assert.True(Directory.GetFiles(FindRepoPath("Medical", "docs"), "*.md", SearchOption.AllDirectories).Length >= 3);
    }

    private static string FindRepoFile(params string[] parts)
    {
        var path = FindRepoPath(parts);
        Assert.True(File.Exists(path), $"Missing file: {path}");
        return path;
    }

    private static string FindRepoPath(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(new[] { dir.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate) || Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return Path.Combine(parts);
    }
}
