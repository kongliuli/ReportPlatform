using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker
{
    public partial class MainWindow : Window
    {
        private JsonTemplateLoader _templateLoader;
        private CanvasRenderer _canvasRenderer;
        private InputControlGenerator _controlGenerator;
        private BatchImportService _batchImportService;
        private HospitalConfigService _hospitalConfigService;
        private ExternalTemplateDefinition _currentTemplate;
        private double _currentZoom = 1.0;
        private BatchImportService.ImportResult _batchImportResult;
        private TemplateLoadResult _lastLoadResult;
        private GridLength _savedInfoWidth = new GridLength(280);

        public MainWindow()
        {
            InitializeComponent();

            _templateLoader = new JsonTemplateLoader();
            _canvasRenderer = new CanvasRenderer();
            _controlGenerator = new InputControlGenerator(_canvasRenderer);
            _batchImportService = new BatchImportService();
            _hospitalConfigService = new HospitalConfigService();

            LoadAdapterConfigs();
            LoadHospitalConfig();

            this.KeyDown += MainWindow_KeyDown;
        }

        /// <summary>
        /// 加载适配器配置，仅从 adapter-config.json 读取 targetPaths 用于 DataAdapter 分类
        /// </summary>
        private void LoadAdapterConfigs()
        {
            try
            {
                var adapterPaths = new HashSet<string>();
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                var adapterConfigPath = Path.Combine(baseDir, "Configs", "adapter-config.json");
                if (File.Exists(adapterConfigPath))
                {
                    var json = File.ReadAllText(adapterConfigPath);
                    var root = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json,
                        new { adapters = new[] { new { targetPaths = new List<string>() } } });
                    if (root?.adapters != null)
                        foreach (var adp in root.adapters)
                            if (adp.targetPaths != null)
                                foreach (var p in adp.targetPaths)
                                    if (!string.IsNullOrEmpty(p))
                                        adapterPaths.Add(p);
                }

                _templateLoader.SetAdapterConfigPaths(adapterPaths);
            }
            catch
            {
                // 配置加载失败不影响主流程
            }
        }

        /// <summary>
        /// 加载医院配置（默认人员、科室等）
        /// </summary>
        private void LoadHospitalConfig()
        {
            try
            {
                var configPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Configs", "hospital-config.json");
                _hospitalConfigService.Load(configPath);
            }
            catch
            {
                // 配置加载失败不影响主流程
            }
        }

        #region Panel Toggle

        private void ToggleInfoPanel_Click(object sender, RoutedEventArgs e)
        {
            if (colInfo.Width.Value > 5)
            {
                _savedInfoWidth = colInfo.Width;
                colInfo.Width = new GridLength(0);
            }
            else
            {
                colInfo.Width = _savedInfoWidth.Value > 5 ? _savedInfoWidth : new GridLength(280);
            }
        }

        #endregion

        #region Drag-Drop

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && Path.GetExtension(files[0]).ToLower() == ".json")
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && Path.GetExtension(files[0]).ToLower() == ".json")
                {
                    LoadTemplateFromFile(files[0]);
                }
            }
        }

        #endregion

        #region Menu Events

        private void MenuItemLoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplate();
        }

        private void MenuItemSaveData_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemRefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "ReportDataMaker v3.1\n基于JSON模板的医疗报告数据预览与录入工具\n\n" +
                "支持 23 种元素类型 | 数据绑定 | 条码/二维码 | 数据库适配",
                "关于 ReportDataMaker",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion

        #region Button Events

        private void BtnLoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplate();
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value = Math.Max(zoomSlider.Minimum, zoomSlider.Value - 25);
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value = Math.Min(zoomSlider.Maximum, zoomSlider.Value + 25);
        }

        private void BtnFitToWindow_Click(object sender, RoutedEventArgs e)
        {
            if (canvasPreview.Width > 0 && canvasPreview.Height > 0)
            {
                var viewerWidth = previewScrollViewer.ActualWidth - 52;
                var viewerHeight = previewScrollViewer.ActualHeight - 52;

                if (viewerWidth > 0 && viewerHeight > 0)
                {
                    var scaleX = viewerWidth / canvasPreview.Width;
                    var scaleY = viewerHeight / canvasPreview.Height;
                    var scale = Math.Min(scaleX, scaleY);

                    zoomSlider.Value = Math.Max(zoomSlider.Minimum,
                        Math.Min(zoomSlider.Maximum, scale * 100));
                }
            }
        }

        private void BtnActualSize_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value = 100;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RenderPreview();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _currentZoom = zoomSlider.Value / 100.0;
            ApplyZoom();
        }

        #endregion

        #region Keyboard Shortcuts

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.O:
                        LoadTemplate();
                        e.Handled = true;
                        break;
                    case Key.S:
                        SaveData();
                        e.Handled = true;
                        break;
                    case Key.OemPlus:
                    case Key.Add:
                        BtnZoomIn_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.OemMinus:
                    case Key.Subtract:
                        BtnZoomOut_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.D0:
                    case Key.NumPad0:
                        BtnFitToWindow_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.D1:
                    case Key.NumPad1:
                        BtnActualSize_Click(null, null);
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.F5)
            {
                BtnRefresh_Click(null, null);
                e.Handled = true;
            }
        }

        #endregion

        #region Status Helpers

        private void SetStatus(string message, string level = "info")
        {
            statusText.Text = message;

            Brush indicatorBrush = level switch
            {
                "error" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                "warning" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                "success" => new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                _ => new SolidColorBrush(Color.FromRgb(16, 185, 129))
            };
            statusIndicator.Fill = indicatorBrush;
        }

        #endregion

        #region Core Functions

        private void LoadTemplate()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON 模板文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "选择报告模板文件",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadTemplateFromFile(openFileDialog.FileName);
            }
        }

        private void LoadTemplateFromFile(string filePath)
        {
            try
            {
                SetStatus("正在加载模板...", "info");

                _lastLoadResult = _templateLoader.LoadFromFileWithStats(filePath);

                if (_lastLoadResult.Errors.Count > 0)
                {
                    SetStatus($"加载失败: {_lastLoadResult.Errors[0]}", "error");
                    return;
                }

                _currentTemplate = _lastLoadResult.Template;

                if (_currentTemplate == null || _currentTemplate.Elements == null)
                {
                    SetStatus("模板加载失败：解析为空", "error");
                    return;
                }

                // 应用医院配置默认值到适配器字段
                _hospitalConfigService.ApplyDefaults(_currentTemplate);

                DisplayTemplateInfo(_lastLoadResult);
                GenerateInputControls();
                RenderPreview();

                SetStatus($"已加载: {_currentTemplate.Name} | 共{_lastLoadResult.TotalElements}元素 "
                    + $"固定{_lastLoadResult.FixedCount} 编辑{_lastLoadResult.EditableCount} 适配{_lastLoadResult.DataAdapterCount}",
                    "success");
                templateInfoText.Text = $"📄 {Path.GetFileName(filePath)}  |  元素: {_currentTemplate.Elements.Count}";
            }
            catch (Exception ex)
            {
                SetStatus($"加载失败: {ex.Message}", "error");
            }
        }

        private void DisplayTemplateInfo(TemplateLoadResult result)
        {
            templateInfoPanel.Children.Clear();

            if (_currentTemplate == null)
                return;

            // 基本信息
            AddSectionHeader("基本信息");
            AddInfoItem("模板名称", _currentTemplate.Name);
            AddInfoItem("模板类型", _currentTemplate.Type);
            AddInfoItem("页面尺寸", $"{_currentTemplate.PageWidth} × {_currentTemplate.PageHeight} mm");

            // 元素汇总
            AddSectionHeader("元素检测汇总");
            AddInfoItem("元素总数", $"{result.TotalElements} 个");
            AddInfoItem("固定元素", $"{result.FixedCount} 个 🔒", result.FixedCount > 0 ? "#334155" : "#94A3B8");
            AddInfoItem("可编辑字段", $"{result.EditableCount} 个 ✏️", result.EditableCount > 0 ? "#2563EB" : "#94A3B8");
            AddInfoItem("数据适配器", $"{result.DataAdapterCount} 个 🔗", result.DataAdapterCount > 0 ? "#10B981" : "#94A3B8");

            // 按类型统计
            AddSectionHeader("按类型分布");
            foreach (var stat in result.ElementTypeStats)
            {
                var icon = stat.Group switch
                {
                    "Fixed" => "🔒",
                    "Editable" => "✏️",
                    "DataAdapter" => "🔗",
                    _ => "  "
                };
                var color = stat.Group switch
                {
                    "Fixed" => "#475569",
                    "Editable" => "#2563EB",
                    "DataAdapter" => "#10B981",
                    _ => "#000000"
                };
                AddInfoItem($"  {icon} {stat.TypeName}", $"{stat.Count} 个", color, 11);
            }
        }

        private void AddSectionHeader(string text)
        {
            var divider = new Border
            {
                Height = 1,
                Background = (Brush)TryFindResource("DividerBrush") ?? new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                Margin = new Thickness(0, 10, 0, 6)
            };

            var header = new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)TryFindResource("TextPrimaryBrush") ?? new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Margin = new Thickness(0, 0, 0, 6)
            };

            templateInfoPanel.Children.Add(divider);
            templateInfoPanel.Children.Add(header);
        }

        private void AddInfoItem(string label, string value, string colorHex = null, double fontSize = 12)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 4)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            if (!string.IsNullOrEmpty(value))
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            Brush valueColor;
            if (!string.IsNullOrEmpty(colorHex))
            {
                try { valueColor = (Brush)new BrushConverter().ConvertFrom(colorHex); }
                catch { valueColor = (Brush)TryFindResource("TextPrimaryBrush") ?? new SolidColorBrush(Color.FromRgb(30, 41, 59)); }
            }
            else
            {
                valueColor = (Brush)TryFindResource("TextPrimaryBrush") ?? new SolidColorBrush(Color.FromRgb(30, 41, 59));
            }

            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = fontSize,
                FontWeight = FontWeights.Medium,
                Foreground = (Brush)TryFindResource("TextSecondaryBrush") ?? new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(labelBlock, 0);

            grid.Children.Add(labelBlock);

            if (!string.IsNullOrEmpty(value))
            {
                var valueBlock = new TextBlock
                {
                    Text = value,
                    FontSize = fontSize,
                    Foreground = valueColor,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Right
                };
                Grid.SetColumn(valueBlock, 1);
                grid.Children.Add(valueBlock);
            }

            templateInfoPanel.Children.Add(grid);
        }

        private void GenerateInputControls()
        {
            inputControlPanel.Children.Clear();

            if (_currentTemplate == null)
                return;

            var fixedElements = _currentTemplate.Elements
                .Where(e => e.Group == ElementGroup.Fixed)
                .OrderBy(e => e.Y).ThenBy(e => e.X).ToList();

            var editableElements = _currentTemplate.Elements
                .Where(e => e.Group == ElementGroup.Editable && e is not ExternalTableElement)
                .OrderBy(e => e.Y).ThenBy(e => e.X).ToList();

            var tableElements = _currentTemplate.Elements
                .OfType<ExternalTableElement>()
                .Where(e => e.Group == ElementGroup.Editable)
                .OrderBy(e => e.Y).ThenBy(e => e.X).ToList();

            var adapterElements = _currentTemplate.Elements
                .Where(e => e.Group == ElementGroup.DataAdapter)
                .OrderBy(e => e.Y).ThenBy(e => e.X).ToList();

            if (fixedElements.Count > 0)
            {
                inputControlPanel.Children.Add(CreateExpanderPanel(
                    "📌 固定元素", fixedElements.Count, CreateFixedElementsContent(fixedElements), false));
            }

            if (editableElements.Count > 0 || tableElements.Count > 0)
            {
                var editableContent = CreateEditableElementsWithTablesContent(editableElements, tableElements);
                var totalCount = editableElements.Count + tableElements.Count;
                inputControlPanel.Children.Add(CreateExpanderPanel(
                    "✏️ 可编辑字段", totalCount, editableContent, true));
            }

            if (adapterElements.Count > 0)
            {
                inputControlPanel.Children.Add(CreateExpanderPanel(
                    "🔗 数据适配器", adapterElements.Count, CreateAdapterElementsContent(adapterElements), false));
            }

            if (fixedElements.Count == 0 && editableElements.Count == 0 && adapterElements.Count == 0)
            {
                inputControlPanel.Children.Add(new TextBlock
                {
                    Text = "此模板没有元素",
                    FontSize = 13,
                    Foreground = (Brush)TryFindResource("TextMutedBrush") ?? new SolidColorBrush(Colors.Gray),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                });
            }

            var totalGenSuccess = inputControlPanel.Children.OfType<Expander>()
                .SelectMany(e => ((e.Content as StackPanel)?.Children.OfType<Border>() ?? Enumerable.Empty<Border>()))
                .Count();
            inputStatusText.Text = $"固定: {fixedElements.Count}  |  可编辑: {editableElements.Count}  |  适配器: {adapterElements.Count}";
        }

        private Expander CreateExpanderPanel(string title, int count, UIElement content, bool isExpanded)
        {
            var expander = new Expander
            {
                IsExpanded = isExpanded,
                Margin = new Thickness(0, 0, 0, 6),
                Background = (Brush)TryFindResource("BgCardBrush") ?? Brushes.White,
                BorderBrush = (Brush)TryFindResource("BorderBrush") ?? new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)TryFindResource("TextPrimaryBrush") ?? new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(titleText, 0);

            var countBadge = new Border
            {
                Background = (Brush)TryFindResource("PrimaryBrush") ?? new SolidColorBrush(Color.FromRgb(37, 99, 235)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(8, 0, 0, 0)
            };

            var countText = new TextBlock
            {
                Text = count.ToString(),
                FontSize = 11,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            };
            countBadge.Child = countText;
            Grid.SetColumn(countBadge, 1);

            headerGrid.Children.Add(titleText);
            headerGrid.Children.Add(countBadge);

            expander.Header = headerGrid;
            expander.Content = content;

            return expander;
        }

        private UIElement CreateFixedElementsContent(List<ExternalElementBase> elements)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(8) };

            foreach (var element in elements)
            {
                var rowGrid = new Grid
                {
                    Margin = new Thickness(0, 4, 0, 4)
                };

                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var label = new TextBlock
                {
                    Text = GetElementDisplayText(element),
                    FontSize = 12,
                    Foreground = (Brush)TryFindResource("TextSecondaryBrush") ?? new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);

                var typeBlock = new TextBlock
                {
                    Text = GetElementTypeName(element),
                    FontSize = 10,
                    Foreground = (Brush)TryFindResource("TextMutedBrush") ?? new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 0, 0)
                };
                Grid.SetColumn(typeBlock, 1);

                rowGrid.Children.Add(label);
                rowGrid.Children.Add(typeBlock);

                stackPanel.Children.Add(rowGrid);
            }

            return stackPanel;
        }

        private (UIElement content, int success, int fail) CreateEditableElementsContent(List<ExternalElementBase> elements)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(8) };

            int successCount = 0;
            int failCount = 0;

            foreach (var element in elements)
            {
                try
                {
                    var control = _controlGenerator.GenerateControl(element, (value) =>
                    {
                        // 实时刷新预览
                        RenderPreview();
                    });

                    if (control != null)
                    {
                        stackPanel.Children.Add(control);
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        // 为不支持的类型显示占位信息
                        var placeholder = new Border
                        {
                            Margin = new Thickness(0, 0, 0, 8),
                            Padding = new Thickness(10),
                            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(6)
                        };
                        placeholder.Child = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = GetElementDisplayText(element),
                                    FontSize = 12,
                                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                                    TextWrapping = TextWrapping.Wrap
                                },
                                new TextBlock
                                {
                                    Text = $"[类型: {GetElementTypeName(element)} - 暂不支持输入控件]",
                                    FontSize = 10,
                                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                                    Margin = new Thickness(0, 2, 0, 0)
                                }
                            }
                        };
                        stackPanel.Children.Add(placeholder);
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    System.Diagnostics.Debug.WriteLine($"控件生成失败 [{element.Id}]: {ex.Message}");
                }
            }

            return (stackPanel, successCount, failCount);
        }

        private UIElement CreateEditableElementsWithTablesContent(List<ExternalElementBase> elements, List<ExternalTableElement> tables)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(8) };

            // 单个字段编辑器
            foreach (var element in elements)
            {
                try
                {
                    var control = _controlGenerator.GenerateControl(element, (value) =>
                    {
                        RenderPreview();
                    });

                    if (control != null)
                        stackPanel.Children.Add(control);
                    else
                        stackPanel.Children.Add(CreateUnsupportedPlaceholder(element));
                }
                catch { }
            }

            // 表格编辑器（可折叠，默认折叠）
            foreach (var table in tables)
            {
                var tableContent = BuildTableEditGrid(table);
                var expander = new Expander
                {
                    Header = $"📊 {table.Id ?? "表格"}: {table.Rows}行 × {table.Columns}列",
                    IsExpanded = false,
                    Content = tableContent,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                    Padding = new Thickness(8)
                };
                stackPanel.Children.Add(expander);
            }

            return stackPanel;
        }

        private UIElement BuildTableEditGrid(ExternalTableElement table)
        {
            var editGrid = new Grid { Margin = new Thickness(0) };
            for (int c = 0; c < table.Columns; c++)
                editGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 60 });
            for (int r = 0; r < table.Rows; r++)
                editGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int r = 0; r < table.CellData.Count && r < table.Rows; r++)
            {
                var row = table.CellData[r];
                for (int c = 0; c < row.Count && c < table.Columns; c++)
                {
                    var cellValue = row[c] ?? "";
                    var isHeader = table.HasHeader && r == 0;
                    var isEmptyDataCell = !isHeader && string.IsNullOrEmpty(cellValue);

                    if (isHeader || !isEmptyDataCell)
                    {
                        var cellBlock = new Border
                        {
                            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                            BorderThickness = new Thickness(0.5),
                            Background = isHeader
                                ? new SolidColorBrush(Color.FromRgb(241, 245, 249))
                                : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                            Padding = new Thickness(6, 4, 6, 4),
                            MinWidth = 60
                        };
                        cellBlock.Child = new TextBlock
                        {
                            Text = cellValue,
                            FontSize = isHeader ? 11 : 12,
                            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                            Foreground = isHeader
                                ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                                : new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };
                        Grid.SetRow(cellBlock, r);
                        Grid.SetColumn(cellBlock, c);
                        editGrid.Children.Add(cellBlock);
                    }
                    else
                    {
                        var cellBg = new Border
                        {
                            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                            BorderThickness = new Thickness(0.5),
                            Background = Brushes.White,
                            Padding = new Thickness(0),
                            MinWidth = 60
                        };
                        var cellInput = new TextBox
                        {
                            Text = "",
                            FontSize = 12,
                            BorderThickness = new Thickness(0),
                            Padding = new Thickness(6, 4, 6, 4),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            MinWidth = 60
                        };
                        var captureR = r;
                        var captureC = c;
                        cellInput.TextChanged += (s, e) =>
                        {
                            if (captureR < table.CellData.Count && captureC < table.CellData[captureR].Count)
                            {
                                table.CellData[captureR][captureC] = cellInput.Text;
                                RenderPreview();
                            }
                        };
                        cellBg.Child = cellInput;
                        Grid.SetRow(cellBg, r);
                        Grid.SetColumn(cellBg, c);
                        editGrid.Children.Add(cellBg);
                    }
                }
            }

            return new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 260,
                Content = editGrid
            };
        }

        private UIElement CreateUnsupportedPlaceholder(ExternalElementBase element)
        {
            return new Border
            {
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = GetElementDisplayText(element),
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                            TextWrapping = TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = $"[类型: {GetElementTypeName(element)} - 暂不支持输入控件]",
                            FontSize = 10,
                            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                            Margin = new Thickness(0, 2, 0, 0)
                        }
                    }
                }
            };
        }

        private UIElement CreateAdapterElementsContent(List<ExternalElementBase> elements)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(8) };

            foreach (var element in elements)
            {
                var rowGrid = new Grid
                {
                    Margin = new Thickness(0, 4, 0, 4)
                };

                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock
                {
                    Text = (element.DataPath ?? element.Id) + ":",
                    FontSize = 12,
                    Foreground = (Brush)TryFindResource("TextSecondaryBrush") ?? new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);

                var valueBlock = new TextBlock
                {
                    Text = GetElementDisplayText(element),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(valueBlock, 1);

                rowGrid.Children.Add(label);
                rowGrid.Children.Add(valueBlock);

                stackPanel.Children.Add(rowGrid);
            }

            return stackPanel;
        }

        private string GetElementDisplayText(ExternalElementBase element)
        {
            if (element is ExternalTextElement textEl && !string.IsNullOrEmpty(textEl.Text))
                return textEl.Text;
            if (!string.IsNullOrEmpty(element.DataPath))
                return element.DataPath;
            if (!string.IsNullOrEmpty(element.Label))
                return element.Label;
            if (!string.IsNullOrEmpty(element.Id))
                return element.Id;
            return "[未命名]";
        }

        private string GetElementTypeName(ExternalElementBase element)
        {
            var name = element.GetType().Name;
            if (name.StartsWith("External"))
                name = name.Substring(8);
            if (name.EndsWith("Element"))
                name = name.Substring(0, name.Length - 7);
            return name;
        }

        private void RenderPreview()
        {
            if (_currentTemplate == null)
                return;

            try
            {
                _canvasRenderer.RenderToCanvas(canvasPreview, _currentTemplate);

                UpdatePreviewInfo();
                UpdateLastUpdateTime();

                templateInfoText.Text = $"📄 {_currentTemplate.Name}  |  元素: {_currentTemplate.Elements.Count}";
            }
            catch (Exception ex)
            {
                SetStatus($"渲染失败: {ex.Message}", "error");
            }
        }

        private void UpdatePreviewInfo()
        {
            if (_currentTemplate != null)
            {
                var width = canvasPreview.Width;
                var height = canvasPreview.Height;
                previewInfoText.Text = $"尺寸: {width:F0} × {height:F0} px  |  元素: {_currentTemplate.Elements.Count}";
            }
        }

        private void UpdateLastUpdateTime()
        {
            lastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void ApplyZoom()
        {
            if (canvasPreview == null) return;

            var scaleTransform = new ScaleTransform(_currentZoom, _currentZoom);
            canvasPreview.LayoutTransform = scaleTransform;

            zoomPercentText.Text = $"{(int)(zoomSlider.Value)}%";
        }

        private void SaveData()
        {
            if (_currentTemplate == null)
            {
                SetStatus("请先加载模板", "warning");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json",
                Title = "保存录入数据",
                FileName = $"{_currentTemplate.Name}_data.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var editableElements = _currentTemplate.Elements
                    .Where(e => e.Group == ElementGroup.Editable)
                    .ToList();

                var dataDict = new Dictionary<string, object>();

                foreach (var element in editableElements)
                {
                    var key = !string.IsNullOrEmpty(element.DataPath)
                        ? element.DataPath
                        : element.Id;

                    object value = element switch
                    {
                        ExternalTextElement txt => txt.Text,
                        ExternalNumberElement num => num.Value > 0 ? num.Value : null,
                        ExternalDateElement date => date.Value,
                        ExternalDropdownElement drop => drop.Value,
                        ExternalCheckboxElement cb => cb.Checked,
                        ExternalRadioElement radio => radio.Checked ? radio.Value : null,
                        _ => element.DefaultValue
                    };

                    if (value != null)
                    {
                        if (dataDict.ContainsKey(key))
                        {
                            int idx = 1;
                            while (dataDict.ContainsKey($"{key}_{idx}"))
                                idx++;
                            dataDict[$"{key}_{idx}"] = value;
                        }
                        else
                        {
                            dataDict[key] = value;
                        }
                    }
                }

                var exportData = new
                {
                    TemplateName = _currentTemplate.Name,
                    TemplateType = _currentTemplate.Type,
                    ExportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ElementCount = editableElements.Count,
                    Data = dataDict
                };

                var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(dialog.FileName, json);

                SetStatus($"数据已保存: {Path.GetFileName(dialog.FileName)} ({dataDict.Count} 个字段)", "success");
            }
            catch (Exception ex)
            {
                SetStatus($"保存失败: {ex.Message}", "error");
            }
        }

        #endregion

        #region Batch Import

        private void BtnGenerateSampleXlsx_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTemplate == null)
            {
                SetStatus("请先加载模板", "warning");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                Title = "保存导入模板",
                FileName = $"{_currentTemplate.Name}_导入模板.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                _batchImportService.GenerateSampleXlsx(dialog.FileName, _currentTemplate);
                SetStatus($"导入模板已生成: {Path.GetFileName(dialog.FileName)}", "success");
            }
            catch (Exception ex)
            {
                SetStatus($"生成失败: {ex.Message}", "error");
            }
        }

        private void BtnSelectBatchFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel 文件 (*.xlsx;*.csv)|*.xlsx;*.csv|所有文件 (*.*)|*.*",
                Title = "选择批量数据文件"
            };

            if (dialog.ShowDialog() == true)
            {
                batchFilePathBox.Text = dialog.FileName;
                batchValidateResult.Text = "";
                batchRowInfoText.Text = "0 条";
                _batchImportResult = null;
            }
        }

        private void BtnImportAndValidate_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTemplate == null)
            {
                SetStatus("请先加载模板", "warning");
                return;
            }

            var filePath = batchFilePathBox.Text;
            if (string.IsNullOrEmpty(filePath) || filePath == "未选择文件...")
            {
                SetStatus("请先选择数据文件", "warning");
                return;
            }

            if (!File.Exists(filePath))
            {
                SetStatus("文件不存在", "error");
                return;
            }

            try
            {
                _batchImportResult = _batchImportService.ImportFromXlsx(filePath, _currentTemplate);

                if (_batchImportResult.Success)
                {
                    batchValidateResult.Text = $"校验通过! 匹配 {_batchImportResult.MatchedRows} 行数据";
                    batchValidateResult.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                    batchRowInfoText.Text = $"{_batchImportResult.MatchedRows} 条";
                    batchRowIndexBox.Text = "1";
                    SetStatus($"批量校验完成: {_batchImportResult.MatchedRows}/{_batchImportResult.TotalRows} 行匹配", "success");
                }
                else
                {
                    batchValidateResult.Text = $"校验失败: {string.Join("; ", _batchImportResult.Errors)}";
                    batchValidateResult.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                    batchRowInfoText.Text = "0 条";
                    SetStatus("批量校验失败", "error");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"导入失败: {ex.Message}", "error");
                batchValidateResult.Text = $"错误: {ex.Message}";
                batchValidateResult.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }
        }

        private void BtnApplyBatchRow_Click(object sender, RoutedEventArgs e)
        {
            if (_batchImportResult == null || _batchImportResult.Data.Count == 0)
            {
                SetStatus("请先导入并校验数据", "warning");
                return;
            }

            if (!int.TryParse(batchRowIndexBox.Text, out var idx) || idx < 1 || idx > _batchImportResult.Data.Count)
            {
                SetStatus($"行号无效，有效范围: 1-{_batchImportResult.Data.Count}", "error");
                return;
            }

            var applied = _batchImportService.ApplyDataToTemplate(
                _batchImportResult.Data, _currentTemplate, idx - 1);

            RenderPreview();
            SetStatus($"已应用第 {idx} 行数据 ({applied} 个字段)", "success");
        }

        private void BtnBatchGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            if (_batchImportResult == null || _batchImportResult.Data.Count == 0)
            {
                SetStatus("请先导入并校验数据", "warning");
                return;
            }

            // 输出到桌面\ReportOutput 目录
            var outputDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "ReportOutput");
            Directory.CreateDirectory(outputDir);

            int successCount = 0;
            int failCount = 0;

            try
            {
                for (int i = 0; i < _batchImportResult.Data.Count; i++)
                {
                    try
                    {
                        _batchImportService.ApplyDataToTemplate(
                            _batchImportResult.Data, _currentTemplate, i);

                        var outputPath = Path.Combine(outputDir,
                            $"{_currentTemplate.Name}_{i + 1}.png");

                        RenderPreview();
                        SaveCanvasAsPng(outputPath);

                        successCount++;
                    }
                    catch
                    {
                        failCount++;
                    }
                }

                SetStatus($"批量完成: {successCount} 成功 / {failCount} 失败", successCount > 0 ? "success" : "error");
                batchGenerateResult.Text = $"已生成 {successCount} 个文件到:\n{outputDir}";
                batchGenerateResult.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                SetStatus($"批量生成失败: {ex.Message}", "error");
            }
        }

        /// <summary>
        /// 将 Canvas 保存为 PNG 图片
        /// </summary>
        private void SaveCanvasAsPng(string outputPath)
        {
            canvasPreview.UpdateLayout();
            var renderTarget = new RenderTargetBitmap(
                (int)canvasPreview.ActualWidth,
                (int)canvasPreview.ActualHeight,
                96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(canvasPreview);

            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTarget));

            using var stream = File.Create(outputPath);
            encoder.Save(stream);
        }

        #endregion
    }
}
