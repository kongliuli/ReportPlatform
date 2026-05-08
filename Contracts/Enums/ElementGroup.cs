namespace Xinglin.ReportEditor.Contracts.Enums;

/// <summary>
/// 元素适配分组，决定元素如何获取数据
/// </summary>
public enum ElementGroup
{
    /// <summary>
    /// 固定内容，无需数据适配
    /// </summary>
    Fixed,

    /// <summary>
    /// 上下文配置自动填充（如医院名称、日期等）
    /// </summary>
    Context,

    /// <summary>
    /// 手动编辑输入
    /// </summary>
    Editable,

    /// <summary>
    /// 外部数据适配器填充（如数据库、API等）
    /// </summary>
    DataAdapter
}
