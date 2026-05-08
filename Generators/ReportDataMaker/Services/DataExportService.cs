using System.IO;
using Newtonsoft.Json;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    public class DataExportService
    {
        public void ExportData(TemplateData data, string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"导出数据失败: {ex.Message}");
            }
        }

        public void ExportTemplateWithData(ReportTemplateDefinition template, TemplateData data, string filePath)
        {
            try
            {
                // 创建一个包含模板和数据的复合对象
                var exportData = new
                {
                    Template = template,
                    Data = data
                };

                var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"导出模板和数据失败: {ex.Message}");
            }
        }
    }
}