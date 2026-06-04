using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xinglin.ReportEditor.Contracts.Converters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Contracts;

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

    public static TemplateDefinition Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<TemplateDefinition>(json, Settings)
            ?? throw new InvalidOperationException("Failed to deserialize template");
    }

    public static string Serialize(TemplateDefinition template)
    {
        return JsonConvert.SerializeObject(template, Settings);
    }

    public static List<ElementBase> DeserializeElements(string json)
    {
        var wrapper = Deserialize(json);
        return wrapper.Elements;
    }

    public static string SerializeElements(IEnumerable<ElementBase> elements)
    {
        var template = new TemplateDefinition { Elements = elements.ToList() };
        return Serialize(template);
    }
}
