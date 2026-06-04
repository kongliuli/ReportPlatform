using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public class CanvasRenderer
{
    private const double MM_TO_PX = 3.7795275591;

    public void RenderToCanvas(Canvas canvas, TemplateDefinition template)
    {
        if (canvas == null || template == null) return;

        canvas.Children.Clear();
        canvas.Width = template.PageSettings.PageWidth * MM_TO_PX;
        canvas.Height = template.PageSettings.PageHeight * MM_TO_PX;
        canvas.Background = ParseBrush(template.PageSettings.BackgroundColor);
        RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);

        foreach (var element in template.Elements)
        {
            if (element is not ExternalElementBase extElement) continue;
            if (!extElement.IsVisible) continue;

            try
            {
                var uiElement = RenderElement(extElement) ?? RenderPlaceholder(extElement);
                var w = Math.Max(extElement.Width * MM_TO_PX, 1);
                var h = Math.Max(extElement.Height * MM_TO_PX, 1);
                if (uiElement is FrameworkElement fe)
                {
                    if (double.IsNaN(fe.Width) || fe.Width <= 0) fe.Width = w;
                    if (double.IsNaN(fe.Height) || fe.Height <= 0) fe.Height = h;
                }
                Canvas.SetLeft(uiElement, extElement.X * MM_TO_PX);
                Canvas.SetTop(uiElement, extElement.Y * MM_TO_PX);
                Canvas.SetZIndex(uiElement, extElement.ZIndex);
                canvas.Children.Add(uiElement);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CanvasRenderer] 渲染元素失败 IsVisible={extElement.IsVisible}: {ex.Message}"); }
        }
    }

    private static UIElement? RenderElement(ElementBase element)
    {
        return element switch
        {
            TextElement t => RenderTextElement(t),
            LineElement l => RenderLineElement(l),
            NumberElement n => RenderNumberElement(n),
            DateElement dt => RenderDateElement(dt),
            DropdownElement dd => RenderDropdownElement(dd),
            TableElement tb => RenderTableElement(tb),
            DividerElement d => RenderDividerElement(d),
            ShapeElement s => RenderShapeElement(s),
            ImageElement i => RenderImageElement(i),
            CheckboxElement cb => RenderCheckboxElement(cb),
            SignatureElement sg => RenderSignatureElement(sg),
            BarcodeElement bc => RenderBarcodeElement(bc),
            QrCodeElement qr => RenderQrCodeElement(qr),
            WatermarkElement wm => RenderWatermarkElement(wm),
            PageNumberElement pn => RenderPageNumberElement(pn),
            ContainerElement ct => RenderContainerElement(ct),
            HeaderElement hd => RenderHeaderFooter(hd.Children.Cast<ElementBase>().ToList(), hd),
            FooterElement ft => RenderHeaderFooter(ft.Children.Cast<ElementBase>().ToList(), ft),
            _ => null
        };
    }

    private static UIElement RenderTextElement(TextElement element)
    {
        var text = element.Text ?? element.DefaultValue ?? "";
        var hasValue = !string.IsNullOrWhiteSpace(text);

        if (element.Group == ElementGroup.Editable && !string.IsNullOrEmpty(element.Label))
            text = hasValue ? element.Label + ": " + text : element.Label + ": —";
        else if (!hasValue)
            text = !string.IsNullOrEmpty(element.DataPath) ? $"[{element.DataPath}]" : "";

        if (string.IsNullOrEmpty(text)) return null!;

        var tb = new TextBlock
        {
            Text = text,
            Width = element.Width * MM_TO_PX,
            FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI"),
            FontSize = element.FontSize > 0 ? (double)element.FontSize : 10,
            FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
            FontStyle = element.FontStyle == "italic" ? FontStyles.Italic : FontStyles.Normal,
            Foreground = ParseForeground(element.ForegroundColor),
            Background = ParseBrush(element.BackgroundColor),
            TextAlignment = ParseAlign(element.TextAlignment),
            TextWrapping = TextWrapping.NoWrap,
            UseLayoutRounding = true, SnapsToDevicePixels = true
        };
        TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
        TextOptions.SetTextRenderingMode(tb, TextRenderingMode.ClearType);
        return tb;
    }

    private static UIElement RenderLineElement(LineElement element)
    {
        double dx = (element.X2 - element.X1) * MM_TO_PX;
        double dy = (element.Y2 - element.Y1) * MM_TO_PX;
        if (dx <= 0 && dy <= 0) dx = element.Width * MM_TO_PX;

        var line = new Line
        {
            X1 = 0, Y1 = 0, X2 = Math.Max(dx, 1), Y2 = Math.Max(dy, 0),
            Stroke = ParseBrush(element.StrokeColor),
            StrokeThickness = element.StrokeWidth > 0 ? element.StrokeWidth : 1,
            SnapsToDevicePixels = true, UseLayoutRounding = true
        };
        RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);
        return line;
    }

    private static UIElement RenderNumberElement(NumberElement element)
    {
        string text;
        if (double.TryParse(element.Value, out var numVal) && numVal > 0)
        {
            var valStr = element.DecimalPlaces > 0 ? numVal.ToString($"F{element.DecimalPlaces}") : numVal.ToString();
            text = valStr + (string.IsNullOrEmpty(element.Unit) ? "" : " " + element.Unit);
        }
        else
            text = element.DefaultValue ?? "";

        var hasValue = !string.IsNullOrWhiteSpace(text);
        return MakeTextBlock(text, hasValue, element);
    }

    private static UIElement RenderDateElement(DateElement element)
    {
        var text = element.Value ?? element.DefaultValue ?? "";
        if (!string.IsNullOrEmpty(text) && DateTime.TryParse(text, out var dt))
            text = dt.ToString(!string.IsNullOrEmpty(element.Format) ? element.Format : "yyyy-MM-dd");
        return MakeTextBlock(text, !string.IsNullOrWhiteSpace(text), element);
    }

    private static UIElement RenderDropdownElement(DropdownElement element)
    {
        var text = !string.IsNullOrEmpty(element.SelectedValue) ? element.SelectedValue : element.Placeholder ?? "";
        return MakeTextBlock(text, !string.IsNullOrEmpty(element.SelectedValue), element);
    }

    private static UIElement RenderTableElement(TableElement element)
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
            grid.Children.Add(new TextBlock { Text = "[空表格]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 10 });
            return grid;
        }

        for (int c = 0; c < element.Cols; c++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (int r = 0; r < element.Rows; r++) grid.RowDefinitions.Add(new RowDefinition());

        var ff = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI");
        var hbg = new SolidColorBrush(Color.FromRgb(240, 245, 250));
        var abg = new SolidColorBrush(Color.FromRgb(248, 250, 252));
        var bb = new SolidColorBrush(Colors.Black);

        for (int r = 0; r < element.CellData.Count && r < element.Rows; r++)
        {
            var row = element.CellData[r];
            for (int c = 0; c < row.Count && c < element.Cols; c++)
            {
                var tb = new TextBlock
                {
                    Text = row[c] ?? "",
                    FontFamily = ff,
                    FontSize = element.FontSize > 0 ? (double)element.FontSize : 10,
                    FontWeight = element.HasHeader && r == 0 ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = ParseForeground(element.ForegroundColor),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(element.CellPadding),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
                if (element.HasHeader && r == 0) tb.Background = hbg;
                else if (r % 2 == 0) tb.Background = abg;

                var b = new Border { BorderBrush = bb, BorderThickness = new Thickness(1), Child = tb };
                Grid.SetRow(b, r); Grid.SetColumn(b, c);
                grid.Children.Add(b);
            }
        }
        return grid;
    }

    private static UIElement RenderDividerElement(DividerElement element)
    {
        var horiz = element.Width >= element.Height;
        var t = element.Thickness > 0 ? element.Thickness : 1;
        if (element.Style == "dashed" || element.Style == "dotted")
        {
            var da = element.Style == "dashed" ? new DoubleCollection { 4, 2 } : new DoubleCollection { 1, 2 };
            return new Line { X1 = 0, Y1 = 0, X2 = horiz ? Math.Max(element.Width * MM_TO_PX, 1) : 0, Y2 = horiz ? 0 : Math.Max(element.Height * MM_TO_PX, 1), Stroke = ParseBrush(element.Color ?? "#000000"), StrokeThickness = t, StrokeDashArray = da, SnapsToDevicePixels = true };
        }
        return new Border { Width = horiz ? Math.Max(element.Width * MM_TO_PX, 1) : t, Height = horiz ? t : Math.Max(element.Height * MM_TO_PX, 1), Background = ParseBrush(element.Color ?? "#000000") };
    }

    private static UIElement RenderShapeElement(ShapeElement element)
    {
        var w = (element.Width > 0 ? element.Width : 100) * MM_TO_PX;
        var h = (element.Height > 0 ? element.Height : 100) * MM_TO_PX;
        Shape s = element.ShapeType?.ToLower() switch
        {
            "ellipse" => new Ellipse { Width = w, Height = h },
            _ => new Rectangle { Width = w, Height = h }
        };
        s.Fill = ParseBrush(element.FillColor ?? "#E2E8F0");
        s.Stroke = ParseBrush(element.StrokeColor ?? "#000000");
        s.StrokeThickness = element.StrokeWidth > 0 ? element.StrokeWidth : 1;
        return s;
    }

    private static UIElement RenderImageElement(ImageElement element)
    {
        var c = new Border { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = ParseBrush("#F5F5F5"), BorderBrush = ParseBrush("#D0D5DD"), BorderThickness = new Thickness(1) };
        c.Child = new TextBlock { Text = element.AltText ?? "[图片]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 10 };
        return c;
    }

    private static UIElement RenderCheckboxElement(CheckboxElement element)
    {
        var s = element.FontSize > 0 ? (double)element.FontSize : 14;
        var g = new Grid { Width = s, Height = s };
        g.Children.Add(new Border { Width = s, Height = s, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Background = Brushes.White });
        if (element.Checked) g.Children.Add(new TextBlock { Text = "✓", FontSize = s - 2, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        return g;
    }

    private static UIElement RenderSignatureElement(SignatureElement element)
    {
        var c = new Border { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = ParseBrush("#FAFAFA"), BorderBrush = ParseBrush("#94A3B8"), BorderThickness = new Thickness(1) };
        c.Child = new TextBlock { Text = element.Placeholder ?? "[签章]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 12 };
        return c;
    }

    private static UIElement RenderBarcodeElement(BarcodeElement element)
    {
        var c = new Border { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = Brushes.White, BorderBrush = ParseBrush("#D0D5DD"), BorderThickness = new Thickness(1) };
        if (!string.IsNullOrEmpty(element.Value))
        {
            try
            {
                var fmt = element.Format?.ToUpper() switch { "CODE39" => ZXing.BarcodeFormat.CODE_39, "EAN13" => ZXing.BarcodeFormat.EAN_13, _ => ZXing.BarcodeFormat.CODE_128 };
                var bmp = new ZXing.BarcodeWriter<WriteableBitmap> { Format = fmt, Options = new ZXing.Common.EncodingOptions { Width = (int)(element.Width * MM_TO_PX), Height = (int)(element.Height * MM_TO_PX), Margin = 1 } }.Write(element.Value);
                if (bmp != null) { c.Child = new Image { Source = bmp, Stretch = Stretch.Uniform }; return c; }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CanvasRenderer] RenderBarcode 失败: {ex.Message}"); }
        }
        c.Child = new TextBlock { Text = "[条码]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 10 };
        return c;
    }

    private static UIElement RenderQrCodeElement(QrCodeElement element)
    {
        var sz = Math.Min(element.Width, element.Height) * MM_TO_PX; if (sz <= 0) sz = 120;
        var c = new Border { Width = sz, Height = sz, Background = Brushes.White, BorderBrush = ParseBrush("#D0D5DD"), BorderThickness = new Thickness(1) };
        if (!string.IsNullOrEmpty(element.Value))
        {
            try
            {
                var bmp = new ZXing.BarcodeWriter<WriteableBitmap> { Format = ZXing.BarcodeFormat.QR_CODE, Options = new ZXing.Common.EncodingOptions { Width = (int)sz, Height = (int)sz, Margin = 1 } }.Write(element.Value);
                if (bmp != null) { c.Child = new Image { Source = bmp, Stretch = Stretch.Uniform }; return c; }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CanvasRenderer] RenderQRCode 失败: {ex.Message}"); }
        }
        c.Child = new TextBlock { Text = "[二维码]", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 9 };
        return c;
    }

    private static UIElement RenderWatermarkElement(WatermarkElement element)
    {
        return new TextBlock { Text = element.Text ?? "", FontSize = (double)(element.FontSize > 0 ? element.FontSize : 48) }; // FontSize is nullable
    }

    private static UIElement RenderPageNumberElement(PageNumberElement element)
    {
        var fmt = !string.IsNullOrEmpty(element.Format) ? element.Format : "第 {page} 页";
        var text = fmt.Replace("{page}", (element.StartPage > 0 ? element.StartPage : 1).ToString());
        return MakeTextBlock(text, true, element);
    }

    private static UIElement RenderContainerElement(ContainerElement element)
    {
        var cv = new Canvas { Width = element.Width * MM_TO_PX, Height = element.Height * MM_TO_PX, Background = ParseBrush(element.BackgroundColor), ClipToBounds = element.ClipContent };
        if (element.Children != null)
            foreach (var ch in element.Children)
                if (ch.IsVisible) try { var u = RenderElement(ch); if (u != null) { Canvas.SetLeft(u, ch.X * MM_TO_PX); Canvas.SetTop(u, ch.Y * MM_TO_PX); cv.Children.Add(u); } } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CanvasRenderer] Container子元素渲染失败: {ex.Message}"); }
        return cv;
    }

    private static UIElement RenderHeaderFooter(List<ElementBase> children, ElementBase e)
    {
        var cv = new Canvas { Width = e.Width * MM_TO_PX, Height = e.Height * MM_TO_PX, Background = ParseBrush(e.BackgroundColor) };
        if (children != null)
            foreach (var ch in children)
                if (ch.IsVisible) try { var u = RenderElement(ch); if (u != null) { Canvas.SetLeft(u, ch.X * MM_TO_PX); Canvas.SetTop(u, ch.Y * MM_TO_PX); cv.Children.Add(u); } } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CanvasRenderer] HeaderFooter子元素渲染失败: {ex.Message}"); }
        return cv;
    }

    private static UIElement RenderPlaceholder(ElementBase element)
    {
        var w = Math.Max(element.Width * MM_TO_PX, 20);
        var h = Math.Max(element.Height * MM_TO_PX, 14);
        var b = new Border { Width = w, Height = h, BorderBrush = new SolidColorBrush(Color.FromArgb(60, 148, 163, 184)), BorderThickness = new Thickness(1), Background = new SolidColorBrush(Color.FromArgb(15, 100, 116, 139)), CornerRadius = new CornerRadius(2) };
        var typeName = element.GetType().Name.Replace("External", "").Replace("Element", "");
        b.Child = new TextBlock { Text = typeName, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 9 };
        return b;
    }

    private static TextBlock MakeTextBlock(string text, bool hasValue, ElementBase element)
    {
        var extElem = element as ExternalElementBase;
        if (extElem?.Group == ElementGroup.Editable && !string.IsNullOrEmpty(extElem.Label))
            text = hasValue ? extElem.Label + ": " + text : extElem.Label + ": —";
        else if (!hasValue && extElem?.Group == ElementGroup.Editable)
            text = $"[{extElem.DataPath ?? extElem.Id}]";

        var tb = new TextBlock
        {
            Text = text, Width = element.Width * MM_TO_PX,
            FontFamily = new FontFamily(!string.IsNullOrEmpty(element.FontFamily) ? element.FontFamily : "Microsoft YaHei UI"),
            FontSize = element.FontSize > 0 ? (double)element.FontSize : 10,
            FontWeight = element.FontWeight == "bold" ? FontWeights.Bold : FontWeights.Normal,
            Foreground = ParseForeground(element.ForegroundColor),
            TextAlignment = ParseAlign(element.TextAlignment),
            UseLayoutRounding = true, SnapsToDevicePixels = true
        };
        TextOptions.SetTextFormattingMode(tb, TextFormattingMode.Display);
        TextOptions.SetTextRenderingMode(tb, TextRenderingMode.ClearType);
        return tb;
    }

    private static Brush ParseBrush(string? colorStr)
    {
        if (string.IsNullOrEmpty(colorStr) || colorStr == "transparent") return Brushes.Transparent;
        try { return (Brush)new BrushConverter().ConvertFrom(colorStr)!; } catch { return Brushes.Transparent; }
    }

    private static Brush ParseForeground(string? colorStr)
    {
        if (string.IsNullOrEmpty(colorStr) || colorStr == "transparent") return Brushes.Black;
        try { return (Brush)new BrushConverter().ConvertFrom(colorStr)!; } catch { return Brushes.Black; }
    }

    private static TextAlignment ParseAlign(string? a) => a?.ToLower() switch { "center" => TextAlignment.Center, "right" => TextAlignment.Right, _ => TextAlignment.Left };
}
