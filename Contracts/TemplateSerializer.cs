using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xinglin.ReportEditor.Contracts.Converters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Contracts;

/// <summary>
/// 模板序列化器，提供统一的 JSON 序列化/反序列化能力
/// </summary>
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

    /// <summary>
    /// 反序列化为 TemplateDefinition
    /// </summary>
    public static TemplateDefinition Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<TemplateDefinition>(json, Settings) 
            ?? throw new InvalidOperationException("Failed to deserialize template");
    }

    /// <summary>
    /// 序列化 TemplateDefinition 为 JSON
    /// </summary>
    public static string Serialize(TemplateDefinition template)
    {
        return JsonConvert.SerializeObject(template, Settings);
    }

    /// <summary>
    /// 反序列化为元素列表
    /// </summary>
    public static List<ExternalElementBase> DeserializeElements(string json)
    {
        var wrapper = Deserialize(json);
        return wrapper.Elements;
    }

    /// <summary>
    /// 序列化元素列表
    /// </summary>
    public static string SerializeElements(IEnumerable<ExternalElementBase> elements)
    {
        var template = new TemplateDefinition { Elements = elements.ToList() };
        return Serialize(template);
    }
}
