using System.Windows;
using System.Windows.Controls;
using ReportDataMaker.ViewModels.Tabs;

namespace ReportDataMaker.Infrastructure;

public class FieldDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? MultiLineTextTemplate { get; set; }
    public DataTemplate? DropdownTemplate { get; set; }
    public DataTemplate? NumberTemplate { get; set; }
    public DataTemplate? DateTemplate { get; set; }
    public DataTemplate? CheckboxTemplate { get; set; }
    public DataTemplate? RadioTemplate { get; set; }
    public DataTemplate? ReadOnlyTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not FieldViewModel field) return TextTemplate;

        if (field.FieldType == FieldDataType.ReadOnly) return ReadOnlyTemplate;

        return field.FieldType switch
        {
            FieldDataType.Text => field.IsMultiLine ? MultiLineTextTemplate : TextTemplate,
            FieldDataType.Dropdown => DropdownTemplate,
            FieldDataType.Number => NumberTemplate,
            FieldDataType.Date => DateTemplate,
            FieldDataType.Boolean when field.Options.Count > 0 => RadioTemplate,
            FieldDataType.Boolean => CheckboxTemplate,
            _ => TextTemplate
        };
    }
}
