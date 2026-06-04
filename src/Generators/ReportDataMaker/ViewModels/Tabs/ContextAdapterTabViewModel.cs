using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using ReportDataMaker.Services.ContextAdapter;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class ContextAdapterTabViewModel : MainTabViewModel
{
    [ObservableProperty] private string _selectedAdapterId = string.Empty;
    [ObservableProperty] private ObservableCollection<AdapterItem> _adapters = new();

    private readonly ContextAdapterFactory _contextAdapterFactory = new();

    public ContextAdapterTabViewModel(MainViewModel mainViewModel) : base(mainViewModel) { }

    public override void OnTemplateChanged()
    {
        base.OnTemplateChanged();
        if (CurrentTemplate == null) return;

        Adapters.Clear();
        var adapterIds = CurrentTemplate.Elements
            .Where(e => e is ExternalElementBase eb && eb.Group == ElementGroup.Editable && !string.IsNullOrEmpty(eb.AdapterId))
            .Select(e => ((ExternalElementBase)e).AdapterId)
            .Distinct();

        foreach (var id in adapterIds)
        {
            Adapters.Add(new AdapterItem { Id = id!, Name = $"适配器: {id}" });
        }
    }

    [RelayCommand]
    private void ApplyAdapter()
    {
        if (CurrentTemplate == null || string.IsNullOrEmpty(SelectedAdapterId)) return;
        var service = _contextAdapterFactory.Create(CurrentTemplate);
        StatusText = $"已应用上下文适配器: {SelectedAdapterId}";
    }
}

public class AdapterItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
