namespace ReportDataMaker.Infrastructure;

/// <summary>对话框服务接口，定义文件选择和消息提示的契约</summary>
public interface IDialogService
{
    /// <summary>打开文件选择对话框</summary>
    /// <param name="filter">文件筛选器</param>
    /// <param name="title">对话框标题</param>
    /// <returns>选中的文件路径，未选择返回null</returns>
    string? OpenFile(string filter, string title);
    /// <summary>打开文件保存对话框</summary>
    /// <param name="filter">文件筛选器</param>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultName">默认文件名</param>
    /// <returns>保存的文件路径，未选择返回null</returns>
    string? SaveFile(string filter, string title, string? defaultName = null);
    /// <summary>显示确认对话框</summary>
    /// <param name="message">确认消息</param>
    /// <param name="title">对话框标题</param>
    /// <returns>用户是否确认</returns>
    bool Confirm(string message, string title);
    /// <summary>显示信息提示对话框</summary>
    /// <param name="message">信息内容</param>
    /// <param name="title">对话框标题</param>
    void ShowInfo(string message, string title);
    /// <summary>显示错误提示对话框</summary>
    /// <param name="message">错误信息</param>
    /// <param name="title">对话框标题</param>
    void ShowError(string message, string title);
    /// <summary>显示成功提示对话框</summary>
    /// <param name="message">成功信息</param>
    /// <param name="title">对话框标题</param>
    void ShowSuccess(string message, string title = "成功");
}
