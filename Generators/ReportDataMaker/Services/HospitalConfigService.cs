using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services
{
    /// <summary>
    /// 医院配置服务 - 加载医院信息、默认人员等配置
    /// </summary>
    public class HospitalConfigService
    {
        public HospitalConfig Config { get; private set; }

        public void Load(string configPath)
        {
            if (!File.Exists(configPath))
                return;

            var json = File.ReadAllText(configPath);
            Config = JsonConvert.DeserializeObject<HospitalConfig>(json)
                     ?? new HospitalConfig();
        }

        /// <summary>
        /// 将默认配置应用到模板的适配器元素
        /// </summary>
        public void ApplyDefaults(ExternalTemplateDefinition template)
        {
            if (Config == null || template?.Elements == null) return;

            var defaults = Config.Defaults;
            if (defaults == null) return;

            foreach (var element in template.Elements)
            {
                if (string.IsNullOrEmpty(element.DataPath))
                    continue;

                // 处理 DataAdapter 和 Editable 元素（值空时才填充默认值）
                if (element.Group != ElementGroup.DataAdapter && element.Group != ElementGroup.Editable)
                    continue;

                var value = GetDefaultForPath(element.DataPath);
                if (value == null) continue;

                switch (element)
                {
                    case ExternalTextElement t:
                        if (string.IsNullOrEmpty(t.Text))
                            t.Text = value;
                        break;
                    case ExternalDropdownElement d:
                        if (string.IsNullOrEmpty(d.Value))
                            d.Value = value;
                        break;
                    case ExternalDateElement dt:
                        if (string.IsNullOrEmpty(dt.Value))
                            dt.Value = value;
                        break;
                    case ExternalNumberElement n:
                        if (n.Value <= 0 && double.TryParse(value, out var dv))
                            n.Value = dv;
                        break;
                }
            }
        }

        private string GetDefaultForPath(string dataPath)
        {
            var defaults = Config?.Defaults;
            if (defaults == null) return null;

            return dataPath switch
            {
                "Technician" => defaults.Technician,
                "Reviewer" => defaults.Reviewer,
                "DoctorName" => defaults.Doctor,
                "NurseName" => defaults.NurseName,
                "ReportingDoctor" => defaults.ReportingDoctor,
                "Pharmacist" => defaults.Pharmacist,
                "Department" => defaults.Department,
                "SampleType" => defaults.SampleType,
                "ReportDate" => string.IsNullOrEmpty(defaults.ReportDate)
                    ? DateTime.Now.ToString("yyyy-MM-dd") : defaults.ReportDate,
                "VisitDate" => string.IsNullOrEmpty(defaults.VisitDate)
                    ? DateTime.Now.ToString("yyyy-MM-dd") : defaults.VisitDate,
                _ => null
            };
        }
    }

    public class HospitalConfig
    {
        public HospitalInfo Hospital { get; set; } = new();
        public DefaultValues Defaults { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public StaffInfo Staff { get; set; } = new();
    }

    public class HospitalInfo
    {
        public string Name { get; set; } = "";
        public string ShortName { get; set; } = "";
        public string Grade { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Website { get; set; } = "";
        public string LogoPath { get; set; } = "";
    }

    public class DefaultValues
    {
        public string Technician { get; set; } = "";
        public string Reviewer { get; set; } = "";
        public string Doctor { get; set; } = "";
        public string NurseName { get; set; } = "";
        public string ReportingDoctor { get; set; } = "";
        public string Pharmacist { get; set; } = "";
        public string Department { get; set; } = "";
        public string SampleType { get; set; } = "";
        public string ReportDate { get; set; } = "";
        public string VisitDate { get; set; } = "";
    }

    public class StaffInfo
    {
        public List<string> Technicians { get; set; } = new();
        public List<string> Reviewers { get; set; } = new();
        public List<string> Doctors { get; set; } = new();
    }
}
