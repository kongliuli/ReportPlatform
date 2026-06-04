# 跨模块对比分析报告

> 分析版本：基于 CODE_WIKI.md 对比三大模块深度分析
> 分析范围：Contracts / Editor / Generators 跨模块差异性与一致性

---

## 1. 架构差异性矩阵

### 1.1 设计范式差异

| 维度 | Contracts | Editor | Generators |
|------|-----------|--------|------------|
| 设计范式 | 契约定义（声明式） | 分层架构（命令式） | MVVM（事件驱动） |
| 核心关注 | 数据结构一致性 | 数据持久化与 API | 用户交互与数据生产 |
| 变化频率 | 低（契约稳定） | 中（业务演进） | 高（功能迭代） |
| 依赖方向 | 被依赖（底层） | 依赖 Contracts | 依赖 Contracts |
| 测试策略 | 序列化正确性 | 单元/集成测试 | 手动测试为主 |

### 1.2 技术栈差异

| 维度 | Contracts | Editor | Generators |
|------|-----------|--------|------------|
| 目标框架 | .NET 8 | .NET 8 | .NET 10 (WPF) |
| JSON 库 | Newtonsoft.Json 13.0.3 | Newtonsoft.Json 13.0.3 | Newtonsoft.Json 13.0.4 |
| PDF 引擎 | — | QuestPDF + SkiaSharp | QuestPDF + SkiaSharp |
| DI 容器 | — | ASP.NET Core 内置 | Microsoft.Extensions.DependencyInjection |
| 数据库 | — | EF Core (SQLite/SqlServer) | 4 种原生驱动 |
| 日志 | — | ILogger (ASP.NET Core) | FileLogger (自定义) |

**关键差异**：

1. **目标框架不一致**：Contracts 和 Editor 使用 .NET 8，Generators 使用 .NET 10。这意味着 Generators 可以使用最新的 C# 语言特性，但 Contracts 层不能，导致代码风格差异。
2. **Newtonsoft.Json 版本不一致**：Contracts 和 Editor 使用 13.0.3，Generators 使用 13.0.4。虽然小版本差异通常兼容，但属于不必要的风险。
3. **DI 容器不同**：Editor 使用 ASP.NET Core 内置 DI，Generators 使用 `Microsoft.Extensions.DependencyInjection`。两者 API 兼容但注册策略不同（Editor 以 Scoped 为主，Generators 以 Singleton 为主）。

---

## 2. 模型体系差异性

### 2.1 三套模型体系

```
Contracts 层                    Editor 层                     Generators 层
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────────┐
│ TemplateDefinition│    │ TemplateEntity   │    │ ExternalTemplateDefinition│
│ (共享契约)       │    │ (数据库实体)     │    │ (桌面应用模型)            │
├─────────────────┤    ├──────────────────┤    ├─────────────────────────┤
│ ExternalElementBase│   │ ContentJson      │    │ ReportExternalElementBase │
│ (元素基类)       │    │ (JSON 字符串)    │    │ (扩展元素基类)            │
├─────────────────┤    └──────────────────┘    ├─────────────────────────┤
│ 22种 XxxElement  │                            │ 23种 ExternalXxxElement  │
│ (契约元素)       │                            │ (应用元素)               │
└─────────────────┘                            └─────────────────────────┘
```

**转换路径**：

```
Contracts → JSON 字符串 → Generators
TemplateDefinition → ContentJson → ExternalTemplateDefinition
ExternalElementBase → JSON → ReportExternalElementBase
```

**问题**：

1. **三套模型的数据一致性**：Contracts 定义了标准模型，Editor 存储为 JSON 字符串，Generators 反序列化为自己的模型。任何一方的模型变更都可能破坏其他方。
2. **缺少模型版本化**：JSON 字符串没有版本标识，模型变更后无法兼容旧数据。
3. **Generators 层的模型是 Contracts 层的"影子副本"**：`ReportExternalElementBase` 继承 `ExternalElementBase` 但用 `new` 隐藏属性，这种设计在 Contracts 层变更时容易遗漏同步。

### 2.2 属性类型差异

| 属性 | Contracts 层 | Generators 层 | 差异影响 |
|------|-------------|---------------|----------|
| `BackgroundColor` | `string?` (nullable) | `string` (non-nullable, default "") | 序列化/反序列化时 null 与 "" 的语义差异 |
| `FontSize` | `double?` (nullable, default 12) | `double` (non-nullable, default 0) | 默认值不一致（12 vs 0） |
| `Opacity` | `double?` (nullable, default 1.0) | `double` (non-nullable, default 1) | 类型差异 |
| `Version` | `int` | `string` | 类型不兼容 |
| `BindingType` | `BindingType` 枚举 | `string` | 类型安全 vs 字符串 |

**风险**：从 Contracts 模型序列化的 JSON 反序列化为 Generators 模型时，nullable 属性的 null 值会被替换为非空默认值，导致数据语义变化。

---

## 3. 适配器架构差异性

### 3.1 适配器实现对比

| 维度 | Excel 适配器 | 数据库适配器 | 上下文适配器 |
|------|-------------|-------------|-------------|
| 数据源 | 文件系统 | 数据库 | 内存/系统 |
| 读取模式 | 同步 | 异步 | 同步 |
| 批量支持 | ✅ ReadBatchData | ✅ ReadBatchDataAsync | ❌ |
| 校验支持 | ✅ Validate | ✅ ValidateConfigAsync | ❌ |
| 配置持久化 | AdapterConfigStore | AdapterConfigStore | ContextProfileStore |
| 连接管理 | 无 | ConnectionPoolManager | 无 |
| 安全措施 | 无 | DPAPI 加密 | 无 |
| SQL 注入风险 | — | 🔴 高 | — |

**统一性评分**：⭐⭐☆☆☆

三种适配器没有实现共同的接口，方法签名不一致，功能支持程度差异大。

### 3.2 适配器与 Contracts 层的关系

Contracts 层定义了 `AdapterType` 枚举和 `AdapterConfigBase`，但：

1. **`IDataAdapter` 接口未在 Contracts 层定义**：README 中描述了该接口，但代码中不存在
2. **适配器配置模型分散**：`ExcelAdapterConfig`、`DatabaseAdapterConfig`、`ContextAdapterConfig` 各自在 Generators 层定义，与 Contracts 层的 `AdapterConfigBase` 继承关系不明确
3. **适配器结果类型不统一**：Excel 和数据库适配器返回 `AdapterResult`，但上下文适配器也返回 `AdapterResult`，填充方式不同

---

## 4. PDF 渲染差异性

### 4.1 双重实现

| 维度 | Editor.Core | Generators |
|------|------------|------------|
| 实现类 | `PdfTemplateRenderer` (427 行) | `PdfExportService` + `PdfElementRenderer` |
| 渲染引擎 | QuestPDF + SkiaSharp | QuestPDF + SkiaSharp |
| 输入模型 | `TemplateDefinition` (Contracts) | `ExternalTemplateDefinition` (Generators) |
| 元素类型 | 22 种 Contracts 元素 | 23 种 Generators 元素 |
| 页面布局 | 自定义 | `PdfPageLayoutEngine` |
| 批量支持 | ❌ | ✅ BatchExportService |

**问题**：两套独立的 PDF 渲染实现，功能重复，维护成本高。当新增元素类型时，需要在两处添加渲染逻辑。

**改进建议**：将 PDF 渲染逻辑提取到共享层（如 Contracts 层或新建 Rendering 层），统一输入模型和渲染逻辑。

---

## 5. 安全性差异

### 5.1 安全措施对比

| 安全维度 | Contracts | Editor | Generators |
|----------|-----------|--------|------------|
| 认证 | — | JWT Bearer | — |
| 授权 | — | Role-based | — |
| 数据加密 | — | — | DPAPI (连接字符串) |
| 输入验证 | — | ⚠️ 不完整 | — |
| SQL 注入防护 | — | — | ❌ RawSQL 模式 |
| 密钥管理 | — | ❌ 硬编码 | — |
| 审计日志 | — | API 日志 | FileLogger |

**整体安全评级**：⭐⭐☆☆☆

- Editor 端有基本的认证授权，但密钥管理不当
- Generators 端几乎没有安全措施，SQL 注入风险高
- Contracts 层的序列化存在类型注入的理论风险

---

## 6. 扩展性差异

### 6.1 扩展点对比

| 扩展场景 | Contracts | Editor | Generators |
|----------|-----------|--------|------------|
| 新增元素类型 | 修改 5 个文件 | 修改 ContentJson 解析 | 修改 6 个文件 |
| 新增适配器类型 | 添加枚举值 | 无影响 | 修改 MainViewModel + 5 个新文件 |
| 新增数据库 | — | — | 实现 IDatabaseProvider + 注册 |
| 新增 API 端点 | 可能需要新 DTO | 添加 Controller | — |
| 新增生产器 | 无影响 | 无影响 | 新项目 + 引用 Contracts |

**扩展性评分**：

| 模块 | 评分 | 瓶颈 |
|------|------|------|
| Contracts | ⭐⭐☆☆☆ | 静态注册表，新增元素需修改多处 |
| Editor | ⭐⭐⭐⭐☆ | 分层清晰，新增 API 端点简单 |
| Generators | ⭐⭐☆☆☆ | MainViewModel 耦合，适配器无插件化 |

### 6.2 开闭原则遵循度

| 模块 | 新增元素 | 新增适配器 | 新增数据库 |
|------|----------|-----------|-----------|
| Contracts | ❌ 修改 5 文件 | ⚠️ 添加枚举值 | ⚠️ 添加枚举值 |
| Editor | ✅ 无影响 | ✅ 无影响 | ✅ 无影响 |
| Generators | ❌ 修改 6 文件 | ❌ 修改 MainViewModel | ✅ 实现 + 注册 |

---

## 7. 合理性综合评估

### 7.1 架构决策合理性

| 决策 | 合理性 | 理由 |
|------|--------|------|
| 契约驱动架构 | ✅ 合理 | 解耦编辑端与生产端，支持独立演进 |
| 双分组体系 | ⚠️ 部分合理 | 运行时分组和设计时分组的场景确实不同，但实现分散 |
| 双重模型体系 | ❌ 不合理 | Contracts 和 Generators 的模型重复，`new` 隐藏风险大 |
| 适配器工厂模式 | ✅ 合理 | 封装适配器创建逻辑，但缺少统一接口 |
| WPF 桌面应用 | ✅ 合理 | 医疗场景需要本地数据访问和离线能力 |
| 版本快照策略 | ✅ 合理 | ContentJson 全量快照简单可靠，但长期存储冗余 |

### 7.2 技术债务清单

| 债务 | 严重程度 | 影响模块 | 修复成本 |
|------|----------|----------|----------|
| 双重模型体系 + `new` 隐藏 | 🔴 高 | Contracts + Generators | 高（需重构模型层） |
| SQL 注入风险 | 🔴 高 | Generators | 中（参数化 + 黑名单） |
| JWT 密钥硬编码 | 🔴 高 | Editor | 低（环境变量） |
| IDataAdapter 接口缺失 | 🟡 中 | Contracts + Generators | 中（需统一适配器 API） |
| PDF 渲染双重实现 | 🟡 中 | Editor + Generators | 高（需提取共享层） |
| MainViewModel God Object | 🟡 中 | Generators | 中（需拆分职责） |
| FileLogger 过度使用 | 🟢 低 | Generators | 低（替换为 ILogger） |
| 命名空间不一致 | 🟢 低 | 全局 | 低（重命名） |
| Newtonsoft.Json 版本不一致 | 🟢 低 | 全局 | 低（统一版本） |

---

## 8. 架构演进建议

### 8.1 短期改进（1-2 周）

1. **JWT 密钥外部化**：从环境变量读取，appsettings.json 仅作为开发默认值
2. **SQL 注入防护**：RawSQL 模式添加只读限制，VisualBuilder 模式 WHERE 子句参数化
3. **AsyncRelayCommand 异常处理**：捕获异常并路由到错误处理服务
4. **统一 Newtonsoft.Json 版本**：全部使用 13.0.4

### 8.2 中期改进（1-2 月）

1. **定义 IDataAdapter 接口**：在 Contracts 层定义统一适配器接口，三种适配器实现该接口
2. **消除 `new` 隐藏**：使用组合模式替代继承，或使用 override
3. **拆分 MainViewModel**：引入 AdapterManager、ExportManager、TabManager
4. **替换 FileLogger**：使用 Microsoft.Extensions.Logging + Serilog
5. **增加 VersionsController 和 ContextController 测试**

### 8.3 长期改进（3-6 月）

1. **提取共享渲染层**：将 PDF 渲染逻辑从 Editor.Core 和 Generators 提取到独立项目
2. **统一模型体系**：消除 Generators 层的影子模型，直接使用 Contracts 层模型 + 扩展属性
3. **适配器插件化**：实现 IAdapterPlugin 注册机制，新增适配器无需修改 MainViewModel
4. **元素类型动态注册**：使用 Attribute + 反射替代静态注册表
5. **模型版本化**：为模板 JSON 添加版本标识，支持向后兼容

### 8.4 架构目标

```
                    ┌─────────────────────────────────────────┐
                    │          目标架构                        │
                    │                                          │
                    │  ┌───────────────┐                      │
                    │  │   Contracts   │  (契约 + IDataAdapter) │
                    │  └───────┬───────┘                      │
                    │          │                               │
                    │    ┌─────┴──────┐                       │
                    │    ▼            ▼                       │
                    │  ┌──────┐  ┌──────────┐                │
                    │  │Editor│  │Generators │                │
                    │  └──┬───┘  └─────┬────┘                │
                    │     │            │                      │
                    │     │    ┌───────┴───────┐              │
                    │     │    ▼               ▼              │
                    │     │  ┌────────┐  ┌──────────┐        │
                    │     │  │Plugin A│  │Plugin B  │        │
                    │     │  │(Excel) │  │(Database)│        │
                    │     │  └────────┘  └──────────┘        │
                    │     │                                  │
                    │  ┌───┴────────────┐                    │
                    │  │  Shared        │                    │
                    │  │  Rendering     │  (统一PDF渲染)     │
                    │  └────────────────┘                    │
                    └─────────────────────────────────────────┘
```

---

## 9. 综合评分

| 维度 | Contracts | Editor | Generators | 整体 |
|------|-----------|--------|------------|------|
| 架构合理性 | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| 代码质量 | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| 安全性 | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ |
| 扩展性 | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |
| 一致性 | — | — | — | ⭐⭐ |
| 测试覆盖 | ⭐⭐ | ⭐⭐ | ⭐ | ⭐⭐ |

**整体评价**：项目架构方向正确（契约驱动 + 适配器模式），核心功能完整，但在跨模块一致性、安全性和扩展性方面存在明显的技术债务。优先解决安全问题和模型一致性问题，将显著提升项目的生产就绪度。
