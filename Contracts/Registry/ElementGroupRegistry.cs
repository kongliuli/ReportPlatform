using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Registry;

/// <summary>元素分组注册表，管理元素类型与适配分组的映射关系</summary>
public static class ElementGroupRegistry
{
    private static readonly Dictionary<ElementAdaptationGroup, HashSet<Type>> GroupElements = new();

    static ElementGroupRegistry()
    {
        RegisterGroup(ElementAdaptationGroup.Basic,
            typeof(LineElement), typeof(DividerElement), typeof(ShapeElement));

        RegisterGroup(ElementAdaptationGroup.Form,
            typeof(TextElement), typeof(NumberElement), typeof(DateElement),
            typeof(DropdownElement), typeof(CheckboxElement), typeof(RadioElement));

        RegisterGroup(ElementAdaptationGroup.Data,
            typeof(TableElement), typeof(RepeatElement), typeof(ChartElement));

        RegisterGroup(ElementAdaptationGroup.Advanced,
            typeof(ImageElement), typeof(BarcodeElement), typeof(QrCodeElement),
            typeof(SignatureElement), typeof(HyperlinkElement), typeof(IconElement),
            typeof(ContainerElement), typeof(HeaderElement), typeof(FooterElement),
            typeof(PageNumberElement), typeof(WatermarkElement));
    }

    private static void RegisterGroup(ElementAdaptationGroup group, params Type[] types)
    {
        GroupElements[group] = new HashSet<Type>(types);
    }

    /// <summary>根据元素类型获取所属适配分组</summary>
    /// <param name="elementType">元素类型</param>
    /// <returns>元素所属的适配分组</returns>
    public static ElementAdaptationGroup GetGroup(Type elementType)
    {
        foreach (var kvp in GroupElements)
        {
            if (kvp.Value.Contains(elementType))
                return kvp.Key;
        }
        return ElementAdaptationGroup.Basic;
    }

    /// <summary>获取指定分组中的所有元素类型</summary>
    /// <param name="group">适配分组</param>
    /// <returns>分组中的元素类型集合</returns>
    public static IEnumerable<Type> GetElementsInGroup(ElementAdaptationGroup group)
    {
        return GroupElements.GetValueOrDefault(group, new HashSet<Type>());
    }

    /// <summary>获取所有已注册的元素类型</summary>
    /// <returns>所有元素类型的集合</returns>
    public static IEnumerable<Type> GetAllElementTypes()
    {
        return GroupElements.Values.SelectMany(x => x);
    }

    /// <summary>获取所有适配分组</summary>
    /// <returns>所有适配分组的集合</returns>
    public static IEnumerable<ElementAdaptationGroup> GetAllGroups()
    {
        return Enum.GetValues<ElementAdaptationGroup>();
    }
}
