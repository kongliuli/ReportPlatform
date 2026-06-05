using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class ContextAdapterTabViewModel : MainTabViewModel
{
    [ObservableProperty] private string _selectedAdapterId = string.Empty;
    [ObservableProperty] private ObservableCollection<AdapterItem> _adapters = new();

    private readonly AdapterRegistry _registry;
    private IDataAdapter? _dataAdapter;

    public ContextAdapterTabViewModel(MainViewModel mainViewModel, AdapterRegistry registry) : base(mainViewModel) { _registry = registry; }

    public override void OnTemplateChanged()
    {
        base.OnTemplateChanged();
        if (CurrentTemplate == null) return;

        _dataAdapter = _registry.GetByType(AdapterType.Context)?.CreateService(CurrentTemplate);

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
    private async Task ApplyAdapterAsync()
    {
        if (CurrentTemplate == null || string.IsNullOrEmpty(SelectedAdapterId) || _dataAdapter == null) return;
        var result = await _dataAdapter.ReadDataAsync();
        StatusText = result.Success
            ? $"已应用上下文适配器: {SelectedAdapterId}"
            : $"应用失败: {result.ErrorMessage}";
    }
}

public class AdapterItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
