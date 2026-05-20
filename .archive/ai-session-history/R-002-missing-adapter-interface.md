# 风险报告 R-002：适配器接口缺失与不统一

| 字段 | 值 |
|------|-----|
| 风险编号 | R-002 |
| 风险等级 | 🟡 中 |
| 影响模块 | Contracts + Generators |
| 发现日期 | 2026-05-20 |
| 状态 | 待修复 |

---

## 1. 风险描述

项目 README 和 CODE_WIKI 中描述了 `IDataAdapter` 接口作为适配器的统一契约，但实际代码中该接口**未在 Contracts 层定义**。三种适配器（Excel/Database/Context）各自独立实现，方法签名、异步模式、功能支持程度均不一致，缺乏统一的接口约束。

---

## 2. 涉及代码

### 2.1 文档中描述的接口

README.md 中定义的 `IDataAdapter`：

```csharp
public interface IDataAdapter
{
    string AdapterId { get; }
    string AdapterName { get; }
    AdapterType Type { get; }
    IReadOnlyList<string> TargetDataPaths { get; }
    Task<AdapterResult> ReadDataAsync();
    Task<AdapterResult> ReadBatchDataAsync();
    Task<ValidationResult> ValidateConfigAsync();
}
```

### 2.2 实际实现对比

| 维度 | Excel 适配器 | 数据库适配器 | 上下文适配器 |
|------|-------------|-------------|-------------|
| 工厂类 | `ExcelAdapterFactory` | `DatabaseAdapterFactory` | `ContextAdapterFactory` |
| 读取方法 | `ReadData()` **同步** | `ReadDataAsync()` **异步** | `FillContext()` **同步** |
| 批量读取 | `ReadBatchData()` **同步** | `ReadBatchDataAsync()` **异步** | ❌ 不支持 |
| 校验方法 | `Validate()` 返回 `ValidationReport` | `ValidateConfigAsync()` 返回 `ValidationResult` | ❌ 不支持 |
| 配置类 | `ExcelAdapterConfig` | `DatabaseAdapterConfig` | `ContextAdapterConfig` |
| 结果类型 | `AdapterResult` | `AdapterResult` | `AdapterResult` |
| 接口约束 | 无 | 无 | 无 |

### 2.3 方法签名差异

**Excel 适配器**（[ExcelAdapterFactory.cs](../../Generators/ReportDataMaker/Services/ExcelAdapter/ExcelAdapterFactory.cs)）：

```csharp
public AdapterResult ReadData(string filePath, ExcelTemplateSchema schema)
public AdapterResult ReadBatchData(string filePath, ExcelTemplateSchema schema)
public ValidationReport Validate(TemplateFieldSchema schema, List<Dictionary<string, object>> batchData)
```

**数据库适配器**（[DatabaseAdapterFactory.cs](../../Generators/ReportDataMaker/Services/DatabaseAdapter/DatabaseAdapterFactory.cs)）：

```csharp
public async Task<AdapterResult> ExecuteQueryAsync(DatabaseAdapterConfig config)
public async Task<AdapterResult> PreviewAsync(DatabaseAdapterConfig config, int limit = 10)
public async Task<ValidationResult> ValidateConfigAsync(DatabaseAdapterConfig config)
```

**上下文适配器**（[ContextAdapterFactory.cs](../../Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterFactory.cs)）：

```csharp
public AdapterResult Fill(ExternalTemplateDefinition template, ContextAdapterConfig config)
public List<string> DetectUnconfiguredFields(...)
```

---

## 3. 风险场景

### 3.1 无法多态管理适配器

由于没有统一接口，MainViewModel 无法将三种适配器存储在同一集合中，也无法统一调用读取/校验方法：

```csharp
// 当前：三种适配器分别持有
private readonly ExcelAdapterFactory _excelFactory;
private readonly DatabaseAdapterFactory _dbFactory;
private readonly ContextAdapterFactory _contextFactory;

// 理想：统一管理
private readonly Dictionary<AdapterType, IDataAdapter> _adapters;
```

### 3.2 新增适配器类型无实现指南

`AdapterType.Api` 已在枚举中预留，但没有接口定义，开发者不知道 API 适配器应该实现哪些方法、返回什么类型。

### 3.3 校验结果类型不统一

- Excel 适配器返回 `ValidationReport`（Generators 层定义）
- 数据库适配器返回 `ValidationResult`（Contracts 层定义）
- 上下文适配器无校验功能

UI 层需要针对不同适配器类型编写不同的校验结果处理逻辑。

### 3.4 同步/异步混用

Excel 和上下文适配器使用同步方法，数据库适配器使用异步方法。在 WPF 应用中，同步 I/O 会阻塞 UI 线程，影响用户体验。

---

## 4. 影响范围

| 影响维度 | 评估 |
|----------|------|
| 代码一致性 | 🟡 中 — 三种适配器风格不统一 |
| 扩展性 | 🔴 高 — 新增适配器无契约指导 |
| 可维护性 | 🟡 中 — 适配器逻辑分散，无法统一管理 |
| UI 层复杂度 | 🟡 中 — 需要针对每种适配器编写不同处理逻辑 |

---

## 5. 修复方案

### 方案 A：在 Contracts 层定义 IDataAdapter 接口（推荐）

```csharp
// Contracts/Models/Adapters/IDataAdapter.cs
public interface IDataAdapter
{
    string AdapterId { get; }
    string AdapterName { get; }
    AdapterType Type { get; }
    IReadOnlyList<string> TargetDataPaths { get; }
    Task<AdapterResult> ReadDataAsync();
    Task<AdapterResult> ReadBatchDataAsync();
    Task<ValidationResult> ValidateConfigAsync();
}
```

三种适配器工厂统一实现该接口，同步方法包装为异步（`Task.FromResult`）。

**改动范围**：Contracts 新增接口 + Generators 三种适配器工厂

**优点**：统一契约，支持多态管理，新增适配器有明确指南
**缺点**：Excel 和上下文适配器需要添加异步包装

### 方案 B：在 Generators 层定义接口

仅在 Generators 层定义 `IDataAdapter`，Contracts 层不涉及。

**优点**：不影响 Contracts 层的稳定性
**缺点**：如果未来有新的 Generator 项目，接口无法共享

---

## 6. 建议优先级

**P1 — 中期修复（1-2 月）**

建议采用方案 A，在 Contracts 层定义 `IDataAdapter` 接口，与 `AdapterType` 枚举和 `AdapterConfigBase` 形成完整的适配器契约体系。同时将 Excel 和上下文适配器的同步方法包装为异步，统一 API 风格。
