using System.Collections.Generic;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    public class DataEntryService
    {
        private Dictionary<string, object> _dataValues;

        public DataEntryService()
        {
            _dataValues = new Dictionary<string, object>();
        }

        public void SetValue(string path, object value)
        {
            if (!_dataValues.ContainsKey(path))
            {
                _dataValues.Add(path, value);
            }
            else
            {
                _dataValues[path] = value;
            }
        }

        public object GetValue(string path)
        {
            return _dataValues.TryGetValue(path, out var value) ? value : null;
        }

        public bool ValidateData(ReportTemplateDefinition template)
        {
            // 这里实现数据验证逻辑
            // 目前简单实现，实际项目中需要根据模板定义进行更复杂的验证
            return true;
        }

        public Dictionary<string, object> GetAllValues()
        {
            return _dataValues;
        }
    }
}