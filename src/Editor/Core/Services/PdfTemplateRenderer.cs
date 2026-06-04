using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Xinglin.ReportEditor.Contracts;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;
using Xinglin.ReportEditor.Core.SharedInterfaces;

namespace Xinglin.ReportEditor.Core.Services;

[Obsolete("过渡实现，后续迁移到独立 Rendering 项目")]
public class PdfTemplateRenderer : IPdfSharpTemplateRenderer
{
    private const float MmToPoints = 2.835f;
    private const float DefaultPageWidthMm = 210f;
    private const float DefaultPageHeightMm = 297f;
    private const float DefaultMarginMm = 10f;

    public byte[] RenderToPdf(string templateJson)
    {
        var template = TemplateSerializer.Deserialize(templateJson);
        return RenderToPdf(template);
    }

    public byte[] RenderToPdf(TemplateDefinition template)
    {
        var page = template.PageSettings;

        var pageWidth = (page?.PageWidth > 0 ? (float)page.PageWidth : DefaultPageWidthMm) * MmToPoints;
        var pageHeight = (page?.PageHeight > 0 ? (float)page.PageHeight : DefaultPageHeightMm) * MmToPoints;
        var marginLeft = (page?.MarginLeft > 0 ? (float)page.MarginLeft : DefaultMarginMm) * MmToPoints;
        var marginRight = (page?.MarginRight > 0 ? (float)page.MarginRight : DefaultMarginMm) * MmToPoints;
        var marginTop = (page?.MarginTop > 0 ? (float)page.MarginTop : DefaultMarginMm) * MmToPoints;
        var marginBottom = (page?.MarginBottom > 0 ? (float)page.MarginBottom : DefaultMarginMm) * MmToPoints;

        var allElements = template.Elements
            .Where(e => e.IsVisible)
            .OrderBy(e => e.ZIndex)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize(pageWidth, pageHeight));
                page.MarginLeft(marginLeft);
                page.MarginRight(marginRight);
                page.MarginTop(marginTop);
                page.MarginBottom(marginBottom);

                page.Content().Canvas((canvas, _) =>
                {
                    foreach (var el in allElements)
                        RenderElement((SKCanvas)canvas, el);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void RenderElement(SKCanvas canvas, ElementBase element)
    {
        var x = (float)element.X * MmToPoints;
        var y = (float)element.Y * MmToPoints;
        var w = (float)element.Width * MmToPoints;
        var h = (float)element.Height * MmToPoints;

        // Build paint from element font style
        var paint = new SKPaint
        {
            Color = ParseColor(element.ForegroundColor),
            TextSize = element.FontSize > 0 ? (float)element.FontSize : 12f,
            IsAntialias = true
        };

        switch (element)
        {
            case TextElement textEl:
            {
                var text = textEl.Text ?? textEl.Label ?? "";
                canvas.DrawText(text, x, y + paint.TextSize, paint);
                break;
            }
            case NumberElement numEl:
            {
                var text = numEl.Value ?? "";
                if (!string.IsNullOrEmpty(numEl.Unit)) text += $" {numEl.Unit}";
                canvas.DrawText(text, x, y + paint.TextSize, paint);
                break;
            }
            case DateElement dateEl:
            {
                var dateText = dateEl.Value ?? "";
                canvas.DrawText(dateText, x, y + paint.TextSize, paint);
                break;
            }
            case TableElement tableEl:
            {
                var rows = tableEl.Rows;
                var cols = tableEl.Cols;
                var cellW = w / cols;
                var cellH = h / rows;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var cx = x + c * cellW;
                        var cy = y + r * cellH;
                        var cellPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Gray, StrokeWidth = 1, IsAntialias = true };
                        canvas.DrawRect(cx, cy, cellW, cellH, cellPaint);
                        if (tableEl.HasHeader && r == 0 && tableEl.HeaderStyle != null)
                        {
                            var bg = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColor.Parse("F0F0F0") };
                            canvas.DrawRect(cx, cy, cellW, cellH, bg);
                        }
                        var cellData = tableEl.CellData;
                        if (cellData != null && r < cellData.Count && c < cellData[r].Count)
                        {
                            var ct = cellData[r][c];
                            if (!string.IsNullOrEmpty(ct))
                                canvas.DrawText(ct, cx + 2, cy + cellH - 3, new SKPaint { TextSize = 10, Color = SKColors.Black, IsAntialias = true });
                        }
                    }
                }
                break;
            }
            case ImageElement imgEl:
            {
                var imgPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.LightGray, StrokeWidth = 1, IsAntialias = true };
                canvas.DrawRect(x, y, w, h, imgPaint);
                canvas.DrawText(imgEl.AltText ?? "Image", x + 2, y + h / 2 + 4, new SKPaint { TextSize = 10, Color = SKColors.Gray, IsAntialias = true });
                break;
            }
            case CheckboxElement cbEl:
            {
                var boxSize = Math.Min(w, h);
                var boxPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = 1.5f, IsAntialias = true };
                canvas.DrawRect(x, y, boxSize, boxSize, boxPaint);
                if (cbEl.Checked)
                {
                    var checkPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColor.Parse(cbEl.CheckColor ?? "#409eff"), StrokeWidth = 2, IsAntialias = true };
                    canvas.DrawLine(x + 2, y + boxSize / 2, x + boxSize / 3, y + boxSize - 2, checkPaint);
                    canvas.DrawLine(x + boxSize / 3, y + boxSize - 2, x + boxSize - 2, y + 2, checkPaint);
                }
                canvas.DrawText(cbEl.Label ?? "", x + boxSize + 4, y + boxSize - 3, paint);
                break;
            }
            case LineElement:
            {
                var linePaint = new SKPaint
                {
                    Color = ParseColor(element.ForegroundColor),
                    StrokeWidth = (float)(element.BorderWidth ?? 1),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawLine(x, y, x + w, y, linePaint);
                break;
            }
            case DividerElement divEl:
            {
                var divPaint = new SKPaint
                {
                    Color = ParseColor(divEl.Color ?? element.ForegroundColor),
                    StrokeWidth = (float)(divEl.Thickness > 0 ? divEl.Thickness : 1),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawLine(x, y, x + w, y, divPaint);
                break;
            }
            case ShapeElement shapeEl:
            {
                var shapePaint = new SKPaint
                {
                    Color = string.IsNullOrEmpty(shapeEl.BackgroundColor) || shapeEl.BackgroundColor == "transparent"
                        ? SKColors.Transparent
                        : ParseColor(shapeEl.BackgroundColor),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                var strokePaint = new SKPaint
                {
                    Color = ParseColor(element.ForegroundColor),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = (float)(element.BorderWidth ?? 1),
                    IsAntialias = true
                };
                canvas.DrawRect(x, y, w, h, shapePaint);
                canvas.DrawRect(x, y, w, h, strokePaint);
                break;
            }
            case HeaderElement headerEl:
            {
                paint.TextSize = 10f;
                canvas.DrawText(headerEl.Content ?? headerEl.Label ?? "", x, y + paint.TextSize, paint);
                break;
            }
            case FooterElement footerEl:
            {
                paint.TextSize = 10f;
                canvas.DrawText(footerEl.Content ?? footerEl.Label ?? "", x, y + paint.TextSize, paint);
                break;
            }
            case PageNumberElement pageNumEl:
            {
                canvas.DrawText("1", x, y + paint.TextSize, paint);
                break;
            }
            case BarcodeElement barcodeEl:
            {
                var barPaint = new SKPaint
                {
                    Color = ParseColor(barcodeEl.LineColor),
                    StrokeWidth = 2,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };
                var code = barcodeEl.Value ?? barcodeEl.Label ?? "";
                var barCount = code.Length * 4 + 3;
                var barW = w / barCount;
                for (int i = 0; i < code.Length; i++)
                {
                    var bits = "00110"; // simplified encoding
                    for (int j = 0; j < 4; j++)
                    {
                        if (j % 2 == 0)
                            canvas.DrawLine(x + (i * 4 + j) * barW, y, x + (i * 4 + j) * barW, y + h, barPaint);
                    }
                }
                if (barcodeEl.ShowText)
                    canvas.DrawText(code, x, y + h + paint.TextSize, paint);
                break;
            }
            case QrCodeElement qrEl:
            {
                var qrSize = Math.Min(w, h);
                var qrPaint = new SKPaint
                {
                    Color = ParseColor(qrEl.Color),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(x, y, qrSize, qrSize, qrPaint);
                // Draw simplified QR pattern: outer border + inner square
                var innerPaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill };
                canvas.DrawRect(x + 2, y + 2, qrSize - 4, qrSize - 4, innerPaint);
                // Draw QR finder patterns (3 corners)
                var finderSize = qrSize * 0.25f;
                for (int ci = 0; ci < 3; ci++)
                {
                    var fx = ci == 0 ? x + 2 : ci == 2 ? x + qrSize - finderSize - 2 : x + 2;
                    var fy = ci == 0 ? y + 2 : ci == 1 ? y + 2 : y + qrSize - finderSize - 2;
                    canvas.DrawRect(fx, fy, finderSize, finderSize, qrPaint);
                    canvas.DrawRect(fx + 3, fy + 3, finderSize - 6, finderSize - 6, innerPaint);
                    canvas.DrawRect(fx + 6, fy + 6, finderSize - 12, finderSize - 12, qrPaint);
                }
                break;
            }
            case ChartElement chartEl:
            {
                var chartPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Gray, IsAntialias = true };
                // Draw axes
                canvas.DrawLine(x, y + h, x + w, y + h, chartPaint);  // X axis
                canvas.DrawLine(x, y, x, y + h, chartPaint);         // Y axis
                // Draw bars if data exists
                if (chartEl.Labels != null && chartEl.Series != null && chartEl.Series.Count > 0)
                {
                    var values = chartEl.Series[0].Values;
                    var barCount = values.Count;
                    var barW = (w - (barCount + 1) * 2) / barCount;
                    var maxVal = values.Count > 0 ? values.Max() : 1;
                    for (int i = 0; i < barCount; i++)
                    {
                        var barH = (float)(values[i] / maxVal * (h - 4));
                        var barPaint2 = new SKPaint
                        {
                            Color = ParseColor(chartEl.Series[0].Color ?? "#2d7d91"),
                            Style = SKPaintStyle.Fill,
                            IsAntialias = true
                        };
                        canvas.DrawRect(x + 2 + i * (barW + 2), y + h - barH - 2, barW, barH, barPaint2);
                    }
                }
                if (!string.IsNullOrEmpty(chartEl.Title))
                    canvas.DrawText(chartEl.Title, x, y - 3, paint);
                break;
            }
            case SignatureElement sigEl:
            {
                var sigPaint = new SKPaint
                {
                    Color = ParseColor(sigEl.LineColor),
                    StrokeWidth = (float)sigEl.LineWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawLine(x, y + h * 0.5f, x + w, y + h * 0.5f, sigPaint);
                canvas.DrawLine(x, y + h * 0.6f, x + w, y + h * 0.6f, sigPaint);
                var placeholder = sigEl.Placeholder ?? sigEl.Label ?? "";
                var placeholderPaint = new SKPaint { Color = SKColors.Gray, TextSize = 9f, IsAntialias = true };
                canvas.DrawText(placeholder, x + 2, y + h * 0.45f, placeholderPaint);
                break;
            }
            case WatermarkElement wmEl:
            {
                using var wmPaint = new SKPaint
                {
                    Color = ParseColor(wmEl.Color ?? "#cccccc").WithAlpha(64),
                    TextSize = Math.Min(w, h) * 0.15f,
                    IsAntialias = true
                };
                var text = wmEl.Text ?? "";
                canvas.Save();
                canvas.RotateDegrees((float)wmEl.Angle, x + w / 2, y + h / 2);
                if (wmEl.Repeat)
                {
                    for (float r = -h; r < h + w; r += wmPaint.TextSize * 2)
                        canvas.DrawText(text, x, y + r, wmPaint);
                }
                else
                {
                    canvas.DrawText(text, x + w / 4, y + h / 2, wmPaint);
                }
                canvas.Restore();
                break;
            }
            case ContainerElement containerEl:
            {
                // Draw container background/border
                var containerPaint = new SKPaint
                {
                    Color = SKColors.LightGray,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1,
                    IsAntialias = true
                };
                canvas.DrawRect(x, y, w, h, containerPaint);
                // Draw children with offset
                var offset = containerEl.Padding > 0 ? (float)containerEl.Padding : 2f;
                foreach (var child in containerEl.Children)
                {
                    var savedX = child.X;
                    var savedY = child.Y;
                    child.X += offset;
                    child.Y += offset;
                    RenderElement(canvas, child);
                    child.X = savedX;
                    child.Y = savedY;
                }
                break;
            }
            case RepeatElement repeatEl:
            {
                var maxCount = repeatEl.MaxCount ?? 3;
                var gap = (float)repeatEl.Gap;
                var dir = repeatEl.Direction ?? "horizontal";
                var totalW = dir == "horizontal" ? (w - gap * (maxCount - 1)) / maxCount : w;
                var totalH = dir == "vertical" ? (h - gap * (maxCount - 1)) / maxCount : h;
                for (int i = 0; i < maxCount; i++)
                {
                    var rx = dir == "horizontal" ? x + i * (totalW + gap) : x;
                    var ry = dir == "vertical" ? y + i * (totalH + gap) : y;
                    var fallbackPaint = new SKPaint { Color = SKColors.LightGray, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
                    canvas.DrawRect(rx, ry, totalW, totalH, fallbackPaint);
                    canvas.DrawText($"#{i + 1}", rx + 2, ry + paint.TextSize, paint);
                }
                break;
            }
            case IconElement iconEl:
            {
                // Draw placeholder circle with label
                var iconSize = Math.Min(w, h);
                var iconBg = new SKPaint { Color = ParseColor(iconEl.Color ?? "#e8eef3"), Style = SKPaintStyle.Fill, IsAntialias = true };
                canvas.DrawCircle(x + iconSize / 2, y + iconSize / 2, iconSize / 3, iconBg);
                var iconPaint = new SKPaint { Color = SKColors.White, TextSize = iconSize * 0.3f, IsAntialias = true };
                canvas.DrawText(iconEl.IconName?.Substring(0, Math.Min(1, iconEl.IconName?.Length ?? 0)) ?? "?", x + iconSize / 2 - 3, y + iconSize / 2 + 4, iconPaint);
                break;
            }
            case HyperlinkElement linkEl:
            {
                var linkPaint = new SKPaint
                {
                    Color = ParseColor(linkEl.Color ?? "#2d7d91"),
                    TextSize = paint.TextSize,
                    IsAntialias = true
                };
                var displayText = linkEl.Text ?? linkEl.Label ?? linkEl.Url ?? "";
                canvas.DrawText(displayText, x, y + paint.TextSize, linkPaint);
                // Draw underline
                var underlinePaint = new SKPaint
                {
                    Color = linkPaint.Color,
                    StrokeWidth = 1,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };
                var textWidth = displayText.Length * paint.TextSize * 0.6f;
                canvas.DrawLine(x, y + paint.TextSize + 1, x + textWidth, y + paint.TextSize + 1, underlinePaint);
                break;
            }
            default:
            {
                // fallback: draw a border rectangle with label
                var fallbackPaint = new SKPaint
                {
                    Color = SKColors.LightGray,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f,
                    IsAntialias = true
                };
                canvas.DrawRect(x, y, w, h, fallbackPaint);
                var label = element.Label ?? element.GetType().Name;
                canvas.DrawText(label, x + 2, y + paint.TextSize, paint);
                break;
            }
        }
    }

    private static SKColor ParseColor(string? hex)
    {
        if (string.IsNullOrEmpty(hex)) return SKColors.Black;
        return SKColor.TryParse(hex, out var color) ? color : SKColors.Black;
    }
}


