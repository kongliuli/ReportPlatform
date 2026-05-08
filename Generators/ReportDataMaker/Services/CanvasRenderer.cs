using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    public class CanvasRenderer
    {
        private const double MM_TO_PX = 3.7795275591;

        public RenderStats RenderToCanvas(Canvas canvas, ExternalTemplateDefinition template, object data = null)
        {
            var stats = new RenderStats();
            if (canvas == null || template == null) return stats;

            canvas.Children.Clear();
            canvas.Width = template.PageWidth * MM_TO_PX;
            canvas.Height = template.PageHeight * MM_TO_PX;
            canvas.Background = ParseBrush(template.BackgroundColor);
            RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);

            foreach (var element in template.Elements)
            {
                if (!element.IsVisible) { stats.Skipped++; continue; }

                try
                {
                    var uiElement = RenderElement(element, data) ?? RenderPlaceholder(element);
                    var w = Math.Max(element.Width * MM_TO_PX, 1);
                    var h = Math.Max(element.Height * MM_TO_PX, 1);
                    if (uiElement is FrameworkElement fe)
                    {
                        if (double.IsNaN(fe.Width) || fe.Width <= 0) fe.Width = w;
                        if (double.IsNaN(fe.Height) || fe.Height <= 0) fe.Height = h;
                    }
                    Canvas.SetLeft(uiElement, element.X * MM_TO_PX);
                    Canvas.SetTop(uiElement, element.Y * MM_TO_PX);
                    Canvas.SetZIndex(uiElement, element.ZIndex);
                    canvas.Children.Add(uiElement);
                    stats.Rendered++;
                }
                catch (Exception ex)
                {
                    stats.Skipped++;
                    stats.Errors.Add($"{element.Id}: {ex.Message}");
                }
            }
            return stats;
        }

        private static UIElement RenderElement(ExternalElementBase element, object data)
        {
            return element switch
            {
                ExternalTextElement t => RenderTextElement(t, data),
                ExternalLineElement l => RenderLineElement(l),
                ExternalImageElement i => RenderImageElement(i, data),
                ExternalShapeElement s => RenderShapeElement(s),
                ExternalDividerElement d => RenderDividerElement(d),
                ExternalNumberElement n => RenderNumberElement(n, data),
                ExternalDateElement dt => RenderDateElement(dt, data),
                ExternalDropdownElement dd => RenderDropdownElement(dd, data),
                ExternalTableElement tb => RenderTableElement(tb, data),
                ExternalCheckboxElement cb => RenderCheckboxElement(cb),
                ExternalRadioElement r => RenderRadioElement(r),
                ExternalSignatureElement sg => RenderSignatureElement(sg),
                ExternalBarcodeElement bc => RenderBarcodeElement(bc),
                ExternalQrCodeElement qr => RenderQrCodeElement(qr),
                ExternalWatermarkElement wm => RenderWatermarkElement(wm),
                ExternalPageNumberElement pn => RenderPageNumberElement(pn),
                ExternalContainerElement ct => RenderContainerElement(ct, data),
                ExternalRepeatElement rp => RenderRepeatElement(rp, data),
                ExternalHeaderElement hd => RenderHeaderElement(hd, data),
                ExternalFooterElement ft => RenderFooterElement(ft, data),
                ExternalChartElement ch => RenderChartElement(ch),
                ExternalIconElement ic => RenderIconElement(ic),
                ExternalHyperlinkElement hl => RenderHyperlinkElement(hl),
                _ => null
            };
        }

        #region Renderers

        private static UIElement RenderTextElement(ExternalTextElement element, object data)
        {
            var text = GetElementText(element);
            var hasValue = !string.IsNullOrWhiteSpace(text);

            if (!hasValue && !string.IsNullOrEmpty(element.DataPath) && data is ReportDataContext ctx)
            {
                var r = new DataPathResolver().Resolve(element.DataPath, ctx);
                if (r != null) { text = r.ToString(); hasValue = true; }
            }

            // 可编辑元素：Label: value 格式（无值时显示占位横线）
            if (element.Group == ElementGroup.Editable && !string.IsNullOrEmpty(element.Label))
            {
                text = hasValue ? element.Label + ": " + text : element.Label + ": —";
            }
            else if (!hasValue)
            {
                text = !string.IsNullOrEmpty(element.DataPath) ? $"[{element.DataPath}]" : "[空]";
            }

            var tb = new TextBlock
            {
                Text = text,
                Width = element.Width * MM_TO_PX,
                FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = element.FontStyle == "italic" ? FontStyles.Italic : FontStyles.Normal,
                Foreground = ParseForegroundBrush(element.ForegroundColor),
                Background = ParseBrush(element.BackgroundColor),
                TextAlignment = ParseTextAlignment(element.TextAlignment),
                TextWrapping = TextWrapping.NoWrap,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };
            TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(tb, TextRenderingMode.ClearType);
            return tb;
        }

        private static string GetElementText(ExternalTextElement element)
        {
            return element.Text ?? element.DefaultValue ?? "";
        }

        private static UIElement RenderLineElement(ExternalLineElement element)
        {
            double dx = (element.EndX - element.StartX) * MM_TO_PX;
            double dy = (element.EndY - element.StartY) * MM_TO_PX;
            if (dx <= 0 && dy <= 0) dx = element.Width * MM_TO_PX;

            var line = new Line
            {
                X1 = 0, Y1 = 0, X2 = Math.Max(dx, 1), Y2 = Math.Max(dy, 0),
                Stroke = ParseBrush(element.LineColor),
                StrokeThickness = element.LineWidth > 0 ? element.LineWidth : 1,
                SnapsToDevicePixels = true, UseLayoutRounding = true
            };
            RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);

            if (element.LineStyle == "dashed") line.StrokeDashArray = new DoubleCollection { 4, 2 };
            else if (element.LineStyle == "dotted") line.StrokeDashArray = new DoubleCollection { 1, 2 };
            return line;
        }

        private static UIElement RenderNumberElement(ExternalNumberElement element, object data)
        {
            string text;
            if (element.Value > 0)
            {
                var valStr = element.DecimalPlaces > 0
                    ? element.Value.ToString($"F{element.DecimalPlaces}")
                    : element.Value.ToString();
                text = valStr + (string.IsNullOrEmpty(element.Unit) ? "" : " " + element.Unit);
            }
            else
            {
                text = element.DefaultValue ?? "";
            }
            var hasValue = !string.IsNullOrWhiteSpace(text);
            return MakeTextBlock(text, hasValue, element.Width * MM_TO_PX, element, element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal);
        }

        private static UIElement RenderDateElement(ExternalDateElement element, object data)
        {
            var text = element.Value ?? element.DefaultValue ?? "";
            var hasValue = !string.IsNullOrWhiteSpace(text);
            return MakeTextBlock(text, hasValue, element.Width * MM_TO_PX, element, element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal);
        }

        private static UIElement RenderDropdownElement(ExternalDropdownElement element, object data)
        {
            var text = !string.IsNullOrEmpty(element.Value) ? element.Value : element.Placeholder ?? "";
            var hasValue = !string.IsNullOrEmpty(element.Value);
            return MakeTextBlock(text, hasValue, element.Width * MM_TO_PX, element, FontWeights.Normal);
        }

        private static UIElement RenderTableElement(ExternalTableElement element, object data)
        {
            var grid = new Grid
            {
                Width = Math.Max(element.Width * MM_TO_PX, 10),
                Height = Math.Max(element.Height * MM_TO_PX, 10),
                UseLayoutRounding = true, SnapsToDevicePixels = true
            };

            if (element.CellData == null || element.CellData.Count == 0)
            {
                grid.Background = new SolidColorBrush(Color.FromArgb(20, 100, 116, 139));
                grid.Children.Add(new TextBlock
                {
                    Text = "[空表格]",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    FontSize = 10
                });
                return grid;
            }

            for (int c = 0; c < element.Columns; c++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int r = 0; r < element.Rows; r++) grid.RowDefinitions.Add(new RowDefinition());

            var ff = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI");
            var cp = new Thickness(element.CellPadding);
            var bt = new Thickness(element.TableBorder);
            var bb = new SolidColorBrush(Colors.Black);
            var hbg = new SolidColorBrush(Color.FromRgb(240, 245, 250));
            var abg = new SolidColorBrush(Color.FromRgb(248, 250, 252));

            for (int r = 0; r < element.CellData.Count && r < element.Rows; r++)
            {
                var row = element.CellData[r];
                for (int c = 0; c < row.Count && c < element.Columns; c++)
                {
                    var tb = new TextBlock
                    {
                        Text = row[c] ?? "",
                        FontFamily = ff,
                        FontSize = element.FontSize > 0 ? element.FontSize : 10,
                        FontWeight = element.HasHeader && r == 0 ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = ParseForegroundBrush(element.ForegroundColor),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Padding = cp, TextTrimming = TextTrimming.CharacterEllipsis,
                        UseLayoutRounding = true, SnapsToDevicePixels = true
                    };
                    TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
                    TextOptions.SetTextRenderingMode(tb, TextRenderingMode.ClearType);
                    if (element.HasHeader && r == 0) tb.Background = hbg;
                    else if (r % 2 == 0) tb.Background = abg;

                    var b = new Border { BorderBrush = bb, BorderThickness = bt, Child = tb };
                    Grid.SetRow(b, r); Grid.SetColumn(b, c);
                    grid.Children.Add(b);
                }
            }
            return grid;
        }

        private static UIElement RenderImageElement(ExternalImageElement element, object data)
        {
            var c = new Border
            {
                Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX,
                Background = ParseBrush(element.BackgroundColor ?? "#F5F5F5"),
                BorderBrush = ParseBrush(element.BorderColor ?? "#D0D5DD"),
                BorderThickness = new Thickness(element.BorderWidth > 0 ? element.BorderWidth : 1),
                CornerRadius = new CornerRadius(element.CornerRadius)
            };

            if (!string.IsNullOrEmpty(element.Src))
            {
                try
                {
                    BitmapImage bmp = null;
                    if (element.Src.StartsWith("data:image/"))
                    {
                        var idx = element.Src.IndexOf("base64,");
                        if (idx >= 0)
                        {
                            var bytes = Convert.FromBase64String(element.Src.Substring(idx + 7));
                            bmp = new BitmapImage();
                            using (var ms = new System.IO.MemoryStream(bytes))
                            { bmp.BeginInit(); bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.StreamSource = ms; bmp.EndInit(); }
                        }
                    }
                    else if (Uri.TryCreate(element.Src, UriKind.Absolute, out var uri))
                    {
                        bmp = new BitmapImage();
                        bmp.BeginInit(); bmp.UriSource = uri; bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.EndInit();
                    }
                    if (bmp != null)
                    {
                        var img = new Image
                        {
                            Source = bmp,
                            Stretch = element.Fit?.ToLower() == "fill" ? Stretch.Fill :
                                      element.Fit?.ToLower() == "uniform" ? Stretch.Uniform : Stretch.UniformToFill
                        };
                        RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                        c.Child = img; return c;
                    }
                }
                catch { }
            }

            c.Child = new TextBlock
            {
                Text = element.AltText ?? "[图片占位]",
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                FontSize = element.FontSize > 0 ? element.FontSize : 10
            };
            return c;
        }

        private static UIElement RenderShapeElement(ExternalShapeElement element)
        {
            var w = (element.Width > 0 ? element.Width : 100) * MM_TO_PX;
            var h = (element.Height > 0 ? element.Height : 100) * MM_TO_PX;
            Shape s = element.ShapeType?.ToLower() switch
            {
                "ellipse" => new Ellipse { Width = w, Height = h },
                "triangle" => new Polygon { Points = new PointCollection { new(w / 2, 0), new(w, h), new(0, h) } },
                "diamond" => new Polygon { Points = new PointCollection { new(w / 2, 0), new(w, h / 2), new(w / 2, h), new(0, h / 2) } },
                _ => new Rectangle { Width = w, Height = h }
            };
            s.Fill = ParseBrush(element.FillColor ?? "#E2E8F0");
            s.Stroke = ParseBrush(element.StrokeColor ?? "#000000");
            s.StrokeThickness = element.StrokeWidth > 0 ? element.StrokeWidth : 1;
            s.SnapsToDevicePixels = true; s.UseLayoutRounding = true;
            RenderOptions.SetEdgeMode(s, EdgeMode.Aliased);
            return s;
        }

        private static UIElement RenderDividerElement(ExternalDividerElement element)
        {
            var t = element.Thickness > 0 ? element.Thickness : 1;
            var horiz = element.Width >= element.Height;
            var dashed = element.Style == "dashed" || element.Style == "dotted";

            if (dashed)
            {
                var da = element.Style == "dashed" ? new DoubleCollection { 4, 2 } : new DoubleCollection { 1, 2 };
                var l = new Line
                {
                    X1 = 0, Y1 = 0,
                    X2 = horiz ? Math.Max(element.Width * MM_TO_PX, 1) : 0,
                    Y2 = horiz ? 0 : Math.Max(element.Height * MM_TO_PX, 1),
                    Stroke = ParseBrush(element.Color ?? "#000000"),
                    StrokeThickness = t, StrokeDashArray = da,
                    SnapsToDevicePixels = true, UseLayoutRounding = true
                };
                return l;
            }
            return new Border
            {
                Width = horiz ? Math.Max(element.Width * MM_TO_PX, 1) : t,
                Height = horiz ? t : Math.Max(element.Height * MM_TO_PX, 1),
                Background = ParseBrush(element.Color ?? "#000000")
            };
        }

        private static UIElement RenderCheckboxElement(ExternalCheckboxElement element)
        {
            var s = element.FontSize > 0 ? element.FontSize : 14;
            var g = new Grid { Width = s, Height = s };
            g.Children.Add(new Border { Width = s, Height = s, BorderBrush = ParseBrush(element.CheckColor ?? "#000000"), BorderThickness = new Thickness(1), Background = Brushes.White });
            if (element.Checked) g.Children.Add(new TextBlock { Text = "\u2713", FontSize = s - 2, Foreground = ParseBrush(element.CheckColor ?? "#000000"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
            return g;
        }

        private static UIElement RenderRadioElement(ExternalRadioElement element)
        {
            var s = element.FontSize > 0 ? element.FontSize : 14;
            return new Ellipse { Width = s, Height = s, Stroke = ParseBrush(element.ForegroundColor ?? "#000000"), StrokeThickness = 1, Fill = element.Checked ? ParseBrush(element.ForegroundColor ?? "#000000") : Brushes.Transparent };
        }

        private static UIElement RenderSignatureElement(ExternalSignatureElement element)
        {
            var c = new Border
            {
                Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX,
                Background = ParseBrush(element.BackgroundColor ?? "#FAFAFA"),
                BorderBrush = ParseBrush(element.LineColor ?? "#94A3B8"),
                BorderThickness = new Thickness(element.LineWidth > 0 ? element.LineWidth : 1),
                CornerRadius = new CornerRadius(element.CornerRadius)
            };
            c.Child = new TextBlock
            {
                Text = element.Placeholder ?? "[签章区域]",
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Foreground = ParseBrush(element.ForegroundColor ?? "#94A3B8"),
                FontSize = element.FontSize > 0 ? element.FontSize : 12,
                FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "KaiTi")
            };
            TextOptions.SetTextFormattingMode((TextBlock)c.Child, TextFormattingMode.Display);
            return c;
        }

        private static UIElement RenderBarcodeElement(ExternalBarcodeElement element)
        {
            var w = element.Width * MM_TO_PX; var h = element.Height * MM_TO_PX;
            var c = new Border
            {
                Width = w, Height = h, Background = ParseBrush(element.BackgroundColor ?? "#FFFFFF"),
                BorderBrush = ParseBrush(element.BorderColor ?? "#D0D5DD"),
                BorderThickness = new Thickness(element.BorderWidth > 0 ? element.BorderWidth : 1),
                CornerRadius = new CornerRadius(element.CornerRadius)
            };
            if (!string.IsNullOrEmpty(element.Value))
            {
                try
                {
                    var bmp = GenBarcode(element);
                    if (bmp != null) { var img = new Image { Source = bmp, Stretch = Stretch.Uniform }; RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality); c.Child = img; return c; }
                }
                catch { }
            }
            var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            sp.Children.Add(new TextBlock { Text = "[条码占位]", HorizontalAlignment = HorizontalAlignment.Center, Foreground = ParseBrush(element.ForegroundColor ?? "#94A3B8"), FontSize = element.FontSize > 0 ? element.FontSize : 10 });
            if (!string.IsNullOrEmpty(element.Value)) sp.Children.Add(new TextBlock { Text = element.Value, HorizontalAlignment = HorizontalAlignment.Center, Foreground = ParseBrush(element.LineColor ?? "#334155"), FontSize = element.FontSize > 0 ? element.FontSize - 2 : 9 });
            c.Child = sp; return c;
        }

        private static BitmapSource GenBarcode(ExternalBarcodeElement e)
        {
            var fmt = e.Format?.ToUpper() switch
            {
                "CODE39" => ZXing.BarcodeFormat.CODE_39, "EAN13" => ZXing.BarcodeFormat.EAN_13,
                "EAN8" => ZXing.BarcodeFormat.EAN_8, "UPC_A" => ZXing.BarcodeFormat.UPC_A,
                "ITF" => ZXing.BarcodeFormat.ITF, "CODABAR" => ZXing.BarcodeFormat.CODABAR,
                "QR_CODE" => ZXing.BarcodeFormat.QR_CODE, _ => ZXing.BarcodeFormat.CODE_128
            };
            var w = (int)(e.Width * MM_TO_PX); var h = (int)(e.Height * MM_TO_PX);
            if (w <= 0) w = 200; if (h <= 0) h = 60; if (e.ShowText) h -= 20;
            return new ZXing.BarcodeWriter<WriteableBitmap> { Format = fmt, Options = new ZXing.Common.EncodingOptions { Width = w, Height = h, Margin = 1 } }.Write(e.Value);
        }

        private static UIElement RenderQrCodeElement(ExternalQrCodeElement element)
        {
            var sz = Math.Min(element.Width, element.Height) * MM_TO_PX; if (sz <= 0) sz = 120;
            var c = new Border { Width = sz, Height = sz, Background = ParseBrush(element.BackgroundColor ?? "#FFFFFF"), BorderBrush = ParseBrush(element.BorderColor ?? "#D0D5DD"), BorderThickness = new Thickness(element.BorderWidth > 0 ? element.BorderWidth : 1) };
            if (!string.IsNullOrEmpty(element.Value))
            {
                try { var bmp = new ZXing.BarcodeWriter<WriteableBitmap> { Format = ZXing.BarcodeFormat.QR_CODE, Options = new ZXing.Common.EncodingOptions { Width = (int)sz, Height = (int)sz, Margin = (int)(element.Margin > 0 ? element.Margin : 1) } }.Write(element.Value); if (bmp != null) { var img = new Image { Source = bmp, Stretch = Stretch.Uniform }; RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality); c.Child = img; return c; } } catch { }
            }
            c.Child = new TextBlock { Text = !string.IsNullOrEmpty(element.Value) ? $"[QR: {element.Value}]" : "[二维码占位]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Foreground = ParseBrush(element.Color ?? "#334155"), FontSize = 9 };
            return c;
        }

        private static UIElement RenderWatermarkElement(ExternalWatermarkElement element)
        {
            var tb = new TextBlock
            {
                Text = element.Text ?? "", FontSize = element.FontSize > 0 ? element.FontSize : 48,
                Foreground = ParseBrush(element.Color ?? "#CBD5E1"),
                Opacity = element.Opacity > 0 ? element.Opacity : 0.15,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = new RotateTransform(element.Angle > 0 ? element.Angle : -30),
                RenderTransformOrigin = new Point(0.5, 0.5), UseLayoutRounding = true
            };
            TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
            return tb;
        }

        private static UIElement RenderPageNumberElement(ExternalPageNumberElement element)
        {
            var fmt = !string.IsNullOrEmpty(element.Format) ? element.Format : "第 {page} 页";
            var text = fmt.Replace("{page}", (element.StartPage > 0 ? element.StartPage : 1).ToString());
            return MakeTextBlock(text, true, element.Width * MM_TO_PX, element, FontWeights.Normal);
        }

        private static UIElement RenderContainerElement(ExternalContainerElement element, object data)
        {
            var cv = new Canvas { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = ParseBrush(element.BackgroundColor), ClipToBounds = element.ClipContent };
            if (element.Children != null)
                foreach (var ch in element.Children)
                    if (ch.IsVisible) try { var u = RenderElement(ch, data); if (u != null) { Canvas.SetLeft(u, ch.X * MM_TO_PX); Canvas.SetTop(u, ch.Y * MM_TO_PX); Canvas.SetZIndex(u, ch.ZIndex); cv.Children.Add(u); } } catch { }
            return cv;
        }

        private static UIElement RenderRepeatElement(ExternalRepeatElement element, object data) => new Border { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = new SolidColorBrush(Color.FromArgb(20, 100, 116, 139)), BorderBrush = new SolidColorBrush(Color.FromRgb(148, 163, 184)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Child = new TextBlock { Text = $"[重复区域: {element.DataSource ?? "未配置"}]", FontSize = element.FontSize > 0 ? element.FontSize : 10, Foreground = ParseBrush(element.ForegroundColor ?? "#94A3B8"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
        private static UIElement RenderHeaderElement(ExternalHeaderElement e, object d) => RenderContainerLike(e.Children, e, d);
        private static UIElement RenderFooterElement(ExternalFooterElement e, object d) => RenderContainerLike(e.Children, e, d);
        private static UIElement RenderContainerLike(List<ExternalElementBase> children, ExternalElementBase e, object d)
        {
            var cv = new Canvas { Width = e.Width * MM_TO_PX, Height = e.Height * MM_TO_PX, Background = ParseBrush(e.BackgroundColor) };
            if (children != null) foreach (var ch in children) if (ch.IsVisible) try { var u = RenderElement(ch, d); if (u != null) { Canvas.SetLeft(u, ch.X * MM_TO_PX); Canvas.SetTop(u, ch.Y * MM_TO_PX); Canvas.SetZIndex(u, ch.ZIndex); cv.Children.Add(u); } } catch { }
            return cv;
        }

        private static UIElement RenderChartElement(ExternalChartElement element)
        {
            var c = new Border { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = ParseBrush(element.BackgroundColor ?? "#FFFFFF"), BorderBrush = ParseBrush(element.BorderColor ?? "#D0D5DD"), BorderThickness = new Thickness(element.BorderWidth > 0 ? element.BorderWidth : 1), CornerRadius = new CornerRadius(element.CornerRadius) };
            c.Child = new TextBlock { Text = element.Title ?? $"[图表: {element.ChartType ?? "未知"}]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = ParseBrush(element.ForegroundColor ?? "#94A3B8"), FontSize = element.FontSize > 0 ? element.FontSize : 14 };
            return c;
        }

        private static UIElement RenderIconElement(ExternalIconElement element) => new TextBlock { Text = element.IconName ?? "\u2B21", Width = element.Width * MM_TO_PX, FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Segoe MDL2 Assets"), FontSize = element.Size > 0 ? (double)element.Size : element.FontSize > 0 ? (double)element.FontSize : 16, Foreground = ParseBrush(element.Color ?? element.ForegroundColor ?? "#000000"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center };

        private static UIElement RenderHyperlinkElement(ExternalHyperlinkElement element)
        {
            var tb = new TextBlock { Width = element.Width * MM_TO_PX, FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI"), FontSize = element.FontSize > 0 ? element.FontSize : 10, TextDecorations = TextDecorations.Underline, Foreground = ParseBrush(element.ForegroundColor ?? "#0066CC"), TextAlignment = ParseTextAlignment(element.TextAlignment), UseLayoutRounding = true, SnapsToDevicePixels = true, Cursor = System.Windows.Input.Cursors.Hand };
            tb.Inlines.Add(new System.Windows.Documents.Run(element.Text ?? element.Url ?? "[链接]"));
            TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(tb, TextRenderingMode.ClearType);
            return tb;
        }

        private static UIElement RenderPlaceholder(ExternalElementBase element)
        {
            var w = Math.Max(element.Width * MM_TO_PX, 20);
            var h = Math.Max(element.Height * MM_TO_PX, 14);
            var b = new Border
            {
                Width = w, Height = h,
                BorderBrush = new SolidColorBrush(Color.FromArgb(60, 148, 163, 184)),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromArgb(15, 100, 116, 139)),
                CornerRadius = new CornerRadius(2)
            };
            var typeName = element.GetType().Name;
            if (typeName.StartsWith("External")) typeName = typeName.Substring(8);
            if (typeName.EndsWith("Element")) typeName = typeName.Substring(0, typeName.Length - 7);
            b.Child = new TextBlock { Text = typeName, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 9 };
            return b;
        }

        #endregion

        #region Helpers

        private static TextBlock MakeTextBlock(string text, bool hasValue, double widthPx, ExternalElementBase element, FontWeight fw)
        {
            // 可编辑元素：Label: value 格式（无值时显示占位横线）
            if (element.Group == ElementGroup.Editable && !string.IsNullOrEmpty(element.Label))
            {
                text = hasValue ? element.Label + ": " + text : element.Label + ": —";
            }
            else if (!hasValue && element.Group == ElementGroup.Editable)
            {
                text = $"[{element.DataPath ?? element.Id}]";
            }

            var tb = new TextBlock
            {
                Text = text, Width = widthPx,
                FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI"),
                FontSize = element.FontSize > 0 ? element.FontSize : 10,
                FontWeight = fw,
                Foreground = ParseForegroundBrush(element.ForegroundColor),
                TextAlignment = ParseTextAlignment(element.TextAlignment),
                UseLayoutRounding = true, SnapsToDevicePixels = true
            };
            TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(tb, TextRenderingMode.ClearType);
            return tb;
        }

        private static Brush ParseBrush(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr) || colorStr == "transparent") return Brushes.Transparent;
            try { return (Brush)new BrushConverter().ConvertFrom(colorStr); } catch { return Brushes.Black; }
        }

        private static Brush ParseForegroundBrush(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr)) return Brushes.Black;
            if (colorStr == "transparent") return Brushes.Transparent;
            try { return (Brush)new BrushConverter().ConvertFrom(colorStr); } catch { return Brushes.Black; }
        }

        private static TextAlignment ParseTextAlignment(string a) => a?.ToLower() switch { "center" => TextAlignment.Center, "right" => TextAlignment.Right, "justify" => TextAlignment.Justify, _ => TextAlignment.Left };

        #endregion
    }

    public class RenderStats { public int Rendered; public int Skipped; public List<string> Errors = new(); }
}
