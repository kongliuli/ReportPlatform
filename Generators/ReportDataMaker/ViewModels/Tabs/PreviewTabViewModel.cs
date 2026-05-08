using System.Windows.Media;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels.Tabs;

/// <summary>预览标签页视图模型，提供模板的可视化预览功能</summary>
public class PreviewTabViewModel : TabViewModelBase
{
    private readonly ITemplatePreviewService _previewService;

    /// <summary>关联的模板定义</summary>
    public ExternalTemplateDefinition? Template { get; }

    private Visual? _previewVisual;
    /// <summary>预览可视化对象</summary>
    public Visual? PreviewVisual { get => _previewVisual; private set => SetProperty(ref _previewVisual, value); }

    private double _zoomLevel = 100;
    /// <summary>缩放级别</summary>
    public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }

    /// <summary>刷新预览命令</summary>
    public RelayCommand RefreshCommand { get; }

    /// <summary>初始化预览标签页视图模型</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="previewService">模板预览服务</param>
    public PreviewTabViewModel(ExternalTemplateDefinition template, ITemplatePreviewService previewService)
    {
        _previewService = previewService;
        Template = template;
        Title = "预览";
        IsClosable = false;
        RefreshCommand = new RelayCommand(_ => RefreshPreview());
    }

    /// <summary>刷新预览内容</summary>
    public void RefreshPreview()
    {
        if (Template == null) return;
        PreviewVisual = _previewService.GeneratePreview(Template);
    }
}
