using System.Windows.Controls;
using ReportDataMaker.Adapter.Database.Common.Services;
using ReportDataMaker.ViewModels.Tabs;

namespace ReportDataMaker.Views.Tabs;

public partial class DatabaseAdapterTab : UserControl
{
    public DatabaseAdapterTab(DatabaseAdapterTabViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnColumnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not DatabaseAdapterTabViewModel vm) return;
        if (sender is not ListBox listBox) return;
        vm.SelectedColumnNames.Clear();
        foreach (var item in listBox.SelectedItems)
        {
            if (item is ColumnInfo col)
                vm.SelectedColumnNames.Add(col.Name);
        }
    }
}
