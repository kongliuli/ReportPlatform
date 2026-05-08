using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.Services
{
    public class ExternalElementConverter : JsonConverter
    {
        private static readonly JsonSerializerSettings CamelSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly JsonSerializerSettings PascalSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private bool _isOldFormat;

        public ExternalElementConverter(bool isOldFormat = false)
        {
            _isOldFormat = isOldFormat;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ExternalElementBase) || objectType.IsSubclassOf(typeof(ExternalElementBase))
                || objectType == typeof(ReportExternalElementBase) || objectType.IsSubclassOf(typeof(ReportExternalElementBase));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            Type targetType;
            JsonSerializerSettings settings;

            var typeToken = jo["$type"];
            if (typeToken != null)
            {
                targetType = MapNewType(typeToken.ToString());
                settings = CamelSettings;
            }
            else
            {
                var oldTypeToken = jo["Type"] ?? jo["type"];
                if (oldTypeToken != null)
                {
                    targetType = MapOldType(oldTypeToken.ToString());
                }
                else
                {
                    targetType = typeof(ExternalTextElement);
                }
                settings = PascalSettings;
            }

            var result = JsonConvert.DeserializeObject(jo.ToString(), targetType, settings);

            if (typeToken == null && result is ReportExternalElementBase baseEl)
            {
                ConvertOldFormatProperties(baseEl, jo);
            }

            RecursivelyConvertChildren(result, jo);

            return result;
        }

        private void ConvertOldFormatProperties(ReportExternalElementBase element, JObject jo)
        {
            var dbp = jo["DataBindingPath"];
            if (dbp != null && !string.IsNullOrEmpty(dbp.ToString()))
            {
                element.DataPath = dbp.ToString();
            }

            var idbp = jo["InputDataBindingPath"];
            if (idbp != null && !string.IsNullOrEmpty(idbp.ToString()))
            {
                element.DataPath = idbp.ToString();
            }

            if (element is ExternalTextElement textEl)
            {
                var labelText = jo["LabelText"]?.ToString() ?? "";
                var placeholder = jo["InputPlaceholder"]?.ToString() ?? "";
                var inputValue = jo["InputText"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(textEl.Text))
                {
                    if (!string.IsNullOrEmpty(inputValue))
                        textEl.Text = inputValue;
                    else if (!string.IsNullOrEmpty(labelText))
                        textEl.Text = labelText;
                    else if (!string.IsNullOrEmpty(placeholder))
                        textEl.Text = $"[{placeholder}]";
                }

                if (!string.IsNullOrEmpty(labelText) && string.IsNullOrEmpty(element.Label))
                {
                    element.Label = labelText;
                }
            }

            if (element is ExternalTableElement tableEl)
            {
                var cells = jo["Cells"];
                if (cells is JArray cellsArray && (tableEl.CellData == null || tableEl.CellData.Count == 0))
                {
                    tableEl.CellData = new List<List<string>>();
                    foreach (var cellToken in cellsArray)
                    {
                        if (cellToken is JObject cellObj)
                        {
                            var rowIdx = cellObj["RowIndex"]?.Value<int>() ?? 0;
                            var colIdx = cellObj["ColumnIndex"]?.Value<int>() ?? 0;
                            var content = cellObj["Content"]?.ToString() ?? "";

                            while (tableEl.CellData.Count <= rowIdx)
                                tableEl.CellData.Add(new List<string>());
                            while (tableEl.CellData[rowIdx].Count <= colIdx)
                                tableEl.CellData[rowIdx].Add("");
                            tableEl.CellData[rowIdx][colIdx] = content;
                        }
                    }
                }

                if (tableEl.Rows == 0) tableEl.Rows = tableEl.CellData?.Count ?? 0;
                if (tableEl.Columns == 0 && tableEl.CellData?.Count > 0)
                    tableEl.Columns = tableEl.CellData[0].Count;
            }
        }

        private void RecursivelyConvertChildren(object element, JObject jo)
        {
            if (element is ExternalContainerElement containerEl && jo["Children"] is JArray childrenArray)
            {
                containerEl.Children = ConvertChildrenArray(childrenArray);
            }
            else if (element is ExternalHeaderElement headerEl && jo["Children"] is JArray headerChildren)
            {
                headerEl.Children = ConvertChildrenArray(headerChildren);
            }
            else if (element is ExternalFooterElement footerEl && jo["Children"] is JArray footerChildren)
            {
                footerEl.Children = ConvertChildrenArray(footerChildren);
            }
        }

        private List<ReportExternalElementBase> ConvertChildrenArray(JArray childrenArray)
        {
            var children = new List<ReportExternalElementBase>();
            var converter = new ExternalElementConverter(_isOldFormat);

            foreach (var childToken in childrenArray)
            {
                if (childToken is JObject childObj)
                {
                    var child = converter.ReadJson(
                        childObj.CreateReader(),
                        typeof(ReportExternalElementBase),
                        null,
                        JsonSerializer.CreateDefault()) as ReportExternalElementBase;

                    if (child != null)
                        children.Add(child);
                }
            }

            return children;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static Type MapNewType(string typeString)
        {
            if (typeString.StartsWith("template.element."))
            {
                var shortName = typeString["template.element.".Length..];
                return shortName switch
                {
                    "text" => typeof(ExternalTextElement),
                    "image" => typeof(ExternalImageElement),
                    "line" => typeof(ExternalLineElement),
                    "table" => typeof(ExternalTableElement),
                    "date" => typeof(ExternalDateElement),
                    "dropdown" => typeof(ExternalDropdownElement),
                    "number" => typeof(ExternalNumberElement),
                    "shape" => typeof(ExternalShapeElement),
                    "divider" => typeof(ExternalDividerElement),
                    "checkbox" => typeof(ExternalCheckboxElement),
                    "radio" => typeof(ExternalRadioElement),
                    "signature" => typeof(ExternalSignatureElement),
                    "barcode" => typeof(ExternalBarcodeElement),
                    "qrcode" => typeof(ExternalQrCodeElement),
                    "chart" => typeof(ExternalChartElement),
                    "container" => typeof(ExternalContainerElement),
                    "repeat" => typeof(ExternalRepeatElement),
                    "header" => typeof(ExternalHeaderElement),
                    "footer" => typeof(ExternalFooterElement),
                    "pageNumber" => typeof(ExternalPageNumberElement),
                    "pagenumber" => typeof(ExternalPageNumberElement),
                    "watermark" => typeof(ExternalWatermarkElement),
                    "icon" => typeof(ExternalIconElement),
                    "hyperlink" => typeof(ExternalHyperlinkElement),
                    _ => typeof(ExternalTextElement)
                };
            }

            var typeName = typeString.Split(',')[0].Trim();
            var simpleTypeName = typeName.Split('.')[^1];

            return simpleTypeName switch
            {
                "TextElement" or "LabelElement" or "LabelInputBoxElement" => typeof(ExternalTextElement),
                "ImageElement" => typeof(ExternalImageElement),
                "LineElement" => typeof(ExternalLineElement),
                "TableElement" => typeof(ExternalTableElement),
                "DateElement" => typeof(ExternalDateElement),
                "DropdownElement" => typeof(ExternalDropdownElement),
                "NumberElement" => typeof(ExternalNumberElement),
                "RectangleElement" or "EllipseElement" => typeof(ExternalShapeElement),
                "BarcodeElement" => typeof(ExternalBarcodeElement),
                "SignatureElement" => typeof(ExternalSignatureElement),
                "AutoNumberElement" => typeof(ExternalPageNumberElement),
                _ => typeof(ExternalTextElement)
            };
        }

        private static Type MapOldType(string typeString)
        {
            return typeString.ToLower() switch
            {
                "text" => typeof(ExternalTextElement),
                "labelinputbox" or "label" => typeof(ExternalTextElement),
                "line" => typeof(ExternalLineElement),
                "table" => typeof(ExternalTableElement),
                "image" => typeof(ExternalImageElement),
                "date" => typeof(ExternalDateElement),
                "dropdown" => typeof(ExternalDropdownElement),
                "number" => typeof(ExternalNumberElement),
                "shape" or "rectangle" or "ellipse" => typeof(ExternalShapeElement),
                "checkbox" => typeof(ExternalCheckboxElement),
                "radio" => typeof(ExternalRadioElement),
                "signature" => typeof(ExternalSignatureElement),
                "barcode" => typeof(ExternalBarcodeElement),
                "qrcode" => typeof(ExternalQrCodeElement),
                "chart" => typeof(ExternalChartElement),
                "container" => typeof(ExternalContainerElement),
                "repeat" => typeof(ExternalRepeatElement),
                "header" => typeof(ExternalHeaderElement),
                "footer" => typeof(ExternalFooterElement),
                "pagenumber" => typeof(ExternalPageNumberElement),
                "watermark" => typeof(ExternalWatermarkElement),
                "icon" => typeof(ExternalIconElement),
                "hyperlink" => typeof(ExternalHyperlinkElement),
                _ => typeof(ExternalTextElement)
            };
        }
    }
}
