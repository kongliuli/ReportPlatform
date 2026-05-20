# 架构决策聚合报告

| 字段 | 值 |
|------|-----|
| 决策日期 | 2026-05-20 |
| 决策数量 | 15 |
| 决策者 | 项目负责人 |

---

## 一、决策汇总

### 1.1 系统架构层（3 个决策）

| 编号 | 决策点 | 选择 | 核心理由 |
|------|--------|------|----------|
| A | 系统拓扑演进 | **A1 保持离线隔离** | 医疗场景需要离线能力，零网络依赖 |
| B | 新增系统级模块 | **B3 同时引入 Rendering + Plugin Host** | 渲染统一 + 第三方扩展，一步到位 |
| C | 仓库管理策略 | **C3 Monorepo + NuGet** | 兼顾开发便利和分发规范 |

### 1.2 项目层（6 个决策）

| 编号 | 决策点 | 选择 | 核心理由 |
|------|--------|------|----------|
| D | Contracts 职责边界 | **D1 纯契约** | 零依赖，可被任何项目引用 |
| E | 元素类型注册机制 | **E2 Attribute + 反射** | 新增元素无需修改注册表，启动时一次性开销可接受 |
| F | Editor Core/Server 拆分 | **F3 渲染迁移到共享层** | 最干净的拆分，Editor.Core 不含渲染逻辑 |
| G | Editor 前端演进 | **G3 Blazor 内置界面** | 单项目部署，C# 全栈，非技术人员可用 |
| H | WPF 应用架构演进 | **H3 Core + 多壳** | 后续考虑分布式系统和多种录入端，需支持不同样式前端 |
| I | 适配器架构演进 | **I2 工厂注册表** | 新增适配器无需修改 MainViewModel，编译时安全 |

### 1.3 组件层（6 个决策）

| 编号 | 决策点 | 选择 | 核心理由 |
|------|--------|------|----------|
| J | PDF 渲染组件归属 | **J2 独立 Rendering 项目** | 渲染一致性保障，单一实现 |
| K | 数据库 Provider 拆离 | **K2 拆分 NuGet 包** | 按需安装，减小部署体积 |
| L | Excel 适配器拆离 | **L2 拆分为独立包** | 不使用 Excel 时可裁剪 ClosedXML |
| M | MVVM 基础设施 | **M3 社区框架替代** | CommunityToolkit.Mvvm 功能更完善，Source Generator 支持 |
| N | 配置存储统一 | **N3 SQLite 本地库** | 事务安全、并发友好、查询灵活 |
| O | 模型转换层聚合 | **O3 消除转换层** | 随 R-001 修复统一模型，最彻底 |

---

## 二、决策关联与约束

### 2.1 依赖关系图

```
O3 消除转换层 ──依赖──▶ R-001 双重模型修复
    │
    ▼
J2 独立Rendering项目 ──依赖──▶ O3 (统一输入模型)
F3 渲染迁移到共享层 ──依赖──▶ J2 (Rendering项目先存在)

K2 数据库Provider拆包 ──依赖──▶ I2 工厂注册表 (适配器先有统一接口)
L2 Excel适配器拆包 ──依赖──▶ I2 工厂注册表
                              ──依赖──▶ H3 Core类库提取 (适配器逻辑需在Core中)

B3 Rendering+PluginHost ──依赖──▶ J2 + I2

M3 社区框架替代 ──独立──▶ 可先行实施
N3 SQLite本地库 ──独立──▶ 可先行实施
E2 Attribute+反射 ──独立──▶ 可先行实施
G3 Blazor内置界面 ──独立──▶ 可先行实施
```

### 2.2 关键路径

```
R-001模型修复 → O3消除转换层 → J2独立Rendering → F3渲染迁移 → B3完整模块引入
                                    ↑
I2工厂注册表 ───────────────────────┘
                                    ↑
H3 Core类库提取 → K2/L2适配器拆包 ──┘
```

---

## 三、目标架构

基于全部 15 个决策，目标架构如下：

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    ReportPlatform Monorepo + NuGet                      │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌────────────────┐   ┌────────────────┐   ┌────────────────────────┐  │
│  │   Contracts    │   │   Rendering    │   │    PluginHost          │  │
│  │   (纯契约)     │   │   (共享渲染)   │   │    (插件宿主)          │  │
│  │                │   │                │   │                        │  │
│  │ • 元素模型     │   │ • ITemplate    │   │ • IAdapterPlugin       │  │
│  │   +Attribute  │   │   Renderer     │   │ • IElementPlugin       │  │
│  │ • IDataAdapter │   │ • PdfTemplate  │   │ • PluginLoader         │  │
│  │ • DTO/Enum    │   │   Renderer     │   │ • PluginRegistry       │  │
│  │ • ApiResponse │   │ • PdfElement   │   │                        │  │
│  │                │   │   Renderer     │   │                        │  │
│  │ NuGet 发布     │   │ • PageLayout   │   │                        │  │
│  └───────┬────────┘   │ • Paginator    │   │                        │  │
│          │            │                │   │                        │  │
│          │            │ NuGet 发布      │   │                        │  │
│          │            └───────┬────────┘   └───────────┬────────────┘  │
│          │                    │                        │               │
│          ▼                    ▼                        ▼               │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                     Editor                                       │  │
│  │  ┌─────────────────┐  ┌──────────────────┐                      │  │
│  │  │  Editor.Core    │  │  Editor.Server   │                      │  │
│  │  │  (业务逻辑)     │  │  (API + Blazor)  │                      │  │
│  │  │  • TemplateSvc  │  │  • Controllers   │                      │  │
│  │  │  • AuthSvc      │  │  • Blazor Pages  │                      │  │
│  │  │  • VersionSvc   │  │  • Middleware     │                      │  │
│  │  │  • ContextSvc   │  │                  │                      │  │
│  │  └─────────────────┘  └──────────────────┘                      │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                ReportDataMaker.Core (类库)                       │  │
│  │  ┌──────────────┐  ┌─────────────────┐  ┌──────────────────┐   │  │
│  │  │ AdapterReg.  │  │ DataBindingSvc  │  │ ConfigStore      │   │  │
│  │  │ (工厂注册表) │  │                 │  │ (SQLite)         │   │  │
│  │  └──────┬───────┘  └─────────────────┘  └──────────────────┘   │  │
│  │         │                                                        │  │
│  │         ▼                                                        │  │
│  │  ┌──────────────────────────────────────────────────────────┐   │  │
│  │  │ 适配器 NuGet 包（按需安装）                              │   │  │
│  │  │ ┌────────────┐ ┌──────────────────────┐ ┌─────────────┐ │   │  │
│  │  │ │Adapter.Excel│ │Adapter.Database.Common│ │Adapter.Ctx  │ │   │  │
│  │  │ │ClosedXML   │ │+ SqlServer (SqlClient)│ │             │ │   │  │
│  │  │ │            │ │+ MySql (MySqlConnector)│ │             │ │   │  │
│  │  │ │            │ │+ Sqlite (Data.Sqlite) │ │             │ │   │  │
│  │  │ │            │ │+ PostgreSql (Npgsql)  │ │             │ │   │  │
│  │  │ └────────────┘ └──────────────────────┘ └─────────────┘ │   │  │
│  │  └──────────────────────────────────────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│          │                                                               │
│          ▼                                                              │
│  ┌──────────────────┐  ┌──────────────────────┐                        │
│  │ ReportDataMaker  │  │ ReportDataMaker      │                        │
│  │ .WPF (壳)        │  │ .Web (BlazorWasm壳)  │                        │
│  │ CommunityToolkit │  │                      │                        │
│  │ .Mvvm            │  │                      │                        │
│  └──────────────────┘  └──────────────────────┘                        │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 四、解决方案结构变更

### 4.1 当前结构

```
ReportPlatform.sln
├── Contracts/
├── Editor/Core/
├── Editor/Server/
├── Editor/Server.Tests/
└── Generators/ReportDataMaker/
```

### 4.2 目标结构

```
ReportPlatform.sln
├── src/
│   ├── Contracts/                                    # 纯契约 (NuGet)
│   │   ├── Models/Elements/  (+ Attribute 标注)
│   │   ├── Models/Adapters/  (+ IDataAdapter)
│   │   ├── Models/Template/
│   │   ├── DTOs/
│   │   ├── Requests/
│   │   ├── Responses/
│   │   ├── Enums/
│   │   └── IdGenerator.cs
│   │
│   ├── Rendering/                                    # 共享渲染 (NuGet)
│   │   ├── ITemplateRenderer.cs
│   │   ├── PdfTemplateRenderer.cs
│   │   ├── PdfElementRenderer.cs
│   │   ├── PdfPageLayoutEngine.cs
│   │   └── ReportDocumentPaginator.cs
│   │
│   ├── PluginHost/                                   # 插件宿主 (NuGet)
│   │   ├── IAdapterPlugin.cs
│   │   ├── IElementPlugin.cs
│   │   ├── PluginLoader.cs
│   │   └── PluginRegistry.cs
│   │
│   ├── Editor/
│   │   ├── Core/                                     # 业务逻辑 (无渲染)
│   │   │   ├── Data/
│   │   │   ├── Services/
│   │   │   └── Extensions/
│   │   └── Server/                                   # API + Blazor
│   │       ├── Controllers/
│   │       ├── Middleware/
│   │       └── Pages/ (Blazor)
│   │
│   └── Generators/
│       ├── Core/                                     # 核心类库
│       │   ├── Services/
│       │   │   ├── DataBindingService.cs
│       │   │   ├── TemplateLoaderService.cs
│       │   │   ├── AdapterRegistry.cs
│       │   │   └── ConfigStore/ (SQLite)
│       │   └── Models/ (直接使用 Contracts 模型)
│       │
│       ├── Adapter.Excel/                            # Excel 适配器包
│       │   ├── ExcelAdapterFactory.cs
│       │   ├── TemplateFlattenService.cs
│       │   ├── ExcelSchemaExporter.cs
│       │   ├── ExcelContractReader.cs
│       │   └── ExcelDataValidator.cs
│       │
│       ├── Adapter.Database.Common/                  # 数据库适配器公共包
│       │   ├── IDatabaseProvider.cs
│       │   ├── DatabaseAdapterBase.cs
│       │   ├── DatabaseAdapterFactory.cs
│       │   ├── SqlBuilder.cs
│       │   ├── ConnectionPoolManager.cs
│       │   └── DatabaseProviderRegistry.cs
│       │
│       ├── Adapter.Database.SqlServer/               # SQL Server 包
│       ├── Adapter.Database.MySql/                   # MySQL 包
│       ├── Adapter.Database.Sqlite/                  # SQLite 包
│       ├── Adapter.Database.PostgreSql/              # PostgreSQL 包
│       │
│       ├── Adapter.Context/                          # 上下文适配器包
│       │   ├── ContextAdapterFactory.cs
│       │   ├── ContextAdapterService.cs
│       │   └── ContextProfileStore.cs
│       │
│       ├── ReportDataMaker.WPF/                      # WPF 壳
│       │   ├── Views/
│       │   ├── ViewModels/ (CommunityToolkit.Mvvm)
│       │   └── App.xaml
│       │
│       └── ReportDataMaker.Web/                      # Blazor WebAssembly 壳
│           ├── Pages/
│           └── Program.cs
│
└── tests/
    ├── Editor.Server.Tests/
    ├── Rendering.Tests/
    ├── Generators.Core.Tests/
    ├── Adapter.Excel.Tests/
    └── Adapter.Database.Tests/
```

---

## 五、分阶段实施计划

### Phase 1：基础加固（1-2 周）

**目标**：安全修复 + 独立决策点实施

| 任务 | 决策点 | 前置依赖 |
|------|--------|----------|
| JWT 密钥外部化 | — | 无 |
| SqlBuilder SQL 注入防护 | — | 无 |
| AsyncRelayCommand 异常处理 | — | 无 |
| 输入验证注解完善 | — | 无 |
| **E2: 元素类型 Attribute + 反射注册** | E2 | 无 |
| **M3: 引入 CommunityToolkit.Mvvm** | M3 | 无 |
| **N3: SQLite 本地配置库** | N3 | 无 |

### Phase 2：核心重构（1-2 月）

**目标**：模型统一 + 核心类库提取 + 适配器注册表

| 任务 | 决策点 | 前置依赖 |
|------|--------|----------|
| **O3: 消除双重模型（R-001 修复）** | O3 | Phase 1 |
| **H3: ReportDataMaker.Core 类库提取** | H3 | O3 |
| **I2: 适配器工厂注册表** | I2 | H3 |
| **K2: 数据库 Provider 拆分 NuGet 包** | K2 | I2 |
| **L2: Excel 适配器拆分 NuGet 包** | L2 | I2 |
| **G3: Editor.Server 添加 Blazor 页面** | G3 | 无 |

### Phase 3：模块整合（3-6 月）

**目标**：共享渲染层 + 插件宿主 + 多壳部署

| 任务 | 决策点 | 前置依赖 |
|------|--------|----------|
| **J2: 独立 Rendering 项目** | J2 | O3 |
| **F3: Editor.Core 渲染迁移到 Rendering** | F3 | J2 |
| **B3: 引入 PluginHost 模块** | B3 | I2 + J2 |
| **H3: ReportDataMaker.Web (BlazorWasm 壳)** | H3 | H3 Core |
| **C3: Contracts + Rendering 发布 NuGet 包** | C3 | J2 |

### Phase 4：远期演进

| 任务 | 决策点 | 说明 |
|------|--------|------|
| 插件 DLL 热加载 | I3 → B3 扩展 | 第三方适配器开发 |
| 分布式模板仓库 | A2 | 如需多终端自动同步 |
| 微服务拆分 | A3 | 如需大规模 SaaS |

---

## 六、风险与缓解

| 风险 | 涉及决策 | 缓解措施 |
|------|----------|----------|
| O3 模型统一改动量大 | O3 | 分批迁移，先组合模式后消除影子模型 |
| M3 CommunityToolkit.Mvvm 迁移 | M3 | 逐步替换，ViewModelBase → ObservableObject |
| K2 数据库 Provider 拆包后版本管理 | K2 | 统一版本号，使用 Directory.Build.props |
| H3 多壳维护成本 | H3 | Core 类库覆盖 90% 逻辑，壳仅负责 UI |
| G3 Blazor 增加 Server 复杂度 | G3 | Blazor Server 模式，不引入 WebAssembly 的双端复杂度 |
| J2 Rendering 项目需统一输入模型 | J2 | 依赖 O3 完成后实施 |

---

## 七、决策不可逆性标注

| 决策 | 可逆性 | 说明 |
|------|--------|------|
| A1 保持离线隔离 | ✅ 可逆 | 后续可引入 TemplateStore |
| B3 Rendering + PluginHost | ⚠️ 部分可逆 | Rendering 可逆，PluginHost 接口一旦发布不可变 |
| C3 Monorepo + NuGet | ✅ 可逆 | 可随时拆分多仓库 |
| D1 纯契约 | ✅ 可逆 | 可后续向 D2/D3 演进 |
| E2 Attribute + 反射 | ⚠️ 部分可逆 | Attribute 定义一旦发布不可变 |
| F3 渲染迁移 | ❌ 不可逆 | 一旦迁移不可回退 |
| G3 Blazor 内置界面 | ⚠️ 部分可逆 | 可移除 Blazor 但页面需重写 |
| H3 Core + 多壳 | ❌ 不可逆 | 项目结构根本性变更 |
| I2 工厂注册表 | ⚠️ 部分可逆 | IAdapterPlugin 接口一旦发布不可变 |
| J2 独立 Rendering | ❌ 不可逆 | 项目结构根本性变更 |
| K2 数据库 Provider 拆包 | ⚠️ 部分可逆 | 可合并回但 NuGet 版本需管理 |
| L2 Excel 适配器拆包 | ⚠️ 部分可逆 | 同 K2 |
| M3 社区框架替代 | ❌ 不可逆 | ViewModel 全面重写 |
| N3 SQLite 本地库 | ⚠️ 部分可逆 | 可回退到 JSON 但数据需迁移 |
| O3 消除转换层 | ❌ 不可逆 | 模型体系统一后不可回退 |
