using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReportDataMaker.ViewModels;

public partial class SidePanelViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded = true;

    [RelayCommand]
    private void Toggle() => IsExpanded = !IsExpanded;
}
