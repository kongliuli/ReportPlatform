using ReportDataMaker.Infrastructure;

namespace ReportDataMaker.ViewModels;

public class SidePanelViewModel : ViewModelBase
{
    private bool _isExpanded = true;
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }

    public RelayCommand ToggleCommand { get; }

    public SidePanelViewModel()
    {
        ToggleCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
    }
}
