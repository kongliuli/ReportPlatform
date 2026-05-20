# 风险报告 R-004：扩展性瓶颈 — MainViewModel God Object 与静态注册表

| 字段 | 值 |
|------|-----|
| 风险编号 | R-004 |
| 风险等级 | 🟡 中 |
| 影响模块 | Contracts + Generators |
| 发现日期 | 2026-05-20 |
| 状态 | 待修复 |

---

## 1. 风险描述

项目在两个层面存在扩展性瓶颈：

1. **Generators 层**：`MainViewModel` 承担了模板加载、适配器管理、标签页生命周期、导出路由等多重职责，新增适配器类型必须修改该类，违反开闭原则。
2. **Contracts 层**：`ElementJsonConverter.WebShortTypeMap` 和 `ElementGroupRegistry` 使用静态注册表，新增元素类型必须修改现有代码，无法通过扩展方式注册。

---

## 2. 涉及代码

### 2.1 MainViewModel God Object

[MainViewModel.cs](../../Generators/ReportDataMaker/ViewModels/MainViewModel.cs) 的职责清单：

| 职责 | 代码位置 | 依赖服务数 |
|------|----------|-----------|
| 模板加载与初始化 | `ExecuteLoadTemplate()` + `LoadTemplate()` | 3 |
| 适配器标签页创建 | `ExecuteAddExcelAdapter()` + `ExecuteAddDbAdapter()` + `ExecuteEditContext()` | 3 |
| 导出功能路由 | `ExecuteExportPdf()` + `ExecuteBatchExport()` + `ExecutePrintPreview()` | 3 |
| 标签页生命周期 | `OnTabCloseRequested()` + `Tabs` 集合管理 | 0 |
| 上下文数据回填 | `OnContextApplied()` | 1 |
| 状态管理 | `StatusText` + `StatusInfo` + `StatusColor` | 0 |
| 保存功能 | `ExecuteSaveAsync()` | 1 |

**构造函数注入 11 个服务**（通过 3 个聚合对象）：

```csharp
public MainViewModel(
    TemplateServices templateServices,    // 3 个服务
    AdapterServices adapterServices,      // 5 个服务
    ExportServices exportServices,        // 3 个服务
    IDialogService dialogService)
```

**10 个命令属性**：

```csharp
LoadTemplateCommand, ToggleSidePanelCommand,
AddExcelAdapterCommand, AddDbAdapterCommand,
EditContextCommand, ExportPdfCommand,
BatchExportCommand, PrintPreviewCommand,
SaveCommand, ExitCommand
```

### 2.2 新增适配器需修改 MainViewModel

以新增 API 适配器为例，需要修改：

```csharp
// 1. 添加字段
private readonly ApiAdapterFactory _apiFactory;

// 2. 修改构造函数
public MainViewModel(..., ApiAdapterFactory apiFactory)

// 3. 添加命令
public RelayCommand AddApiAdapterCommand { get; }

// 4. 添加命令初始化
AddApiAdapterCommand = new RelayCommand(_ => ExecuteAddApiAdapter(), _ => IsTemplateLoaded);

// 5. 添加执行方法
private void ExecuteAddApiAdapter() { ... }

// 6. 修改 App.xaml.cs 注册服务
services.AddSingleton<ApiAdapterFactory>();
```

### 2.3 静态注册表

**ElementJsonConverter.WebShortTypeMap**（[ElementJsonConverter.cs](../../Contracts/Converters/ElementJsonConverter.cs)）：

```csharp
private static readonly Dictionary<string, Type> WebShortTypeMap = new(...)
{
    ["text"] = typeof(TextElement),
    ["line"] = typeof(LineElement),
    // ... 22 种映射
};
```

**ElementGroupRegistry**（[ElementGroupRegistry.cs](../../Contracts/Registry/ElementGroupRegistry.cs)）：

```csharp
static ElementGroupRegistry()
{
    RegisterGroup(ElementAdaptationGroup.Basic, typeof(LineElement), ...);
    RegisterGroup(ElementAdaptationGroup.Form, typeof(TextElement), ...);
    // ...
}
```

### 2.4 新增元素类型需修改的文件

| 步骤 | 修改文件 | 违反 OCP |
|------|----------|----------|
| 1. 创建元素类 | 新增文件 | ✅ |
| 2. 注册类型映射 | `ElementJsonConverter.WebShortTypeMap` | ❌ |
| 3. 注册适配分组 | `ElementGroupRegistry` 静态构造函数 | ❌ |
| 4. 创建外部元素 | `ExternalExtendedElements.cs` | ❌ |
| 5. 添加类型转换 | `ReportExternalElementConverter` | ❌ |
| 6. 添加 PDF 渲染 | `PdfElementRenderer`（×2） | ❌ |

**共需修改 5 个现有文件**，严重违反开闭原则。

---

## 3. 风险场景

### 3.1 新增适配器类型的开发摩擦

每次新增适配器都需要：
- 修改 MainViewModel（添加命令 + 执行方法）
- 修改 App.xaml.cs（注册 DI 服务）
- 创建 TabViewModel + Tab View
- 创建适配器服务 + 工厂 + 配置类

开发摩擦高，且容易遗漏步骤。

### 3.2 第三方扩展不可能

静态注册表和 MainViewModel 的硬编码适配器逻辑使得第三方无法通过插件方式扩展：
- 无法注册自定义元素类型
- 无法注册自定义适配器
- 无法添加自定义标签页

### 3.3 标签页插入位置脆弱

```csharp
Tabs.Insert(Tabs.Count - 1, tab);  // 在导出标签页前插入
```

硬编码假设导出标签页始终在最后，如果标签页顺序变化，插入位置将错误。

### 3.4 单元测试困难

- `ElementGroupRegistry` 的静态构造函数无法在测试中重置
- `MainViewModel` 的 11 个依赖使得 Mock 设置繁琐
- `ElementJsonConverter.WebShortTypeMap` 是 private static，无法在测试中扩展

---

## 4. 影响范围

| 影响维度 | 评估 |
|----------|------|
| 开发效率 | 🟡 中 — 新增功能需修改多处 |
| 第三方扩展 | 🔴 高 — 无法插件化扩展 |
| 代码可测试性 | 🟡 中 — 静态注册表和 God Object 增加测试难度 |
| 维护成本 | 🟡 中 — 适配器逻辑集中在 MainViewModel |

---

## 5. 修复方案

### 5.1 MainViewModel 拆分

引入管理器类分担职责：

```csharp
MainViewModel (协调者)
├── TemplateManager      — 模板加载/初始化/上下文填充
├── AdapterManager       — 适配器增删/标签页创建/数据回填
├── ExportManager        — PDF导出/批量导出/打印预览
└── TabManager           — 标签页生命周期/关闭/切换
```

### 5.2 适配器插件化

定义适配器插件接口：

```csharp
public interface IAdapterPlugin
{
    AdapterType Type { get; }
    string DisplayName { get; }
    TabViewModelBase CreateTab(ExternalTemplateDefinition template, IServiceProvider services);
    void RegisterServices(IServiceCollection services);
}
```

MainViewModel 通过遍历已注册的 `IAdapterPlugin` 自动创建适配器命令和标签页，无需硬编码。

### 5.3 元素类型动态注册

将静态注册表改为可动态注册：

```csharp
public static class ElementGroupRegistry
{
    public static void RegisterElement(ElementAdaptationGroup group, Type elementType);
    public static void RegisterTypeMapping(string shortName, Type elementType);
}
```

或使用 Attribute 标注：

```csharp
[ElementAdaptation(ElementAdaptationGroup.Form, ShortName = "text")]
public class TextElement : ExternalElementBase { }
```

启动时通过反射扫描程序集自动注册。

---

## 6. 建议优先级

**P2 — 长期修复（3-6 月）**

建议分阶段实施：
1. 先拆分 MainViewModel（中期，降低复杂度）
2. 再实现适配器插件化（长期，支持扩展）
3. 最后改造元素类型注册机制（长期，支持第三方扩展）
