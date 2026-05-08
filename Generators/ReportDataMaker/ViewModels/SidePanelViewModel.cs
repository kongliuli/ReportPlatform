using ReportDataMaker.Infrastructure;

namespace ReportDataMaker.ViewModels;

/// <summary>侧边面板视图模型，控制面板的展开和折叠</summary>
public class SidePanelViewModel : ViewModelBase
{
    private bool _isExpanded = true;
    /// <summary>面板是否展开</summary>
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }

    /// <summary>切换面板展开状态的命令</summary>
    public RelayCommand ToggleCommand { get; }

    /// <summary>初始化侧边面板视图模型</summary>
    public SidePanelViewModel()
    {
        ToggleCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
    }
}
