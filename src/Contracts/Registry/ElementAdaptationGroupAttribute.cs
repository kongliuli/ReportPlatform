using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Registry;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ElementAdaptationGroupAttribute : Attribute
{
    public ElementAdaptationGroup Group { get; }
    public ElementAdaptationGroupAttribute(ElementAdaptationGroup group) => Group = group;
}
