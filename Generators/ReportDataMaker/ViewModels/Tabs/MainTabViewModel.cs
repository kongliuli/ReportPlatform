using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels.Tabs;

public class MainTabViewModel : TabViewModelBase
{
    private readonly IDataBindingService _dataBindingService;
    private readonly ITemplatePreviewService _previewService;
    private readonly DispatcherTimer _previewDebounceTimer;

    public ExternalTemplateDefinition? Template { get; }
    public ObservableCollection<FieldViewModel> Fields { get; } = new();
    public ObservableCollection<SectionViewModel> Sections { get; } = new();
    public ObservableCollection<UnsupportedElementInfo> UnsupportedElements { get; } = new();

    private object? _previewVisual;
    public object? PreviewVisual { get => _previewVisual; private set => SetProperty(ref _previewVisual, value); }

    private double _zoomLevel = 100;
    public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearAllCommand { get; }
    public RelayCommand FillFromAdapterCommand { get; }

    public MainTabViewModel(ExternalTemplateDefinition template, IDataBindingService dataBindingService, ITemplatePreviewService previewService)
    {
        _dataBindingService = dataBindingService;
        _previewService = previewService;
        Template = template;
        Title = "报告编辑";
        IsClosable = false;

        RefreshCommand = new RelayCommand(_ => RefreshPreview());
        ClearAllCommand = new RelayCommand(_ => ExecuteClearAll());
        FillFromAdapterCommand = new RelayCommand(_ => { });

        _previewDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _previewDebounceTimer.Tick += (_, _) => { _previewDebounceTimer.Stop(); RefreshPreview(); };

        LoadFields();
        DetectUnsupportedElements();
        RefreshPreview();
    }

    private void LoadFields()
    {
        if (Template?.Elements == null) return;
        Sections.Clear();
        Fields.Clear();

        var fixedSection = new SectionViewModel { Title = "固定内容", IsExpanded = false };
        var manualSection = new SectionViewModel { Title = "手动录入" };
        var adapterSection = new SectionViewModel { Title = "适配器数据" };

        var ordered = Template.Elements.OrderBy(e => e.Y).ThenBy(e => e.X).ToList();

        foreach (var element in ordered)
        {
            if (element is ExternalLineElement or ExternalDividerElement) continue;

            if (element.Group == ElementGroup.Fixed)
            {
                var text = element switch
                {
                    ExternalTextElement t => t.Text ?? element.DefaultValue,
                    _ => element.DefaultValue
                };
                var displayLabel = element.Label ?? text ?? string.Empty;
                if (string.IsNullOrEmpty(displayLabel)) continue;

                fixedSection.Fields.Add(new FieldViewModel
                {
                    ElementId = element.Id,
                    Label = element.Label ?? string.Empty,
                    DataPath = element.DataPath ?? string.Empty,
                    Value = text ?? string.Empty,
                    FieldType = FieldDataType.ReadOnly
                });
                continue;
            }

            if (string.IsNullOrEmpty(element.DataPath)) continue;

            var field = CreateFieldViewModel(element);
            field.PropertyChanged += OnFieldPropertyChanged;

            if (element.Group == ElementGroup.Context || element.Group == ElementGroup.DataAdapter
                || !string.IsNullOrEmpty(element.DefaultValue))
            {
                adapterSection.Fields.Add(field);
            }
            else
            {
                manualSection.Fields.Add(field);
            }
            Fields.Add(field);
        }

        if (fixedSection.Fields.Count > 0) Sections.Add(fixedSection);
        if (manualSection.Fields.Count > 0) Sections.Add(manualSection);
        if (adapterSection.Fields.Count > 0) Sections.Add(adapterSection);
    }

    private FieldViewModel CreateFieldViewModel(ReportExternalElementBase element)
    {
        var field = new FieldViewModel
        {
            ElementId = element.Id,
            Label = element.Label ?? string.Empty,
            DataPath = element.DataPath!,
            Value = element.DefaultValue
        };

        switch (element)
        {
            case ExternalNumberElement num:
                field.FieldType = FieldDataType.Number;
                field.Unit = num.Unit;
                field.DecimalPlaces = num.DecimalPlaces;
                break;
            case ExternalDateElement date:
                field.FieldType = FieldDataType.Date;
                field.DateFormat = !string.IsNullOrEmpty(date.Format) ? date.Format : "yyyy-MM-dd";
                if (DateTime.TryParse(date.Value, out var dt)) field.DateValue = dt;
                break;
            case ExternalDropdownElement:
                field.FieldType = FieldDataType.Dropdown;
                field.Options = element.Options?.ToList() ?? new();
                break;
            case ExternalCheckboxElement cb:
                field.FieldType = FieldDataType.Boolean;
                field.IsChecked = cb.Checked;
                break;
            case ExternalRadioElement radio:
                field.FieldType = FieldDataType.Boolean;
                field.Options = element.Options?.ToList() ?? new();
                field.GroupName = radio.GroupName;
                field.SelectedOption = radio.Value;
                break;
            case ExternalTextElement:
                field.FieldType = FieldDataType.Text;
                field.IsMultiLine = element.Height > 12;
                break;
            default:
                field.FieldType = FieldDataType.Text;
                break;
        }

        return field;
    }

    private void OnFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FieldViewModel.Value))
            SchedulePreviewRefresh();
    }

    private void SchedulePreviewRefresh()
    {
        _previewDebounceTimer.Stop();
        _previewDebounceTimer.Start();
    }

    private void ExecuteClearAll()
    {
        foreach (var field in Fields)
        {
            field.Value = string.Empty;
            field.IsChecked = false;
            field.SelectedOption = string.Empty;
            field.DateValue = null;
        }
        RefreshPreview();
    }

    public void ApplyDataFromAdapter(string dataPath, object value)
    {
        var field = Fields.FirstOrDefault(f => f.DataPath == dataPath);
        if (field != null) field.Value = value?.ToString() ?? string.Empty;
    }

    public void RefreshPreview()
    {
        if (Template == null) return;

        try
        {
            var templateWithData = new ExternalTemplateDefinition
            {
                Id = Template.Id,
                Name = Template.Name,
                Type = Template.Type,
                Version = Template.Version,
                PageWidth = Template.PageWidth,
                PageHeight = Template.PageHeight,
                Orientation = Template.Orientation,
                MarginLeft = Template.MarginLeft,
                MarginRight = Template.MarginRight,
                MarginTop = Template.MarginTop,
                MarginBottom = Template.MarginBottom,
                BackgroundColor = Template.BackgroundColor,
                GlobalFontSize = Template.GlobalFontSize,
                EnableGlobalFontSize = Template.EnableGlobalFontSize,
                Elements = new List<ReportExternalElementBase>()
            };

            foreach (var element in Template.Elements)
            {
                var clonedElement = CloneElement(element);
                if (clonedElement == null) continue;
                if (!string.IsNullOrEmpty(element.DataPath))
                {
                    var field = Fields.FirstOrDefault(f => f.DataPath == element.DataPath);
                    if (field != null && !string.IsNullOrEmpty(field.Value))
                        clonedElement.DefaultValue = field.Value;
                }
                templateWithData.Elements.Add(clonedElement);
            }

            PreviewVisual = _previewService.GeneratePreview(templateWithData);
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[MainTabVM] RefreshPreview 异常: {ex.Message}");
        }
    }

    private static readonly Newtonsoft.Json.JsonSerializerSettings _cloneSettings = new()
    {
        Converters = { new ReportExternalElementConverter() },
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Populate
    };

    private static ReportExternalElementBase? CloneElement(ReportExternalElementBase source)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(source, _cloneSettings);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<ReportExternalElementBase>(json, _cloneSettings);
    }

    private void DetectUnsupportedElements()
    {
        UnsupportedElements.Clear();
        if (Template?.Elements == null) return;

        foreach (var element in Template.Elements)
        {
            var info = element switch
            {
                ExternalChartElement ch => new UnsupportedElementInfo { ElementType = "图表", Description = $"{ch.ChartType ?? "未知类型"} - {ch.Title ?? ch.DataSource ?? ""}", Reason = "图表渲染需要数据源和图表库支持" },
                ExternalRepeatElement rp => new UnsupportedElementInfo { ElementType = "重复区域", Description = rp.DataSource ?? "未配置数据源", Reason = "重复区域需要数据源绑定" },
                ExternalContainerElement ct when ct.Children.Count > 0 => new UnsupportedElementInfo { ElementType = "容器", Description = $"包含 {ct.Children.Count} 个子元素", Reason = "容器内子元素布局为占位渲染" },
                ExternalBarcodeElement bc when string.IsNullOrEmpty(bc.Value) => new UnsupportedElementInfo { ElementType = "条形码", Description = bc.Format ?? "CODE128", Reason = "缺少条码值" },
                ExternalQrCodeElement qr when string.IsNullOrEmpty(qr.Value) => new UnsupportedElementInfo { ElementType = "二维码", Description = "", Reason = "缺少二维码内容" },
                _ => null
            };
            if (info != null) UnsupportedElements.Add(info);
        }
    }
}

public class UnsupportedElementInfo
{
    public string ElementType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
