using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using IDataAdapter = ReportDataMaker.Models.IDataAdapter;

namespace ReportDataMaker.Services
{
    public class DataAdapterService
    {
        private readonly List<IDataAdapter> _adapters = new List<IDataAdapter>();

        public void RegisterAdapter(IDataAdapter adapter)
        {
            if (adapter == null)
                throw new ArgumentNullException(nameof(adapter));

            _adapters.Add(adapter);
        }

        public HashSet<string> GetTargetPaths()
        {
            var paths = new HashSet<string>();
            foreach (var adapter in _adapters)
            {
                foreach (var path in adapter.TargetDataPaths)
                {
                    paths.Add(path);
                }
            }
            return paths;
        }

        public Dictionary<string, object> ExecuteAdapter(string adapterName, IReadOnlyDictionary<string, object> parameters)
        {
            var adapter = _adapters.FirstOrDefault(a => a.AdapterName == adapterName);
            if (adapter == null)
                throw new InvalidOperationException($"Adapter '{adapterName}' not found.");

            return adapter.ReadData(parameters);
        }

        public void LoadAdapterConfig(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Adapter config file not found: {configPath}");

            var jsonContent = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<AdapterConfigRoot>(jsonContent);

            if (config?.Adapters == null)
                return;

            foreach (var entry in config.Adapters)
            {
                var adapter = new ConfigurableAdapter(entry.Name, entry.TargetPaths ?? new List<string>());
                _adapters.Add(adapter);
            }
        }

        private class AdapterConfigRoot
        {
            public List<AdapterConfigEntry> Adapters { get; set; }
        }

        private class AdapterConfigEntry
        {
            public string Name { get; set; }
            public List<string> TargetPaths { get; set; }
            public string ConfigFile { get; set; }
        }

        private class ConfigurableAdapter : IDataAdapter
        {
            public string AdapterId => AdapterName;
            public string AdapterName { get; }
            public AdapterType Type => AdapterType.Context;
            public IReadOnlyList<string> TargetDataPaths { get; }

            public ConfigurableAdapter(string name, List<string> targetPaths)
            {
                AdapterName = name;
                TargetDataPaths = targetPaths.AsReadOnly();
            }

            public Dictionary<string, object> ReadData(IReadOnlyDictionary<string, object> parameters)
            {
                return new Dictionary<string, object>();
            }

            public Task<AdapterResult> ReadDataAsync()
            {
                return Task.FromResult(new AdapterResult { Success = true });
            }

            public Task<AdapterResult> ReadBatchDataAsync()
            {
                return Task.FromResult(new AdapterResult { Success = true });
            }

            public Task<ValidationResult> ValidateConfigAsync()
            {
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}
