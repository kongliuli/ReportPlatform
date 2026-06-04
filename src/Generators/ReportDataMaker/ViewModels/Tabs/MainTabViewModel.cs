using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xinglin.ReportEditor.Contracts;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class MainTabViewModel
{
    [ObservableProperty] private string _templateInfo = string.Empty;
    [ObservableProperty] private string _pageInfo = string.Empty;
    [ObservableProperty] private int _elementCount;
    [ObservableProperty] private string _orientation = string.Empty;
    [ObservableProperty] private double _pageWidth;
    [ObservableProperty] private double _pageHeight;
    [ObservableProperty] private double _marginLeft;
    [ObservableProperty] private double _marginRight;
    [ObservableProperty] private double _marginTop;
    [ObservableProperty] private double _marginBottom;

    public virtual void OnTemplateChanged()
    {
        IsEnabled = CurrentTemplate != null;
        if (CurrentTemplate == null) return;

        TemplateInfo = $"{CurrentTemplate.Name} (v{CurrentTemplate.Version})";
        ElementCount = CurrentTemplate.Elements?.Count ?? 0;
        PageWidth = CurrentTemplate.PageSettings.PageWidth;
        PageHeight = CurrentTemplate.PageSettings.PageHeight;
        MarginLeft = CurrentTemplate.PageSettings.MarginLeft;
        MarginRight = CurrentTemplate.PageSettings.MarginRight;
        MarginTop = CurrentTemplate.PageSettings.MarginTop;
        MarginBottom = CurrentTemplate.PageSettings.MarginBottom;
        Orientation = CurrentTemplate.PageSettings.Orientation.ToString();
        PageInfo = $"{PageWidth}×{PageHeight}mm ({Orientation})";
    }

    [RelayCommand]
    private void CloneTemplate()
    {
        if (CurrentTemplate == null) return;
        try
        {
            var json = TemplateSerializer.Serialize(CurrentTemplate);
            var cloned = TemplateSerializer.Deserialize(json);
            MainViewModel.CurrentTemplate = cloned;
            StatusText = "模板已克隆";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"克隆模板失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ResetData()
    {
        if (CurrentTemplate == null) return;
        foreach (var element in CurrentTemplate.Elements)
        {
            element.DefaultValue = string.Empty;
        }
        StatusText = "数据已重置";
    }

    public string StatusText
    {
        get => MainViewModel.StatusText;
        set => MainViewModel.StatusText = value;
    }
}
