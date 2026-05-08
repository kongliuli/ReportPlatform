using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Models;

public interface IDataAdapter : Xinglin.ReportEditor.Contracts.Models.Adapters.IDataAdapter
{
    Dictionary<string, object> ReadData(IReadOnlyDictionary<string, object> parameters);
}
