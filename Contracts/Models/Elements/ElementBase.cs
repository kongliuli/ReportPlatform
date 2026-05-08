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
}
