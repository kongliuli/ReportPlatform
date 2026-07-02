namespace Xinglin.Medical.Models;

public sealed class LabReportData
{
    public MedicalReportHeader Header { get; set; } = new();
    public PatientInfo Patient { get; set; } = new();
    public List<LabResultItem> Results { get; set; } = new();
    public MedicalReportFooter Footer { get; set; } = new();
}

public sealed class MedicalReportHeader
{
    public string HospitalName { get; set; } = string.Empty;
    public string ReportTitle { get; set; } = "检验报告单";
    public DateTime ReportDate { get; set; } = DateTime.Today;
}

public sealed class LabResultItem
{
    public string ItemName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
}

public sealed class MedicalReportFooter
{
    public string Technician { get; set; } = string.Empty;
    public string Reviewer { get; set; } = string.Empty;
}
