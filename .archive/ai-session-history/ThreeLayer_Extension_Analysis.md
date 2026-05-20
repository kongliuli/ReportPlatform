# 三层扩展方向分析报告

> 评估日期：2026-05-20
> 本报告从系统架构层、项目层、组件层三个维度分析扩展方向，每个决策点提供多种方案供选择。

---

## 第一层：系统架构级扩展方向

### 当前架构拓扑

```
Editor.Server ──▶ Editor.Core ──▶ Contracts ◀── ReportDataMaker
```

三模块单向依赖，Contracts 为共享基石。此拓扑的核心限制是：**Editor 和 Generators 之间无通信通道**，模板从编辑到生产需要人工导出 JSON 文件再手动加载。

---

### 决策点 A：系统拓扑演进方向

当前拓扑在离线场景下合理（医疗场景需要本地数据访问），但随着平台成熟，需要决定是否引入 Editor 和 Generators 之间的直接通信。

**方案 A1：保持离线隔离（保守）**

```
Editor.Server ──▶ Editor.Core ──▶ Contracts ◀── ReportDataMaker
                                         │
                                    JSON 文件（人工传递）
```

- Editor 导出模板 JSON → 用户手动拷贝 → Generators 加载
- 优点：零网络依赖，符合医疗离线场景
- 缺点：模板同步靠人工，易出错；版本管理断裂
- 适合：单机部署、内网隔离环境

**方案 A2：引入模板仓库中间层（渐进）**

```
Editor.Server ──▶ Editor.Core ──▶ Contracts ◀── ReportDataMaker
       │                                │              │
       └────▶ TemplateStore (共享存储) ◀─┘──────────────┘
              (文件系统 / 对象存储 / 数据库)
```

- 新增 TemplateStore 抽象层，Editor 发布模板到 Store，Generators 从 Store 拉取
- Store 可以是文件系统共享目录、MinIO/S3 对象存储、或数据库 BLOB
- 优点：自动化模板分发，保留离线能力（本地 Store 缓存）
- 缺点：新增一个组件，需要部署和维护
- 适合：多终端协作、需要模板版本管理的场景

**方案 A3：微服务化（激进）**

```
                    ┌─────────────────┐
                    │  API Gateway    │
                    └────────┬────────┘
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
      ┌──────────────┐ ┌──────────┐ ┌──────────────┐
      │ Template Svc │ │ Auth Svc │ │ Renderer Svc │
      └──────┬───────┘ └──────────┘ └──────┬───────┘
             ▼                            ▼
      ┌──────────────┐            ┌──────────────┐
      │  Contracts   │            │  Contracts   │
      └──────────────┘            └──────────────┘
              ▲
      ┌───────┴────────┐
      │ ReportDataMaker│ (通过 API 拉取模板)
      └────────────────┘
```

- 将 Editor.Core 拆分为 Template Service、Auth Service、Renderer Service
- Generators 通过 HTTP API 获取模板
- 优点：完全解耦，独立部署和扩展
- 缺点：复杂度大幅增加，运维成本高
- 适合：大规模多租户 SaaS 场景

---

### 决策点 B：是否引入新的系统级模块

当前系统缺少以下能力模块：

**方案 B1：引入 Rendering 共享模块**

```
Contracts ◀── Rendering (新) ──▶ QuestPDF + SkiaSharp
    ▲              ▲
    │              │
Editor.Core ──────┘
ReportDataMaker ──┘
```

- 将 Editor.Core 的 PdfTemplateRenderer 和 Generators 的 PdfExportService 合并
- 统一渲染入口，消除双重实现
- 优点：渲染一致性有保障，维护成本减半
- 缺点：需要统一输入模型（依赖 R-001 修复）

**方案 B2：引入 Plugin Host 模块**

```
Contracts ◀── PluginHost (新) ──▶ IAdapterPlugin / IElementPlugin
    ▲
    │
ReportDataMaker ──▶ 加载插件 DLL
```

- 定义插件接口和加载机制
- 适配器和元素类型通过插件注册
- 优点：第三方可扩展，核心代码稳定
- 缺点：插件沙箱安全性需要额外保障

**方案 B3：同时引入 Rendering + Plugin Host**

- 最完整的方案，但改动量最大
- 适合长期规划

---

### 决策点 C：仓库管理策略

**方案 C1：保持单体仓库（Monorepo）**

- 当前状态，所有项目在一个 sln 中
- 优点：原子提交，重构方便
- 缺点：项目间耦合容易蔓延

**方案 C2：拆分为多仓库**

```
ReportPlatform-Contracts     (独立 NuGet 包)
ReportPlatform-Editor        (引用 Contracts NuGet)
ReportPlatform-Rendering     (引用 Contracts NuGet)
ReportPlatform-Generators    (引用 Contracts + Rendering NuGet)
```

- 优点：强制模块边界，独立版本管理
- 缺点：跨仓库重构困难，CI/CD 复杂度增加

**方案 C3：Monorepo + NuGet 包发布**

- 保持单体仓库，但 Contracts 和 Rendering 发布为 NuGet 包
- 优点：兼顾开发便利和分发规范
- 缺点：需要维护 CI/CD 发布流水线

---

## 第二层：项目级扩展与变迁

### 2.1 Contracts 项目

#### 决策点 D：Contracts 的职责边界

**方案 D1：纯契约（当前路线）**

- Contracts 仅包含数据结构定义（模型、枚举、DTO、接口）
- 所有实现逻辑放在使用方
- 优点：零依赖，可被任何项目引用
- 缺点：序列化逻辑（ElementJsonConverter）和注册逻辑（ElementGroupRegistry）放在契约层有争议

**方案 D2：契约 + 基础设施**

- 将序列化、注册、ID 生成等"基础设施"逻辑保留在 Contracts 中
- 但将适配器接口（IDataAdapter）和校验逻辑移到新项目
- 优点：职责更清晰
- 缺点：多一个项目引用

**方案 D3：契约 + 基础设施 + 渲染抽象**

- Contracts 包含 ITemplateRenderer 接口定义
- 渲染实现放在 Rendering 项目
- 优点：渲染契约与数据契约统一管理
- 缺点：Contracts 依赖 QuestPDF/SkiaSharp 的类型（仅接口层面）

#### 决策点 E：元素类型注册机制

**方案 E1：静态注册表 + 手动维护（当前）**

- ElementJsonConverter.WebShortTypeMap 和 ElementGroupRegistry 使用静态字典
- 新增元素类型需修改现有代码
- 优点：简单直接，编译时检查
- 缺点：违反开闭原则

**方案 E2：Attribute 标注 + 反射扫描**

```csharp
[ElementContract(ShortName = "text", Group = ElementAdaptationGroup.Form)]
public class TextElement : ExternalElementBase { }
```

- 启动时扫描程序集，自动注册
- 优点：新增元素类型只需添加类和 Attribute，无需修改注册表
- 缺点：反射性能开销（启动时一次性，可接受）；编译时无法检查遗漏

**方案 E3：Source Generator 自动生成注册代码**

```csharp
[GenerateElementRegistration]
public partial class ElementRegistration { }
// 编译时自动生成 WebShortTypeMap 和 ElementGroupRegistry 的初始化代码
```

- 优点：编译时生成，无反射开销，编译时检查
- 缺点：增加构建复杂度，调试困难

---

### 2.2 Editor 项目

#### 决策点 F：Editor.Core 与 Editor.Server 的合并/拆分

**方案 F1：保持 Core + Server 二分（当前）**

- Core 负责业务逻辑，Server 负责 API 展示
- 优点：分层清晰
- 缺点：Core 中的 PDF 渲染逻辑与 Generators 重复

**方案 F2：Core 拆分为 Core + Rendering**

```
Editor.Core (业务逻辑) ──▶ Editor.Rendering (PDF渲染) ──▶ Contracts
Editor.Server ──▶ Editor.Core + Editor.Rendering
```

- 将 PdfTemplateRenderer 和 PdfRenderService 移到 Editor.Rendering
- 后续 Generators 也引用 Editor.Rendering（或提取为共享 Rendering 项目）
- 优点：渲染逻辑可共享
- 缺点：多一个项目

**方案 F3：Core 渲染逻辑迁移到共享 Rendering 项目**

```
Editor.Core (业务逻辑，无渲染) ──▶ Contracts
Rendering (共享渲染) ──▶ Contracts
Editor.Server ──▶ Editor.Core + Rendering
```

- Editor.Core 完全不包含渲染逻辑
- 优点：最干净的拆分
- 缺点：Editor.Server 需要同时引用 Core 和 Rendering

#### 决策点 G：Editor 的前端演进

**方案 G1：纯 API 后端（当前）**

- Editor.Server 仅提供 REST API
- 前端由独立项目实现（如 Vue/React）
- 优点：前后端完全解耦
- 缺点：需要独立的前端项目

**方案 G2：API + 内置 Swagger UI**

- 当前状态，Swagger 作为简易前端
- 优点：零前端开发成本
- 缺点：非技术人员无法使用

**方案 G3：API + Blazor Server 内置管理界面**

- 在 Editor.Server 中添加 Blazor Server 模板管理界面
- 优点：单项目部署，C# 全栈
- 缺点：增加 Server 项目复杂度

---

### 2.3 Generators/ReportDataMaker 项目

#### 决策点 H：WPF 桌面应用的架构演进

**方案 H1：保持 WPF 单体（当前）**

- 所有功能在一个 WPF 应用中
- 优点：部署简单，离线可用
- 缺点：MainViewModel God Object，无法 Web 化

**方案 H2：核心逻辑提取为类库 + WPF 壳**

```
ReportDataMaker.Core (类库) ──▶ Contracts
ReportDataMaker.WPF (壳) ──▶ ReportDataMaker.Core
```

- Core 包含所有适配器、数据绑定、PDF 导出逻辑
- WPF 壳仅包含 View + ViewModel + DI 注册
- 优点：核心逻辑可被其他 UI 框架复用（如 MAUI、Web）
- 缺点：需要拆分项目

**方案 H3：Core 类库 + WPF 壳 + Web 壳**

```
ReportDataMaker.Core (类库) ──▶ Contracts
ReportDataMaker.WPF (壳) ──▶ ReportDataMaker.Core
ReportDataMaker.Web (壳) ──▶ ReportDataMaker.Core  (Blazor WebAssembly)
```

- 同时提供桌面和 Web 两种访问方式
- 优点：覆盖更多使用场景
- 缺点：开发和维护成本翻倍

#### 决策点 I：适配器架构演进

**方案 I1：硬编码适配器（当前）**

- 三种适配器各自实现，MainViewModel 硬编码创建
- 优点：简单
- 缺点：新增适配器需修改 MainViewModel

**方案 I2：适配器工厂注册表**

```csharp
public class AdapterRegistry
{
    public void Register(AdapterType type, IAdapterPlugin plugin);
    public IAdapterPlugin Get(AdapterType type);
    public IReadOnlyList<AdapterType> SupportedTypes { get; }
}
```

- MainViewModel 通过 AdapterRegistry 动态获取适配器
- 新增适配器只需注册到 Registry
- 优点：无需修改 MainViewModel
- 缺点：适配器仍需编译时引用

**方案 I3：适配器插件 DLL 热加载**

```csharp
public class PluginLoader
{
    public IEnumerable<IAdapterPlugin> LoadFrom(string pluginDirectory);
}
```

- 适配器编译为独立 DLL，放入 plugins/ 目录自动加载
- 优点：第三方可开发适配器，无需修改主程序
- 缺点：插件安全性、版本兼容性需要额外保障

---

## 第三层：组件拆离与聚合

### 3.1 需要拆离的组件

#### 决策点 J：PDF 渲染组件的归属

**方案 J1：留在各自项目（当前）**

```
Editor.Core/PdfTemplateRenderer.cs     (427行)
Generators/Services/PdfExport/         (5个文件)
```

- 两套独立实现
- 优点：零改动
- 缺点：双重维护，渲染不一致

**方案 J2：提取到独立 Rendering 项目**

```
Rendering/
├── Rendering.csproj                   (引用 Contracts + QuestPDF + SkiaSharp)
├── ITemplateRenderer.cs               (渲染接口)
├── PdfTemplateRenderer.cs             (统一渲染实现)
├── PdfElementRenderer.cs              (元素渲染)
├── PdfPageLayoutEngine.cs             (页面布局)
└── ReportDocumentPaginator.cs         (分页)
```

- Editor.Core 和 Generators 均引用 Rendering
- 优点：单一实现，一致性保障
- 缺点：需统一输入模型（依赖 R-001）

**方案 J3：提取到 Contracts 层作为子命名空间**

```
Contracts/
├── Models/...
├── Rendering/                         (新增)
│   ├── ITemplateRenderer.cs
│   └── RenderContext.cs
└── ...
```

- 渲染接口放在 Contracts，实现在新项目
- 优点：契约与实现分离
- 缺点：Contracts 项目依赖 QuestPDF/SkiaSharp（仅接口层可避免）

#### 决策点 K：数据库 Provider 组件的拆离

当前 4 个数据库 Provider 在 Generators 项目内部：

```
Generators/Services/DatabaseAdapter/
├── SqlServerProvider.cs    (依赖 Microsoft.Data.SqlClient)
├── MySqlProvider.cs        (依赖 MySqlConnector)
├── SqliteProvider.cs       (依赖 Microsoft.Data.Sqlite)
└── PostgreSqlProvider.cs   (依赖 Npgsql)
```

**方案 K1：保持在 Generators 内（当前）**

- 4 个 Provider 与 DatabaseAdapterBase 紧密耦合
- 优点：无需拆分
- 缺点：ReportDataMaker 引入了 4 个数据库驱动，即使用户只使用一种

**方案 K2：按数据库拆分为独立 NuGet 包**

```
ReportDataMaker.Adapter.Database.Common    (IDatabaseProvider + DatabaseAdapterBase)
ReportDataMaker.Adapter.Database.SqlServer  (SqlServerProvider + SqlClient)
ReportDataMaker.Adapter.Database.MySql      (MySqlProvider + MySqlConnector)
ReportDataMaker.Adapter.Database.Sqlite     (SqliteProvider + Sqlite)
ReportDataMaker.Adapter.Database.PostgreSql (PostgreSqlProvider + Npgsql)
```

- 用户按需安装数据库包
- 优点：减小部署体积，按需加载
- 缺点：多包管理复杂度

**方案 K3：运行时动态加载**

- 主程序不直接引用数据库驱动
- 通过 Assembly.LoadFrom 在运行时按需加载
- 优点：部署时可裁剪
- 缺点：调试困难，类型转换不安全

#### 决策点 L：Excel 适配器组件的拆离

**方案 L1：保持在 Generators 内（当前）**

- Excel 适配器与 TemplateFlattenService 紧密耦合
- 优点：无需拆分
- 缺点：ClosedXML 库体积较大（~2MB）

**方案 L2：提取为独立适配器包**

```
ReportDataMaker.Adapter.Excel    (ExcelAdapterFactory + 6个组件 + ClosedXML)
```

- 主程序按需引用
- 优点：不使用 Excel 适配器时可裁剪
- 缺点：TemplateFlattenService 是 Excel 和数据库适配器的共享依赖，需单独提取

#### 决策点 M：MVVM 基础设施的拆离

**方案 M1：保持在 Generators 内（当前）**

- ViewModelBase、RelayCommand、AsyncRelayCommand 等
- 优点：简单
- 缺点：不可复用

**方案 M2：提取为独立 MVVM 工具包**

```
Xinglin.MvvmToolkit/
├── ViewModelBase.cs
├── RelayCommand.cs
├── AsyncRelayCommand.cs
├── IDialogService.cs
├── DialogService.cs
└── FileLogger.cs (或替换为 ILogger 适配器)
```

- 优点：可被其他 WPF 项目复用
- 缺点：多一个项目维护

**方案 M3：使用社区 MVVM 框架替代**

- 使用 CommunityToolkit.Mvvm 替代自研 MVVM 基础设施
- 优点：社区维护，功能更完善（Source Generator 支持）
- 缺点：需要迁移现有 ViewModel，学习成本

### 3.2 需要聚合的组件

#### 决策点 N：适配器配置存储的统一

当前存在 3 个独立的配置存储：

```
AdapterConfigStore      → adapters.json      (Excel + Database 适配器配置)
ContextProfileStore     → context-profiles.json (上下文适配器配置)
ExportHistoryStore      → export-history.json   (导出历史)
```

**方案 N1：保持独立存储（当前）**

- 优点：各存储互不影响
- 缺点：3 个 JSON 文件，管理分散

**方案 N2：统一配置中心**

```
ReportConfigStore → report-config.json
{
  "adapters": { ... },
  "contextProfiles": { ... },
  "exportHistory": [ ... ]
}
```

- 优点：单一文件，统一管理
- 缺点：文件变大，并发写入风险集中

**方案 N3：SQLite 本地数据库**

```
ReportConfigDb → report-config.db
├── AdapterConfigs 表
├── ContextProfiles 表
└── ExportHistory 表
```

- 优点：事务安全，并发友好，查询灵活
- 缺点：引入 SQLite 依赖（Generators 已有 SqliteProvider）

#### 决策点 O：模型转换层的聚合

当前模型转换逻辑分散在多处：

```
ReportExternalElementConverter  (Infrastructure/)
DataBindingService              (Services/)
TemplateFlattenService          (Services/ExcelAdapter/)
```

**方案 O1：保持分散（当前）**

- 优点：各取所需
- 缺点：转换逻辑不一致，难以测试

**方案 O2：引入统一 ModelMapper**

```
ModelMapper/
├── IModelMapper.cs
├── TemplateMapper.cs           (TemplateDefinition ↔ ExternalTemplateDefinition)
├── ElementMapper.cs            (ExternalElementBase ↔ ReportExternalElementBase)
└── MappingProfile.cs           (配置映射规则)
```

- 优点：集中管理，可测试，可配置
- 缺点：新增一层抽象

**方案 O3：消除转换层（随 R-001 修复）**

- 统一模型后不再需要转换
- 优点：最彻底的方案
- 缺点：依赖 R-001 完成

---

## 决策汇总表

| 编号 | 决策点 | 方案数 | 推荐方案 | 影响范围 |
|------|--------|--------|----------|----------|
| A | 系统拓扑演进 | 3 | A2 渐进式 | 全局 |
| B | 新增系统级模块 | 3 | B1 先引入 Rendering | 全局 |
| C | 仓库管理策略 | 3 | C3 Monorepo + NuGet | 全局 |
| D | Contracts 职责边界 | 3 | D1 纯契约 | Contracts |
| E | 元素类型注册机制 | 3 | E2 Attribute + 反射 | Contracts |
| F | Editor Core/Server 拆分 | 3 | F3 渲染迁移到共享层 | Editor |
| G | Editor 前端演进 | 3 | G1 纯 API 后端 | Editor |
| H | WPF 应用架构演进 | 3 | H2 核心逻辑提取为类库 | Generators |
| I | 适配器架构演进 | 3 | I2 工厂注册表 | Generators |
| J | PDF 渲染组件归属 | 3 | J2 独立 Rendering 项目 | Editor + Generators |
| K | 数据库 Provider 拆离 | 3 | K1 保持内部 | Generators |
| L | Excel 适配器拆离 | 2 | L1 保持内部 | Generators |
| M | MVVM 基础设施 | 3 | M3 社区框架替代 | Generators |
| N | 适配器配置存储统一 | 3 | N3 SQLite 本地库 | Generators |
| O | 模型转换层聚合 | 3 | O3 消除转换层 | Contracts + Generators |

---

## 推荐组合方案

基于各决策点的推荐方案，形成以下组合：

### 渐进式演进路线

```
当前状态
│
▼ Phase 1 (P0, 1-2周): 安全加固
│   • A1 保持离线隔离
│   • C1 保持 Monorepo
│   • K1/L1 保持组件内部
│   • 安全修复（JWT、SQL注入、异常处理）
│
▼ Phase 2 (P1, 1-2月): 架构治理
│   • D1 Contracts 保持纯契约
│   • E2 Attribute + 反射注册
│   • H2 核心逻辑提取为类库
│   • I2 适配器工厂注册表
│   • O3 消除转换层（R-001 修复）
│   • M3 CommunityToolkit.Mvvm 迁移
│
▼ Phase 3 (P2, 3-6月): 生态扩展
│   • A2 引入模板仓库中间层
│   • B1 引入 Rendering 共享模块
│   • C3 Monorepo + NuGet 发布
│   • F3 渲染迁移到共享层
│   • J2 独立 Rendering 项目
│   • N3 SQLite 本地配置库
│
▼ Phase 4 (远期): 平台化
    • A3 微服务化（按需）
    • B2 引入 Plugin Host
    • G3 Blazor 内置管理界面
    • H3 Web 壳
    • I3 插件 DLL 热加载
    • K2 数据库 Provider 独立包
```

---

## 目标架构（Phase 3 完成后）

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ReportPlatform Monorepo                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────────────┐ │
│  │  Contracts   │  │  Rendering   │  │  TemplateStore (新增)     │ │
│  │  (纯契约)    │  │  (共享渲染)  │  │  (模板仓库抽象)           │ │
│  │  + Attribute │  │  QuestPDF    │  │  FS/S3/DB                │ │
│  │  + IDataAdapter│ │  + SkiaSharp │  │                          │ │
│  └──────┬───────┘  └──────┬───────┘  └───────────┬──────────────┘ │
│         │                 │                       │                │
│         ▼                 ▼                       ▼                │
│  ┌──────────────┐  ┌──────────────────────────────────────────┐   │
│  │ Editor.Core  │  │        ReportDataMaker.Core (类库)        │   │
│  │ (业务逻辑)   │  │  ┌──────────┬──────────┬───────────────┐ │   │
│  │              │  │  │Excel适配 │ │DB适配    │ │Context适配   │ │   │
│  └──────┬───────┘  │  │(可选引用) │ │(可选引用) │ │              │ │   │
│         │          │  └──────────┴──────────┴───────────────┘ │   │
│         ▼          └──────────────────┬───────────────────────┘   │
│  ┌──────────────┐                      │                          │
│  │ Editor.Server│              ┌───────┴────────┐                 │
│  │ (REST API)   │              │ ReportDataMaker │                 │
│  │              │              │ .WPF (壳)       │                 │
│  └──────────────┘              │ CommunityToolkit│                 │
│                                │ .Mvvm           │                 │
│                                └────────────────┘                 │
└─────────────────────────────────────────────────────────────────────┘
```
