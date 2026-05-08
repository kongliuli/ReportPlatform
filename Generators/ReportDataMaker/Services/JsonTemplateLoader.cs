using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    /// <summary>
    /// 模板加载结果
    /// </summary>
    public class TemplateLoadResult
    {
        public ExternalTemplateDefinition Template { get; set; }
        public int TotalElements { get; set; }
        public int FixedCount { get; set; }
        public int EditableCount { get; set; }
        public int DataAdapterCount { get; set; }
        public List<ElementStats> ElementTypeStats { get; set; } = new List<ElementStats>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ElementStats
    {
        public string TypeName { get; set; }
        public string Group { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// JSON 模板加载服务 - 支持新旧两种模板格式
    /// </summary>
    public class JsonTemplateLoader
    {
        /// <summary>
        /// 从文件加载模板（返回结果含统计信息）
        /// </summary>
        public TemplateLoadResult LoadFromFileWithStats(string filePath)
        {
            var result = new TemplateLoadResult();

            if (!File.Exists(filePath))
            {
                result.Errors.Add($"模板文件不存在: {filePath}");
                return result;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var jObject = JObject.Parse(json);

                // 检测模板格式
                bool isOldFormat = IsOldFormat(jObject);

                // 使用对应的设置反序列化
                var settings = isOldFormat
                    ? new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Converters = new JsonConverter[] { new ExternalElementConverter(isOldFormat: true) }
                    }
                    : new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None,
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new JsonConverter[] { new ExternalElementConverter(isOldFormat: false) }
                    };

                result.Template = JsonConvert.DeserializeObject<ExternalTemplateDefinition>(json, settings);

                if (result.Template == null)
                {
                    result.Errors.Add("模板解析失败：结果为 null");
                    return result;
                }

                if (result.Template.Elements == null)
                {
                    result.Template.Elements = new List<ExternalElementBase>();
                }

                // 分类并统计
                ClassifyElements(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"加载异常: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 检测是否为旧格式模板（元素用 "Type" 而非 "$type"）
        /// </summary>
        private static bool IsOldFormat(JObject jObject)
        {
            // 检查根对象是否有 PascalCase 的 "Elements" 或 "Name" 字段
            if (jObject["Name"] != null && jObject["Elements"] != null)
                return true;

            // 检查第一个元素是否有 "Type" (旧格式) 或 "$type" (新格式)
            var elements = jObject["elements"] ?? jObject["Elements"];
            if (elements is JArray arr && arr.Count > 0 && arr[0] is JObject firstEl)
            {
                if (firstEl["Type"] != null && firstEl["$type"] == null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 从文件加载模板
        /// </summary>
        public ExternalTemplateDefinition LoadFromFile(string filePath)
        {
            var result = LoadFromFileWithStats(filePath);

            if (result.Errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors));
            }

            return result.Template;
        }

        /// <summary>
        /// 从 JSON 字符串加载模板
        /// </summary>
        public ExternalTemplateDefinition LoadFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            bool isOldFormat = IsOldFormat(jObject);

            var settings = isOldFormat
                ? new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new JsonConverter[] { new ExternalElementConverter(isOldFormat: true) }
                }
                : new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new JsonConverter[] { new ExternalElementConverter(isOldFormat: false) }
                };

            var template = JsonConvert.DeserializeObject<ExternalTemplateDefinition>(json, settings);

            if (template == null)
                throw new InvalidOperationException("模板解析失败");

            if (template.Elements == null)
                template.Elements = new List<ExternalElementBase>();

            var result = new TemplateLoadResult { Template = template };
            ClassifyElements(result);

            return template;
        }

        private HashSet<string> _adapterPaths;

        /// <summary>
        /// 设置适配器配置路径（区分 DataAdapter 和 Editable）
        /// </summary>
        public void SetAdapterConfigPaths(HashSet<string> paths)
        {
            _adapterPaths = paths;
        }

        /// <summary>
        /// 分类元素并填充统计信息
        /// </summary>
        private void ClassifyElements(TemplateLoadResult result)
        {
            var template = result.Template;
            if (template?.Elements == null)
                return;

            result.TotalElements = template.Elements.Count;

            var typeCounts = new Dictionary<string, (int fixedCount, int editableCount, int adapterCount)>();

            foreach (var element in template.Elements)
            {
                var typeName = GetShortTypeName(element);

                // 分类逻辑：
                // 1. dataPath 在适配器配置中 → DataAdapter（自动填充）
                // 2. dataPath 不在适配器但支持输入 → Editable（手动/xlsx导入）
                // 3. dataPath 不在适配器且不可编辑 → Fixed
                // 4. 无 dataPath 但 isRequired → Editable
                // 5. 无 dataPath → Fixed

                if (!string.IsNullOrEmpty(element.DataPath))
                {
                    // 检查是否在适配器配置中
                    if (_adapterPaths != null && _adapterPaths.Contains(element.DataPath))
                    {
                        // 有 label 的元素用户可直接编辑，无 label 的保持自动填充
                        element.Group = !string.IsNullOrEmpty(element.Label)
                            ? ElementGroup.Editable
                            : ElementGroup.DataAdapter;
                    }
                    else if (element is ExternalTextElement textEl)
                    {
                        // 文本元素始终可编辑
                        element.Group = ElementGroup.Editable;
                    }
                    else if (IsEditableInputType(element))
                    {
                        element.Group = ElementGroup.Editable;
                    }
                    else
                    {
                        element.Group = ElementGroup.Fixed;
                    }
                }
                else if (element.IsRequired)
                {
                    element.Group = ElementGroup.Editable;
                }
                else if (IsEditableInputType(element))
                {
                    element.Group = ElementGroup.Editable;
                }
                else
                {
                    element.Group = ElementGroup.Fixed;
                }

                // 统计
                if (!typeCounts.ContainsKey(typeName))
                    typeCounts[typeName] = (0, 0, 0);

                var current = typeCounts[typeName];
                switch (element.Group)
                {
                    case ElementGroup.Fixed:
                        typeCounts[typeName] = (current.fixedCount + 1, current.editableCount, current.adapterCount);
                        break;
                    case ElementGroup.Editable:
                        typeCounts[typeName] = (current.fixedCount, current.editableCount + 1, current.adapterCount);
                        break;
                    case ElementGroup.DataAdapter:
                        typeCounts[typeName] = (current.fixedCount, current.editableCount, current.adapterCount + 1);
                        break;
                }
            }

            result.FixedCount = template.Elements.Count(e => e.Group == ElementGroup.Fixed);
            result.EditableCount = template.Elements.Count(e => e.Group == ElementGroup.Editable);
            result.DataAdapterCount = template.Elements.Count(e => e.Group == ElementGroup.DataAdapter);

            foreach (var kvp in typeCounts.OrderByDescending(x => x.Value.fixedCount + x.Value.editableCount + x.Value.adapterCount))
            {
                if (kvp.Value.fixedCount > 0)
                    result.ElementTypeStats.Add(new ElementStats { TypeName = kvp.Key, Group = "Fixed", Count = kvp.Value.fixedCount });
                if (kvp.Value.editableCount > 0)
                    result.ElementTypeStats.Add(new ElementStats { TypeName = kvp.Key, Group = "Editable", Count = kvp.Value.editableCount });
                if (kvp.Value.adapterCount > 0)
                    result.ElementTypeStats.Add(new ElementStats { TypeName = kvp.Key, Group = "DataAdapter", Count = kvp.Value.adapterCount });
            }
        }

        private static bool IsEditableInputType(ExternalElementBase element)
        {
            return element is ExternalNumberElement
                || element is ExternalDateElement
                || element is ExternalDropdownElement
                || element is ExternalCheckboxElement
                || element is ExternalRadioElement
                || element is ExternalImageElement
                || element is ExternalBarcodeElement
                || element is ExternalQrCodeElement
                || element is ExternalSignatureElement
                || element is ExternalTableElement;
        }

        private static string GetShortTypeName(ExternalElementBase element)
        {
            var name = element.GetType().Name;
            if (name.StartsWith("External"))
                name = name.Substring(8);
            if (name.EndsWith("Element"))
                name = name.Substring(0, name.Length - 7);
            return name;
        }
    }
}
