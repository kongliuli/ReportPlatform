# ReportPlatform 全景架构图

| 字段 | 值 |
|------|-----|
| 基准分支 | 0520_master |
| 创建日期 | 2026-05-20 |
| 对应计划 | .sisyphus/plans/architecture-evolution.md |
| 状态图例 | ✅ 已完成 · 🔶 部分完成 · ❌ 未开始 |

---

## 一、解决方案全景

```
ReportPlatform.sln
│
├─ src/
│   ├─ Contracts/                    ✅ 存在 (net8.0)
│   │   ├─ Models/Elements/          ✅ 25 个元素类
│   │   ├─ Models/Adapters/          ✅ IDataAdapter + AdapterResult
│   │   ├─ Models/Template/          ✅ TemplateDefinition
│   │   ├─ DTOs/                     ✅ 5 个 DTO
│   │   ├─ Requests/                 ✅ 4 个 Request (已有验证注解)
│   │   ├─ Responses/                ✅ ApiResponse + PagedResponse
│   │   ├─ Enums/                    ✅ AdapterType, ElementGroup
│   │   ├─ Registry/                 🔶 手动注册 (目标: Attribute+反射)
│   │   ├─ Converters/               ✅ ElementJsonConverter
│   │   └─ Attributes/              ❌ ElementTypeAttribute 不存在
│   │
│   ├─ Rendering/                    ❌ 项目不存在
│   │   ├─ ITemplateRenderer         ❌
│   │   ├─ PdfTemplateRenderer       ❌
│   │   ├─ PdfElementRenderer        ❌
│   │   ├─ PdfPageLayoutEngine       ❌
│   │   └─ ReportDocumentPaginator   ❌
│   │
│   ├─ PluginHost/                   ❌ 项目不存在
│   │   ├─ IAdapterPlugin            ❌
│   │   ├─ IElementPlugin            ❌
│   │   ├─ PluginLoader              ❌
│   │   └─ PluginRegistry            ❌
│   │
│   ├─ Editor/
│   │   ├─ Core/                     ✅ 存在 (net8.0)
│   │   │   ├─ Data/                 ✅ EF Core + SQLite/SqlServer
│   │   │   ├─ Services/             🔶 含渲染代码 (目标: 迁出)
│   │   │   │   ├─ PdfRenderService       ⚠️ 待迁移到 Rendering
│   │   │   │   ├─ PdfTemplateRenderer    ⚠️ 待迁移到 Rendering
│   │   │   │   ├─ ContextService         ✅
│   │   │   │   ├─ DataBindingEngine      ✅
│   │   │   │   ├─ TemplateService        ✅
│   │   │   │   └─ VersionService         ✅
│   │   │   ├─ SharedInterfaces/     🔶 含渲染接口 (目标: 迁出)
│   │   │   │   ├─ IPdfSharpTemplateRenderer  ⚠️ 待迁移
│   │   │   │   ├─ IDataBindingEngine        ✅
│   │   │   │   └─ IJsonTemplateSerializer   ✅
│   │   │   └─ Extensions/           ✅ ServiceCollectionExtensions
│   │   │
│   │   ├─ Server/                   ✅ 存在 (net8.0 ASP.NET Core)
│   │   │   ├─ Controllers/          ✅ API 控制器
│   │   │   ├─ Middleware/           ✅ 日志 + 异常处理
│   │   │   ├─ Components/Pages/     ❌ Blazor 页面不存在
│   │   │   └─ Program.cs           🔶 无 Blazor 配置
│   │   │
│   │   ├─ Server.Tests/            ✅ 存在
│   │   └─ frontend/                ✅ Vue.js 前端 (独立)
│   │
│   └─ Generators/
│       ├─ Core/                     ❌ 类库不存在 (逻辑在 WPF 项目中)
│       │
│       ├─ Adapter.Excel/            ❌ 独立项目不存在
│       ├─ Adapter.Database.Common/  ❌ 独立项目不存在
│       ├─ Adapter.Database.SqlServer/ ❌
│       ├─ Adapter.Database.MySql/   ❌
│       ├─ Adapter.Database.Sqlite/  ❌
│       ├─ Adapter.Database.PostgreSql/ ❌
│       ├─ Adapter.Context/          ❌ 独立项目不存在
│       │
│       ├─ ReportDataMaker.WPF/     ✅ 存在 (名为 ReportDataMaker, net10.0-windows)
│       │   ├─ Views/               ✅ WPF XAML
│       │   ├─ ViewModels/          🔶 自定义 ViewModelBase (目标: CommunityToolkit)
│       │   ├─ Services/            🔶 全部耦合在 WPF 项目中 (目标: 迁到 Core)
│       │   ├─ Models/              ⚠️ 影子模型存在 (目标: 消除)
│       │   ├─ Infrastructure/      🔶 自定义 MVVM 基础设施
│       │   └─ Configs/             🔶 JSON 文件存储 (目标: SQLite)
│       │
│       └─ ReportDataMaker.Web/     ❌ 项目不存在
│
└─ tests/
    ├─ Editor.Server.Tests/          ✅ 存在
    ├─ Rendering.Tests/              ❌ 不存在
    ├─ Generators.Core.Tests/        ❌ 不存在
    ├─ Adapter.Excel.Tests/          ❌ 不存在
    └─ Adapter.Database.Tests/       ❌ 不存在
```

---

## 二、模块状态详情

### 2.1 Contracts — 纯契约层

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| 元素模型定义 | ✅ 25 个元素类 | ✅ 保持 | — |
| IDataAdapter 接口 | ✅ 已定义 | ✅ 保持 | — |
| DTO/Request/Response | ✅ 已定义 | ✅ 保持 | — |
| 输入验证注解 | ✅ [Required]+[StringLength] | ✅ 已达标 | — |
| ElementTypeAttribute | ❌ 不存在 | ✅ 所有元素类标注 | **P1-05** |
| ElementTypeRegistry (反射) | ❌ 手动注册 | ✅ 反射扫描 | **P1-05** |
| NuGet 包发布配置 | ❌ 无 PackageId | ✅ 可打包 | **P3-05** |

### 2.2 Rendering — 共享渲染层

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| 独立项目 | ❌ 不存在 | ✅ src/Rendering/ | **P3-01** |
| ITemplateRenderer 接口 | ❌ 分散在两处 | ✅ 统一接口 | **P3-01** |
| PdfTemplateRenderer | ⚠️ Editor.Core 中 | ✅ Rendering 中 | **P3-02** |
| PdfElementRenderer | ⚠️ ReportDataMaker 中 | ✅ Rendering 中 | **P3-01** |
| NuGet 包发布 | ❌ | ✅ 可打包 | **P3-05** |

### 2.3 PluginHost — 插件宿主

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| 独立项目 | ❌ 不存在 | ✅ src/PluginHost/ | **P3-03** |
| IAdapterPlugin | ❌ | ✅ 接口定义 | **P3-03** |
| IElementPlugin | ❌ | ✅ 接口定义 | **P3-03** |
| PluginLoader | ❌ | ✅ DLL 加载 | **P3-03** |
| PluginRegistry | ❌ | ✅ 注册管理 | **P3-03** |
| DLL 热加载 | ❌ | ✅ FileSystemWatcher | **P4-01** |

### 2.4 Editor.Core — 业务逻辑层

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| 模板 CRUD 服务 | ✅ TemplateService | ✅ 保持 | — |
| 版本管理服务 | ✅ VersionService | ✅ 保持 | — |
| 数据绑定引擎 | ✅ DataBindingEngine | ✅ 保持 | — |
| PDF 渲染服务 | ⚠️ 存在 (应迁出) | ❌ 迁到 Rendering | **P3-02** |
| IPdfSharpTemplateRenderer | ⚠️ 存在 (应迁出) | ❌ 替换为 ITemplateRenderer | **P3-02** |
| QuestPDF/SkiaSharp 依赖 | ⚠️ 直接引用 | ❌ 通过 Rendering 间接引用 | **P3-02** |

### 2.5 Editor.Server — API + Web UI

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| REST API | ✅ Controllers | ✅ 保持 | — |
| Swagger | ✅ 已配置 | ✅ 保持 | — |
| JWT 认证 | 🔶 密钥硬编码在 appsettings | ✅ 环境变量注入 | **P1-01** |
| SignalR | ✅ 已配置 | ✅ 保持 | — |
| Blazor Server 页面 | ❌ 不存在 | ✅ 模板管理 UI | **P2-06** |
| Vue.js 前端 | ✅ 存在 (frontend/) | ✅ 保持 (与 Blazor 共存) | — |

### 2.6 Generators/Core — 核心类库

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| 独立类库项目 | ❌ 不存在 | ✅ Generators/Core/ | **P2-02** |
| DataBindingService | ⚠️ 在 WPF 项目中 | ✅ 迁到 Core | **P2-02** |
| TemplateLoaderService | ⚠️ 在 WPF 项目中 | ✅ 迁到 Core | **P2-02** |
| AdapterRegistry (统一注册表) | ❌ 无统一注册表 | ✅ 工厂注册表 | **P2-03** |
| ConfigStore (SQLite) | ❌ 当前用 JSON | ✅ SQLite 存储 | **P1-07** |
| 无 UI 依赖 | ❌ 耦合在 WPF 中 | ✅ 纯 .NET 类库 | **P2-02** |

### 2.7 适配器 — 独立包

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| Excel 适配器 | ⚠️ 在 WPF 项目 Services/ 中 | ✅ 独立 Adapter.Excel 项目 | **P2-05** |
| Database 公共层 | ⚠️ 在 WPF 项目中 | ✅ Adapter.Database.Common | **P2-04** |
| SqlServer Provider | ⚠️ 在 WPF 项目中 | ✅ 独立包 | **P2-04** |
| MySql Provider | ⚠️ 在 WPF 项目中 | ✅ 独立包 | **P2-04** |
| Sqlite Provider | ⚠️ 在 WPF 项目中 | ✅ 独立包 | **P2-04** |
| PostgreSql Provider | ⚠️ 在 WPF 项目中 | ✅ 独立包 | **P2-04** |
| Context 适配器 | ⚠️ 在 WPF 项目中 | ✅ 独立包 | **P2-04** |
| IDataAdapterFactory 接口 | ❌ 无统一工厂接口 | ✅ Contracts 中定义 | **P2-03** |
| SQL 参数化 | ✅ 已参数化 | ✅ 已达标 | — |

### 2.8 ReportDataMaker.WPF — 桌面壳

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| WPF 应用 | ✅ 存在 | ✅ 保持 (瘦壳) | — |
| MVVM 框架 | 🔶 自定义 ViewModelBase | ✅ CommunityToolkit.Mvvm | **P1-06** |
| 影子模型 | ⚠️ Models/ 有 2 个文件 | ❌ 消除，用 Contracts | **P2-01** |
| 全局异常处理 | ❌ 无 | ✅ Dispatcher + Task 异常 | **P1-03** |
| 配置存储 | 🔶 JSON 文件 | ✅ SQLite | **P1-07** |
| 服务层位置 | ⚠️ 全部在本项目 | ✅ 迁到 Core 类库 | **P2-02** |

### 2.9 ReportDataMaker.Web — Web 壳

| 能力项 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| Blazor WebAssembly 项目 | ❌ 不存在 | ✅ 可运行 | **P3-04** |
| 模板加载 | ❌ | ✅ 加载并展示 | **P3-04** |
| 数据录入 | ❌ | ✅ 字段录入 | **P3-04** |
| PDF 预览 | ❌ | ✅ PDF.js 展示 | **P3-04** |

---

## 三、横切关注点状态

### 3.1 安全

| 关注点 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| JWT 密钥管理 | ⚠️ 硬编码在 appsettings.json | ✅ 环境变量 | **P1-01** |
| SQL 注入防护 | ✅ 参数化查询 | ✅ 已达标 | — |
| 全局异常处理 | ❌ WPF 端无 | ✅ 注册处理器 | **P1-03** |
| 输入验证 | ✅ Request DTO 有注解 | ✅ 已达标 | — |

### 3.2 架构模式

| 模式 | 当前状态 | 目标状态 | 差距 |
|------|----------|----------|------|
| 元素注册 | 手动 switch/字典 | Attribute + 反射 | **P1-05** |
| 适配器注册 | 各工厂独立，无统一入口 | 工厂注册表 | **P2-03** |
| 模型层 | 双重模型 + 转换层 | 统一 Contracts 模型 | **P2-01** |
| 渲染层 | 两套实现 (Editor + WPF) | 统一 Rendering 项目 | **P3-01** |
| 插件体系 | 无 | PluginHost + 接口 | **P3-03** |
| 多端部署 | 仅 WPF | WPF + Web + Blazor | **P2-06 + P3-04** |

### 3.3 依赖管理

| 关注点 | 当前状态 | 目标状态 | 差距 |
|--------|----------|----------|------|
| NuGet 包发布 | ❌ 无 | ✅ Contracts + Rendering | **P3-05** |
| 按需裁剪 | ❌ 全量引用 | ✅ Provider 独立包 | **P2-04/05** |
| 版本统一 | ❌ 各项目独立管理 | ✅ Directory.Build.props | **P2-04** |
| ClosedXML 隔离 | ❌ 主项目直接引用 | ✅ 仅 Adapter.Excel 引用 | **P2-05** |

---

## 四、项目依赖拓扑

### 4.1 当前依赖图

```
┌─────────────────────────────────────────────────────┐
│                    当前状态                           │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Contracts ◄──────── Editor.Core ◄──── Editor.Server│
│      ▲                    │                         │
│      │                    │ (QuestPDF, SkiaSharp)    │
│      │                    ▼                         │
│      │              PDF 渲染逻辑                     │
│      │                                              │
│      └──────────── ReportDataMaker (WPF)            │
│                         │                           │
│                         ├── Services/ (全部耦合)     │
│                         ├── Models/ (影子模型)       │
│                         ├── ViewModels/ (自定义MVVM) │
│                         └── PDF 渲染逻辑 (重复)      │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### 4.2 目标依赖图

```
┌──────────────────────────────────────────────────────────────────┐
│                       目标状态                                     │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Contracts (NuGet) ◄─── Rendering (NuGet) ◄─── PluginHost (NuGet)│
│      ▲                       ▲                       ▲           │
│      │                       │                       │           │
│      ├── Editor.Core ────────┘                       │           │
│      │       ▲                                       │           │
│      │       └── Editor.Server (+Blazor)             │           │
│      │                                               │           │
│      ├── Generators/Core ────────────────────────────┘           │
│      │       ▲                                                   │
│      │       ├── Adapter.Excel (NuGet, ClosedXML)                │
│      │       ├── Adapter.Database.Common                         │
│      │       │       ▲                                           │
│      │       │       ├── .SqlServer (SqlClient)                  │
│      │       │       ├── .MySql (MySqlConnector)                 │
│      │       │       ├── .Sqlite (Data.Sqlite)                   │
│      │       │       └── .PostgreSql (Npgsql)                    │
│      │       ├── Adapter.Context                                 │
│      │       │                                                   │
│      │       ├── ReportDataMaker.WPF (瘦壳, CommunityToolkit)    │
│      │       └── ReportDataMaker.Web (Blazor WASM)               │
│      │                                                           │
└──────┴───────────────────────────────────────────────────────────┘
```

---

## 五、Phase 完成度总览

```
Phase 1 基础加固 (7 任务)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 0%
P1-01 JWT 外部化        [░░░░░░░░░░] ❌
P1-02 SQL 注入防护      [██████████] ✅ (已参数化，仅需审计确认)
P1-03 异常处理          [░░░░░░░░░░] ❌
P1-04 输入验证          [██████████] ✅ (已有注解)
P1-05 E2 Attribute注册  [░░░░░░░░░░] ❌
P1-06 M3 CommunityToolkit [░░░░░░░░░░] ❌
P1-07 N3 SQLite配置     [░░░░░░░░░░] ❌

Phase 2 核心重构 (6 任务)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 0%
P2-01 O3 消除双重模型   [░░░░░░░░░░] ❌
P2-02 H3 Core类库提取   [░░░░░░░░░░] ❌
P2-03 I2 适配器注册表   [░░░░░░░░░░] ❌
P2-04 K2 DB Provider拆包 [░░░░░░░░░░] ❌
P2-05 L2 Excel拆包      [░░░░░░░░░░] ❌
P2-06 G3 Blazor页面     [░░░░░░░░░░] ❌

Phase 3 模块整合 (5 任务)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 0%
P3-01 J2 Rendering项目  [░░░░░░░░░░] ❌
P3-02 F3 渲染迁移       [░░░░░░░░░░] ❌
P3-03 B3 PluginHost     [░░░░░░░░░░] ❌
P3-04 H3 Web壳          [░░░░░░░░░░] ❌
P3-05 C3 NuGet发布      [░░░░░░░░░░] ❌

Phase 4 远期演进 (3 任务)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 0%
P4-01 插件热加载        [░░░░░░░░░░] ❌
P4-02 分布式模板仓库    [░░░░░░░░░░] ❌
P4-03 微服务拆分        [░░░░░░░░░░] ❌
```

---

## 六、迭代更新指南

完成某个任务后，更新本文件的对应位置：

1. **模块状态表**: 将 ❌/🔶/⚠️ 改为 ✅，删除差距列的任务编号
2. **Phase 完成度**: 将 `[░░░░░░░░░░] ❌` 改为 `[██████████] ✅`
3. **依赖拓扑图**: 当新项目创建后，更新"当前依赖图"使其逐步趋近"目标依赖图"
4. **横切关注点**: 安全/模式/依赖管理达标后标记 ✅

当所有 Phase 1-3 完成后，"当前依赖图"应与"目标依赖图"一致。
