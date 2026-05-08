using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services
{
    public class TemplateLoaderService
    {
        private static readonly Dictionary<string, string> DataPathMap = new Dictionary<string, string>
        {
            ["PatientName"] = "Patient.Name",
            ["Gender"] = "Patient.Gender",
            ["Age"] = "Patient.Age",
            ["SampleType"] = "Report.SampleType",
            ["ReportDate"] = "Report.ReportDate",
            ["Technician"] = "Report.Technician",
            ["Reviewer"] = "Report.Reviewer",
            ["Department"] = "Report.Department",
            ["VisitDate"] = "Report.ReportDate",
            ["ChiefComplaint"] = "Report.ChiefComplaint",
            ["PresentIllness"] = "Report.PresentIllness",
            ["Diagnosis"] = "Report.Diagnosis",
            ["Treatment"] = "Report.Treatment",
            ["DoctorName"] = "Report.Technician"
        };

        public ReportTemplateDefinition LoadTemplate(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Template file not found: {filePath}");
            }

            var jsonContent = File.ReadAllText(filePath);
            var template = JsonConvert.DeserializeObject<ReportTemplateDefinition>(jsonContent);
            template.FilePath = filePath;

            return template;
        }

        public bool IsExternalTemplateFormat(string jsonContent)
        {
            return jsonContent.Contains("\"$type\"");
        }

        public ExternalTemplateDefinition LoadExternalTemplate(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Template file not found: {filePath}");
            }

            var jsonContent = File.ReadAllText(filePath);
            return LoadExternalTemplateFromContent(jsonContent);
        }

        public ExternalTemplateDefinition LoadExternalTemplateFromContent(string jsonContent)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[]
                {
                    new ExternalElementConverter()
                }
            };

            var template = JsonConvert.DeserializeObject<ExternalTemplateDefinition>(jsonContent, settings);
            return template;
        }

        public void ClassifyElements(ExternalTemplateDefinition template, IReadOnlySet<string> adapterPaths)
        {
            if (template.Elements == null)
            {
                return;
            }

            foreach (var element in template.Elements)
            {
                if (element is ExternalLineElement)
                {
                    element.Group = ElementGroup.Fixed;
                    continue;
                }

                if (element.IsDataBound)
                {
                    if (adapterPaths.Contains(element.DataPath ?? string.Empty))
                    {
                        element.Group = ElementGroup.DataAdapter;
                    }
                    else
                    {
                        element.Group = ElementGroup.Editable;
                    }
                }
                else
                {
                    if (element is ExternalTextElement textElement && !string.IsNullOrEmpty(textElement.Text))
                    {
                        element.Group = ElementGroup.Fixed;
                    }
                    else
                    {
                        element.Group = ElementGroup.Fixed;
                    }
                }
            }
        }

        public string MapDataPath(string flatDataPath)
        {
            if (DataPathMap.TryGetValue(flatDataPath, out var mappedPath))
            {
                return mappedPath;
            }

            return flatDataPath;
        }
    }
}