using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services.PdfExport;

public class PdfElementRenderer
{
    public void RenderElement(IContainer container, ReportExternalElementBase element, Dictionary<string, object> data)
    {
        if (!element.IsVisible) return;

        switch (element)
        {
            case ExternalTextElement e: RenderTextContainer(container, e, data); break;
            case ExternalNumberElement e: RenderNumberContainer(container, e, data); break;
            case ExternalDateElement e: RenderDateContainer(container, e, data); break;
            case ExternalImageElement e: RenderImageContainer(container, e, data); break;
            case ExternalTableElement e: RenderTableContainer(container, e, data); break;
            case ExternalBarcodeElement e: RenderBarcodeContainer(container, e, data); break;
            case ExternalQrCodeElement e: RenderQrCodeContainer(container, e, data); break;
            case ExternalLineElement e: RenderLineContainer(container, e, data); break;
            case ExternalShapeElement e: RenderShapeContainer(container, e, data); break;
            case ExternalDividerElement e: RenderDividerContainer(container, e, data); break;
            case ExternalCheckboxElement e: RenderCheckboxContainer(container, e, data); break;
            case ExternalRadioElement e: RenderRadioContainer(container, e, data); break;
            case ExternalSignatureElement e: RenderSignatureContainer(container, e, data); break;
            case ExternalHeaderElement e: RenderHeaderContainer(container, e, data); break;
            case ExternalFooterElement e: RenderFooterContainer(container, e, data); break;
            case ExternalPageNumberElement e: RenderPageNumberContainer(container, e, data); break;
            case ExternalWatermarkElement e: RenderWatermarkContainer(container, e, data); break;
            case ExternalContainerElement e: RenderContainerContainer(container, e, data); break;
            case ExternalRepeatElement e: RenderRepeatContainer(container, e, data); break;
            case ExternalHyperlinkElement e: RenderHyperlinkContainer(container, e, data); break;
            case ExternalIconElement e: RenderIconContainer(container, e, data); break;
            case ExternalChartElement e: RenderChartContainer(container, e, data); break;
        }
    }

    public void RenderElement(SKCanvas canvas, ReportExternalElementBase element, Dictionary<string, object> data, PdfPageLayoutEngine layout)
    {
        if (!element.IsVisible) return;

        var x = layout.ConvertX(element.X);
        var y = layout.ConvertY(element.Y);
        var w = element.Width > 0 ? layout.ConvertSize(element.Width) : 0;
        var h = element.Height > 0 ? layout.ConvertSize(element.Height) : 0;

        if (element.Opacity < 1)
            canvas.SaveLayerAlpha(new SKRect(x, y, w > 0 ? x + w : canvas.LocalClipBounds.Right, h > 0 ? y + h : canvas.LocalClipBounds.Bottom), (byte)(element.Opacity * 255));

        switch (element)
        {
            case ExternalTextElement e: DrawText(canvas, e, data, x, y, w, h, layout); break;
            case ExternalNumberElement e: DrawNumber(canvas, e, data, x, y, w, h, layout); break;
            case ExternalDateElement e: DrawDate(canvas, e, data, x, y, w, h, layout); break;
            case ExternalImageElement e: DrawImage(canvas, e, data, x, y, w, h, layout); break;
            case ExternalTableElement e: DrawTable(canvas, e, data, x, y, w, h, layout); break;
            case ExternalBarcodeElement e: DrawBarcode(canvas, e, data, x, y, w, h, layout); break;
            case ExternalQrCodeElement e: DrawQrCode(canvas, e, data, x, y, w, h, layout); break;
            case ExternalLineElement e: DrawLine(canvas, e, data, x, y, w, h, layout); break;
            case ExternalShapeElement e: DrawShape(canvas, e, data, x, y, w, h, layout); break;
            case ExternalDividerElement e: DrawDivider(canvas, e, data, x, y, w, h, layout); break;
            case ExternalCheckboxElement e: DrawCheckbox(canvas, e, data, x, y, w, h, layout); break;
            case ExternalRadioElement e: DrawRadio(canvas, e, data, x, y, w, h, layout); break;
            case ExternalSignatureElement e: DrawSignature(canvas, e, data, x, y, w, h, layout); break;
            case ExternalHeaderElement e: DrawHeader(canvas, e, data, x, y, w, h, layout); break;
            case ExternalFooterElement e: DrawFooter(canvas, e, data, x, y, w, h, layout); break;
            case ExternalPageNumberElement e: DrawPageNumber(canvas, e, data, x, y, w, h, layout); break;
            case ExternalWatermarkElement e: DrawWatermark(canvas, e, data, x, y, w, h, layout); break;
            case ExternalContainerElement e: DrawContainer(canvas, e, data, x, y, w, h, layout); break;
            case ExternalRepeatElement e: DrawRepeat(canvas, e, data, x, y, w, h, layout); break;
            case ExternalHyperlinkElement e: DrawHyperlink(canvas, e, data, x, y, w, h, layout); break;
            case ExternalIconElement e: DrawIcon(canvas, e, data, x, y, w, h, layout); break;
            case ExternalChartElement e: DrawChart(canvas, e, data, x, y, w, h, layout); break;
        }

        if (element.Opacity < 1)
            canvas.Restore();
    }

    private string ResolveValue(ReportExternalElementBase element, Dictionary<string, object> data)
    {
        if (!string.IsNullOrEmpty(element.DataPath) && data.TryGetValue(element.DataPath, out var value))
            return value?.ToString() ?? element.DefaultValue;
        return element.DefaultValue;
    }

    private SKColor ParseColor(string color, byte alpha = 255)
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

    private float GetFontSize(ReportExternalElementBase element)
    {
        return element.FontSize > 0 ? (float)element.FontSize * 2.835f : 12f * 2.835f;
    }

    private SKTextAlign ParseTextAlign(string alignment)
    {
        return alignment?.ToLower() switch
        {
            "center" => SKTextAlign.Center,
            "right" => SKTextAlign.Right,
            _ => SKTextAlign.Left
        };
    }

    #region IContainer Rendering

    private void RenderTextContainer(IContainer container, ExternalTextElement element, Dictionary<string, object> data)
    {
        var text = !string.IsNullOrEmpty(element.DataPath) && data.TryGetValue(element.DataPath, out var val)
            ? val?.ToString() ?? element.Text
            : element.Text;
        if (string.IsNullOrEmpty(text)) text = element.DefaultValue;
        if (string.IsNullOrEmpty(text)) return;

        container.Text(text).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderNumberContainer(IContainer container, ExternalNumberElement element, Dictionary<string, object> data)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value) && element.Value != 0)
            value = element.DecimalPlaces > 0
                ? element.Value.ToString($"F{element.DecimalPlaces}")
                : element.Value.ToString();
        if (!string.IsNullOrEmpty(element.Unit)) value += $" {element.Unit}";

        container.Text(value ?? string.Empty).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderDateContainer(IContainer container, ExternalDateElement element, Dictionary<string, object> data)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var dt))
            value = dt.ToString(!string.IsNullOrEmpty(element.Format) ? element.Format : "yyyy-MM-dd");

        container.Text(value ?? string.Empty).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderImageContainer(IContainer container, ExternalImageElement element, Dictionary<string, object> data)
    {
        var src = ResolveValue(element, data);
        if (string.IsNullOrEmpty(src)) src = element.Src;

        if (!string.IsNullOrEmpty(src) && File.Exists(src))
        {
            container.Image(src);
        }
        else
        {
            container.Text(!string.IsNullOrEmpty(element.AltText) ? $"[{element.AltText}]" : "[图片]")
                .FontSize(element.FontSize > 0 ? element.FontSize : 12)
                .FontColor(Colors.Grey.Medium);
        }
    }

    private void RenderTableContainer(IContainer container, ExternalTableElement element, Dictionary<string, object> data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < element.Columns; i++)
                    columns.RelativeColumn();
            });

            var cellData = element.CellData;
            var hasHeader = element.HasHeader && cellData.Count > 0;
            var headerRows = hasHeader ? Math.Min(element.HeaderRows, cellData.Count) : 0;

            for (int r = 0; r < cellData.Count; r++)
            {
                var isHeader = r < headerRows;
                for (int c = 0; c < Math.Min(cellData[r].Count, element.Columns); c++)
                {
                    var cell = table.Cell().ColumnSpan(1).RowSpan(1);
                    if (isHeader)
                        cell = cell.Background(Colors.Grey.Lighten3);

                    cell.Border(1).BorderColor(Colors.Grey.Darken1)
                        .Padding(4)
                        .Text(cellData[r][c] ?? string.Empty)
                        .FontSize(element.FontSize > 0 ? element.FontSize : 10);
                }
            }
        });
    }

    private void RenderBarcodeContainer(IContainer container, ExternalBarcodeElement element, Dictionary<string, object> data)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (string.IsNullOrEmpty(value))
        {
            container.Text("[条形码]").FontSize(element.FontSize > 0 ? element.FontSize : 12).FontColor(Colors.Grey.Medium);
            return;
        }

        try
        {
            var format = ParseBarcodeFormat(element.Format);
            var writer = new BarcodeWriter { Format = format };
            var bitmap = writer.Write(value);
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            container.Image(ms.ToArray());
        }
        catch
        {
            container.Text($"[条形码: {value}]").FontSize(element.FontSize > 0 ? element.FontSize : 12).FontColor(Colors.Grey.Medium);
        }
    }

    private void RenderQrCodeContainer(IContainer container, ExternalQrCodeElement element, Dictionary<string, object> data)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (string.IsNullOrEmpty(value))
        {
            container.Text("[二维码]").FontSize(element.FontSize > 0 ? element.FontSize : 12).FontColor(Colors.Grey.Medium);
            return;
        }

        try
        {
            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
            var options = new EncodingOptions
            {
                Width = 200,
                Height = 200,
                Margin = (int)Math.Max(element.Margin, 0)
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
                options.Hints[EncodeHintType.ERROR_CORRECTION] = level;
            }
            writer.Options = options;
            var bitmap = writer.Write(value);
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            container.Image(ms.ToArray());
        }
        catch
        {
            container.Text($"[二维码: {value}]").FontSize(element.FontSize > 0 ? element.FontSize : 12).FontColor(Colors.Grey.Medium);
        }
    }

    private void RenderLineContainer(IContainer container, ExternalLineElement element, Dictionary<string, object> data)
    {
        container.LineHorizontal((float)element.LineWidth).LineColor(ParseColor(element.LineColor));
    }

    private void RenderShapeContainer(IContainer container, ExternalShapeElement element, Dictionary<string, object> data)
    {
        var c = container.Width(element.Width > 0 ? element.Width : 100).Height(element.Height > 0 ? element.Height : 50);
        if (!string.IsNullOrEmpty(element.FillColor))
            c = c.Background(ParseColor(element.FillColor));
        if (!string.IsNullOrEmpty(element.StrokeColor) && element.StrokeWidth > 0)
            c = c.Border((float)element.StrokeWidth).BorderColor(ParseColor(element.StrokeColor));
    }

    private void RenderDividerContainer(IContainer container, ExternalDividerElement element, Dictionary<string, object> data)
    {
        var thickness = element.Thickness > 0 ? (float)element.Thickness : 1f;
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color) : SKColors.Black;
        container.LineHorizontal(thickness).LineColor(color);
    }

    private void RenderCheckboxContainer(IContainer container, ExternalCheckboxElement element, Dictionary<string, object> data)
    {
        var symbol = element.Checked ? "☑" : "☐";
        container.Text(symbol).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderRadioContainer(IContainer container, ExternalRadioElement element, Dictionary<string, object> data)
    {
        var symbol = element.Checked ? "◉" : "○";
        container.Text(symbol).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderSignatureContainer(IContainer container, ExternalSignatureElement element, Dictionary<string, object> data)
    {
        container.Text(!string.IsNullOrEmpty(element.Placeholder) ? $"[{element.Placeholder}]" : "[签名]")
            .FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(Colors.Grey.Medium);
    }

    private void RenderHeaderContainer(IContainer container, ExternalHeaderElement element, Dictionary<string, object> data)
    {
        container.Column(column =>
        {
            foreach (var child in element.Children)
                RenderElement(column.Item(), child, data);
        });
    }

    private void RenderFooterContainer(IContainer container, ExternalFooterElement element, Dictionary<string, object> data)
    {
        container.Column(column =>
        {
            foreach (var child in element.Children)
                RenderElement(column.Item(), child, data);
        });
    }

    private void RenderPageNumberContainer(IContainer container, ExternalPageNumberElement element, Dictionary<string, object> data)
    {
        container.Text(text =>
        {
            text.Span("第 ");
            text.CurrentPageNumber();
            text.Span(" 页");
        }).FontSize(element.FontSize > 0 ? element.FontSize : 10)
          .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderWatermarkContainer(IContainer container, ExternalWatermarkElement element, Dictionary<string, object> data)
    {
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color, 40) : SKColors.Gray.WithAlpha(40);
        container.Text(element.Text ?? string.Empty)
            .FontSize(element.FontSize > 0 ? element.FontSize : 48)
            .FontColor(color);
    }

    private void RenderContainerContainer(IContainer container, ExternalContainerElement element, Dictionary<string, object> data)
    {
        var c = container;
        if (!string.IsNullOrEmpty(element.BackgroundColor))
            c = c.Background(ParseColor(element.BackgroundColor));
        if (element.Padding > 0)
            c = c.Padding((float)element.Padding);

        c.Column(column =>
        {
            foreach (var child in element.Children)
                RenderElement(column.Item(), child, data);
        });
    }

    private void RenderRepeatContainer(IContainer container, ExternalRepeatElement element, Dictionary<string, object> data)
    {
        container.Column(column =>
        {
            if (!string.IsNullOrEmpty(element.DataSource) && data.TryGetValue(element.DataSource, out var itemsObj))
            {
                if (itemsObj is System.Collections.IList items)
                {
                    foreach (var item in items)
                    {
                        var itemText = !string.IsNullOrEmpty(element.ItemTemplate)
                            ? element.ItemTemplate.Replace("{value}", item?.ToString() ?? string.Empty)
                            : item?.ToString() ?? string.Empty;
                        column.Item().Text(itemText).FontSize(element.FontSize > 0 ? element.FontSize : 12);
                    }
                }
            }
            else
            {
                column.Item().Text(element.ItemTemplate).FontSize(element.FontSize > 0 ? element.FontSize : 12);
            }
        });
    }

    private void RenderHyperlinkContainer(IContainer container, ExternalHyperlinkElement element, Dictionary<string, object> data)
    {
        var text = !string.IsNullOrEmpty(element.Text) ? element.Text : element.Url;
        container.Text(text ?? string.Empty)
            .FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(Colors.Blue.Medium)
            .Underline();
    }

    private void RenderIconContainer(IContainer container, ExternalIconElement element, Dictionary<string, object> data)
    {
        var label = !string.IsNullOrEmpty(element.IconName) ? $"[{element.IconName}]" : "[图标]";
        container.Text(label).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(ParseColor(element.ForegroundColor));
    }

    private void RenderChartContainer(IContainer container, ExternalChartElement element, Dictionary<string, object> data)
    {
        var label = !string.IsNullOrEmpty(element.ChartType) ? $"[图表: {element.ChartType}]" : "[图表]";
        container.Text(label).FontSize(element.FontSize > 0 ? element.FontSize : 12)
            .FontColor(Colors.Grey.Medium);
    }

    #endregion

    #region SKCanvas Rendering

    private void DrawText(SKCanvas canvas, ExternalTextElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var text = !string.IsNullOrEmpty(element.DataPath) && data.TryGetValue(element.DataPath, out var val)
            ? val?.ToString() ?? element.Text
            : element.Text;
        if (string.IsNullOrEmpty(text)) text = element.DefaultValue;
        if (string.IsNullOrEmpty(text)) return;

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true,
            TextAlign = ParseTextAlign(element.TextAlignment)
        };

        if (element.FontStyle?.ToLower() == "italic")
            paint.Typeface = SKTypeface.FromFamilyName(element.FontFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
        else if (element.FontWeight?.ToLower() == "bold")
            paint.Typeface = SKTypeface.FromFamilyName(element.FontFamily, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        if (!string.IsNullOrEmpty(element.FontFamily) && paint.Typeface == null)
            paint.Typeface = SKTypeface.FromFamilyName(element.FontFamily);

        var textX = paint.TextAlign switch
        {
            SKTextAlign.Center => x + w / 2,
            SKTextAlign.Right => x + w,
            _ => x
        };

        if (w > 0 && h > 0)
        {
            using var bounds = new SKRect();
            paint.MeasureText(text, ref bounds);
            var textY = y + (h + bounds.Height) / 2 - bounds.Top;
            canvas.DrawText(text, textX, textY, paint);
        }
        else
        {
            canvas.DrawText(text, textX, y + paint.TextSize, paint);
        }
    }

    private void DrawNumber(SKCanvas canvas, ExternalNumberElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value) && element.Value != 0)
            value = element.DecimalPlaces > 0
                ? element.Value.ToString($"F{element.DecimalPlaces}")
                : element.Value.ToString();
        if (!string.IsNullOrEmpty(element.Unit)) value += $" {element.Unit}";

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true,
            TextAlign = ParseTextAlign(element.TextAlignment)
        };

        var textX = paint.TextAlign switch
        {
            SKTextAlign.Center => x + w / 2,
            SKTextAlign.Right => x + w,
            _ => x
        };

        canvas.DrawText(value ?? string.Empty, textX, y + paint.TextSize, paint);
    }

    private void DrawDate(SKCanvas canvas, ExternalDateElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var dt))
            value = dt.ToString(!string.IsNullOrEmpty(element.Format) ? element.Format : "yyyy-MM-dd");

        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true,
            TextAlign = ParseTextAlign(element.TextAlignment)
        };

        var textX = paint.TextAlign switch
        {
            SKTextAlign.Center => x + w / 2,
            SKTextAlign.Right => x + w,
            _ => x
        };

        canvas.DrawText(value ?? string.Empty, textX, y + paint.TextSize, paint);
    }

    private void DrawImage(SKCanvas canvas, ExternalImageElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
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
            using var paint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = GetFontSize(element),
                IsAntialias = true
            };
            var label = !string.IsNullOrEmpty(element.AltText) ? $"[{element.AltText}]" : "[图片]";
            canvas.DrawText(label, x, y + paint.TextSize, paint);
        }
    }

    private void DrawTable(SKCanvas canvas, ExternalTableElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        if (element.Columns <= 0 || element.Rows <= 0) return;

        var tableWidth = w > 0 ? w : layout.PageWidth - layout.MarginLeft - layout.MarginRight;
        var rowHeight = h > 0 ? h / element.Rows : 20f;
        var colWidth = tableWidth / element.Columns;

        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = (float)element.TableBorder,
            IsAntialias = true
        };

        using var fillPaint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = element.FontSize > 0 ? (float)element.FontSize * 2.835f : 10f * 2.835f,
            IsAntialias = true
        };

        using var headerFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.LightGray,
            IsAntialias = true
        };

        var headerRows = element.HasHeader ? Math.Min(element.HeaderRows, element.Rows) : 0;

        for (int r = 0; r < element.Rows; r++)
        {
            var cellY = y + r * rowHeight;
            if (r < headerRows)
                canvas.DrawRect(new SKRect(x, cellY, x + tableWidth, cellY + rowHeight), headerFillPaint);

            for (int c = 0; c < element.Columns; c++)
            {
                var cellX = x + c * colWidth;
                canvas.DrawRect(new SKRect(cellX, cellY, cellX + colWidth, cellY + rowHeight), strokePaint);

                if (element.CellData != null && r < element.CellData.Count && c < element.CellData[r].Count)
                {
                    var cellText = element.CellData[r][c] ?? string.Empty;
                    canvas.DrawText(cellText, cellX + 4, cellY + rowHeight - 4, fillPaint);
                }
                else if (element.Cells != null)
                {
                    var cell = element.Cells.FirstOrDefault(cc => cc.Row == r && cc.Col == c);
                    if (cell != null && !string.IsNullOrEmpty(cell.Text))
                    {
                        var cellText = !string.IsNullOrEmpty(cell.DataPath) && data.TryGetValue(cell.DataPath, out var v)
                            ? v?.ToString() ?? cell.Text
                            : cell.Text;
                        canvas.DrawText(cellText, cellX + 4, cellY + rowHeight - 4, fillPaint);
                    }
                }
            }
        }
    }

    private void DrawBarcode(SKCanvas canvas, ExternalBarcodeElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (string.IsNullOrEmpty(value))
        {
            using var paint = new SKPaint { Color = SKColors.Gray, TextSize = GetFontSize(element), IsAntialias = true };
            canvas.DrawText("[条形码]", x, y + paint.TextSize, paint);
            return;
        }

        try
        {
            var format = ParseBarcodeFormat(element.Format);
            var writer = new BarcodeWriter { Format = format };
            var bitmap = writer.Write(value);
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            using var skBitmap = SKBitmap.Decode(ms);
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
            using var paint = new SKPaint { Color = SKColors.Gray, TextSize = GetFontSize(element), IsAntialias = true };
            canvas.DrawText($"[条形码: {value}]", x, y + paint.TextSize, paint);
        }
    }

    private void DrawQrCode(SKCanvas canvas, ExternalQrCodeElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var value = ResolveValue(element, data);
        if (string.IsNullOrEmpty(value)) value = element.Value;
        if (string.IsNullOrEmpty(value))
        {
            using var paint = new SKPaint { Color = SKColors.Gray, TextSize = GetFontSize(element), IsAntialias = true };
            canvas.DrawText("[二维码]", x, y + paint.TextSize, paint);
            return;
        }

        try
        {
            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
            var options = new EncodingOptions
            {
                Width = 200,
                Height = 200,
                Margin = (int)Math.Max(element.Margin, 0)
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
                options.Hints[EncodeHintType.ERROR_CORRECTION] = level;
            }
            writer.Options = options;
            var bitmap = writer.Write(value);
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            using var skBitmap = SKBitmap.Decode(ms);
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
            using var paint = new SKPaint { Color = SKColors.Gray, TextSize = GetFontSize(element), IsAntialias = true };
            canvas.DrawText($"[二维码: {value}]", x, y + paint.TextSize, paint);
        }
    }

    private void DrawLine(SKCanvas canvas, ExternalLineElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ParseColor(element.LineColor),
            StrokeWidth = (float)element.LineWidth * 2.835f,
            IsAntialias = true
        };

        var startX = layout.ConvertX(element.StartX);
        var startY = layout.ConvertY(element.StartY);
        var endX = layout.ConvertX(element.EndX);
        var endY = layout.ConvertY(element.EndY);

        canvas.DrawLine(startX, startY, endX, endY, paint);
    }

    private void DrawShape(SKCanvas canvas, ExternalShapeElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
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

    private void DrawDivider(SKCanvas canvas, ExternalDividerElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
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

    private void DrawCheckbox(SKCanvas canvas, ExternalCheckboxElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var symbol = element.Checked ? "☑" : "☐";
        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true
        };
        canvas.DrawText(symbol, x, y + paint.TextSize, paint);
    }

    private void DrawRadio(SKCanvas canvas, ExternalRadioElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var symbol = element.Checked ? "◉" : "○";
        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true
        };
        canvas.DrawText(symbol, x, y + paint.TextSize, paint);
    }

    private void DrawSignature(SKCanvas canvas, ExternalSignatureElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = GetFontSize(element),
            IsAntialias = true
        };
        var label = !string.IsNullOrEmpty(element.Placeholder) ? $"[{element.Placeholder}]" : "[签名]";
        canvas.DrawText(label, x, y + paint.TextSize, paint);
    }

    private void DrawHeader(SKCanvas canvas, ExternalHeaderElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        foreach (var child in element.Children)
            RenderElement(canvas, child, data, layout);
    }

    private void DrawFooter(SKCanvas canvas, ExternalFooterElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        foreach (var child in element.Children)
            RenderElement(canvas, child, data, layout);
    }

    private void DrawPageNumber(SKCanvas canvas, ExternalPageNumberElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true
        };
        var format = !string.IsNullOrEmpty(element.Format) ? element.Format : "第 {page} 页";
        var text = format.Replace("{page}", "·").Replace("{Page}", "·");
        canvas.DrawText(text, x, y + paint.TextSize, paint);
    }

    private void DrawWatermark(SKCanvas canvas, ExternalWatermarkElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        if (string.IsNullOrEmpty(element.Text)) return;

        var alpha = (byte)(element.Opacity < 1 ? element.Opacity * 255 : 40);
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color, alpha) : SKColors.Gray.WithAlpha(alpha);

        using var paint = new SKPaint
        {
            Color = color,
            TextSize = element.FontSize > 0 ? (float)element.FontSize * 2.835f : 48f * 2.835f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        canvas.Save();
        var centerX = x + w / 2;
        var centerY = y + h / 2;
        canvas.RotateDegrees((float)element.Angle, centerX, centerY);
        canvas.DrawText(element.Text, centerX, centerY, paint);
        canvas.Restore();
    }

    private void DrawContainer(SKCanvas canvas, ExternalContainerElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
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

    private void DrawRepeat(SKCanvas canvas, ExternalRepeatElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = GetFontSize(element),
            IsAntialias = true
        };

        if (!string.IsNullOrEmpty(element.DataSource) && data.TryGetValue(element.DataSource, out var itemsObj))
        {
            if (itemsObj is System.Collections.IList items)
            {
                var gap = element.Gap > 0 ? layout.ConvertSize(element.Gap) : 4f;
                var itemHeight = paint.TextSize + 2;
                for (int i = 0; i < items.Count; i++)
                {
                    var itemText = !string.IsNullOrEmpty(element.ItemTemplate)
                        ? element.ItemTemplate.Replace("{value}", items[i]?.ToString() ?? string.Empty)
                        : items[i]?.ToString() ?? string.Empty;
                    canvas.DrawText(itemText, x, y + (i + 1) * (itemHeight + gap), paint);
                }
            }
        }
        else
        {
            canvas.DrawText(element.ItemTemplate, x, y + paint.TextSize, paint);
        }
    }

    private void DrawHyperlink(SKCanvas canvas, ExternalHyperlinkElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var text = !string.IsNullOrEmpty(element.Text) ? element.Text : element.Url;

        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            TextSize = GetFontSize(element),
            IsAntialias = true,
            UnderlineText = true
        };

        canvas.DrawText(text, x, y + paint.TextSize, paint);
    }

    private void DrawIcon(SKCanvas canvas, ExternalIconElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        var color = !string.IsNullOrEmpty(element.Color) ? ParseColor(element.Color) : ParseColor(element.ForegroundColor);
        var size = element.Size > 0 ? (float)element.Size * 2.835f : GetFontSize(element);

        using var paint = new SKPaint
        {
            Color = color,
            TextSize = size,
            IsAntialias = true
        };

        var label = !string.IsNullOrEmpty(element.IconName) ? $"[{element.IconName}]" : "[图标]";
        canvas.DrawText(label, x, y + paint.TextSize, paint);
    }

    private void DrawChart(SKCanvas canvas, ExternalChartElement element, Dictionary<string, object> data, float x, float y, float w, float h, PdfPageLayoutEngine layout)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = GetFontSize(element),
            IsAntialias = true
        };

        var label = !string.IsNullOrEmpty(element.ChartType) ? $"[图表: {element.ChartType}]" : "[图表]";
        canvas.DrawText(label, x, y + paint.TextSize, paint);
    }

    #endregion

    private BarcodeFormat ParseBarcodeFormat(string format)
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
