using System.Windows.Controls;
using ReportDataMaker.ViewModels.Tabs;

namespace ReportDataMaker.Views.Tabs;

public partial class DatabaseAdapterTab : UserControl
{
    public DatabaseAdapterTab(DatabaseAdapterTabViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
