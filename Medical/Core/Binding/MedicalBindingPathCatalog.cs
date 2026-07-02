using System.Reflection;
using Xinglin.Medical.Models;

namespace Xinglin.Medical.Binding;

public static class MedicalBindingPathCatalog
{
    public static IReadOnlyList<string> GetLabReportPaths() => GetPaths(typeof(LabReportData));

    public static bool IsValidLabReportPath(string path) =>
        GetLabReportPaths().Contains(path, StringComparer.OrdinalIgnoreCase);

    private static List<string> GetPaths(Type type, string prefix = "")
    {
        var paths = new List<string>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var path = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
            if (IsSimple(property.PropertyType))
            {
                paths.Add(path);
            }
            else if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                paths.AddRange(GetPaths(property.PropertyType, path));
            }
        }
        return paths;
    }

    private static bool IsSimple(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(Guid);
    }
}
