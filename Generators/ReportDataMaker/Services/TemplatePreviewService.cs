using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services
{
    public class TemplatePreviewService
    {
        // 毫米转像素的转换系数 (96 DPI)
        private const double MM_TO_PX = 3.7795275591;

        public UIElement RenderElement(ElementBase element)
        {
            switch (element.Type)
            {
                case "Text":
                    return RenderTextElement((TextElement)element);
                case "LabelInputBox":
                    return RenderLabelInputBoxElement((LabelInputBoxElement)element);
                case "Table":
                    return RenderTableElement((TableElement)element);
                default:
                    return null;
            }
        }

        public UIElement RenderExternalElement(ReportExternalElementBase element)
        {
            if (!element.IsVisible)
                return null;

            UIElement content = null;

            switch (element)
            {
                case ExternalLineElement line:
                    content = RenderExternalLine(line);
                    break;
                case ExternalDropdownElement dropdown:
                    content = RenderExternalDropdown(dropdown);
                    break;
                case ExternalTableElement table:
                    content = RenderExternalTable(table);
                    break;
                case ExternalTextElement text:
                    content = RenderExternalText(text);
                    break;
                case ExternalNumberElement number:
                    content = RenderExternalNumber(number);
                    break;
                case ExternalDateElement date:
                    content = RenderExternalDate(date);
                    break;
                default:
                    content = RenderExternalDefault(element);
                    break;
            }

            if (content == null)
                return null;

            // 对于可编辑元素，添加边框容器以标识
            if (element.Group == ElementGroup.Editable)
            {
                var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;
                var heightPx = element.Height > 0 ? element.Height * MM_TO_PX : double.NaN;

                var border = new Border
                {
                    Width = widthPx,
                    Height = heightPx,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(100, 149, 237)),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromArgb(15, 100, 149, 237)),
                    CornerRadius = new CornerRadius(2),
                    Padding = new Thickness(4, 2, 4, 2),
                    Child = content
                };
                return border;
            }

            // 对于固定元素和其他元素，直接返回内容，不添加边框
            return content;
        }

        private UIElement RenderExternalLine(ExternalLineElement element)
        {
            if (element.Height <= 0 || element.EndX <= element.StartX)
            {
                var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : (element.EndX - element.StartX) * MM_TO_PX;
                var line = new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = widthPx,
                    Y2 = 0,
                    Stroke = new SolidColorBrush(ParseColor(element.LineColor)),
                    StrokeThickness = element.LineWidth
                };

                if (element.LineStyle == "dashed")
                    line.StrokeDashArray = new DoubleCollection { 4, 2 };
                else if (element.LineStyle == "dotted")
                    line.StrokeDashArray = new DoubleCollection { 1, 2 };

                return line;
            }

            return null;
        }

        private UIElement RenderExternalText(ExternalTextElement element)
        {
            var displayText = element.Text ?? element.DefaultValue ?? "";

            // 如果文本为空且有 dataPath，显示占位符
            if (string.IsNullOrWhiteSpace(displayText) && !string.IsNullOrEmpty(element.DataPath))
            {
                displayText = $"[{element.DataPath}]";
            }

            var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;
            var heightPx = element.Height > 0 ? element.Height * MM_TO_PX : double.NaN;

            var textBlock = new TextBlock
            {
                Text = displayText,
                Width = widthPx,
                // 不设置 Height，让文本自然显示
                FontFamily = new FontFamily(element.FontFamily ?? "SimSun"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = element.FontStyle == "italic" ? FontStyles.Italic : FontStyles.Normal,
                Foreground = new SolidColorBrush(ParseColor(element.ForegroundColor)),
                TextAlignment = GetTextAlignment(element.TextAlignment),
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.NoWrap,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            // 如果完全为空，显示灰色占位符
            if (string.IsNullOrWhiteSpace(element.Text) && string.IsNullOrWhiteSpace(element.DefaultValue) && string.IsNullOrEmpty(element.DataPath))
            {
                textBlock.Text = "[空]";
                textBlock.Foreground = new SolidColorBrush(Colors.LightGray);
                textBlock.FontStyle = FontStyles.Italic;
            }

            return textBlock;
        }

        private UIElement RenderExternalNumber(ExternalNumberElement element)
        {
            var text = element.Value > 0
                ? $"{element.Value}{(string.IsNullOrEmpty(element.Unit) ? "" : " " + element.Unit)}"
                : (element.DefaultValue ?? $"[{element.DataPath}]");

            var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;

            var textBlock = new TextBlock
            {
                Text = text,
                Width = widthPx,
                FontFamily = new FontFamily(element.FontFamily ?? "SimSun"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new SolidColorBrush(ParseColor(element.ForegroundColor)),
                TextAlignment = GetTextAlignment(element.TextAlignment),
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            return textBlock;
        }

        private UIElement RenderExternalDate(ExternalDateElement element)
        {
            var text = element.Value ?? element.DefaultValue ?? $"[{element.DataPath}]";

            var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;

            var textBlock = new TextBlock
            {
                Text = text,
                Width = widthPx,
                FontFamily = new FontFamily(element.FontFamily ?? "SimSun"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new SolidColorBrush(ParseColor(element.ForegroundColor)),
                TextAlignment = GetTextAlignment(element.TextAlignment),
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            return textBlock;
        }

        private UIElement RenderExternalDropdown(ExternalDropdownElement element)
        {
            var text = !string.IsNullOrEmpty(element.Value)
                ? element.Value
                : (!string.IsNullOrEmpty(element.Placeholder) ? element.Placeholder : $"[{element.DataPath}]");

            var foreground = !string.IsNullOrEmpty(element.Value)
                ? ParseColor(element.ForegroundColor)
                : Colors.Gray;

            var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;

            var textBlock = new TextBlock
            {
                Text = text,
                Width = widthPx,
                FontFamily = new FontFamily(element.FontFamily ?? "SimSun"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = element.FontStyle == "italic" ? FontStyles.Italic : FontStyles.Normal,
                Foreground = new SolidColorBrush(foreground),
                TextAlignment = GetTextAlignment(element.TextAlignment),
                VerticalAlignment = VerticalAlignment.Top,
                TextTrimming = TextTrimming.CharacterEllipsis,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            return textBlock;
        }

        private UIElement RenderExternalDefault(ReportExternalElementBase element)
        {
            var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;

            var textBlock = new TextBlock
            {
                Text = element.DefaultValue ?? "[未知元素]",
                Width = widthPx,
                FontFamily = new FontFamily(element.FontFamily ?? "SimSun"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                Foreground = new SolidColorBrush(ParseColor(element.ForegroundColor)),
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            return textBlock;
        }

        private UIElement RenderExternalTable(ExternalTableElement element)
        {
            if (element.CellData == null || element.CellData.Count == 0)
                return null;

            var widthPx = element.Width > 0 ? element.Width * MM_TO_PX : double.NaN;
            var heightPx = element.Height > 0 ? element.Height * MM_TO_PX : double.NaN;

            var grid = new Grid
            {
                Width = widthPx,
                Height = heightPx
            };

            for (int c = 0; c < element.Columns; c++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int r = 0; r < element.Rows; r++)
                grid.RowDefinitions.Add(new RowDefinition());

            for (int r = 0; r < element.CellData.Count && r < element.Rows; r++)
            {
                var row = element.CellData[r];
                for (int c = 0; c < row.Count && c < element.Columns; c++)
                {
                    var cellText = row[c] ?? "";

                    var textBlock = new TextBlock
                    {
                        Text = cellText,
                        FontFamily = new FontFamily(element.FontFamily ?? "SimSun"),
                        FontSize = element.FontSize > 0 ? element.FontSize : 10,
                        FontWeight = element.HasHeader && r == 0 ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = new SolidColorBrush(ParseColor(element.ForegroundColor)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Padding = new Thickness(element.CellPadding),
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };

                    if (element.HasHeader && r == 0)
                    {
                        textBlock.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                    }

                    var border = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        BorderThickness = new Thickness(element.TableBorder),
                        Child = textBlock
                    };

                    Grid.SetRow(border, r);
                    Grid.SetColumn(border, c);
                    grid.Children.Add(border);
                }
            }

            return grid;
        }

        private UIElement RenderTextElement(TextElement element)
        {
            var textBlock = new TextBlock
            {
                Text = element.Text,
                FontFamily = new FontFamily(element.FontFamily),
                FontSize = element.FontSize,
                FontWeight = element.FontWeight == "Bold" ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = element.FontStyle == "Italic" ? FontStyles.Italic : FontStyles.Normal,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.ForegroundColor)),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.BackgroundColor)),
                HorizontalAlignment = GetHorizontalAlignment(element.HorizontalAlignment),
                VerticalAlignment = GetVerticalAlignment(element.VerticalAlignment),
                TextAlignment = GetTextAlignment(element.TextAlignment)
            };

            return CreateContainer(element, textBlock);
        }

        private UIElement RenderLabelInputBoxElement(LabelInputBoxElement element)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = GetHorizontalAlignment(element.HorizontalAlignment),
                VerticalAlignment = GetVerticalAlignment(element.VerticalAlignment)
            };

            var label = new TextBlock
            {
                Text = element.LabelText,
                FontFamily = new FontFamily(element.LabelFontFamily),
                FontSize = element.LabelFontSize,
                FontWeight = element.LabelFontWeight == "Bold" ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = element.LabelFontStyle == "Italic" ? FontStyles.Italic : FontStyles.Normal,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.LabelForegroundColor)),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.LabelBackgroundColor)),
                Width = element.LabelWidth,
                Height = element.LabelHeight,
                HorizontalAlignment = GetHorizontalAlignment(element.LabelTextAlignment),
                VerticalAlignment = GetVerticalAlignment(element.LabelVerticalAlignment)
            };

            var textBox = new TextBox
            {
                FontFamily = new FontFamily(element.InputFontFamily),
                FontSize = element.InputFontSize,
                FontWeight = element.InputFontWeight == "Bold" ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = element.InputFontStyle == "Italic" ? FontStyles.Italic : FontStyles.Normal,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.InputForegroundColor)),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.InputBackgroundColor)),
                Width = element.InputWidth,
                Height = element.InputHeight,
                HorizontalAlignment = GetHorizontalAlignment(element.InputTextAlignment),
                VerticalAlignment = GetVerticalAlignment(element.InputVerticalAlignment),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.InputBorderColor)),
                BorderThickness = new Thickness(element.InputBorderWidth),
                Text = element.InputPlaceholder
            };

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textBox);

            return CreateContainer(element, stackPanel);
        }

        private UIElement RenderTableElement(TableElement element)
        {
            var grid = new Grid
            {
                ShowGridLines = true,
                Width = element.Width,
                Height = element.Height
            };

            for (int i = 0; i < element.Columns; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < element.Rows; i++)
                grid.RowDefinitions.Add(new RowDefinition());

            foreach (var cell in element.Cells)
            {
                var textBlock = new TextBlock
                {
                    Text = cell.Content,
                    FontFamily = new FontFamily(cell.FontFamily),
                    FontSize = cell.FontSize,
                    FontWeight = cell.FontWeight == "Bold" ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cell.ForegroundColor)),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cell.BackgroundColor)),
                    HorizontalAlignment = GetHorizontalAlignment(cell.TextAlignment),
                    VerticalAlignment = GetVerticalAlignment(cell.VerticalAlignment),
                    TextAlignment = GetTextAlignment(cell.TextAlignment),
                    Padding = new Thickness(element.CellPadding)
                };

                Grid.SetRow(textBlock, cell.RowIndex);
                Grid.SetColumn(textBlock, cell.ColumnIndex);
                Grid.SetRowSpan(textBlock, cell.RowSpan);
                Grid.SetColumnSpan(textBlock, cell.ColumnSpan);

                grid.Children.Add(textBlock);
            }

            return CreateContainer(element, grid);
        }

        private UIElement CreateContainer(ElementBase element, UIElement content)
        {
            var border = new Border
            {
                Width = element.Width,
                Height = element.Height,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.BorderColor)),
                BorderThickness = new Thickness(element.BorderWidth),
                CornerRadius = new CornerRadius(element.CornerRadius),
                Opacity = element.Opacity,
                Child = content
            };

            var canvas = new Canvas
            {
                Width = element.Width,
                Height = element.Height
            };

            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            canvas.Children.Add(border);

            return canvas;
        }

        private static Color ParseColor(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr) || colorStr == "transparent")
                return Colors.Transparent;

            try
            {
                // 尝试直接转换
                var color = (Color)ColorConverter.ConvertFromString(colorStr);
                return color;
            }
            catch
            {
                // 如果转换失败，返回黑色作为默认值
                return Colors.Black;
            }
        }

        private HorizontalAlignment GetHorizontalAlignment(string alignment)
        {
            switch (alignment)
            {
                case "center":
                case "Center":
                    return HorizontalAlignment.Center;
                case "right":
                case "Right":
                    return HorizontalAlignment.Right;
                case "stretch":
                case "Stretch":
                    return HorizontalAlignment.Stretch;
                default:
                    return HorizontalAlignment.Left;
            }
        }

        private VerticalAlignment GetVerticalAlignment(string alignment)
        {
            switch (alignment)
            {
                case "center":
                case "Center":
                    return VerticalAlignment.Center;
                case "bottom":
                case "Bottom":
                    return VerticalAlignment.Bottom;
                case "stretch":
                case "Stretch":
                    return VerticalAlignment.Stretch;
                default:
                    return VerticalAlignment.Top;
            }
        }

        private TextAlignment GetTextAlignment(string alignment)
        {
            switch (alignment)
            {
                case "center":
                case "Center":
                    return TextAlignment.Center;
                case "right":
                case "Right":
                    return TextAlignment.Right;
                default:
                    return TextAlignment.Left;
            }
        }
    }
}
