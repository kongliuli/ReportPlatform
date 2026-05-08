using System.Windows.Media;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels.Tabs;

public class PreviewTabViewModel : TabViewModelBase
{
    private readonly ITemplatePreviewService _previewService;

    public ExternalTemplateDefinition? Template { get; }

    private Visual? _previewVisual;
    public Visual? PreviewVisual { get => _previewVisual; private set => SetProperty(ref _previewVisual, value); }

    private double _zoomLevel = 100;
    public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }

    public RelayCommand RefreshCommand { get; }

    public PreviewTabViewModel(ExternalTemplateDefinition template, ITemplatePreviewService previewService)
    {
        _previewService = previewService;
        Template = template;
        Title = "预览";
        IsClosable = false;
        RefreshCommand = new RelayCommand(_ => RefreshPreview());
    }

    public void RefreshPreview()
    {
        if (Template == null) return;
        PreviewVisual = _previewService.GeneratePreview(Template);
    }
}
