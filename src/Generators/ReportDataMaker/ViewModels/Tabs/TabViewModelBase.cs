using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public abstract partial class TabViewModelBase : ObservableObject
{
    [ObservableProperty] private string _header = string.Empty;
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private bool _isClosable;

    public event Action<TabViewModelBase>? CloseRequested;

    [RelayCommand]
    private void CloseTab()
    {
        CloseRequested?.Invoke(this);
    }
}

public abstract partial class MainTabViewModel : TabViewModelBase
{
    protected readonly MainViewModel MainViewModel;

    protected MainTabViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
    }

    public TemplateDefinition? CurrentTemplate => MainViewModel.CurrentTemplate;
}
