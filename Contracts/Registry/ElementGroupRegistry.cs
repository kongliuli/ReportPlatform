using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Registry;

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

    public static ElementAdaptationGroup GetGroup(Type elementType)
    {
        foreach (var kvp in GroupElements)
        {
            if (kvp.Value.Contains(elementType))
                return kvp.Key;
        }
        return ElementAdaptationGroup.Basic;
    }

    public static IEnumerable<Type> GetElementsInGroup(ElementAdaptationGroup group)
    {
        return GroupElements.GetValueOrDefault(group, new HashSet<Type>());
    }

    public static IEnumerable<Type> GetAllElementTypes()
    {
        return GroupElements.Values.SelectMany(x => x);
    }

    public static IEnumerable<ElementAdaptationGroup> GetAllGroups()
    {
        return Enum.GetValues<ElementAdaptationGroup>();
    }
}
