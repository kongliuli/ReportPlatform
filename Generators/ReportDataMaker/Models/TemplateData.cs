using System.Collections.Generic;

namespace ReportDataMaker.Models
{
    public class TemplateData
    {
        public PatientData Patient { get; set; }
        public ReportData Report { get; set; }
        public List<TestItemData> TestItems { get; set; }

        public TemplateData()
        {
            Patient = new PatientData();
            Report = new ReportData();
            TestItems = new List<TestItemData>();
        }
    }

    public class PatientData
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
    }

    public class ReportData
    {
        public string SampleType { get; set; }
        public string ReportDate { get; set; }
        public string Technician { get; set; }
        public string Reviewer { get; set; }
        public string Department { get; set; }
        public string ChiefComplaint { get; set; }
        public string PresentIllness { get; set; }
        public string Treatment { get; set; }
        public string Complaint { get; set; }
        public string History { get; set; }
        public string Diagnosis { get; set; }
    }

    public class TestItemData
    {
        public string Result { get; set; }
        public string ReferenceRange { get; set; }
        public string Method { get; set; }
    }
}