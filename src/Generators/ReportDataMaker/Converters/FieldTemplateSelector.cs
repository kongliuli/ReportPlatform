using System.Windows;
using System.Windows.Controls;
using ReportDataMaker.ViewModels.Tabs;

namespace ReportDataMaker.Converters;

/// <summary>
/// Selects the appropriate DataTemplate for a FieldViewModel based on FieldType.
/// Table fields get a DataGrid; all others get the default text-field template.
/// </summary>
public class FieldTemplateSelector : DataTemplateSelector
{
    /// <summary>DataTemplate for non-table fields (TextBox-based)</summary>
    public DataTemplate? DefaultFieldTemplate { get; set; }

    /// <summary>DataTemplate for table fields (DataGrid-based)</summary>
    public DataTemplate? TableFieldTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is FieldViewModel fvm && fvm.FieldType == FieldDataType.Table && TableFieldTemplate != null)
            return TableFieldTemplate;

        return DefaultFieldTemplate ?? base.SelectTemplate(item, container);
    }
}
