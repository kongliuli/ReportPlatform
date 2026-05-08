namespace ReportDataMaker.Services.ContextAdapter;

public enum ContextValueSource
{
    Static,
    CurrentDate,
    CurrentTime,
    CurrentDateTime,
    CurrentUser,
    MachineName,
    Custom
}

public class DynamicContextRule
{
    public string DataPath { get; set; } = string.Empty;
    public ContextValueSource Source { get; set; }
    public string? Format { get; set; }
}
