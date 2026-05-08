using ReportDataMaker.Infrastructure;

namespace ReportDataMaker.ViewModels.Tabs;

/// <summary>标签页视图模型基类，提供标签页的通用属性和行为</summary>
public abstract class TabViewModelBase : ViewModelBase
{
    private string _title = string.Empty;
    /// <summary>标签页标题</summary>
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private bool _isClosable = true;
    /// <summary>标签页是否可关闭</summary>
    public bool IsClosable { get => _isClosable; set => SetProperty(ref _isClosable, value); }

    private bool _isSelected;
    /// <summary>标签页是否被选中</summary>
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

    /// <summary>关闭标签页命令</summary>
    public RelayCommand? CloseCommand { get; protected set; }
    /// <summary>关闭请求事件</summary>
    public event Action<TabViewModelBase>? CloseRequested;

    /// <summary>触发关闭请求</summary>
    protected void RequestClose() => CloseRequested?.Invoke(this);
}
