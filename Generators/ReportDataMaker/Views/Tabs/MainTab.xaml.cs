using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using ReportDataMaker.ViewModels.Tabs;

namespace ReportDataMaker.Views.Tabs;

public partial class MainTab : UserControl
{
    public MainTab() { InitializeComponent(); }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Column.CanUserSort = false;
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.DataContext is not FieldViewModel field)
            return;

        if (e.EditingElement is not TextBox textBox)
            return;

        var editedValue = textBox.Text;
        var rowIndex = e.Row.GetIndex();
        var columnIndex = e.Column.DisplayIndex;

        if (rowIndex < 0 || columnIndex < 0)
            return;

        var dataRowIndex = rowIndex + field.TableHeaderRows;

        while (field.TableCellData.Count <= dataRowIndex)
            field.TableCellData.Add(new List<string>());

        while (field.TableCellData[dataRowIndex].Count <= columnIndex)
            field.TableCellData[dataRowIndex].Add(string.Empty);

        field.TableCellData[dataRowIndex][columnIndex] = editedValue;

        if (DataContext is MainTabViewModel vm)
            vm.RefreshPreview();
    }
}
