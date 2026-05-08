using System.Windows;
using System.Windows.Controls;
using ReportDataMaker.Services.ExcelAdapter;
using ReportDataMaker.ViewModels.Tabs;

namespace ReportDataMaker.Views.Tabs;

public partial class ExcelAdapterTab : UserControl
{
    public ExcelAdapterTab()
    {
        InitializeComponent();
    }

    private void OnSingleModeChecked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ExcelAdapterTabViewModel vm)
            vm.ImportModeValue = ImportMode.Single;
    }

    private void OnBatchModeChecked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ExcelAdapterTabViewModel vm)
            vm.ImportModeValue = ImportMode.Batch;
    }
}
