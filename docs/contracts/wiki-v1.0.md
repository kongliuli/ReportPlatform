# Contracts 契约层 Wiki v1.0

> 版本：v1.0 | 更新日期：2026-05-08 | 覆盖版本：v0.3.0 ~ v0.6.0

---

## 1. 概述

**职责**：定义 Editor 和 Generators 之间共享的数据结构、接口契约和枚举类型。作为整个平台的单一数据源（Single Source of Truth）。

**项目路径**：`/Contracts/Xinglin.WebReportEditor.Contracts.csproj`
**目标框架**：`net8.0`
**NuGet 依赖**：`Newtonsoft.Json 13.0.3`

### 命名空间说明

项目存在两套命名空间（历史遗留）：
- `Xinglin.ReportEditor.Contracts.*` — Enums、Models、Converters、Registry
- `Xinglin.WebReportEditor.Contracts.*` — DTOs、Requests、Responses

---

## 2. 枚举定义

| 枚举 | 命名空间 | 值 | 说明 |
|------|---------|-----|------|
| `ElementGroup` | `Contracts.Enums` | Fixed, Context, Editable, DataAdapter | 元素分组 |
| `ElementAdaptationGroup` | `Contracts.Enums` | Basic, Form, Data, Advanced | 元素适配分组 |
| `AdapterType` | `Contracts.Enums` | Context, Excel, Database, Api | 适配器类型 |
| `DatabaseProvider` | `Contracts.Enums` | SqlServer, MySql, Sqlite, PostgreSql | 数据库提供程序 |
| `FieldDataType` | `Contracts.Models.Adapters` | Text, Number, Date, Dropdown, Boolean | 字段数据类型 |
| `BindingType` | `Contracts.Models.Template` | Text, Image, Visibility, Repeat, Style | 数据绑定类型 |

---

## 3. 元素类型体系（23 种）

```
ElementBase (抽象基类)
├── 属性: Id, X, Y, Width, Height, Rotation, ZIndex, Tooltip, IsLocked
├── ID 生成: Guid.NewGuid().ToString("N") (32字符无连字符)
│
└── ExternalElementBase (抽象基类, 支持数据绑定)
    ├── 属性: Label, DataPath, IsDataBound, IsRequired, Group, AdapterId
    │
    ├── Fixed 组 (不可编辑)
    │   ├── LineElement        线条
    │   ├── TextElement        文本
    │   ├── ShapeElement       形状
    │   ├── ImageElement       图片
    │   ├── DividerElement     分隔线
    │   ├── HeaderElement      页眉
    │   ├── FooterElement      页脚
    │   ├── PageNumberElement  页码
    │   ├── WatermarkElement   水印
    │   └── IconElement        图标
    │
    ├── Context 组 (上下文自动填充)
    │   └── HyperlinkElement   超链接
    │
    ├── Editable 组 (可编辑录入)
    │   ├── NumberElement      数字
    │   ├── DateElement        日期
    │   ├── DropdownElement    下拉框
    │   ├── CheckboxElement    复选框
    │   ├── RadioElement       单选
    │   ├── SignatureElement   签名
    │   └── BarcodeElement     条形码
    │
    └── DataAdapter 组 (适配器填充)
        ├── TableElement       表格
        ├── ContainerElement   容器
        ├── RepeatElement      重复区域
        ├── QrCodeElement      二维码
        └── ChartElement       图表
```

---

## 4. 模板定义

### TemplateDefinition

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string? | 模板唯一标识 |
| Name | string | 模板名称 |
| Version | int | 模板版本（默认 1） |
| Type | string | 模板类型 |
| HospitalId | string? | 关联医院标识 |
| PageSettings | PageSettings | 页面配置 |
| DataBindings | List\<DataBindingDefinition\> | 数据绑定定义 |
| Elements | List\<ExternalElementBase\> | 模板元素列表 |
| EnableGlobalFontSize | bool | 全局字号开关 |
| GlobalFontSize | double? | 全局字号值 |

### PageSettings

| 属性 | 类型 | 说明 |
|------|------|------|
| PageSize | string | 页面尺寸（A4/A5/自定义） |
| Orientation | string | 方向（Portrait/Landscape） |
| Margins | object | 页边距（Top, Bottom, Left, Right） |

### DataBindingDefinition

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string? | 绑定定义唯一标识 |
| ElementId | string | 关联元素标识 |
| DataPath | string | 数据路径 |
| BindingType | BindingType | 绑定类型（Text/Image/Visibility/Repeat/Style） |
| FormatString | string? | 格式化字符串 |
| DefaultValue | string? | 默认值 |
| Transform | string? | 数据转换方法 |

---

## 5. 适配器模型

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `IDataAdapter` | `Contracts.Models.Adapters` | 适配器接口（ReadDataAsync, ReadBatchDataAsync, ValidateConfigAsync） |
| `AdapterConfigBase` | `Contracts.Models.Adapters` | 配置基类（AdapterId, Type, DisplayName, IsEnabled） |
| `AdapterResult` | `Contracts.Models.Adapters` | 执行结果（Success, ErrorMessage, Data, BatchData） |
| `ValidationResult` | `Contracts.Models.Adapters` | 验证结果（IsValid, Errors, Warnings） |
| `FieldSchema` | `Contracts.Models.Adapters` | 字段模式（DataPath, Label, DataType, Format, Options, IsRequired, MinValue, MaxValue, DecimalPlaces） |

---

## 6. 关键组件

### ElementJsonConverter（双格式 JSON 转换器）

支持两种 JSON 格式：
1. **Web 短格式**（首选）：`"$type": "template.element.{shortName}"`
2. **Legacy 全格式**：`"Type": "FullClassName"` 或完整程序集限定名

遗留类型映射：
- LabelElement / LabelInputBoxElement → TextElement
- RectangleElement / EllipseElement → ShapeElement
- AutoNumberElement → PageNumberElement

### ElementGroupRegistry（元素分组注册表）

| 分组 | 包含元素 |
|------|---------|
| Basic | Line, Divider, Shape |
| Form | Text, Number, Date, Dropdown, Checkbox, Radio |
| Data | Table, Repeat, Chart |
| Advanced | Image, Barcode, QrCode, Signature, Hyperlink, Icon, Container, Header, Footer, PageNumber, Watermark |

方法：`GetGroup()`, `GetElementsInGroup()`, `GetAllElementTypes()`, `GetAllGroups()`

### TemplateSerializer（静态类）

序列化配置：NullValueHandling.Ignore + CamelCase + ElementJsonConverter + StringEnumConverter

方法：
- `Deserialize(string json): TemplateDefinition`
- `Serialize(TemplateDefinition template): string`
- `DeserializeElements(string json): List<ExternalElementBase>`
- `SerializeElements(IEnumerable<ExternalElementBase> elements): string`

---

## 7. Web API 契约

| 目录 | 内容 |
|------|------|
| `DTOs/` | TemplateDto, TemplateDetailDto, TemplateVersionDto, TemplateVersionDetailDto, AuthDtos (LoginRequest/Response, RefreshTokenResponse, UserDto), VersionDtos (VersionDiffResponse) |
| `Requests/` | CreateTemplateRequest, UpdateTemplateRequest, TemplateFilterRequest, RollbackRequest |
| `Responses/` | ApiResponse\<T\>, PagedResponse\<T\> |

---

## 8. 已知问题

- 命名空间不统一：`Xinglin.ReportEditor.Contracts` vs `Xinglin.WebReportEditor.Contracts`
- 短唯一 ID 方案待实施（当前使用 32 字符 Guid）

---

*文档版本 1.0，基于代码实际状态编写*
