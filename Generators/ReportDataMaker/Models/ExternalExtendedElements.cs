using Newtonsoft.Json;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.Models;

/// <summary>报告外部元素基类，定义所有外部元素的公共属性</summary>
public abstract class ReportExternalElementBase : ExternalElementBase
{
    /// <summary>是否可见</summary>
    public new bool IsVisible { get; set; } = true;
    /// <summary>背景颜色</summary>
    public new string BackgroundColor { get; set; } = string.Empty;
    /// <summary>边框颜色</summary>
    public new string BorderColor { get; set; } = string.Empty;
    /// <summary>边框宽度</summary>
    public new double BorderWidth { get; set; }
    /// <summary>边框样式</summary>
    public new string BorderStyle { get; set; } = string.Empty;
    /// <summary>圆角半径</summary>
    public new double CornerRadius { get; set; }
    /// <summary>不透明度</summary>
    public new double Opacity { get; set; } = 1;
    /// <summary>阴影效果</summary>
    public string Shadow { get; set; } = string.Empty;
    /// <summary>字体族</summary>
    public new string FontFamily { get; set; } = string.Empty;
    /// <summary>字体大小</summary>
    public new double FontSize { get; set; }
    /// <summary>字体粗细</summary>
    public new string FontWeight { get; set; } = string.Empty;
    /// <summary>字体样式</summary>
    public new string FontStyle { get; set; } = string.Empty;
    /// <summary>前景色</summary>
    public new string ForegroundColor { get; set; } = "#000000";
    /// <summary>文本对齐方式</summary>
    public new string TextAlignment { get; set; } = string.Empty;
    /// <summary>标签宽度</summary>
    public double LabelWidth { get; set; }
    /// <summary>默认值</summary>
    public string DefaultValue { get; set; } = string.Empty;
    /// <summary>选项列表</summary>
    public List<string> Options { get; set; } = new();
    /// <summary>格式化字符串</summary>
    public new string FormatString { get; set; } = string.Empty;
    /// <summary>元素类型</summary>
    public string? ElementType { get; set; }
}

/// <summary>外部文本元素</summary>
public class ExternalTextElement : ReportExternalElementBase
{
    /// <summary>文本内容</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>富文本内容</summary>
    public string RichText { get; set; } = string.Empty;
    /// <summary>是否为富文本</summary>
    public bool IsRichText { get; set; }
    /// <summary>文本装饰</summary>
    public string TextDecoration { get; set; } = string.Empty;
    /// <summary>样式引用</summary>
    public string StyleRef { get; set; } = string.Empty;
}

/// <summary>外部线条元素</summary>
public class ExternalLineElement : ReportExternalElementBase
{
    /// <summary>线条颜色</summary>
    public string LineColor { get; set; } = "#000000";
    /// <summary>线宽</summary>
    public double LineWidth { get; set; } = 1;
    /// <summary>线条样式</summary>
    public string LineStyle { get; set; } = string.Empty;
    /// <summary>起点X坐标</summary>
    public double StartX { get; set; }
    /// <summary>起点Y坐标</summary>
    public double StartY { get; set; }
    /// <summary>终点X坐标</summary>
    public double EndX { get; set; }
    /// <summary>终点Y坐标</summary>
    public double EndY { get; set; }
}

/// <summary>外部下拉选择元素</summary>
public class ExternalDropdownElement : ReportExternalElementBase
{
    /// <summary>当前选中值</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>占位提示文本</summary>
    public string Placeholder { get; set; } = string.Empty;
}

/// <summary>外部数字元素</summary>
public class ExternalNumberElement : ReportExternalElementBase
{
    /// <summary>数值</summary>
    public double Value { get; set; }
    /// <summary>数字格式</summary>
    public string Format { get; set; } = string.Empty;
    /// <summary>小数位数</summary>
    public int DecimalPlaces { get; set; } = 2;
    /// <summary>最小值</summary>
    public double? MinValue { get; set; }
    /// <summary>最大值</summary>
    public double? MaxValue { get; set; }
    /// <summary>单位</summary>
    public string Unit { get; set; } = string.Empty;
}

/// <summary>外部日期元素</summary>
public class ExternalDateElement : ReportExternalElementBase
{
    /// <summary>日期值</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>日期格式</summary>
    public string Format { get; set; } = "yyyy-MM-dd";
    /// <summary>最小日期</summary>
    public string MinDate { get; set; } = string.Empty;
    /// <summary>最大日期</summary>
    public string MaxDate { get; set; } = string.Empty;
}

/// <summary>外部表格元素</summary>
public class ExternalTableElement : ReportExternalElementBase
{
    /// <summary>行数</summary>
    public int Rows { get; set; } = 3;
    /// <summary>列数</summary>
    public int Columns { get; set; } = 4;
    /// <summary>单元格数据</summary>
    public List<List<string>> CellData { get; set; } = new();
    /// <summary>单元格内边距</summary>
    public double CellPadding { get; set; }
    /// <summary>表格边框宽度</summary>
    public double TableBorder { get; set; } = 1;
    /// <summary>是否包含表头</summary>
    public bool HasHeader { get; set; } = true;
    /// <summary>表头行数</summary>
    public int HeaderRows { get; set; } = 1;
    /// <summary>单元格定义集合</summary>
    public List<TableCellDefinition>? Cells { get; set; }
}

/// <summary>表格单元格定义</summary>
public class TableCellDefinition
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsEditable { get; set; }
    public string? DataPath { get; set; }
    public string? InputType { get; set; }
    public string? Text { get; set; }
    public List<string>? Options { get; set; }
}

/// <summary>外部图片元素</summary>
public class ExternalImageElement : ReportExternalElementBase
{
    /// <summary>图片源路径</summary>
    public string Src { get; set; } = string.Empty;

    /// <summary>图片适应方式</summary>
    [JsonProperty("stretch")]
    public string Fit { get; set; } = string.Empty;

    /// <summary>是否保持宽高比</summary>
    public bool MaintainAspectRatio { get; set; }

    /// <summary>替代文本</summary>
    public string AltText { get; set; } = string.Empty;
}

/// <summary>外部形状元素</summary>
public class ExternalShapeElement : ReportExternalElementBase
{
    /// <summary>形状类型</summary>
    public string ShapeType { get; set; } = string.Empty;
    /// <summary>填充颜色</summary>
    public string FillColor { get; set; } = string.Empty;
    /// <summary>描边颜色</summary>
    public string StrokeColor { get; set; } = string.Empty;
    /// <summary>描边宽度</summary>
    public double StrokeWidth { get; set; }
}

/// <summary>外部分割线元素</summary>
public class ExternalDividerElement : ReportExternalElementBase
{
    /// <summary>分割线粗细</summary>
    public double Thickness { get; set; }
    /// <summary>分割线颜色</summary>
    public string Color { get; set; } = string.Empty;
    /// <summary>分割线样式</summary>
    public string Style { get; set; } = string.Empty;
}

/// <summary>外部复选框元素</summary>
public class ExternalCheckboxElement : ReportExternalElementBase
{
    /// <summary>是否选中</summary>
    public bool Checked { get; set; }
    /// <summary>勾选颜色</summary>
    public string CheckColor { get; set; } = string.Empty;
}

/// <summary>外部单选按钮元素</summary>
public class ExternalRadioElement : ReportExternalElementBase
{
    /// <summary>单选组名称</summary>
    public string GroupName { get; set; } = string.Empty;
    /// <summary>单选值</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>是否选中</summary>
    public bool Checked { get; set; }
}

/// <summary>外部签名元素</summary>
public class ExternalSignatureElement : ReportExternalElementBase
{
    /// <summary>占位提示文本</summary>
    public string Placeholder { get; set; } = string.Empty;

    /// <summary>笔画颜色</summary>
    [JsonProperty("strokeColor")]
    public string LineColor { get; set; } = string.Empty;

    /// <summary>笔画宽度</summary>
    [JsonProperty("strokeWidth")]
    public double LineWidth { get; set; }
}

/// <summary>外部条形码元素</summary>
public class ExternalBarcodeElement : ReportExternalElementBase
{
    /// <summary>条形码值</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>条形码格式</summary>
    [JsonProperty("barcodeFormat")]
    public string Format { get; set; } = string.Empty;

    /// <summary>是否显示文本</summary>
    public bool ShowText { get; set; }
    /// <summary>线条颜色</summary>
    public string LineColor { get; set; } = string.Empty;
}

/// <summary>外部二维码元素</summary>
public class ExternalQrCodeElement : ReportExternalElementBase
{
    /// <summary>二维码值</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>纠错等级</summary>
    [JsonProperty("errorLevel")]
    public string ErrorCorrectionLevel { get; set; } = string.Empty;

    /// <summary>边距</summary>
    public double Margin { get; set; }
    /// <summary>颜色</summary>
    public string Color { get; set; } = string.Empty;
}

/// <summary>外部图表元素</summary>
public class ExternalChartElement : ReportExternalElementBase
{
    /// <summary>图表类型</summary>
    public string ChartType { get; set; } = string.Empty;
    /// <summary>图表标题</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>数据源</summary>
    public string DataSource { get; set; } = string.Empty;
    /// <summary>是否显示图例</summary>
    public bool ShowLegend { get; set; }
    /// <summary>是否显示网格</summary>
    public bool ShowGrid { get; set; }
}

/// <summary>外部容器元素</summary>
public class ExternalContainerElement : ReportExternalElementBase
{
    /// <summary>子元素列表</summary>
    public List<ReportExternalElementBase> Children { get; set; } = new();
    /// <summary>布局方式</summary>
    public string Layout { get; set; } = string.Empty;
    /// <summary>内边距</summary>
    public double Padding { get; set; }
    /// <summary>是否裁剪内容</summary>
    public bool ClipContent { get; set; }
}

/// <summary>外部重复元素</summary>
public class ExternalRepeatElement : ReportExternalElementBase
{
    /// <summary>数据源</summary>
    public string DataSource { get; set; } = string.Empty;
    /// <summary>项模板</summary>
    public string ItemTemplate { get; set; } = string.Empty;
    /// <summary>排列方向</summary>
    public string Direction { get; set; } = string.Empty;
    /// <summary>间距</summary>
    public double Gap { get; set; }
}

/// <summary>外部页眉元素</summary>
public class ExternalHeaderElement : ReportExternalElementBase
{
    /// <summary>子元素列表</summary>
    public List<ReportExternalElementBase> Children { get; set; } = new();
    /// <summary>是否在首页显示</summary>
    public bool ShowOnFirstPage { get; set; }
    /// <summary>是否在所有页面显示</summary>
    public bool ShowOnAllPages { get; set; }
}

/// <summary>外部页脚元素</summary>
public class ExternalFooterElement : ReportExternalElementBase
{
    /// <summary>子元素列表</summary>
    public List<ReportExternalElementBase> Children { get; set; } = new();
    /// <summary>是否在末页显示</summary>
    public bool ShowOnLastPage { get; set; }
    /// <summary>是否在所有页面显示</summary>
    public bool ShowOnAllPages { get; set; }
}

/// <summary>外部页码元素</summary>
public class ExternalPageNumberElement : ReportExternalElementBase
{
    /// <summary>页码格式</summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>起始页码</summary>
    [JsonProperty("startFrom")]
    public int StartPage { get; set; }
}

/// <summary>外部水印元素</summary>
public class ExternalWatermarkElement : ReportExternalElementBase
{
    /// <summary>水印文本</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>旋转角度</summary>
    public double Angle { get; set; }
    /// <summary>水印颜色</summary>
    public string Color { get; set; } = string.Empty;
    /// <summary>是否重复铺满</summary>
    public bool Repeat { get; set; }
}

/// <summary>外部图标元素</summary>
public class ExternalIconElement : ReportExternalElementBase
{
    /// <summary>图标名称</summary>
    public string IconName { get; set; } = string.Empty;
    /// <summary>图标集名称</summary>
    public string IconSet { get; set; } = string.Empty;

    /// <summary>图标颜色</summary>
    [JsonProperty("iconColor")]
    public string Color { get; set; } = string.Empty;

    /// <summary>图标大小</summary>
    [JsonProperty("iconSize")]
    public double Size { get; set; }
}

/// <summary>外部超链接元素</summary>
public class ExternalHyperlinkElement : ReportExternalElementBase
{
    /// <summary>链接显示文本</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>链接地址</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>是否在新标签页打开</summary>
    public bool OpenInNewTab { get; set; }
}
