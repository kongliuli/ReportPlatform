using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextAdapterService
{
    public Dictionary<string, object> ExtractContextData(TemplateDefinition template)
    {
        var data = new Dictionary<string, object>();
        if (template?.Elements == null) return data;

        foreach (var element in template.Elements)
        {
            if (element.Group != ElementGroup.Editable) continue;
            if (string.IsNullOrEmpty(element.DataPath)) continue;

            var value = element switch
            {
                TextElement te => (object)(te.Text ?? te.DefaultValue ?? string.Empty),
                NumberElement ne => ne.Value ?? ne.DefaultValue ?? string.Empty,
                DateElement de => de.Value ?? de.DefaultValue ?? string.Empty,
                DropdownElement dd => dd.SelectedValue ?? dd.DefaultValue ?? string.Empty,
                CheckboxElement cb => cb.Checked,
                RadioElement re => re.IsChecked,
                TableElement tb => (object)(tb.CellData ?? new List<List<string>>()),
                _ => element.DefaultValue ?? string.Empty
            };

            data[element.DataPath] = value;
        }

        return data;
    }

    public void ApplyContextData(TemplateDefinition template, Dictionary<string, object> data)
    {
        if (template?.Elements == null) return;

        foreach (var element in template.Elements)
        {
            if (element.Group != ElementGroup.Editable) continue;
            if (string.IsNullOrEmpty(element.DataPath)) continue;
            if (!data.TryGetValue(element.DataPath, out var value)) continue;

            switch (element)
            {
                case TextElement te:
                    te.Text = value?.ToString();
                    break;
                case NumberElement ne:
                    ne.Value = value?.ToString();
                    break;
                case DateElement de:
                    de.Value = value?.ToString();
                    break;
                case DropdownElement dd:
                    dd.SelectedValue = value?.ToString();
                    break;
                case CheckboxElement cb:
                    cb.Checked = value is bool b ? b : value?.ToString()?.ToLower() == "true";
                    break;
                case RadioElement re:
                    re.IsChecked = value is bool b2 ? b2 : value?.ToString()?.ToLower() == "true";
                    break;
                case TableElement tb:
                    if (value is List<List<string>> cellData)
                        tb.CellData = cellData;
                    break;
                default:
                    element.DefaultValue = value?.ToString() ?? string.Empty;
                    break;
            }
        }
    }
}
