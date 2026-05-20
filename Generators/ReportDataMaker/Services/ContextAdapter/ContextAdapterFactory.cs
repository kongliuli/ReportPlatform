using System.Collections.Generic;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextAdapterFactory
{
    private readonly ContextProfileStore _profileStore;
    private readonly ContextAdapterService _service;

    public ContextAdapterFactory(ContextProfileStore profileStore, ContextAdapterService service)
    {
        _profileStore = profileStore;
        _service = service;
    }

    public AdapterType Type => AdapterType.Context;

    public AdapterResult Fill(ExternalTemplateDefinition template, ContextAdapterConfig config)
    {
        return _service.FillContext(template, config);
    }

    public AdapterResult FillWithProfile(ExternalTemplateDefinition template, string profileName)
    {
        var config = _profileStore.Load(profileName);
        return _service.FillContext(template, config);
    }

    public ContextAdapterConfig LoadProfile(string profileName) => _profileStore.Load(profileName);
    public void SaveProfile(ContextAdapterConfig config) => _profileStore.Save(config);
    public void DeleteProfile(string profileName) => _profileStore.Delete(profileName);
    public List<string> GetProfileNames() => _profileStore.GetProfileNames();

    public List<string> DetectUnconfiguredFields(ExternalTemplateDefinition template, ContextAdapterConfig config)
    {
        var configured = new HashSet<string>(config.StaticValues.Keys);
        foreach (var rule in config.DynamicRules) configured.Add(rule.DataPath);

        var unconfigured = new List<string>();
        foreach (var element in template.Elements)
        {
            if (!string.IsNullOrEmpty(element.DataPath) && !configured.Contains(element.DataPath))
                unconfigured.Add(element.DataPath);
        }
        return unconfigured;
    }
}
