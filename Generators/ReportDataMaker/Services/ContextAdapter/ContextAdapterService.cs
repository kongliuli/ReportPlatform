using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextAdapterService
{
    public AdapterResult FillContext(ExternalTemplateDefinition template, ContextAdapterConfig config)
    {
        var data = new Dictionary<string, object>();

        foreach (var element in template.Elements)
        {
            if (element.Group != ElementGroup.Context) continue;
            if (string.IsNullOrEmpty(element.DataPath)) continue;

            if (config.StaticValues.TryGetValue(element.DataPath, out var staticVal))
            {
                data[element.DataPath] = staticVal;
                continue;
            }

            var rule = config.DynamicRules.FirstOrDefault(r => r.DataPath == element.DataPath);
            if (rule != null)
            {
                data[element.DataPath] = ResolveDynamic(rule);
                continue;
            }

            data[element.DataPath] = ResolveBuiltIn(element.DataPath);
        }

        return new AdapterResult { Success = true, Data = data };
    }

    private object ResolveDynamic(DynamicContextRule rule)
    {
        var format = rule.Format;
        return rule.Source switch
        {
            ContextValueSource.CurrentDate => DateTime.Now.ToString(format ?? "yyyy-MM-dd"),
            ContextValueSource.CurrentTime => DateTime.Now.ToString(format ?? "HH:mm:ss"),
            ContextValueSource.CurrentDateTime => DateTime.Now.ToString(format ?? "yyyy-MM-dd HH:mm"),
            ContextValueSource.CurrentUser => Environment.UserName,
            ContextValueSource.MachineName => Environment.MachineName,
            ContextValueSource.Static => rule.Format ?? string.Empty,
            _ => string.Empty
        };
    }

    private object ResolveBuiltIn(string dataPath)
    {
        return dataPath switch
        {
            "Context.DateTime.Now" => DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            "Context.DateTime.Date" => DateTime.Now.ToString("yyyy-MM-dd"),
            "Context.DateTime.Time" => DateTime.Now.ToString("HH:mm:ss"),
            "Context.DateTime.Year" => DateTime.Now.Year.ToString(),
            "Context.DateTime.Month" => DateTime.Now.Month.ToString(),
            "Context.DateTime.Day" => DateTime.Now.Day.ToString(),
            _ => string.Empty
        };
    }
}
