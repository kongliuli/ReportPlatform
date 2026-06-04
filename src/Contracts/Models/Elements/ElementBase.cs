namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>元素基类，定义所有元素的公共属性</summary>
public abstract class ElementBase
{
    private string? _id;

    /// <summary>元素唯一标识</summary>
    public string Id
    {
        get => _id ??= GenerateShortId();
        set => _id = value;
    }

    private static string GenerateShortId() => IdGenerator.NewId();

    /// <summary>元素X坐标</summary>
    public double X { get; set; }

    /// <summary>元素Y坐标</summary>
    public double Y { get; set; }

    /// <summary>元素宽度</summary>
    public double Width { get; set; }

    /// <summary>元素高度</summary>
    public double Height { get; set; }

    /// <summary>元素旋转角度</summary>
    public double Rotation { get; set; }

    /// <summary>元素层级</summary>
    public int ZIndex { get; set; }

    /// <summary>元素提示文本</summary>
    public string? Tooltip { get; set; }

    /// <summary>元素是否锁定</summary>
    public bool IsLocked { get; set; }

    /// <summary>元素是否可见</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>背景颜色</summary>
    public string? BackgroundColor { get; set; }

    /// <summary>边框颜色</summary>
    public string? BorderColor { get; set; }

    /// <summary>边框宽度</summary>
    public double? BorderWidth { get; set; }

    /// <summary>边框样式</summary>
    public string? BorderStyle { get; set; } = "solid";

    /// <summary>圆角半径</summary>
    public double? CornerRadius { get; set; }

    /// <summary>透明度</summary>
    public double? Opacity { get; set; } = 1.0;

    /// <summary>前景色（文字颜色）</summary>
    public string? ForegroundColor { get; set; } = "#000000";

    /// <summary>字体族</summary>
    public string? FontFamily { get; set; }

    /// <summary>字体大小</summary>
    public double? FontSize { get; set; } = 12;

    /// <summary>字体粗细</summary>
    public string? FontWeight { get; set; } = "normal";

    /// <summary>字体样式</summary>
    public string? FontStyle { get; set; } = "normal";

    /// <summary>文本对齐方式</summary>
    public string? TextAlignment { get; set; } = "left";

    /// <summary>标签文本</summary>
    public string? Label { get; set; }

    /// <summary>数据绑定路径</summary>
    public string? DataPath { get; set; }

    /// <summary>格式化字符串</summary>
    public string? FormatString { get; set; }
}
