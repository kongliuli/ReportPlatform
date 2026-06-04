# 契约融合实施方案

> 目标：将 `契约/20260507` 中定义的模板结构契约 v2.1.0 完整融入 ReportDataMaker 项目
> 日期：2026-05-07

---

## 现状总结

**契约侧**：定义了 23 种元素类型、双 `$type` 体系（WPF 原生格式 + Web 短格式）、数据绑定规范、序列化规则。

**项目侧**：当前仅支持 6 种元素的完整渲染（Text、Line、Number、Date、Dropdown、Table），`ExternalElementConverter` 只识别 WPF 原生格式，数据绑定未实现。

---

## 实施阶段

### 阶段一：模型层对齐（Models）

**目标**：让项目能正确反序列化契约中所有 23 种元素类型。

#### 1.1 新增元素模型类

在 `Models/` 目录下新建 `ExternalExtendedElements.cs`，添加以下类（均继承 `ExternalElementBase`）：

| 类名 | 对应契约类型 | 关键字段 |
|------|-------------|---------|
| `ExternalImageElement` | image | `Src`, `Fit`, `MaintainAspectRatio`, `AltText` |
| `ExternalShapeElement` | shape | `ShapeType`, `FillColor`, `StrokeColor`, `StrokeWidth` |
| `ExternalDividerElement` | divider | `Thickness`, `Color`, `Style` |
| `ExternalCheckboxElement` | checkbox | `Checked`, `CheckColor` |
| `ExternalRadioElement` | radio | `GroupName`, `Value`, `Checked` |
| `ExternalSignatureElement` | signature | `Placeholder`, `LineColor`, `LineWidth` |
| `ExternalBarcodeElement` | barcode | `Value`, `Format`, `ShowText`, `LineColor` |
| `ExternalQrCodeElement` | qrcode | `Value`, `ErrorCorrectionLevel`, `Margin`, `Color` |
| `ExternalChartElement` | chart | `ChartType`, `Title`, `DataSource`, `ShowLegend`, `ShowGrid` |
| `ExternalContainerElement` | container | `Children` (List<ExternalElementBase>), `Layout`, `Padding`, `ClipContent` |
| `ExternalRepeatElement` | repeat | `DataSource`, `ItemTemplate`, `Direction`, `Gap` |
| `ExternalHeaderElement` | header | `Children`, `ShowOnFirstPage`, `ShowOnAllPages` |
| `ExternalFooterElement` | footer | `Children`, `ShowOnLastPage`, `ShowOnAllPages` |
| `ExternalPageNumberElement` | pageNumber | `Format`, `StartPage` |
| `ExternalWatermarkElement` | watermark | `Text`, `Angle`, `Color`, `Repeat` |
| `ExternalIconElement` | icon | `IconName`, `IconSet`, `Color`, `Size` |
| `ExternalHyperlinkElement` | hyperlink | `Text`, `Url`, `OpenInNewTab` |

#### 1.2 补充 ExternalTemplateDefinition 缺失字段

```csharp
// 新增字段
public string Id { get; set; }
public int Version { get; set; }
public string HospitalId { get; set; }
public bool EnableGlobalFontSize { get; set; }
public List<DataBindingDefinition> DataBindings { get; set; }
```

#### 1.3 新增 DataBindingDefinition 模型

```csharp
public class DataBindingDefinition
{
    public string Id { get; set; }
    public string ElementId { get; set; }
    public string DataPath { get; set; }
    public string BindingType { get; set; }  // text, image, visibility, repeat
    public string FormatString { get; set; }
    public string DefaultValue { get; set; }
}
```

---

### 阶段二：转换器升级（ExternalElementConverter）

**目标**：支持双 `$type` 格式识别。

#### 2.1 重构 MapType 方法

当前逻辑只处理 `Xinglin.Core.Elements.{Name}, Xinglin.Core` 格式。需增加 `template.element.*` 短格式分支：

```csharp
private Type MapType(string typeString)
{
    // 优先匹配 Web 短格式
    if (typeString.StartsWith("template.element."))
    {
        var shortName = typeString["template.element.".Length..];
        return shortName switch
        {
            "text" => typeof(ExternalTextElement),
            "image" => typeof(ExternalImageElement),
            "line" => typeof(ExternalLineElement),
            "table" => typeof(ExternalTableElement),
            "date" => typeof(ExternalDateElement),
            "dropdown" => typeof(ExternalDropdownElement),
            "number" => typeof(ExternalNumberElement),
            "shape" => typeof(ExternalShapeElement),
            "divider" => typeof(ExternalDividerElement),
            "checkbox" => typeof(ExternalCheckboxElement),
            "radio" => typeof(ExternalRadioElement),
            "signature" => typeof(ExternalSignatureElement),
            "barcode" => typeof(ExternalBarcodeElement),
            "qrcode" => typeof(ExternalQrCodeElement),
            "chart" => typeof(ExternalChartElement),
            "container" => typeof(ExternalContainerElement),
            "repeat" => typeof(ExternalRepeatElement),
            "header" => typeof(ExternalHeaderElement),
            "footer" => typeof(ExternalFooterElement),
            "pageNumber" => typeof(ExternalPageNumberElement),
            "watermark" => typeof(ExternalWatermarkElement),
            "icon" => typeof(ExternalIconElement),
            "hyperlink" => typeof(ExternalHyperlinkElement),
            _ => typeof(ExternalElementBase)
        };
    }

    // 原有 WPF 格式匹配逻辑（保留并扩展）
    var typeName = typeString.Split(',')[0].Trim();
    var simpleTypeName = typeName.Split('.')[^1];

    return simpleTypeName switch
    {
        "TextElement" or "LabelElement" or "LabelInputBoxElement" => typeof(ExternalTextElement),
        "ImageElement" => typeof(ExternalImageElement),
        "LineElement" => typeof(ExternalLineElement),
        "TableElement" => typeof(ExternalTableElement),
        "DateElement" => typeof(ExternalDateElement),
        "DropdownElement" => typeof(ExternalDropdownElement),
        "NumberElement" => typeof(ExternalNumberElement),
        "RectangleElement" or "EllipseElement" => typeof(ExternalShapeElement),
        "BarcodeElement" => typeof(ExternalBarcodeElement),
        "SignatureElement" => typeof(ExternalSignatureElement),
        "AutoNumberElement" => typeof(ExternalPageNumberElement),
        _ => typeof(ExternalElementBase)
    };
}
```

#### 2.2 处理 ContainerElement 的递归子元素

`ContainerElement`、`HeaderElement`、`FooterElement` 包含 `children` 数组，需要在反序列化时递归处理。在 `ReadJson` 中增加对 `children` 字段的递归转换逻辑。

---

### 阶段三：渲染器扩展（CanvasRenderer）

**目标**：为新元素类型添加 WPF 渲染能力。按优先级分批实现。

#### 3.1 第一批（高优先级 — 契约模板直接使用）

| 元素 | 渲染方式 |
|------|---------|
| `ExternalImageElement` | 使用 `Image` 控件 + `BitmapImage`，支持 URL 和 Base64 |
| `ExternalShapeElement` | 使用 `Rectangle`/`Ellipse`/`Polygon` |
| `ExternalDividerElement` | 使用 `Line` 或 `Border`（高度 1px） |

#### 3.2 第二批（中优先级 — 完善输入体验）

| 元素 | 渲染方式 |
|------|---------|
| `ExternalCheckboxElement` | `CheckBox` 控件 |
| `ExternalRadioElement` | `RadioButton` 控件 |
| `ExternalSignatureElement` | 占位 `Border` + 提示文字（签名需要手写板支持） |

#### 3.3 第三批（低优先级 — 高级功能）

| 元素 | 渲染方式 |
|------|---------|
| `ExternalBarcodeElement` | 引入 ZXing.Net 生成条码图片 |
| `ExternalQrCodeElement` | 引入 ZXing.Net 生成二维码 |
| `ExternalWatermarkElement` | 半透明旋转 TextBlock 覆盖层 |
| `ExternalPageNumberElement` | TextBlock 显示页码文本 |
| `ExternalContainerElement` | Canvas 嵌套，递归渲染 children |
| `ExternalRepeatElement` | StackPanel 循环渲染 itemTemplate |
| `ExternalHeaderElement` / `ExternalFooterElement` | 固定位置 Canvas 区域 |
| `ExternalChartElement` | 占位（需引入图表库如 LiveCharts） |
| `ExternalIconElement` | 占位或使用 Unicode 符号 |
| `ExternalHyperlinkElement` | `Hyperlink` 控件 |

#### 3.4 渲染器 switch 扩展

```csharp
UIElement result = element switch
{
    ExternalTextElement textEl => RenderTextElement(textEl, data),
    ExternalLineElement lineEl => RenderLineElement(lineEl),
    ExternalImageElement imgEl => RenderImageElement(imgEl, data),
    ExternalShapeElement shapeEl => RenderShapeElement(shapeEl),
    ExternalDividerElement divEl => RenderDividerElement(divEl),
    ExternalNumberElement numEl => RenderNumberElement(numEl, data),
    ExternalDateElement dateEl => RenderDateElement(dateEl, data),
    ExternalDropdownElement dropEl => RenderDropdownElement(dropEl, data),
    ExternalTableElement tableEl => RenderTableElement(tableEl, data),
    ExternalCheckboxElement cbEl => RenderCheckboxElement(cbEl),
    ExternalRadioElement radioEl => RenderRadioElement(radioEl),
    ExternalBarcodeElement barcodeEl => RenderBarcodeElement(barcodeEl),
    ExternalQrCodeElement qrEl => RenderQrCodeElement(qrEl),
    ExternalWatermarkElement wmEl => RenderWatermarkElement(wmEl),
    ExternalContainerElement containerEl => RenderContainerElement(containerEl, data),
    _ => RenderPlaceholder(element)  // 未实现类型显示占位框
};
```

新增 `RenderPlaceholder` 方法：对未实现的元素类型渲染一个虚线边框 + 类型名称，避免静默丢失。

---

### 阶段四：数据绑定实现（DataBindingService）

**目标**：实现契约中定义的数据路径解析和绑定机制。

#### 4.1 数据上下文模型

```csharp
public class ReportDataContext
{
    public Dictionary<string, object> Patient { get; set; }
    public Dictionary<string, object> Report { get; set; }
    public Dictionary<string, object> Doctor { get; set; }
    public Dictionary<string, object> Hospital { get; set; }
    public List<Dictionary<string, object>> Items { get; set; }
}
```

#### 4.2 路径解析器

```csharp
public class DataPathResolver
{
    /// <summary>
    /// 解析 "patient.name" 这样的路径，从数据上下文中取值
    /// </summary>
    public object Resolve(string dataPath, ReportDataContext context);
}
```

支持的路径格式：
- `patient.name` → 简单属性访问
- `items[0].testName` → 数组索引 + 属性
- `items[*].result` → 数组遍历（用于 RepeatElement）

#### 4.3 绑定执行流程

```
1. 加载模板 → 解析 dataBindings 列表
2. 加载数据 → 构建 ReportDataContext
3. 遍历 dataBindings：
   a. 通过 elementId 找到目标元素
   b. 通过 DataPathResolver 解析 dataPath 取值
   c. 应用 formatString 格式化
   d. 根据 bindingType 设置元素属性（text/src/visibility）
4. 遍历元素自身的内联绑定（isDataBound=true 的元素）
5. 触发 Canvas 重新渲染
```

---

### 阶段五：输入控件扩展（InputControlGenerator）

**目标**：为新元素类型生成对应的输入控件。

| 元素类型 | 输入控件 |
|---------|---------|
| `ExternalImageElement` | 文件选择按钮 + 图片预览 |
| `ExternalCheckboxElement` | CheckBox |
| `ExternalRadioElement` | RadioButton（同 groupName 分组） |
| `ExternalSignatureElement` | 签名区域（InkCanvas 或文件选择） |
| `ExternalBarcodeElement` | TextBox（输入条码值，自动生成预览） |
| `ExternalQrCodeElement` | TextBox（输入内容，自动生成预览） |

---

### 阶段六：验证与测试

#### 6.1 导入契约模板测试

将 `契约/20260507/templates/` 下的四个 JSON 文件复制到 `ReportDataMaker/Templates/`，逐一验证：

- [ ] 检验报告单.json — 加载成功，所有元素正确分类和渲染
- [ ] 影像报告单.json — Image 元素正确显示占位或图片
- [ ] 处方单.json — 横向布局正确，表格渲染正常
- [ ] 门诊病历.json — 多文本区域正确渲染

#### 6.2 数据绑定测试

构造测试数据 JSON：

```json
{
  "patient": { "name": "张三", "gender": "男", "age": "45岁" },
  "report": { "reportNo": "R20260507001", "reportDate": "2026-05-07" },
  "doctor": { "name": "李医生" },
  "items": [
    { "testName": "血红蛋白", "result": "135", "reference": "120-160", "unit": "g/L" }
  ]
}
```

验证数据正确填充到模板对应位置。

#### 6.3 双格式兼容测试

- [ ] 加载使用 `Xinglin.Core.Elements.*` 格式的旧模板 — 正常工作
- [ ] 加载使用 `template.element.*` 格式的新模板 — 正常工作
- [ ] 混合格式模板 — 正常工作

---

## 实施顺序与依赖关系

```
阶段一（模型层）──→ 阶段二（转换器）──→ 阶段三（渲染器）
                                    │
                                    └──→ 阶段四（数据绑定）
                                    │
                                    └──→ 阶段五（输入控件）
                                              │
                                              └──→ 阶段六（验证）
```

建议按阶段顺序执行，每完成一个阶段做一次编译验证。阶段三、四、五可以并行推进（三者互不依赖），但都依赖阶段一和二完成。

---

## 需要引入的 NuGet 包

| 包名 | 用途 | 阶段 |
|------|------|------|
| ZXing.Net.Bindings.Windows.Compatibility | 条码/二维码生成 | 阶段三（第三批） |

其余功能使用 WPF 内置控件即可实现，无需额外依赖。

---

## 文件变更清单

| 操作 | 文件路径 | 说明 |
|------|---------|------|
| 新建 | `Models/ExternalExtendedElements.cs` | 17 个新元素模型类 |
| 新建 | `Models/DataBindingDefinition.cs` | 数据绑定定义模型 |
| 新建 | `Models/ReportDataContext.cs` | 数据上下文模型 |
| 新建 | `Services/DataPathResolver.cs` | 数据路径解析器 |
| 修改 | `Models/ExternalTemplateModels.cs` | 补充 Id、Version 等字段 |
| 修改 | `Services/ExternalElementConverter.cs` | 支持双格式 + 新类型映射 |
| 修改 | `Services/CanvasRenderer.cs` | 新增渲染分支 |
| 修改 | `Services/InputControlGenerator.cs` | 新增输入控件类型 |
| 修改 | `Services/DataBindingService.cs` | 实现绑定逻辑 |
| 复制 | `Templates/影像报告单.json` 等 | 契约模板作为测试用例 |
