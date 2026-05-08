namespace Xinglin.ReportEditor.Contracts.Enums;

/// <summary>
/// 元素适配分组类型，用于元素的功能分类
/// </summary>
public enum ElementAdaptationGroup
{
    /// <summary>
    /// 基础元素，仅用于展示（如线条、形状、分隔线）
    /// </summary>
    Basic,

    /// <summary>
    /// 表单元素，用于用户输入（文本、数字、日期、下拉、复选、单选）
    /// </summary>
    Form,

    /// <summary>
    /// 数据元素，由外部数据驱动（表格、重复、图表）
    /// </summary>
    Data,

    /// <summary>
    /// 高级元素，特殊功能（图片、二维码、签名、超链接等）
    /// </summary>
    Advanced
}
