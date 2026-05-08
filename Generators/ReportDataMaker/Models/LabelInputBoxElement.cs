namespace ReportDataMaker.Models;

public class LabelInputBoxElement : ReportExternalElementBase
{
    public string LabelText { get; set; } = string.Empty;
    public string InputPlaceholder { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
}
