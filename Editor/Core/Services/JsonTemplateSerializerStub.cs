using Newtonsoft.Json;
using Xinglin.WebReportEditor.Core.SharedInterfaces;

namespace Xinglin.WebReportEditor.Core.Services;

public class JsonTemplateSerializerStub : IJsonTemplateSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
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
