namespace ReportDataMaker.Models;

public class TemplateData
{
    public PatientData Patient { get; set; } = new();
    public ReportData Report { get; set; } = new();
    public List<TestItemData> TestItems { get; set; } = new();
}

public class PatientData
{
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
}

public class ReportData
{
    public string SampleType { get; set; } = string.Empty;
    public string ReportDate { get; set; } = string.Empty;
    public string Technician { get; set; } = string.Empty;
    public string Reviewer { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string PresentIllness { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Complaint { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
}

public class TestItemData
{
    public string Result { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}
