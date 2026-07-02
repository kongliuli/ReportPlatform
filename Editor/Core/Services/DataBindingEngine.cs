using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xinglin.ReportEditor.Contracts;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;
using Xinglin.WebReportEditor.Core.SharedInterfaces;

namespace Xinglin.WebReportEditor.Core.Services;

/// <summary>
/// 数据绑定引擎：将样本数据按 DataPath 注入到模板元素中。
/// </summary>
public class DataBindingEngine : IDataBindingEngine
{
    public TemplateDefinition ApplyDataBinding(TemplateDefinition template, Dictionary<string, object> sampleData)
    {
        ApplyCore(template, sampleData);
        return template;
    }

    public TemplateDefinition ApplyDataBinding(string templateJson, string sampleDataJson)
    {
        var template = TemplateSerializer.Deserialize(templateJson);
        var data = string.IsNullOrEmpty(sampleDataJson)
            ? new Dictionary<string, object>()
            : JsonConvert.DeserializeObject<Dictionary<string, object>>(sampleDataJson)
                ?? new Dictionary<string, object>();
        ApplyCore(template, data);
        return template;
    }

    public TemplateDefinition ApplyDataBinding(string templateJson, Dictionary<string, object> sampleData)
    {
        var template = TemplateSerializer.Deserialize(templateJson);
        ApplyCore(template, sampleData);
        return template;
    }

    private static void ApplyCore(TemplateDefinition templateDef, Dictionary<string, object> data)
    {
        if (templateDef?.Elements == null) return;

        // 遍历元素，注入绑定值
        foreach (var element in templateDef.Elements)
        {
            if (string.IsNullOrEmpty(element.DataPath))
                continue;

            var resolved = ResolveDataPath(data, element.DataPath);
            if (resolved == null)
                continue;

            // 根据元素类型写入对应的显示值
            ApplyElementValue(element, resolved);
        }

        return;
    }

    private static string? ResolveDataPath(Dictionary<string, object> data, string path)
    {
        var parts = path.Split('.');
        object? current = data;

        foreach (var part in parts)
        {
            if (current == null) return null;

            if (current is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(part, out var val))
                    return null;
                current = val;
            }
            else if (current is JObject jobj)
            {
                var token = jobj[part];
                if (token == null) return null;
                current = token.Type == JTokenType.Object ? token : (object)token.ToString();
            }
            else
            {
                return current.ToString();
            }
        }

        return current?.ToString();
    }

    private static void ApplyElementValue(ElementBase element, string value)
    {
        switch (element)
        {
            case TextElement textEl:
                textEl.Text = value;
                break;
            case NumberElement numEl:
                numEl.Value = value;
                break;
            case DateElement dateEl:
                dateEl.Value = value;
                break;
            case ImageElement imgEl:
                imgEl.Src = value;
                break;
            case CheckboxElement cbEl:
                cbEl.Checked = bool.TryParse(value, out var b) && b;
                break;
            case DropdownElement ddEl:
                ddEl.SelectedValue = value;
                break;
            case RadioElement radioEl:
                radioEl.Value = value;
                break;
            default:
                element.Label = value;
                break;
        }
    }
}
