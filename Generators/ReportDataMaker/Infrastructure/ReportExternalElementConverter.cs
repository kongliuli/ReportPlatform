using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Infrastructure;

public class ReportExternalElementConverter : JsonConverter<ReportExternalElementBase>
{
    private static readonly Dictionary<string, Type> TypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] = typeof(ExternalTextElement),
        ["line"] = typeof(ExternalLineElement),
        ["table"] = typeof(ExternalTableElement),
        ["number"] = typeof(ExternalNumberElement),
        ["date"] = typeof(ExternalDateElement),
        ["dropdown"] = typeof(ExternalDropdownElement),
        ["checkbox"] = typeof(ExternalCheckboxElement),
        ["radio"] = typeof(ExternalRadioElement),
        ["image"] = typeof(ExternalImageElement),
        ["shape"] = typeof(ExternalShapeElement),
        ["divider"] = typeof(ExternalDividerElement),
        ["barcode"] = typeof(ExternalBarcodeElement),
        ["qrcode"] = typeof(ExternalQrCodeElement),
        ["signature"] = typeof(ExternalSignatureElement),
        ["container"] = typeof(ExternalContainerElement),
        ["repeat"] = typeof(ExternalRepeatElement),
        ["header"] = typeof(ExternalHeaderElement),
        ["footer"] = typeof(ExternalFooterElement),
        ["pagenumber"] = typeof(ExternalPageNumberElement),
        ["watermark"] = typeof(ExternalWatermarkElement),
        ["icon"] = typeof(ExternalIconElement),
        ["hyperlink"] = typeof(ExternalHyperlinkElement),
        ["chart"] = typeof(ExternalChartElement)
    };

    private static readonly Dictionary<Type, string> ReverseTypeMap =
        TypeMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    private static readonly JsonSerializerSettings ElementSerializerSettings = new()
    {
        ContractResolver = new ReportElementContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Populate
    };

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override ReportExternalElementBase? ReadJson(JsonReader reader, Type objectType,
        ReportExternalElementBase? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeString = jsonObject["$type"]?.ToString();
        var elementType = MapType(typeString);

        var element = (ReportExternalElementBase?)jsonObject.ToObject(elementType, JsonSerializer.Create(ElementSerializerSettings));

        if (element != null)
        {
            element.ElementType = typeString;
            element.Group = ClassifyElement(element);

            ProcessNestedElements(jsonObject, element);
        }

        return element;
    }

    private void ProcessNestedElements(JObject jsonObject, ReportExternalElementBase element)
    {
        if (element is ExternalContainerElement container)
        {
            container.Children = DeserializeElements(jsonObject["children"]);
        }
        else if (element is ExternalHeaderElement header)
        {
            header.Children = DeserializeElements(jsonObject["children"]);
        }
        else if (element is ExternalFooterElement footer)
        {
            footer.Children = DeserializeElements(jsonObject["children"]);
        }
    }

    private List<ReportExternalElementBase> DeserializeElements(JToken? token)
    {
        var elements = new List<ReportExternalElementBase>();
        
        if (token is JArray array)
        {
            var converter = new ReportExternalElementConverter();
            
            foreach (var item in array)
            {
                if (item is JObject itemObject)
                {
                    using var reader = itemObject.CreateReader();
                    var element = converter.ReadJson(reader, typeof(ReportExternalElementBase), null, false, null!);
                    if (element != null)
                    {
                        elements.Add(element);
                    }
                }
            }
        }
        
        return elements;
    }

    public override void WriteJson(JsonWriter writer, ReportExternalElementBase? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        var baseSerializer = JsonSerializer.Create(ElementSerializerSettings);
        
        var jObject = JObject.FromObject(value, baseSerializer);
        var shortType = ReverseTypeMap.GetValueOrDefault(value.GetType(), "text");
        jObject["$type"] = $"template.element.{shortType}";
        jObject.WriteTo(writer);
    }

    private static Type MapType(string? typeString)
    {
        if (string.IsNullOrEmpty(typeString))
            return typeof(ExternalTextElement);

        if (typeString.StartsWith("template.element.", StringComparison.OrdinalIgnoreCase))
        {
            var shortName = typeString["template.element.".Length..];
            if (TypeMap.TryGetValue(shortName, out var type))
                return type;
        }

        return typeof(ExternalTextElement);
    }

    private static ElementGroup ClassifyElement(ReportExternalElementBase element)
    {
        if (element.DataPath?.StartsWith("Context.", StringComparison.OrdinalIgnoreCase) == true)
            return ElementGroup.Context;

        if (!element.IsDataBound)
            return ElementGroup.Fixed;

        if (!string.IsNullOrEmpty(element.AdapterId))
            return ElementGroup.DataAdapter;

        if (element is ExternalTableElement or ExternalRepeatElement or ExternalChartElement)
            return ElementGroup.DataAdapter;

        return ElementGroup.Editable;
    }
}

internal class ReportElementContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
{
    protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);
        
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var filteredProperties = new List<Newtonsoft.Json.Serialization.JsonProperty>();
        
        foreach (var prop in properties)
        {
            if (seenNames.Add(prop.PropertyName!))
            {
                filteredProperties.Add(prop);
            }
        }
        
        return filteredProperties;
    }
}
