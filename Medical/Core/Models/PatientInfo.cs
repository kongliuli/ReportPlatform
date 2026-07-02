namespace Xinglin.Medical.Models;

public sealed class PatientInfo
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string IdCardNumber { get; set; } = string.Empty;
}
