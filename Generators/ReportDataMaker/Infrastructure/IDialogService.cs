namespace ReportDataMaker.Infrastructure;

public interface IDialogService
{
    string? OpenFile(string filter, string title);
    string? SaveFile(string filter, string title, string? defaultName = null);
    bool Confirm(string message, string title);
    void ShowInfo(string message, string title);
    void ShowError(string message, string title);
    void ShowSuccess(string message, string title = "成功");
}
