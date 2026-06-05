using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextAdapterService : IDataAdapter
{
    public string AdapterId => "context-default";
    public string AdapterName => "上下文适配器";
    public AdapterType Type => AdapterType.Context;
    public IReadOnlyList<string> TargetDataPaths => Array.Empty<string>();

    public Task<AdapterResult> ReadDataAsync() => throw new NotImplementedException();
    public Task<AdapterResult> ReadBatchDataAsync() => throw new NotImplementedException();
    public Task<ValidationResult> ValidateConfigAsync() => Task.FromResult(ValidationResult.Success);

    public Dictionary<string, object> ExtractContextData(TemplateDefinition template)
    {
        var data = new Dictionary<string, object>();
        if (template?.Elements == null) return data;

        foreach (var element in template.Elements)
        {
            if (element is not ExternalElementBase extElem || extElem.Group != ElementGroup.Editable) continue;
            if (string.IsNullOrEmpty(extElem.DataPath)) continue;

            var value = extElem switch
            {
                TextElement te => (object)(te.Text ?? te.DefaultValue ?? string.Empty),
                NumberElement ne => ne.Value ?? ne.DefaultValue ?? string.Empty,
                DateElement de => de.Value ?? de.DefaultValue ?? string.Empty,
                DropdownElement dd => dd.SelectedValue ?? dd.DefaultValue ?? string.Empty,
                CheckboxElement cb => cb.Checked,
                RadioElement re => re.IsChecked,
                TableElement tb => (object)(tb.CellData ?? new List<List<string>>()),
                _ => extElem.DefaultValue ?? string.Empty
            };

            data[extElem.DataPath] = value;
        }

        return data;
    }

    public void ApplyContextData(TemplateDefinition template, Dictionary<string, object> data)
    {
        if (template?.Elements == null) return;

        foreach (var element in template.Elements)
        {
            if (element is not ExternalElementBase extElem || extElem.Group != ElementGroup.Editable) continue;
            if (string.IsNullOrEmpty(extElem.DataPath)) continue;
            if (!data.TryGetValue(extElem.DataPath, out var value)) continue;

            switch (extElem)
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
                    extElem.DefaultValue = value?.ToString() ?? string.Empty;
                    break;
            }
        }
    }
}
