# 元素适配分组与向后兼容策略

> 目标：对契约 v2.2.0 定义的 23 种元素进行适配分组，明确各组的适配策略，并设计向后兼容机制
> 日期：2026-05-08（修订）

---

## 1. 数据来源四分体系

每个带 `dataPath` 的元素必须归属于且仅归属于一个数据来源。整个模板的元素被完整瓜分为四类：

```
┌─────────────────────────────────────────────────────────────┐
│                    模板元素全集                               │
├──────────┬──────────┬──────────────┬────────────────────────┤
│  Fixed   │ Context  │  Editable    │    DataAdapter          │
│  固定元素 │ 上下文   │  可录入      │    外部数据源            │
│          │          │              │                        │
│ 纯装饰   │ 配置文件  │ 用户手动输入  │ Excel/数据库/API        │
│ 无dataPath│ 系统上下文│ 运行时录入   │ 外部系统拉取            │
│          │ 自动填充  │              │                        │
│ 线条     │ 医院名称  │ 患者姓名     │ 检验结果(LIS)          │
│ 形状     │ 科室名称  │ 患者年龄     │ 影像报告(PACS)         │
│ 水印     │ 报告日期  │ 主诉        │ 历史用药(HIS)          │
│ 分隔线   │ 检验医生  │ 诊断        │                        │
│ 图标     │ 审核医生  │             │                        │
│          │ 打印时间  │             │                        │
└──────────┴──────────┴──────────────┴────────────────────────┘
```

### 1.1 ElementGroup 枚举（修订）

```csharp
public enum ElementGroup
{
    Fixed,        // 固定元素：纯装饰/布局，无数据绑定
    Context,      // 上下文元素：从配置文件/系统上下文自动填充
    Editable,     // 可录入元素：用户手动输入
    DataAdapter   // 适配器元素：从外部数据源（Excel/DB/API）获取
}
```

### 1.2 分类判定规则

```csharp
public ElementGroup Classify(ExternalElementBase element, 
                             ISet<string> contextPaths,
                             ISet<string> adapterPaths)
{
    if (string.IsNullOrEmpty(element.DataPath))
        return ElementGroup.Fixed;
    
    if (contextPaths.Contains(element.DataPath))
        return ElementGroup.Context;
    
    if (adapterPaths.Contains(element.DataPath))
        return ElementGroup.DataAdapter;
    
    return ElementGroup.Editable;  // 默认：有 dataPath 但未被其他来源认领 → 手动录入
}
```

---

## 2. Context 适配器 — 配置/上下文数据源

### 2.1 定位

Context 适配器是一个**基础适配器**，从本地配置文件和系统上下文读取数据，自动填充模板中的固定信息字段。它与 Excel/数据库适配器平级，但更轻量：

- 不需要外部连接
- 不需要用户交互
- 模板加载后自动执行
- 配置一次，所有模板共享

### 2.2 数据来源

| 来源 | 示例字段 | 存储位置 |
|------|---------|---------|
| 机构配置 | 医院名称、医院地址、医院电话、Logo | `config/institution.json` |
| 科室配置 | 科室名称、科室代码 | `config/department.json` |
| 用户上下文 | 当前医生姓名、工号、职称 | 登录会话 / `config/user.json` |
| 系统上下文 | 当前日期、当前时间、打印时间 | 运行时生成 |
| 报告元数据 | 报告编号（自增/规则生成） | 运行时生成 |

### 2.3 配置模型

```csharp
public class ContextAdapterConfig : AdapterConfigBase
{
    public InstitutionConfig Institution { get; set; }
    public DepartmentConfig Department { get; set; }
    public UserConfig CurrentUser { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public List<AutoFieldRule> AutoFields { get; set; } = new();
}

public class InstitutionConfig
{
    public string HospitalName { get; set; }
    public string HospitalAddress { get; set; }
    public string HospitalPhone { get; set; }
    public string HospitalLogo { get; set; }  // Base64 或文件路径
}

public class DepartmentConfig
{
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }
}

public class UserConfig
{
    public string DoctorName { get; set; }
    public string DoctorId { get; set; }
    public string DoctorTitle { get; set; }  // 职称
    public string ReviewerName { get; set; } // 审核医生
}

public class AutoFieldRule
{
    public string DataPath { get; set; }
    public AutoFieldType Type { get; set; }
    public string Format { get; set; }
}

public enum AutoFieldType
{
    CurrentDate,      // 当前日期
    CurrentTime,      // 当前时间
    CurrentDateTime,  // 日期+时间
    PrintTime,        // 打印时间（导出时生成）
    SerialNumber,     // 流水号
    Custom            // 自定义表达式
}
```

### 2.4 DataPath 约定

Context 适配器认领的 DataPath 遵循命名约定：

```
Context.Hospital.Name       → 医院名称
Context.Hospital.Address    → 医院地址
Context.Department.Name     → 科室名称
Context.Doctor.Name         → 检验/报告医生
Context.Doctor.Title        → 医生职称
Context.Reviewer.Name       → 审核医生
Context.Report.Date         → 报告日期（自动）
Context.Report.Time         → 报告时间（自动）
Context.Report.PrintTime    → 打印时间（导出时）
Context.Report.SerialNo     → 报告编号
```

模板设计时，将这些 DataPath 分配给对应元素，Context 适配器自动识别并填充。

### 2.5 执行时机

```
模板加载
    │
    ▼
Context 适配器自动执行（最先）
    │ 填充：医院名称、科室、医生、日期...
    ▼
用户手动录入 Editable 字段
    │ 填充：患者姓名、年龄、主诉...
    ▼
DataAdapter 执行（按需）
    │ 填充：检验结果、影像数据...
    ▼
全部数据就绪 → 预览/导出
```

### 2.6 配置 UI（左侧面板或独立 Tab）

Context 适配器的配置相对稳定，可以放在设置页面而非每次都配置：

```
┌─────────────────────────────────────────────────┐
│ 上下文配置                              [设置页] │
├─────────────────────────────────────────────────┤
│ ┌─ 机构信息 ─────────────────────────────────┐  │
│ │ 医院名称: [XX市第一人民医院______________]  │  │
│ │ 医院地址: [XX市XX路XX号_________________]  │  │
│ │ 联系电话: [0571-XXXXXXXX_______________]  │  │
│ │ Logo:    [选择图片...]                     │  │
│ └────────────────────────────────────────────┘  │
│ ┌─ 科室信息 ─────────────────────────────────┐  │
│ │ 科室名称: [检验科______]                    │  │
│ │ 科室代码: [LAB_________]                    │  │
│ └────────────────────────────────────────────┘  │
│ ┌─ 当前用户 ─────────────────────────────────┐  │
│ │ 医生姓名: [张三________]                    │  │
│ │ 工号:     [D001________]                    │  │
│ │ 职称:     [主任技师 ▼]                      │  │
│ │ 审核医生: [李四________]                    │  │
│ └────────────────────────────────────────────┘  │
│ ┌─ 自动字段 ─────────────────────────────────┐  │
│ │ 报告日期格式: [yyyy-MM-dd ▼]               │  │
│ │ 编号规则:     [LAB-{yyyyMMdd}-{###} ▼]     │  │
│ └────────────────────────────────────────────┘  │
│                                                  │
│ [保存配置]                                       │
└─────────────────────────────────────────────────┘
```

---

## 3. 元素类型适配分组（按 $type 分类）

根据元素在数据适配场景中的角色，将 23 种元素分为 4 个适配组：

| 适配组 | 含义 | 元素数量 | 适配器行为 |
|--------|------|---------|-----------|
| **Inputable** | 可输入项，用户录入或适配器填充 | 5 | 直接填充值 |
| **Renderable** | 数据渲染项，承载复杂数据 | 8 | 提供数据源，渲染引擎消费 |
| **Structural** | 结构/布局项 | 5 | 透明穿越，递归处理子元素 |
| **Decorative** | 装饰/辅助项 | 5 | 完全忽略 |

注意：这是按**元素类型**的分组，决定元素「能做什么」。而 ElementGroup（Fixed/Context/Editable/DataAdapter）是按**数据来源**的分组，决定元素的值「从哪来」。两者正交：

```
一个 TextElement (Inputable 类型) 可以是：
  - Context  → 医院名称（从配置读取）
  - Editable → 患者姓名（用户输入）
  - DataAdapter → 检验结论（从 LIS 拉取）
  - Fixed    → 固定标题文字（无 dataPath）
```
| 表格 | `template.element.table` | Cell[row][col] | 按固定行列填充可编辑 Cell |
| 条形码 | `template.element.barcode` | string (编码值) | 适配器提供编码字符串 |
| 二维码 | `template.element.qrcode` | string (编码值) | 适配器提供编码字符串 |
| 图表 | `template.element.chart` | DataSource (数组) | 适配器提供数据系列 |
| 图片 | `template.element.image` | string (路径/Base64) | 适配器提供图片数据 |
| 签名 | `template.element.signature` | string (签名数据) | 适配器提供签名图片 |
| 单选框 | `template.element.radio` | string (选中值) | 适配器提供选中项 |
| 超链接 | `template.element.hyperlink` | string (URL) | 适配器提供链接地址 |

**表格特殊处理**：
- 行列数由模板固定，不可增减
- 每个 Cell 有独立的可编辑标记和类型约束
- 扁平化时只展开可编辑 Cell：`TableId.Cell[row][col]`
- 表头行（通常第 0 行）不参与适配

**条形码/二维码**：
- 本质是一个 string 值的可视化表达
- 适配器只需提供字符串值，渲染引擎负责编码生成
- 归入 Renderable 而非 Inputable，因为用户通常不手动输入条码值

### 2.3 Structural — 结构/布局项（适配器透明穿越）

这些元素定义布局结构，适配器不直接操作它们，但需要递归处理其子元素。

| 元素 | $type | 适配器行为 |
|------|-------|-----------|
| 容器 | `template.element.container` | 递归处理 children 中的可输入项 |
| 重复区域 | `template.element.repeat` | 按模板定义的固定次数展开子元素 |
| 页眉 | `template.element.header` | 递归处理 children（通常为 Fixed） |
| 页脚 | `template.element.footer` | 递归处理 children（通常为 Fixed） |
| 页码 | `template.element.pagenumber` | 完全忽略（自动生成） |

**Container 处理**：
```
Container
├── TextElement (Editable) → 参与适配
├── NumberElement (Editable) → 参与适配
└── ImageElement (Fixed) → 不参与
```

**Repeat 处理**：
- 重复次数由模板定义固定（如固定 5 行检验项目）
- 扁平化时展开为 `Repeat[0].ChildPath`, `Repeat[1].ChildPath`, ...
- 不支持动态增减重复次数（与表格行列固定同理）

### 2.4 Decorative — 装饰/辅助项（适配器完全忽略）

这些元素纯粹用于视觉装饰或辅助信息，不承载业务数据。

| 元素 | $type | 忽略原因 |
|------|-------|---------|
| 线条 | `template.element.line` | 纯装饰 |
| 形状 | `template.element.shape` | 纯装饰 |
| 分隔线 | `template.element.divider` | 纯装饰 |
| 水印 | `template.element.watermark` | 全局装饰 |
| 图标 | `template.element.icon` | 纯装饰 |

---

## 3. 适配能力矩阵

各适配器对不同组的支持程度：

| 适配组 | Excel 适配器 | 数据库适配器 | API 适配器 |
|--------|-------------|-------------|-----------|
| Inputable | ✅ 完整支持 | ✅ 完整支持 | ✅ 完整支持 |
| Renderable.Table | ✅ 按 Cell 展开 | ✅ 按 Cell 映射 | ✅ 按 Cell 映射 |
| Renderable.Barcode/QR | ✅ 字符串列 | ✅ 字符串字段 | ✅ 字符串字段 |
| Renderable.Chart | ⚠️ 多列→数据系列 | ✅ 查询结果→系列 | ✅ JSON→系列 |
| Renderable.Image | ⚠️ 文件路径列 | ⚠️ BLOB/路径 | ✅ URL/Base64 |
| Renderable.Signature | ❌ 不支持 | ⚠️ BLOB | ⚠️ Base64 |
| Renderable.Radio | ✅ 字符串列 | ✅ 字符串字段 | ✅ 字符串字段 |
| Structural | 透明（递归子元素） | 透明 | 透明 |
| Decorative | 忽略 | 忽略 | 忽略 |

---

## 4. 向后兼容策略

### 4.1 设计目标

当契约版本升级新增元素类型时，现有适配器应能以最小改动提供部分支持，而非完全不兼容。

### 4.2 兼容机制：Fallback 适配

```csharp
public enum AdaptationLevel
{
    Full,       // 完整适配（类型感知的校验和转换）
    Partial,    // 部分适配（降级为字符串处理）
    Ignored     // 完全忽略（装饰类元素）
}

public class ElementAdaptationRegistry
{
    private readonly Dictionary<string, AdaptationLevel> _known = new();
    private AdaptationLevel _unknownFallback = AdaptationLevel.Partial;
    
    public AdaptationLevel GetLevel(string elementType)
    {
        return _known.TryGetValue(elementType, out var level) 
            ? level 
            : _unknownFallback;  // 未知元素默认降级为 Partial
    }
}
```

### 4.3 Partial 适配规则

当遇到未知元素类型时，适配器按以下规则提供部分支持：

1. **有 `dataPath`** → 视为可适配，降级为 string 类型处理
2. **有 `label`** → 在 Excel 模板中显示 label 作为列头
3. **无 `dataPath`** → 归入 Decorative，忽略
4. **有 `children`** → 归入 Structural，递归处理子元素

```csharp
public FlatField FallbackFlatten(ExternalElementBase unknownElement)
{
    if (string.IsNullOrEmpty(unknownElement.DataPath))
        return null;  // 无 DataPath，忽略
    
    return new FlatField
    {
        DataPath = unknownElement.DataPath,
        Label = unknownElement.Label ?? unknownElement.DataPath,
        DataType = FieldDataType.Text,  // 降级为纯文本
        IsRequired = false
    };
}
```

### 4.4 新增元素的适配升级路径

```
新元素加入契约
    │
    ▼
┌──────────────────────────────────────────────────────┐
│ Phase 1: 自动 Partial 适配                            │
│ - 有 dataPath 的新元素自动降级为 string 处理           │
│ - 用户可通过 Excel/DB 填充字符串值                     │
│ - 渲染可能不完美，但数据不丢失                         │
└──────────────────────────────────────────────────────┘
    │ 开发者实现专用适配逻辑
    ▼
┌──────────────────────────────────────────────────────┐
│ Phase 2: Full 适配                                    │
│ - 注册专用的 ElementAdapter                           │
│ - 类型感知的校验（如新增的 RatingElement 校验 1-5）    │
│ - 专用的 UI 控件（如星级选择器）                       │
└──────────────────────────────────────────────────────┘
```

### 4.5 版本兼容矩阵示例

假设未来 v2.3.0 新增 `template.element.rating`（评分）和 `template.element.color`（颜色选择器）：

| 元素 | 适配器 v0.5 (当前) | 适配器 v0.6 (升级后) |
|------|-------------------|---------------------|
| rating | Partial: 作为 string "4" | Full: 数字 1-5 校验 |
| color | Partial: 作为 string "#FF0000" | Full: 颜色格式校验 |

用户在 v0.5 适配器中仍可通过 Excel 填入 "4" 或 "#FF0000"，只是没有类型校验。升级到 v0.6 后获得完整校验能力。

### 4.6 适配组注册表

```csharp
public static class ElementGroupRegistry
{
    private static readonly Dictionary<string, AdaptationGroup> _groups = new()
    {
        // Inputable
        ["template.element.text"] = AdaptationGroup.Inputable,
        ["template.element.dropdown"] = AdaptationGroup.Inputable,
        ["template.element.number"] = AdaptationGroup.Inputable,
        ["template.element.date"] = AdaptationGroup.Inputable,
        ["template.element.checkbox"] = AdaptationGroup.Inputable,
        
        // Renderable
        ["template.element.table"] = AdaptationGroup.Renderable,
        ["template.element.barcode"] = AdaptationGroup.Renderable,
        ["template.element.qrcode"] = AdaptationGroup.Renderable,
        ["template.element.chart"] = AdaptationGroup.Renderable,
        ["template.element.image"] = AdaptationGroup.Renderable,
        ["template.element.signature"] = AdaptationGroup.Renderable,
        ["template.element.radio"] = AdaptationGroup.Renderable,
        ["template.element.hyperlink"] = AdaptationGroup.Renderable,
        
        // Structural
        ["template.element.container"] = AdaptationGroup.Structural,
        ["template.element.repeat"] = AdaptationGroup.Structural,
        ["template.element.header"] = AdaptationGroup.Structural,
        ["template.element.footer"] = AdaptationGroup.Structural,
        ["template.element.pagenumber"] = AdaptationGroup.Structural,
        
        // Decorative
        ["template.element.line"] = AdaptationGroup.Decorative,
        ["template.element.shape"] = AdaptationGroup.Decorative,
        ["template.element.divider"] = AdaptationGroup.Decorative,
        ["template.element.watermark"] = AdaptationGroup.Decorative,
        ["template.element.icon"] = AdaptationGroup.Decorative,
    };
    
    public static AdaptationGroup Classify(string elementType)
    {
        if (_groups.TryGetValue(elementType, out var group))
            return group;
        
        // 未知元素的启发式分类
        return AdaptationGroup.Unknown;
    }
}

public enum AdaptationGroup
{
    Inputable,
    Renderable,
    Structural,
    Decorative,
    Unknown  // 触发 Fallback 逻辑
}
```

---

## 5. 扁平化时的分组过滤

```csharp
public class TemplateFlattenService
{
    public TemplateFieldSchema Flatten(ExternalTemplateDefinition template)
    {
        var fields = new List<FlatField>();
        
        foreach (var element in template.Elements)
        {
            var group = ElementGroupRegistry.Classify(element.Type);
            
            switch (group)
            {
                case AdaptationGroup.Inputable:
                    if (element.Group == ElementGroup.Editable)
                        fields.Add(FlattenInputable(element));
                    break;
                    
                case AdaptationGroup.Renderable:
                    fields.AddRange(FlattenRenderable(element));
                    break;
                    
                case AdaptationGroup.Structural:
                    fields.AddRange(FlattenStructural(element));  // 递归子元素
                    break;
                    
                case AdaptationGroup.Decorative:
                    break;  // 忽略
                    
                case AdaptationGroup.Unknown:
                    var fallback = FallbackFlatten(element);
                    if (fallback != null) fields.Add(fallback);
                    break;
            }
        }
        
        return new TemplateFieldSchema { Fields = fields };
    }
}
```

---

## 6. 冗余能力设计

为确保向后兼容的部分适配能力，系统提供以下冗余机制：

| 机制 | 说明 |
|------|------|
| String Fallback | 任何有 dataPath 的未知元素都可作为 string 处理 |
| Label 继承 | 未知元素的 label 字段用于 Excel 列头和 UI 显示 |
| Children 递归 | 未知容器类元素的 children 仍会被递归处理 |
| 宽松校验模式 | 导入时可选择「宽松模式」跳过未知类型的校验 |
| 适配级别标记 | UI 中标记 Partial 适配的字段（如黄色警告图标） |

这意味着即使契约新增了 5 种元素类型，只要它们遵循公共字段规范（有 dataPath 和 label），现有适配器无需任何代码修改就能提供基础的数据填充能力。
