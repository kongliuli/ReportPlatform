using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Registry;

/// <summary>
/// 元素分组注册表，提供元素类型到分组的映射
/// </summary>
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

    /// <summary>
    /// 获取元素类型对应的适配分组
    /// </summary>
    public static ElementAdaptationGroup GetGroup(Type elementType)
    {
        foreach (var kvp in GroupElements)
        {
            if (kvp.Value.Contains(elementType))
                return kvp.Key;
        }
        return ElementAdaptationGroup.Basic;
    }

    /// <summary>
    /// 获取指定分组中的所有元素类型
    /// </summary>
    public static IEnumerable<Type> GetElementsInGroup(ElementAdaptationGroup group)
    {
        return GroupElements.GetValueOrDefault(group, new HashSet<Type>());
    }

    /// <summary>
    /// 获取所有元素类型
    /// </summary>
    public static IEnumerable<Type> GetAllElementTypes()
    {
        return GroupElements.Values.SelectMany(x => x);
    }

    /// <summary>
    /// 获取所有适配分组
    /// </summary>
    public static IEnumerable<ElementAdaptationGroup> GetAllGroups()
    {
        return Enum.GetValues<ElementAdaptationGroup>();
    }
}
