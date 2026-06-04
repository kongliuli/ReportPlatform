using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Converters;

public class ElementJsonConverter : JsonConverter<ElementBase>
{
    private static readonly Dictionary<string, Type> WebShortTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] = typeof(TextElement),
        ["line"] = typeof(LineElement),
        ["table"] = typeof(TableElement),
        ["number"] = typeof(NumberElement),
        ["date"] = typeof(DateElement),
        ["dropdown"] = typeof(DropdownElement),
        ["checkbox"] = typeof(CheckboxElement),
        ["radio"] = typeof(RadioElement),
        ["image"] = typeof(ImageElement),
        ["shape"] = typeof(ShapeElement),
        ["divider"] = typeof(DividerElement),
        ["barcode"] = typeof(BarcodeElement),
        ["qrcode"] = typeof(QrCodeElement),
        ["signature"] = typeof(SignatureElement),
        ["container"] = typeof(ContainerElement),
        ["repeat"] = typeof(RepeatElement),
        ["header"] = typeof(HeaderElement),
        ["footer"] = typeof(FooterElement),
        ["pageNumber"] = typeof(PageNumberElement),
        ["watermark"] = typeof(WatermarkElement),
        ["icon"] = typeof(IconElement),
        ["hyperlink"] = typeof(HyperlinkElement),
        ["chart"] = typeof(ChartElement)
    };

    private static readonly Dictionary<Type, string> ReverseTypeMap =
        WebShortTypeMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public override ElementBase? ReadJson(JsonReader reader, Type objectType,
        ElementBase? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeString = jsonObject["$type"]?.ToString() ?? jsonObject["Type"]?.ToString();

        var elementType = MapType(typeString);
        var element = (ElementBase?)jsonObject.ToObject(elementType, serializer);

        if (element is ExternalElementBase extElement)
        {
            extElement.Group = ClassifyElement(extElement);
        }

        return element;
    }

    public override void WriteJson(JsonWriter writer, ElementBase? value, JsonSerializer serializer)
    {
        var jObject = JObject.FromObject(value, serializer);
        var shortType = ReverseTypeMap.GetValueOrDefault(value.GetType(), "text");
        jObject["$type"] = $"template.element.{shortType}";
        jObject.WriteTo(writer);
    }

    private static Type MapType(string? typeString)
    {
        if (string.IsNullOrEmpty(typeString))
            return typeof(TextElement);

        if (typeString.StartsWith("template.element.", StringComparison.OrdinalIgnoreCase))
        {
            var shortName = typeString["template.element.".Length..];
            if (WebShortTypeMap.TryGetValue(shortName, out var type))
                return type;
        }

        var parts = typeString.Split(',')[0].Trim();
        var simpleName = parts.Split('.').Last();

        return simpleName switch
        {
            "TextElement" or "LabelElement" or "LabelInputBoxElement" => typeof(TextElement),
            "LineElement" => typeof(LineElement),
            "TableElement" => typeof(TableElement),
            "NumberElement" => typeof(NumberElement),
            "DateElement" => typeof(DateElement),
            "DropdownElement" => typeof(DropdownElement),
            "ImageElement" => typeof(ImageElement),
            "RectangleElement" or "EllipseElement" => typeof(ShapeElement),
            "BarcodeElement" => typeof(BarcodeElement),
            "QrCodeElement" => typeof(QrCodeElement),
            "SignatureElement" => typeof(SignatureElement),
            "CheckboxElement" => typeof(CheckboxElement),
            "RadioElement" => typeof(RadioElement),
            "AutoNumberElement" => typeof(PageNumberElement),
            "DividerElement" => typeof(DividerElement),
            "IconElement" => typeof(IconElement),
            "HyperlinkElement" => typeof(HyperlinkElement),
            "ChartElement" => typeof(ChartElement),
            "ContainerElement" => typeof(ContainerElement),
            "RepeatElement" => typeof(RepeatElement),
            "HeaderElement" => typeof(HeaderElement),
            "FooterElement" => typeof(FooterElement),
            "WatermarkElement" => typeof(WatermarkElement),
            _ => typeof(TextElement)
        };
    }

    private static ElementGroup ClassifyElement(ExternalElementBase element)
    {
        if (element.DataPath?.StartsWith("Context.", StringComparison.OrdinalIgnoreCase) == true)
            return ElementGroup.Context;

        if (!element.IsDataBound)
            return ElementGroup.Fixed;

        if (!string.IsNullOrEmpty(element.AdapterId))
            return ElementGroup.DataAdapter;

        if (element is TableElement or RepeatElement or ChartElement)
            return ElementGroup.DataAdapter;

        return ElementGroup.Editable;
    }
}
