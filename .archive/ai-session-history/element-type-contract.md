# 元素类型契约文档

## 概述

本文档描述了报告模板 JSON `$type` 鉴别器、CLR 元素类型以及 WPF DataTemplate 之间的映射关系。该契约定义了模板服务端序列化与客户端 UI 渲染之间的类型约定。

## 类型映射表

| JSON `$type` | CLR 类型 | DataTemplate Key | FieldDataType | 说明 |
|---|---|---|---|---|
| Text | ExternalTextElement | TextFieldTemplate / MultiLineTextFieldTemplate | Text | 普通文本或多行文本（根据 Height > 12px） |
| Date | ExternalDateElement | DateFieldTemplate | Date | 日期选择器 |
| Number | ExternalNumberElement | NumberFieldTemplate | Number | 数字输入框（可带单位） |
| Dropdown | ExternalDropdownElement | DropdownFieldTemplate | Dropdown | 下拉选择框 |
| Checkbox | ExternalCheckboxElement | CheckboxFieldTemplate | Boolean | 复选框 |
| Radio | ExternalRadioElement | CheckboxFieldTemplate | Boolean | 单选框（使用 SelectedOption + GroupName） |
| Table | ExternalTableElement | TableFieldTemplate | Table | 表格编辑器 |
| Image | ExternalImageElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Barcode | ExternalBarcodeElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| QRCode | ExternalQrCodeElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Signature | ExternalSignatureElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Hyperlink | ExternalHyperlinkElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Divider | ExternalDividerElement | — | — | 跳过（不创建 FieldViewModel） |
| Line | ExternalLineElement | — | — | 跳过（不创建 FieldViewModel） |
| Shape | ExternalShapeElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Icon | ExternalIconElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Header | ExternalHeaderElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Footer | ExternalFooterElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| PageNumber | ExternalPageNumberElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Watermark | ExternalWatermarkElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Chart | ExternalChartElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Container | ExternalContainerElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |
| Repeat | ExternalRepeatElement | ReadOnlyFieldTemplate | ReadOnly | 只读显示 |

## 映射逻辑

### `CreateFieldViewModel`（MainTabViewModel.cs）

核心映射逻辑在 `MainTabViewModel.CreateFieldViewModel` 中：

```csharp
switch (element)
{
    case ExternalNumberElement:    → FieldDataType.Number
    case ExternalDateElement:      → FieldDataType.Date
    case ExternalDropdownElement:  → FieldDataType.Dropdown
    case ExternalCheckboxElement:  → FieldDataType.Boolean
    case ExternalRadioElement:     → FieldDataType.Boolean
    case ExternalTextElement:      → FieldDataType.Text
    case ExternalTableElement:     → FieldDataType.Table
    default:                       → FieldDataType.Text
}
```

### 启发式回退（Heuristic Fallback）

当模板 JSON 的 `$type` 标记错误（例如服务端将 Date 类型错误序列化为 Text）时，`CreateFieldViewModel` 在 switch 语句之后执行启发式检测：

1. **日期检测**：如果 label 包含 "日期" 或 "date"（不区分大小写），且字段类型为 Text → 覆盖为 `FieldDataType.Date`
2. **性别/下拉检测**：如果 label 包含 "性别" 或字段有 Options → 覆盖为 `FieldDataType.Dropdown`

```csharp
if (field.FieldType == FieldDataType.Text)
{
    if (label.Contains("日期") || label.Contains("date"))
        field.FieldType = FieldDataType.Date;
    else if (label.Contains("性别") || (element.Options?.Any() == true))
        field.FieldType = FieldDataType.Dropdown;
}
```

启发式仅在字段落入 switch `default` 分支（FieldDataType.Text）时触发，不会覆盖明确指定的类型。

### `FieldDataTemplateSelector`

`FieldDataTemplateSelector` 根据 `FieldViewModel.FieldType` 路由到对应的 DataTemplate：

```
FieldDataType.Text     → TextFieldTemplate / MultiLineTextFieldTemplate
FieldDataType.Number   → NumberFieldTemplate
FieldDataType.Date     → DateFieldTemplate
FieldDataType.Dropdown → DropdownFieldTemplate
FieldDataType.Boolean  → CheckboxFieldTemplate
FieldDataType.Table    → TableFieldTemplate
FieldDataType.ReadOnly → ReadOnlyFieldTemplate
```

## 已知问题

### `$type` 序列化不一致

当前观察到模板服务端序列化时，部分字段的 `$type` 鉴别器未正确反映实际类型。例如：
- Date 类型的字段被序列化为 Text → 客户端表现为普通输入框而非日期选择器
- Dropdown 类型的字段被序列化为 Text → 客户端表现为输入框而非下拉框

**临时解决方案**：在 `MainTabViewModel.CreateFieldViewModel` 中增加启发式回退（见上文），通过字段 Label 关键词匹配推断正确控件类型。

**根本修复方案**：修复服务端（ReportEditor）的 JSON 序列化配置，确保 `$type` 正确标记。ReportEditor 不在当前仓库范围内。

### 只读类型

Image、Barcode、QRCode、Signature 等元素在录入面板中仅显示为只读标签，不支持数据录入。这些类型的 `DefaultValue` 不会在 `MainTabViewModel.LoadFields` 中参与分类。

## 参见

- `Contracts/Models/Elements/` — 所有 CLR 元素类型定义
- `Contracts/Enums/ElementAdaptationGroup.cs` — FieldDataType 枚举
- `Generators/ReportDataMaker/Infrastructure/FieldDataTemplateSelector.cs` — DataTemplate 路由逻辑
- `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs` — CreateFieldViewModel 映射 + 启发式回退
