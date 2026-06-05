using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class PreviewTabViewModel : MainTabViewModel
{
    [ObservableProperty] private System.Windows.Controls.Canvas? _previewCanvas;
    [ObservableProperty] private double _zoomLevel = 1.0;

    private readonly CanvasRenderer _canvasRenderer = new();

    public PreviewTabViewModel(MainViewModel mainViewModel) : base(mainViewModel) { }

    public override void OnTemplateChanged()
    {
        base.OnTemplateChanged();
        RefreshPreview();
    }

    public override void RefreshPreview()
    {
        if (CurrentTemplate == null) return;

        PreviewCanvas = new System.Windows.Controls.Canvas();
        _canvasRenderer.RenderToCanvas(PreviewCanvas, CurrentTemplate);
    }

    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel + 0.1, 3.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel - 0.1, 0.3);
    }

    [RelayCommand]
    private void ResetZoom()
    {
        ZoomLevel = 1.0;
    }
}
