using System.IO;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Rendering.Services;

public class PdfElementRenderer
{
    public void RenderElement(SKCanvas canvas, ElementBase element, Dictionary<string, object> data, PdfPageLayoutEngine layout)
    {
        if (!element.IsVisible) return;

        ApplyCanvasLayout(canvas, element, layout, out var x, out var y, out var w, out var h);

        switch (element)
        {
            case TextElement e: DrawText(canvas, e, data, x, y, w, h, layout); break;
            case NumberElement e: DrawNumber(canvas, e, data, x, y, w, h, layout); break;
            case DateElement e: DrawDate(canvas, e, data, x, y, w, h, layout); break;
            case ImageElement e: DrawImage(canvas, e, data, x, y, w, h, layout); break;
            case TableElement e: DrawTable(canvas, e, data, x, y, w, h, layout); break;
            case BarcodeElement e: DrawBarcode(canvas, e, data, x, y, w, h, layout); break;
            case QrCodeElement e: DrawQrCode(canvas, e, data, x, y, w, h, layout); break;
            case LineElement e: DrawLine(canvas, e, data, x, y, w, h, layout); break;
            case ShapeElement e: DrawShape(canvas, e, data, x, y, w, h, layout); break;
            case DividerElement e: DrawDivider(canvas, e, data, x, y, w, h, layout); break;
            case CheckboxElement e: DrawCheckbox(canvas, e, data, x, y, w, h, layout); break;
            case RadioElement e: DrawRadio(canvas, e, data, x, y, w, h, layout); break;
            case SignatureElement e: DrawSignature(canvas, e, data, x, y, w, h, layout); break;
            case HeaderElement e: DrawHeader(canvas, e, data, x, y, w, h, layout); break;
            case FooterElement e: DrawFooter(canvas, e, data, x, y, w, h, layout); break;
            case PageNumberElement e: DrawPageNumber(canvas, e, data, x, y, w, h, layout); break;
            case WatermarkElement e: DrawWatermark(canvas, e, data, x, y, w, h, layout); break;
            case ContainerElement e: DrawContainer(canvas, e, data, x, y, w, h, layout); break;
            case RepeatElement e: DrawRepeat(canvas, e, data, x, y, w, h, layout); break;
            case HyperlinkElement e: DrawHyperlink(canvas, e, data, x, y, w, h, layout); break;
            case IconElement e: DrawIcon(canvas, e, data, x, y, w, h, layout); break;
            case ChartElement e: DrawChart(canvas, e, data, x, y, w, h, layout); break;
        }

        if (element.Opacity < 1)
            canvas.Restore();
    }

    private static void ApplyCanvasLayout(SKCanvas canvas, ElementBase element, PdfPageLayoutEngine layout,
        out float x, out float y, out float w, out float h)
    {
        x = layout.ConvertX(element.X);
        y = layout.ConvertY(element.Y);
        w = element.Width > 0 ? layout.ConvertSize(element.Width) : 0;
        h = element.Height > 0 ? layout.ConvertSize(element.Height) : 0;

        if (element.Opacity < 1)
        {
            using var alphaPaint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(element.Opacity * 255)) };
            canvas.SaveLayer(alphaPaint);
        }
    }

    private string ResolveValue(ElementBase element, Dictionary<string, object> data)
    {
        if (!string.IsNullOrEmpty(element.DataPath) && data.TryGetValue(element.DataPath!, out var value))
            return value?.ToString() ?? (element is ExternalElementBase eb ? eb.DefaultValue ?? string.Empty : string.Empty);
        return element is ExternalElementBase eb2 ? eb2.DefaultValue ?? string.Empty : string.Empty;
    }

    private SKColor ParseColor(string? color, byte alpha = 255)
    {
        if (string.IsNullOrEmpty(color) || color == "transparent")
            return SKColors.Transparent;
        try
        {
            var skColor = SKColor.Parse(color);
            return skColor.WithAlpha(alpha);
        }
        catch
        {
            return SKColors.Black;
        }
    }

    private float GetFontSize(ElementBase element)
    {
        return element.FontSize > 0 ? (float)element.FontSize * 2.835f : 12f * 2.835f;
    }

    private SKTextAlign ParseTextAlign(string? alignment)
    {
        return alignment?.ToLower() switch
        {
            "center" => SKTextAlign.Center,
            "right" => SKTextAlign.Right,
            _ => SKTextAlign.Left
        };
    }

    #region SKCanvas Rendering

    private void DrawText(SKCanvas canvas, TextElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var text = !string.IsNullOrEmpty(element.DataPath) && data.TryGetValue(element.DataPath, out var val)
            ? val?.ToString() ?? element.Text
            : element.Text;
        if (string.IsNullOrEmpty(text)) text = element.DefaultValue;

        var hasLabel = !string.IsNullOrEmpty(element.Label);
        if (string.IsNullOrEmpty(text) && !hasLabel) return;

        var displayText = hasLabel
            ? (string.IsNullOrEmpty(text) ? $"{element.Label}: ____" : $"{element.Label}: {text}")
            : text!;

        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        var textColor = (string.IsNullOrEmpty(text) && hasLabel)
            ? ParseColor("#94A3B8")
            : ParseColor(element.ForegroundColor);

        using var paint = new SKPaint
        {
            Color = textColor,
            IsAntialias = true
        };

        SKTypeface? typeface = null;
        if ((element.FontStyle?.ToLower() ?? "normal") == "italic")
            typeface = SKTypeface.FromFamilyName(element.FontFamily ?? string.Empty, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
        else if ((element.FontWeight?.ToLower() ?? "normal") == "bold")
            typeface = SKTypeface.FromFamilyName(element.FontFamily ?? string.Empty, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        if (!string.IsNullOrEmpty(element.FontFamily) && typeface == null)
            typeface = SKTypeface.FromFamilyName(element.FontFamily!);

        if (typeface != null)
            font.Typeface = typeface;

        var textAlign = ParseTextAlign(element.TextAlignment);
        var textX = textAlign switch
        {
            SKTextAlign.Center => x + w / 2,
            SKTextAlign.Right => x + w,
            _ => x
        };

        if (w > 0 && h > 0)
        {
            var textY = y + h / 2 + font.Metrics.Descent / 2;
            canvas.DrawText(displayText, textX, textY, textAlign, font, paint);
        }
        else
        {
            canvas.DrawText(displayText, textX, y + font.Size, textAlign, font, paint);
        }
    }

    private void DrawNumber(SKCanvas canvas, NumberElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value) && double.TryParse(element.Value, out var numVal) && numVal != 0)
            value = element.DecimalPlaces > 0
                ? numVal.ToString($"F{element.DecimalPlaces}")
                : numVal.ToString();
        if (!string.IsNullOrEmpty(element.Unit) && !string.IsNullOrEmpty(value)) value += $" {element.Unit}";

        var hasLabel = !string.IsNullOrEmpty(element.Label);
        var displayText = hasLabel
            ? (string.IsNullOrEmpty(value) ? $"{element.Label}: ____" : $"{element.Label}: {value}")
            : (value ?? string.Empty);

        var textColor = (string.IsNullOrEmpty(value) && hasLabel)
            ? ParseColor("#94A3B8")
            : ParseColor(element.ForegroundColor);

        using var font = new SKFont { Size = GetFontSize(element) };
        using var paint = new SKPaint { Color = textColor, IsAntialias = true };

        var textAlign = ParseTextAlign(element.TextAlignment);
        var textX = textAlign switch
        {
            SKTextAlign.Center => x + w / 2,
            SKTextAlign.Right => x + w,
            _ => x
        };

        canvas.DrawText(displayText, textX, y + font.Size, textAlign, font, paint);
    }

    private void DrawDate(SKCanvas canvas, DateElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var dt))
            value = dt.ToString(!string.IsNullOrEmpty(element.Format) ? element.Format : "yyyy-MM-dd");

        var hasLabel = !string.IsNullOrEmpty(element.Label);
        var displayText = hasLabel
            ? (string.IsNullOrEmpty(value) ? $"{element.Label}: ____" : $"{element.Label}: {value}")
            : (value ?? string.Empty);

        var textColor = (string.IsNullOrEmpty(value) && hasLabel)
            ? ParseColor("#94A3B8")
            : ParseColor(element.ForegroundColor);

        using var font = new SKFont { Size = GetFontSize(element) };
        using var paint = new SKPaint { Color = textColor, IsAntialias = true };

        var textAlign = ParseTextAlign(element.TextAlignment);
        var textX = textAlign switch
        {
            SKTextAlign.Center => x + w / 2,
            SKTextAlign.Right => x + w,
            _ => x
        };

        canvas.DrawText(displayText, textX, y + font.Size, textAlign, font, paint);
    }

    private void DrawImage(SKCanvas canvas, ImageElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var src = ResolveValue(element, data);
        if (string.IsNullOrEmpty(src)) src = element.Src;

        if (!string.IsNullOrEmpty(src) && File.Exists(src))
        {
            using var bitmap = SKBitmap.Decode(src);
            if (bitmap != null)
            {
                var rect = w > 0 && h > 0
                    ? new SKRect(x, y, x + w, y + h)
                    : new SKRect(x, y, x + bitmap.Width, y + bitmap.Height);
                canvas.DrawBitmap(bitmap, rect);
            }
        }
        else
        {
            using var font = new SKFont
            {
                Size = GetFontSize(element)
            };

            using var paint = new SKPaint
            {
                Color = SKColors.Gray,
                IsAntialias = true
            };
            var label = !string.IsNullOrEmpty(element.AltText) ? $"[{element.AltText}]" : "[图片]";
            canvas.DrawText(label, x, y + font.Size, SKTextAlign.Left, font, paint);
        }
    }

    private void DrawTable(SKCanvas canvas, TableElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        if (element.Cols <= 0 || element.Rows <= 0) return;

        var tableWidth = w > 0 ? w : layout.PageWidth - layout.MarginLeft - layout.MarginRight;
        var rowHeight = h > 0 ? h / element.Rows : 20f;
        var colWidth = tableWidth / element.Cols;

        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 1f,
            IsAntialias = true
        };

        using var font = new SKFont
        {
            Size = element.FontSize > 0 ? (float)element.FontSize * 2.835f : 10f * 2.835f
        };

        using var fillPaint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            IsAntialias = true
        };

        using var headerFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.LightGray,
            IsAntialias = true
        };

        var headerRows = element.HasHeader ? Math.Min(element.HeaderRows, element.Rows) : 0;
        var cellData = element.CellData;

        for (int r = 0; r < element.Rows; r++)
        {
            var cellY = y + r * rowHeight;
            if (r < headerRows)
                canvas.DrawRect(new SKRect(x, cellY, x + tableWidth, cellY + rowHeight), headerFillPaint);

            for (int c = 0; c < element.Cols; c++)
            {
                var cellX = x + c * colWidth;
                canvas.DrawRect(new SKRect(cellX, cellY, cellX + colWidth, cellY + rowHeight), strokePaint);

                if (cellData != null && r < cellData.Count && c < cellData[r].Count)
                {
                    var cellText = cellData[r][c] ?? string.Empty;
                    canvas.DrawText(cellText, cellX + 4, cellY + rowHeight - 4, SKTextAlign.Left, font, fillPaint);
                }
                else if (element.Cells != null)
                {
                    var cell = element.Cells.FirstOrDefault(cc => cc.Row == r && cc.Col == c);
                    if (cell != null && !string.IsNullOrEmpty(cell.Text))
                    {
                        var cellText = !string.IsNullOrEmpty(cell.DataPath) && data.TryGetValue(cell.DataPath, out var v)
                            ? v?.ToString() ?? cell.Text
                            : cell.Text;
                        canvas.DrawText(cellText, cellX + 4, cellY + rowHeight - 4, SKTextAlign.Left, font, fillPaint);
                    }
                }
            }
        }
    }

    private void DrawBarcode(SKCanvas canvas, BarcodeElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (string.IsNullOrEmpty(value))
        {
            using var font = new SKFont { Size = GetFontSize(element) };
            using var paint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
            canvas.DrawText("[条形码]", x, y + font.Size, SKTextAlign.Left, font, paint);
            return;
        }

        try
        {
            var format = ParseBarcodeFormat(element.Format ?? string.Empty);
            var writer = new BarcodeWriter
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = Math.Max(1, (int)w),
                    Height = Math.Max(1, (int)h)
                }
            };
            var skBitmap = writer.Write(value);
            if (skBitmap != null)
            {
                var rect = w > 0 && h > 0
                    ? new SKRect(x, y, x + w, y + h)
                    : new SKRect(x, y, x + skBitmap.Width, y + skBitmap.Height);
                canvas.DrawBitmap(skBitmap, rect);
            }
        }
        catch
        {
            using var font = new SKFont { Size = GetFontSize(element) };
            using var paint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
            canvas.DrawText($"[条形码: {value}]", x, y + font.Size, SKTextAlign.Left, font, paint);
        }
    }

    private void DrawQrCode(SKCanvas canvas, QrCodeElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (string.IsNullOrEmpty(value))
        {
            using var font = new SKFont { Size = GetFontSize(element) };
            using var paint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
            canvas.DrawText("[二维码]", x, y + font.Size, SKTextAlign.Left, font, paint);
            return;
        }

        try
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = 200,
                    Height = 200,
                    Margin = Math.Max(element.Margin, 0)
                }
            };
            if (!string.IsNullOrEmpty(element.ErrorCorrectionLevel))
            {
                var level = element.ErrorCorrectionLevel.ToUpper() switch
                {
                    "L" => ZXing.QrCode.Internal.ErrorCorrectionLevel.L,
                    "M" => ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
                    "Q" => ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
                    "H" => ZXing.QrCode.Internal.ErrorCorrectionLevel.H,
                    _ => ZXing.QrCode.Internal.ErrorCorrectionLevel.M
                };
                writer.Options.Hints[EncodeHintType.ERROR_CORRECTION] = level;
            }
            var skBitmap = writer.Write(value);
            if (skBitmap != null)
            {
                var rect = w > 0 && h > 0
                    ? new SKRect(x, y, x + w, y + h)
                    : new SKRect(x, y, x + skBitmap.Width, y + skBitmap.Height);
                canvas.DrawBitmap(skBitmap, rect);
            }
        }
        catch
        {
            using var font = new SKFont { Size = GetFontSize(element) };
            using var paint = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
            canvas.DrawText($"[二维码: {value}]", x, y + font.Size, SKTextAlign.Left, font, paint);
        }
    }

    private void DrawLine(SKCanvas canvas, LineElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ParseColor(element.StrokeColor),
            StrokeWidth = (float)element.StrokeWidth * 2.835f,
            IsAntialias = true
        };

        var startX = layout.ConvertX(element.X1);
        var startY = layout.ConvertY(element.Y1);
        var endX = layout.ConvertX(element.X2);
        var endY = layout.ConvertY(element.Y2);

        canvas.DrawLine(startX, startY, endX, endY, paint);
    }

    private void DrawShape(SKCanvas canvas, ShapeElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var shapeType = element.ShapeType?.ToLower() ?? "rectangle";

        if (!string.IsNullOrEmpty(element.FillColor))
        {
            using var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ParseColor(element.FillColor),
                IsAntialias = true
            };

            if (shapeType == "ellipse")
                canvas.DrawOval(new SKRect(x, y, x + w, y + h), fillPaint);
            else
                canvas.DrawRect(new SKRect(x, y, x + w, y + h), fillPaint);
        }

        if (!string.IsNullOrEmpty(element.StrokeColor) && element.StrokeWidth > 0)
        {
            using var strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ParseColor(element.StrokeColor),
                StrokeWidth = (float)element.StrokeWidth * 2.835f,
                IsAntialias = true
            };

            if (shapeType == "ellipse")
                canvas.DrawOval(new SKRect(x, y, x + w, y + h), strokePaint);
            else
                canvas.DrawRect(new SKRect(x, y, x + w, y + h), strokePaint);
        }
    }

    private void DrawDivider(SKCanvas canvas, DividerElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var thickness = element.Thickness > 0 ? (float)element.Thickness * 2.835f : 1f;
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color) : SKColors.Black;

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = color,
            StrokeWidth = thickness,
            IsAntialias = true
        };

        var lineY = y + h / 2;
        var lineEndX = w > 0 ? x + w : canvas.LocalClipBounds.Right;
        canvas.DrawLine(x, lineY, lineEndX, lineY, paint);
    }

    private void DrawCheckbox(SKCanvas canvas, CheckboxElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var symbol = element.Checked ? "☑" : "☐";
        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            IsAntialias = true
        };
        canvas.DrawText(symbol, x, y + font.Size, SKTextAlign.Left, font, paint);
    }

    private void DrawRadio(SKCanvas canvas, RadioElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var symbol = element.IsChecked ? "◉" : "○";
        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            IsAntialias = true
        };
        canvas.DrawText(symbol, x, y + font.Size, SKTextAlign.Left, font, paint);
    }

    private void DrawSignature(SKCanvas canvas, SignatureElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            IsAntialias = true
        };
        var label = !string.IsNullOrEmpty(element.Placeholder) ? $"[{element.Placeholder}]" : "[签名]";
        canvas.DrawText(label, x, y + font.Size, SKTextAlign.Left, font, paint);
    }

    private void DrawHeader(SKCanvas canvas, HeaderElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        foreach (var child in element.Children)
            RenderElement(canvas, child, data, layout);
    }

    private void DrawFooter(SKCanvas canvas, FooterElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        foreach (var child in element.Children)
            RenderElement(canvas, child, data, layout);
    }

    private void DrawPageNumber(SKCanvas canvas, PageNumberElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            IsAntialias = true
        };
        var format = !string.IsNullOrEmpty(element.Format) ? element.Format : "第 {page} 页";
        var text = format.Replace("{page}", "·").Replace("{Page}", "·");
        canvas.DrawText(text, x, y + font.Size, SKTextAlign.Left, font, paint);
    }

    private void DrawWatermark(SKCanvas canvas, WatermarkElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        if (string.IsNullOrEmpty(element.Text)) return;

        var alpha = (byte)(element.Opacity < 1 ? element.Opacity * 255 : 40);
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color, alpha) : SKColors.Gray.WithAlpha(alpha);

        using var font = new SKFont
        {
            Size = element.FontSize > 0 ? (float)element.FontSize * 2.835f : 48f * 2.835f
        };

        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };

        canvas.Save();
        var centerX = x + w / 2;
        var centerY = y + h / 2;
        canvas.RotateDegrees((float)element.Angle, centerX, centerY);
        canvas.DrawText(element.Text, centerX, centerY, SKTextAlign.Center, font, paint);
        canvas.Restore();
    }

    private void DrawContainer(SKCanvas canvas, ContainerElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        if (!string.IsNullOrEmpty(element.BackgroundColor))
        {
            using var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ParseColor(element.BackgroundColor),
                IsAntialias = true
            };
            canvas.DrawRect(new SKRect(x, y, x + w, y + h), fillPaint);
        }

        if (!string.IsNullOrEmpty(element.BorderColor) && element.BorderWidth > 0)
        {
            using var strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ParseColor(element.BorderColor),
                StrokeWidth = (float)element.BorderWidth * 2.835f,
                IsAntialias = true
            };
            canvas.DrawRect(new SKRect(x, y, x + w, y + h), strokePaint);
        }

        foreach (var child in element.Children)
            RenderElement(canvas, child, data, layout);
    }

    private void DrawRepeat(SKCanvas canvas, RepeatElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            IsAntialias = true
        };

        if (!string.IsNullOrEmpty(element.DataSource) && data.TryGetValue(element.DataSource, out var itemsObj))
        {
            if (itemsObj is System.Collections.IList items)
            {
                var gap = element.Gap > 0 ? layout.ConvertSize(element.Gap) : 4f;
                var itemHeight = font.Size + 2;
                for (int i = 0; i < items.Count; i++)
                {
                    var itemText = !string.IsNullOrEmpty(element.ItemTemplate)
                        ? element.ItemTemplate.Replace("{value}", items[i]?.ToString() ?? string.Empty)
                        : items[i]?.ToString() ?? string.Empty;
                    canvas.DrawText(itemText, x, y + (i + 1) * (itemHeight + gap), SKTextAlign.Left, font, paint);
                }
            }
        }
        else
        {
            canvas.DrawText(element.ItemTemplate ?? string.Empty, x, y + font.Size, SKTextAlign.Left, font, paint);
        }
    }

    private void DrawHyperlink(SKCanvas canvas, HyperlinkElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var text = (!string.IsNullOrEmpty(element.Text) ? element.Text : element.Url) ?? string.Empty;
        var fontSize = GetFontSize(element);

        using var font = new SKFont
        {
            Size = fontSize
        };

        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            IsAntialias = true
        };

        canvas.DrawText(text, x, y + fontSize, SKTextAlign.Left, font, paint);

        using var underlinePaint = new SKPaint
        {
            Color = SKColors.Blue,
            StrokeWidth = 1,
            IsAntialias = true
        };
        canvas.DrawLine(x, y + fontSize + 2, x + (text.Length * fontSize * 0.6f), y + fontSize + 2, underlinePaint);
    }

    private void DrawIcon(SKCanvas canvas, IconElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color) : ParseColor(element.ForegroundColor);
        var size = element.Size > 0 ? (float)element.Size * 2.835f : GetFontSize(element);

        using var font = new SKFont
        {
            Size = size
        };

        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };

        var label = !string.IsNullOrEmpty(element.IconName) ? $"[{element.IconName}]" : "[图标]";
        canvas.DrawText(label, x, y + font.Size, SKTextAlign.Left, font, paint);
    }

    private void DrawChart(SKCanvas canvas, ChartElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var font = new SKFont
        {
            Size = GetFontSize(element)
        };

        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            IsAntialias = true
        };

        var label = !string.IsNullOrEmpty(element.ChartType) ? $"[图表: {element.ChartType}]" : "[图表]";
        canvas.DrawText(label, x, y + font.Size, SKTextAlign.Left, font, paint);
    }

    #endregion

    private BarcodeFormat ParseBarcodeFormat(string? format)
    {
        return format?.ToUpper() switch
        {
            "CODE_128" => BarcodeFormat.CODE_128,
            "CODE_39" => BarcodeFormat.CODE_39,
            "EAN_13" => BarcodeFormat.EAN_13,
            "EAN_8" => BarcodeFormat.EAN_8,
            "UPC_A" => BarcodeFormat.UPC_A,
            "UPC_E" => BarcodeFormat.UPC_E,
            "ITF" => BarcodeFormat.ITF,
            "CODABAR" => BarcodeFormat.CODABAR,
            _ => BarcodeFormat.CODE_128
        };
    }
}
