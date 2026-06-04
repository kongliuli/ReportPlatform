using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

/// <summary>适配器插件接口，定义适配器的注册和创建契约</summary>
public interface IAdapterPlugin
{
    /// <summary>适配器类型标识</summary>
    string AdapterType { get; }

    /// <summary>适配器显示名称</summary>
    string DisplayName { get; }

    /// <summary>创建适配器服务实例</summary>
    object CreateService(TemplateDefinition template);
}
