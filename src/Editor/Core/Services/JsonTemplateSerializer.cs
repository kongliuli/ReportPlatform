using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xinglin.ReportEditor.Contracts.Converters;
using Xinglin.ReportEditor.Core.SharedInterfaces;

namespace Xinglin.ReportEditor.Core.Services;

/// <summary>
/// JSON模板序列化器，使用与 Contracts.TemplateSerializer 相同的序列化设置
/// </summary>
public class JsonTemplateSerializer : IJsonTemplateSerializer
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

    public string Serialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, Settings);
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, Settings)!;
    }

    public object Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type, Settings)!;
    }
}
