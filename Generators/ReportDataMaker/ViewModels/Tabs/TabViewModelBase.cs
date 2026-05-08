using ReportDataMaker.Infrastructure;

namespace ReportDataMaker.ViewModels.Tabs;

public abstract class TabViewModelBase : ViewModelBase
{
    private string _title = string.Empty;
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private bool _isClosable = true;
    public bool IsClosable { get => _isClosable; set => SetProperty(ref _isClosable, value); }

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

    public RelayCommand? CloseCommand { get; protected set; }
    public event Action<TabViewModelBase>? CloseRequested;

    protected void RequestClose() => CloseRequested?.Invoke(this);
}
