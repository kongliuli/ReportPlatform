using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>数据适配器接口，定义数据读取和验证的契约</summary>
public interface IDataAdapter
{
    /// <summary>适配器唯一标识</summary>
    string AdapterId { get; }

    /// <summary>适配器名称</summary>
    string AdapterName { get; }

    /// <summary>适配器类型</summary>
    AdapterType Type { get; }

    /// <summary>目标数据路径列表</summary>
    IReadOnlyList<string> TargetDataPaths { get; }

    /// <summary>异步读取数据</summary>
    /// <returns>适配器读取结果</returns>
    Task<AdapterResult> ReadDataAsync();

    /// <summary>异步批量读取数据</summary>
    /// <returns>适配器读取结果</returns>
    Task<AdapterResult> ReadBatchDataAsync();

    /// <summary>异步验证配置</summary>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateConfigAsync();
}
