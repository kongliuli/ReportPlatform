using Xinglin.WebReportEditor.Core.SharedInterfaces;

namespace Xinglin.WebReportEditor.Core.Services;

public class DataBindingEngineStub : IDataBindingEngine
{
    public object ApplyDataBinding(object template, object sampleData)
    {
        return template;
    }
}
