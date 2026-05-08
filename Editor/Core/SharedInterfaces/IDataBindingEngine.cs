namespace Xinglin.WebReportEditor.Core.SharedInterfaces;

public interface IDataBindingEngine
{
    object ApplyDataBinding(object template, object sampleData);
}
