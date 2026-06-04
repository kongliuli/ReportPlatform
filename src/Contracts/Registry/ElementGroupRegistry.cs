using System.Reflection;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Registry;

public static class ElementGroupRegistry
{
    private static readonly Dictionary<ElementAdaptationGroup, HashSet<Type>> GroupElements = new();
    private static bool _initialized;
    private static readonly object _lock = new();

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<ElementAdaptationGroupAttribute>();
                if (attr != null && typeof(ElementBase).IsAssignableFrom(type))
                {
                    if (!GroupElements.ContainsKey(attr.Group))
                        GroupElements[attr.Group] = new HashSet<Type>();
                    GroupElements[attr.Group].Add(type);
                }
            }
            _initialized = true;
        }
    }

    public static ElementAdaptationGroup GetGroup(Type elementType)
    {
        EnsureInitialized();
        foreach (var kvp in GroupElements)
        {
            if (kvp.Value.Contains(elementType))
                return kvp.Key;
        }
        return ElementAdaptationGroup.Basic;
    }

    public static IEnumerable<Type> GetElementsInGroup(ElementAdaptationGroup group)
    {
        EnsureInitialized();
        return GroupElements.GetValueOrDefault(group, new HashSet<Type>());
    }

    public static IEnumerable<Type> GetAllElementTypes()
    {
        EnsureInitialized();
        return GroupElements.Values.SelectMany(x => x);
    }

    public static IEnumerable<ElementAdaptationGroup> GetAllGroups()
    {
        return Enum.GetValues<ElementAdaptationGroup>();
    }
}
