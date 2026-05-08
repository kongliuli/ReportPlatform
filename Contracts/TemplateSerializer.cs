using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xinglin.ReportEditor.Contracts.Converters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Contracts;

/// <summary>模板序列化器，提供模板的JSON序列化与反序列化功能</summary>
public static class TemplateSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = new JsonConverter[]
        {
            new ElementJsonConverter(),
            new StringEnumConverter()
        }
    };

    /// <summary>从JSON字符串反序列化模板定义</summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>模板定义对象</returns>
    public static TemplateDefinition Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<TemplateDefinition>(json, Settings)
            ?? throw new InvalidOperationException("Failed to deserialize template");
    }

    /// <summary>将模板定义序列化为JSON字符串</summary>
    /// <param name="template">模板定义对象</param>
    /// <returns>JSON字符串</returns>
    public static string Serialize(TemplateDefinition template)
    {
        return JsonConvert.SerializeObject(template, Settings);
    }

    /// <summary>从JSON字符串反序列化元素列表</summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>元素列表</returns>
    public static List<ExternalElementBase> DeserializeElements(string json)
    {
        var wrapper = Deserialize(json);
        return wrapper.Elements;
    }

    /// <summary>将元素列表序列化为JSON字符串</summary>
    /// <param name="elements">元素列表</param>
    /// <returns>JSON字符串</returns>
    public static string SerializeElements(IEnumerable<ExternalElementBase> elements)
    {
        var template = new TemplateDefinition { Elements = elements.ToList() };
        return Serialize(template);
    }
}
