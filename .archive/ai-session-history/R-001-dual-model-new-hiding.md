# 风险报告 R-001：双重模型体系与 `new` 隐藏

| 字段 | 值 |
|------|-----|
| 风险编号 | R-001 |
| 风险等级 | 🔴 高 |
| 影响模块 | Contracts + Generators |
| 发现日期 | 2026-05-20 |
| 状态 | 待修复 |

---

## 1. 风险描述

项目中存在两套并行的元素模型体系：Contracts 层定义了 `ExternalElementBase` 及 22 种 `XxxElement`，Generators 层又定义了 `ReportExternalElementBase` 及 23 种 `ExternalXxxElement`。后者继承前者但使用 `new` 关键字隐藏了 14 个属性，将可空类型重新定义为非空类型。

---

## 2. 涉及代码

### 2.1 Contracts 层基类

[ElementBase.cs](../../Contracts/Models/Elements/ElementBase.cs) 定义了 25+ 个属性，其中样式属性为可空类型：

```csharp
public string? BackgroundColor { get; set; }
public double? BorderWidth { get; set; }
public double? Opacity { get; set; } = 1.0;
public double? FontSize { get; set; } = 12;
```

[ExternalElementBase.cs](../../Contracts/Models/Elements/ExternalElementBase.cs) 继承 ElementBase，增加 4 个数据绑定属性。

### 2.2 Generators 层基类

[ExternalExtendedElements.cs](../../Generators/ReportDataMaker/Models/ExternalExtendedElements.cs) 使用 `new` 隐藏了 14 个属性：

```csharp
public abstract class ReportExternalElementBase : ExternalElementBase
{
    public new bool IsVisible { get; set; } = true;
    public new string BackgroundColor { get; set; } = string.Empty;
    public new double BorderWidth { get; set; }
    public new double Opacity { get; set; } = 1;
    public new string FontFamily { get; set; } = string.Empty;
    public new double FontSize { get; set; }          // 默认值从 12 变为 0
    public new string FontWeight { get; set; } = string.Empty;
    public new string FontStyle { get; set; } = string.Empty;
    public new string ForegroundColor { get; set; } = "#000000";
    public new string TextAlignment { get; set; } = string.Empty;
    public new string FormatString { get; set; } = string.Empty;
    public new string BorderStyle { get; set; } = string.Empty;
    public new double CornerRadius { get; set; }
    public new string BorderColor { get; set; } = string.Empty;
}
```

### 2.3 模板定义差异

| 属性 | Contracts `TemplateDefinition` | Generators `ExternalTemplateDefinition` |
|------|-------------------------------|----------------------------------------|
| 页面属性 | 嵌套 `PageSettings` 对象 | 扁平化为直接属性 |
| 版本号 | `int Version` | `string Version` |
| 数据绑定 | `DataBindingDefinition`（BindingType 枚举） | `LegacyDataBindingDefinition`（BindingType 字符串） |

---

## 3. 风险场景

### 3.1 多态访问陷阱

```csharp
ExternalElementBase baseRef = new ExternalTextElement();
baseRef.BackgroundColor = "#FF0000";

ExternalTextElement derivedRef = (ExternalTextElement)baseRef;
string bg = derivedRef.BackgroundColor;  // 结果："" 而非 "#FF0000"
```

通过基类引用设置属性值时，修改的是基类的 `BackgroundColor`（nullable）；通过派生类引用读取时，获取的是派生类的 `BackgroundColor`（non-nullable，默认空字符串）。**两个属性独立存储，互不影响。**

### 3.2 序列化歧义

Newtonsoft.Json 在序列化 `ReportExternalElementBase` 实例时，可能同时输出基类和派生类的同名属性，导致 JSON 结构异常：

```json
{
  "backgroundColor": "#FF0000",
  "BackgroundColor": "",
  "fontSize": 14,
  "FontSize": 0
}
```

### 3.3 默认值不一致

| 属性 | Contracts 默认值 | Generators 默认值 | 差异影响 |
|------|-----------------|------------------|----------|
| `FontSize` | 12 | 0 | 未设置字体的元素在两端渲染结果不同 |
| `BackgroundColor` | null | "" | null 与空字符串语义不同 |
| `Opacity` | 1.0 | 1 | 类型不同（double? vs double） |

### 3.4 数据往返丢失

模板 JSON 从 Editor 导出 → Generators 加载 → 修改 → 导出 → Editor 重新加载，经过两套模型的转换后，nullable 属性的 null 值会被替换为非空默认值，导致原始数据语义丢失。

---

## 4. 影响范围

| 影响维度 | 评估 |
|----------|------|
| 数据一致性 | 🔴 高 — 双向转换可能丢失数据语义 |
| 渲染正确性 | 🟡 中 — 默认值差异导致两端渲染不一致 |
| 序列化可靠性 | 🟡 中 — 同名属性可能产生歧义 JSON |
| 维护成本 | 🔴 高 — Contracts 变更需同步 Generators，遗漏风险大 |
| 新人理解 | 🟡 中 — 两套模型增加认知负担 |

---

## 5. 修复方案

### 方案 A：消除影子模型（推荐）

将 Generators 层的 `ReportExternalElementBase` 及其子类替换为直接使用 Contracts 层的 `ExternalElementBase`，通过扩展方法或包装类添加 Generators 特有的属性（`DefaultValue`、`Options`、`Shadow`、`LabelWidth`）。

**改动范围**：Generators 层 Models/ + Services/ + ViewModels/ + Infrastructure/

**优点**：彻底消除双重模型，数据一致性有保障
**缺点**：改动量大，需要逐步迁移

### 方案 B：组合替代继承

将 `ReportExternalElementBase` 改为包含 `ExternalElementBase` 实例的组合类：

```csharp
public class ReportExternalElement
{
    public ExternalElementBase ContractElement { get; set; }
    public string DefaultValue { get; set; }
    public List<string> Options { get; set; }
    public string Shadow { get; set; }
    public double LabelWidth { get; set; }
}
```

**优点**：不修改 Contracts 层，消除 `new` 隐藏
**缺点**：需要调整所有访问元素属性的代码

### 方案 C：override 替代 new

将 Contracts 层的属性改为 `virtual`，Generators 层使用 `override` 替代 `new`。

**优点**：改动最小
**缺点**：仍需统一属性类型（nullable vs non-nullable），Contracts 层的 nullable 语义可能被破坏

---

## 6. 建议优先级

**P1 — 中期修复（1-2 月）**

建议采用方案 B（组合替代继承），在不修改 Contracts 层的前提下消除 `new` 隐藏风险，同时为 Generators 层的扩展属性提供明确的归属。
