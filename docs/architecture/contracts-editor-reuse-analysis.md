# 架构报告：Contracts/Editor 可复用抽象分析

> 目标：识别 Contracts 和 Editor 中可直接提供给 Generators 层调用的抽象，建立清晰的依赖边界
> 日期：2026-05-08

---

## 1. 当前依赖关系

```
Editor.Server ──▶ Editor.Core ──▶ Contracts
                                       ▲
                                       │
Generators (ReportDataMaker) ──────────┘ (目前仅引用 Contracts)
```

**现状问题**：ReportDataMaker 虽然引用了 Contracts，但实际上自行实现了一套完整的模板模型体系（`ExternalElementBase` 及其 20+ 子类），与 Editor 侧的 `ContentJson` 结构虽然兼容但完全独立维护，存在重复和潜在的不一致风险。

---

## 2. Contracts 层可直接复用的资产

Contracts 是零依赖的纯 DTO 程序集，Generators 可无成本引用。

| 类型 | 用途 | Generator 场景 |
|------|------|---------------|
| `TemplateDto` | 模板摘要 | 模板列表展示、选择 |
| `TemplateDetailDto` | 含 ContentJson 的完整模板 | 加载模板进行数据填充 |
| `TemplateVersionDto` | 版本信息 | 版本选择、回溯 |
| `ApiResponse<T>` | 统一响应包装 | 调用 Editor API 时的反序列化 |
| `PagedResponse<T>` | 分页响应 | 模板列表分页 |
| `TemplateFilterRequest` | 查询过滤 | 按类型/医院筛选模板 |

**建议**：Generators 应通过 Contracts 中的 DTO 与 Editor API 交互，而非直接访问数据库。

---

## 3. Editor.Core SharedInterfaces — 为 Generator 设计的抽象

Editor.Core 已预留了 `SharedInterfaces` 命名空间，明确为跨层复用设计：

```csharp
// 模板 JSON 序列化/反序列化
public interface IJsonTemplateSerializer
{
    string Serialize<T>(T obj);
    T Deserialize<T>(string json);
    object Deserialize(string json, Type type);
}

// 数据绑定引擎 — 将实际数据合并到模板
public interface IDataBindingEngine
{
    void ApplyDataBinding(object template, object sampleData);
}

// PDF 渲染器
public interface IPdfSharpTemplateRenderer
{
    byte[] RenderToPdf(object templateDefinition);
}
```

**当前状态**：这三个接口在 Editor.Core 中仅有 Stub 实现（空操作/直通）。真正的实现应由 Generators 提供。

---

## 4. 建议新增的可复用抽象

基于 ReportDataMaker 中已实现但未抽象的能力，建议将以下内容提升到共享层：

### 4.1 模板模型定义（最高优先级）

当前 ReportDataMaker 自行定义了 `ExternalTemplateDefinition` 和 20+ 种 `ExternalElementBase` 子类。这套模型应提升为共享契约：

```
建议位置：Contracts/TemplateModels/
├── ExternalTemplateDefinition.cs
├── ExternalElementBase.cs
├── Elements/
│   ├── ExternalTextElement.cs
│   ├── ExternalTableElement.cs
│   ├── ExternalImageElement.cs
│   └── ... (20+ 元素类型)
├── DataBindingDefinition.cs
└── ElementGroup.cs (enum: Fixed, Editable, DataAdapter)
```

**收益**：
- Editor 和所有 Generators 共享同一套模板模型
- 契约变更有单一来源
- 消除 ReportDataMaker 中 `ExternalElementConverter` 的重复维护

### 4.2 数据适配器接口

```csharp
// 建议位置：Contracts/Adapters/AdapterType.cs
public enum AdapterType
{
    Excel,
    Database,
    Api
}

// 建议位置：Contracts/Adapters/IDataAdapter.cs
public interface IDataAdapter
{
    string AdapterName { get; }
    AdapterType Type { get; }
    IReadOnlyList<string> TargetDataPaths { get; }
    Task<Dictionary<string, object>> ReadDataAsync();
}

// 建议位置：Contracts/Adapters/IDataAdapterFactory.cs
public interface IDataAdapterFactory
{
    AdapterType Type { get; }
    IDataAdapter Create(AdapterConfigBase config);
}
```

### 4.3 模板加载与分类服务接口

```csharp
// 建议位置：Contracts/Services/ITemplateClassifier.cs
public interface ITemplateClassifier
{
    void ClassifyElements(ExternalTemplateDefinition template, ISet<string> adapterPaths);
}

// 建议位置：Contracts/Services/ITemplateResolver.cs  
public interface ITemplateResolver
{
    Task<ExternalTemplateDefinition> ResolveAsync(Guid templateId);
    Task<ExternalTemplateDefinition> ResolveFromJsonAsync(string json);
}
```

---

## 5. 推荐的目标架构

```
┌─────────────────────────────────────────────────────────────────┐
│                         Contracts                                │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────────┐  │
│  │   DTOs       │  │ TemplateModels│  │  Adapter Interfaces   │  │
│  │   Requests   │  │ (元素定义)    │  │  Service Interfaces   │  │
│  │   Responses  │  │              │  │                       │  │
│  └──────────────┘  └──────────────┘  └───────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
         ▲                    ▲                     ▲
         │                    │                     │
    ┌────┴────┐          ┌────┴────┐          ┌────┴────┐
    │ Editor  │          │Generator│          │Adapters │
    │ (Core)  │          │  (WPF)  │          │ (实现)  │
    └─────────┘          └─────────┘          └─────────┘
```

---

## 6. 迁移路径

| 阶段 | 动作 | 影响 |
|------|------|------|
| Phase 1 | 将 `ExternalElementBase` 体系从 ReportDataMaker 移入 Contracts | 两侧共享模型 |
| Phase 2 | 将 `IDataAdapter` 接口移入 Contracts | 适配器可独立开发 |
| Phase 3 | 将 `ExternalElementConverter` 移入 Contracts | JSON 反序列化统一 |
| Phase 4 | Editor.Core 的 SharedInterfaces 实现由 Generator 提供 | 打通 PDF 渲染链路 |

---

## 7. 风险与约束

1. **模型迁移的破坏性**：将 20+ 个类从 ReportDataMaker 移到 Contracts 需要大量 namespace 变更，建议一次性完成
2. **Newtonsoft.Json 依赖**：`ExternalElementConverter` 依赖 Newtonsoft.Json 的 `$type` 机制，Contracts 目前是零依赖的，引入 Newtonsoft 需要评估
3. **版本同步**：模板模型变更将同时影响 Editor 和所有 Generators，需要严格的契约版本管理
4. **WPF 耦合**：`TemplatePreviewService` 返回 `UIElement`，这部分不可共享，需保留在 Generator 内部
