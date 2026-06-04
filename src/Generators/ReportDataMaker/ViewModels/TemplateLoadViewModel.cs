using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels;

public partial class TemplateLoadViewModel : ObservableObject
{
    [ObservableProperty] private string _templateFilePath = string.Empty;
    [ObservableProperty] private string _templateJson = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<TemplateDefinition> _recentTemplates = new();

    private readonly ITemplateLoaderService _templateLoaderService;

    public TemplateLoadViewModel(ITemplateLoaderService templateLoaderService)
    {
        _templateLoaderService = templateLoaderService;
    }

    public event Action<TemplateDefinition>? TemplateLoaded;

    [RelayCommand]
    private void LoadFromFile()
    {
        IsLoading = true;
        StatusMessage = "正在加载模板文件...";
        try
        {
            var template = _templateLoaderService.LoadFromFile(TemplateFilePath);
            StatusMessage = $"加载成功: {template.Name} ({template.Elements?.Count ?? 0} 个元素)";
            TemplateLoaded?.Invoke(template);
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void LoadFromJson()
    {
        IsLoading = true;
        StatusMessage = "正在解析模板JSON...";
        try
        {
            var template = _templateLoaderService.LoadFromJson(TemplateJson);
            StatusMessage = $"解析成功: {template.Name} ({template.Elements?.Count ?? 0} 个元素)";
            TemplateLoaded?.Invoke(template);
        }
        catch (Exception ex)
        {
            StatusMessage = $"解析失败: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
