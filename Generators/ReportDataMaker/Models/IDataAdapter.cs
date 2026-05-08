using System.Collections.Generic;

namespace ReportDataMaker.Models
{
    public interface IDataAdapter
    {
        string AdapterName { get; }
        IReadOnlyList<string> TargetDataPaths { get; }
        Dictionary<string, object> ReadData(IReadOnlyDictionary<string, object> parameters);
    }
}
