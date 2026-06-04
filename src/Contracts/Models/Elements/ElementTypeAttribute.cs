namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 标注元素类型的简短名称，用于 JSON 序列化/反序列化时的类型映射。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ElementTypeAttribute : Attribute
{
    public string ShortName { get; }

    public ElementTypeAttribute(string shortName)
    {
        ShortName = shortName;
    }
}
