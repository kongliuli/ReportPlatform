# Xinglin.ReportPlatform 项目 Wiki v1.0

> 版本：v1.0 | 更新日期：2026-05-08 | 覆盖版本：v0.3.0 ~ v0.6.0
> 本文档基于代码实际状态编写，对原 code-wiki.md 进行了勘误和补齐。

---

## 目录

1. [项目概览](#1-项目概览)
2. [架构设计](#2-架构设计)
3. [Contracts 契约层](#3-contracts-契约层)
4. [Editor 编辑器模块](#4-editor-编辑器模块)
5. [ReportDataMaker 数据录入工具](#5-reportdatamaker-数据录入工具)
6. [适配器体系](#6-适配器体系)
7. [依赖关系](#7-依赖关系)
8. [版本演进](#8-版本演进)
9. [运行指南](#9-运行指南)
10. [勘误记录](#10-勘误记录)

---

## 1. 项目概览

### 1.1 项目简介

**Xinglin.ReportPlatform（杏林报告平台）** 是一个面向医疗行业的报告单生成平台，采用契约驱动（Contract-Driven）架构，实现模板设计、版本管理和多格式报告生成的一体化解决方案。

### 1.2 技术栈

| 模块 | 技术框架 | 版本 | 说明 |
|------|---------|------|------|
| Contracts | .NET Class Library | .NET 8 | 契约定义层（单一数据源） |
| Editor.Core | .NET Class Library | .NET 8 | 核心业务逻辑库 |
| Editor.Server | ASP.NET Core | .NET 8 | Web API 服务端 |
| ReportDataMaker | WPF + HandyControl | .NET 10 | Windows 桌面数据录入工具 |

### 1.3 解决方案结构

```
ReportPlatform.sln
├── Contracts/                              # 契约定义层（v0.3.0）
│   ├── Enums/                              # 枚举定义
│   ├── Models/Elements/                    # 23种元素类型
│   ├── Models/Template/                    # 模板定义 + 数据绑定
│   ├── Models/Adapters/                    # 适配器接口与模型
│   ├── Converters/                         # ElementJsonConverter（双格式）
│   ├── Registry/                           # ElementGroupRegistry
│   ├── DTOs/                               # 数据传输对象
│   ├── Requests/                           # 请求模型
│   ├── Responses/                          # 响应模型
│   └── TemplateSerializer.cs               # 模板序列化器（静态类）
│
├── Editor/                                 # 在线编辑器模块
│   ├── Core/                               # 核心业务逻辑
│   │   ├── Data/                           # EF Core 实体 + DbContext + Migrations
│   │   ├── Services/                       # 业务服务（Template/Version/Auth/PdfRender）
│   │   ├── SharedInterfaces/               # 跨模块共享接口（Stub 实现）
│   │   └── Extensions/                     # ServiceCollectionExtensions
│   └── Server/                             # Web API 服务
│       ├── Controllers/                    # 5 个 API 控制器
│       ├── Middleware/                     # 异常处理 + 日志中间件
│       └── Program.cs                      # 启动配置
│
├── Generators/                             # 生产器层
│   └── ReportDataMaker/                    # WPF 数据录入工具（v0.4.0+）
│       ├── Models/                         # 桥接模型层（23个 External 元素）
│       ├── Infrastructure/                 # MVVM 基础设施
│       ├── Services/                       # 业务服务
│       │   ├── ExcelAdapter/               # v0.5.0 Excel 适配器（7个类）
│       │   └── DatabaseAdapter/            # v0.6.0 数据库适配器（11个类）
│       ├── ViewModels/                     # MVVM 视图模型
│       │   ├── Tabs/                       # 5 个 Tab ViewModel
│       │   └── Dialogs/                    # AddAdapterDialogViewModel
│       ├── Views/                          # XAML 视图
│       │   └── Tabs/                       # 4 个 Tab View
│       └── Converters/                     # BoolToVisibilityConverter
│
├── Adapters/                               # 预留目录（空）
├── Shared/                                 # 预留目录（空）
└── deploy/                                 # Docker 部署配置
    ├── docker-compose.yml
    ├── editor-server/Dockerfile
    └── nginx/nginx.conf
```

---

## 2. 架构设计

### 2.1 整体架构图

```
┌──────────────────────────────────────────────────────────────────┐
│                      Xinglin.ReportPlatform                       │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────┐     ┌──────────────┐     ┌───────────────────┐    │
│  │  Editor  │◀───▶│  Contracts   │◀───▶│  ReportDataMaker  │    │
│  │ (编辑器) │     │  (契约层)    │     │   (数据录入工具)  │    │
│  └──────────┘     └──────────────┘     └───────────────────┘    │
│       │                  │                     │                 │
│       ▼                  ▼                     ▼                 │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                      数据层                               │   │
│  │   EF Core (SQLite / SQL Server) + 适配器数据源           │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

### 2.2 核心设计原则

1. **契约优先（Contract-First）**：Contracts 层作为单一数据源，Editor 和 Generators 通过契约解耦
2. **Bridge 模式**：ReportDataMaker 通过 `ReportExternalElementBase` 桥接 Contracts `ExternalElementBase` 并添加 WPF 渲染属性
3. **Provider 模式**：数据库适配器通过 `IDatabaseProvider` 接口隔离不同数据库的 SQL 方言差异
4. **工厂模式**：`ExcelAdapterFactory` / `DatabaseAdapterFactory` 统一组件创建和生命周期
5. **依赖注入**：`Microsoft.Extensions.DependencyInjection` + `ServiceLocator` 管理所有服务

### 2.3 数据流

```
1. 模板设计 ──▶ Editor 编辑模板 ──▶ 保存至数据库
                      │
                      ▼
2. 契约导出 ──▶ 生成标准 JSON 契约 ──▶ TemplateDefinition + Elements
                      │
                      ▼
3. 数据生产 ──▶ ReportDataMaker 加载契约 ──▶ 数据录入 ──▶ 报告生成
                      │
                      ▼
4. 适配支持 ──▶ ExcelAdapter / DatabaseAdapter / ContextAdapter 获取数据
                      │
                      ▼
5. 数据绑定 ──▶ DataPath 映射 ──▶ 元素填充 ──▶ 预览/导出
```

### 2.4 适配器执行顺序

```
Context 适配器 ──▶ DataAdapter 适配器 ──▶ Editable 元素手动录入
   (自动填充)         (批量导入)              (人工补充)
```

---

## 3. Contracts 契约层

详见：[docs/contracts/wiki-v1.0.md](contracts/wiki-v1.0.md)

**项目路径**：`/Contracts/Xinglin.WebReportEditor.Contracts.csproj`
**目标框架**：`net8.0`
**NuGet**：`Newtonsoft.Json 13.0.3`

### 命名空间（历史遗留双命名空间）

- `Xinglin.ReportEditor.Contracts.*` — Enums、Models、Converters、Registry
- `Xinglin.WebReportEditor.Contracts.*` — DTOs、Requests、Responses

### 枚举

| 枚举 | 值 |
|------|-----|
| ElementGroup | Fixed, Context, Editable, DataAdapter |
| ElementAdaptationGroup | Basic, Form, Data, Advanced |
| AdapterType | Context, Excel, Database, Api |
| DatabaseProvider | SqlServer, MySql, Sqlite, PostgreSql |
| FieldDataType | Text, Number, Date, Dropdown, Boolean |
| BindingType | Text, Image, Visibility, Repeat, Style |

### 元素类型（23 种）

- **Fixed**：Line, Text, Shape, Image, Divider, Header, Footer, PageNumber, Watermark, Icon
- **Context**：Hyperlink
- **Editable**：Number, Date, Dropdown, Checkbox, Radio, Signature, Barcode
- **DataAdapter**：Table, Container, Repeat, QrCode, Chart

### 关键组件

- **ElementJsonConverter** — 双格式 JSON 转换器（Web 短格式 + Legacy 全格式）
- **ElementGroupRegistry** — 元素类型 → 适配分组映射（Basic/Form/Data/Advanced）
- **TemplateSerializer** — 统一序列化/反序列化（CamelCase + NullIgnore）

---

## 4. Editor 编辑器模块

详见：[docs/editor/wiki-v1.0.md](editor/wiki-v1.0.md)

### 4.1 Editor.Core

**命名空间**：`Xinglin.WebReportEditor.Core`

| 服务 | 职责 |
|------|------|
| TemplateService | 模板 CRUD + 分页 |
| VersionService | 版本管理 + 回滚 + Diff |
| AuthService | JWT 认证 + Token 刷新 |
| PdfRenderService | PDF 渲染（Stub） |

数据实体：TemplateEntity, TemplateVersionEntity, UserEntity, RefreshTokenEntity

### 4.2 Editor.Server

| 控制器 | 路由 |
|--------|------|
| TemplatesController | `/api/templates` |
| VersionsController | `/api/versions` |
| AuthController | `/api/auth` |
| PreviewController | `/api/preview` |
| HealthController | `/api/health` |

中间件：GlobalExceptionMiddleware, ApiLoggingMiddleware
配置：JWT + Swagger + EF Core + CORS + SignalR

---

## 5. ReportDataMaker 数据录入工具

详见：[docs/generators/wiki-v1.0.md](generators/wiki-v1.0.md)

**目标框架**：`net10.0-windows`
**UI 框架**：HandyControl 3.5.1

### 5.1 核心依赖

| 包 | 版本 |
|----|------|
| ClosedXML | 0.105.0 |
| Microsoft.Data.SqlClient | 7.0.1 |
| Microsoft.Data.Sqlite | 10.0.7 |
| MySqlConnector | 2.5.0 |
| Npgsql | 9.0.3 |
| ZXing.Net | 0.16.13 |

### 5.2 MVVM 架构

- **Infrastructure**：ViewModelBase, RelayCommand, AsyncRelayCommand, ServiceLocator
- **ViewModels**：MainViewModel + 5 个 Tab VM + 2 个 Dialog VM + SidePanelViewModel
- **Views**：MainWindow + 4 个 Tab View
- **Services**：TemplateLoader, Preview, DataBinding, AdapterConfigStore

### 5.3 适配器服务

- **ExcelAdapter**（7 个类）：TemplateFlattenService → ExcelSchemaExporter → ExcelContractReader → ExcelDataValidator
- **DatabaseAdapter**（11 个类）：IDatabaseProvider × 4 实现 + SqlBuilder + DatabaseAdapterBase + Factory

---

## 6. 适配器体系

### 6.1 架构

```
┌─────────────────────────────────────────────────────────┐
│                    适配器体系                             │
├─────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Context    │  │    Excel     │  │  Database    │  │
│  │  (v0.8.0)   │  │   (v0.5.0)   │  │   (v0.6.0)  │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│  ┌──────────────┐                                       │
│  │     API      │                                       │
│  │  (v0.7.0)   │                                       │
│  └──────────────┘                                       │
├─────────────────────────────────────────────────────────┤
│              Contracts (IDataAdapter, AdapterConfigBase) │
└─────────────────────────────────────────────────────────┘
```

### 6.2 数据库 Provider 对照

| Provider | 标识符引用 | 参数前缀 | 分页语法 |
|----------|-----------|---------|---------|
| SqlServer | `[name]` | `@` | OFFSET...FETCH |
| MySql | `` `name` `` | `@` | LIMIT...OFFSET |
| Sqlite | `"name"` | `@` | LIMIT...OFFSET |
| PostgreSql | `"name"` | `:` | LIMIT...OFFSET |

---

## 7. 依赖关系

```
Editor.Server (.NET 8, ASP.NET Core)
    └── Editor.Core (.NET 8)
            └── Contracts (.NET 8)

ReportDataMaker (.NET 10, WPF)
    └── Contracts (.NET 8)
```

---

## 8. 版本演进

| 版本 | 主题 | 状态 |
|------|------|------|
| v0.3.0 | 架构统一 — Contracts 契约层 | ✅ 完成 |
| v0.4.0 | WPF MVVM 重构 | ✅ 完成 |
| v0.5.0 | Excel 适配器 | ✅ 完成 |
| v0.6.0 | 数据库适配器 | ✅ 完成 |
| v0.7.0 | API 适配器 | 📋 计划中 |
| v0.8.0 | 上下文适配器 | 📋 计划中 |
| v0.9.0 | 模板编辑器增强 | 📋 计划中 |
| v1.0.0 | 生产就绪 | 📋 计划中 |

---

## 9. 运行指南

### Editor.Server

```bash
cd Editor/Server
dotnet restore && dotnet ef database update && dotnet run
# http://localhost:5000 | Swagger: http://localhost:5000/swagger
```

### ReportDataMaker

```bash
cd Generators/ReportDataMaker
dotnet restore && dotnet run
```

### 默认用户

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | admin123 | admin |
| editor | editor123 | editor |

---

## 10. 勘误记录

以下为原 `code-wiki.md` 中发现的错误，已在本文档中修正：

| 项目 | 原文档记录 | 实际值 |
|------|-----------|--------|
| ClosedXML 版本 | 0.104.2 | **0.105.0** |
| Microsoft.Data.SqlClient 版本 | 6.0.1 | **7.0.1** |
| Microsoft.Data.Sqlite 版本 | 9.0.4 | **10.0.7** |
| Microsoft.Extensions.DI 版本 | 9.0.4 | **8.0.1** |
| ZXing.Net 版本 | 0.16.12 | **0.16.13** |
| WPF 元素类型数量 | 20 个 | **23 个**（含 Header/Footer/PageNumber） |
| 命名空间 | 未提及双命名空间 | 存在 `Xinglin.ReportEditor.Contracts` 和 `Xinglin.WebReportEditor.Contracts` 两套 |
| Editor csproj 引用路径 | 未提及 | 路径不正确（不影响 sln 构建） |
| DI 注册 | 未列出 ViewModel | 实际注册了 MainViewModel, TemplateLoadViewModel, SidePanelViewModel（Transient） |
| deploy 目录 | 未提及 | 存在完整 Docker 部署配置 |
| DataBindingDefinition | 未列出 BindingType 枚举和 Transform 属性 | 已补齐 |
| ReportExternalElementBase | 未列出 Shadow 属性 | 已补齐 |

---

## 子项目 Wiki 索引

| 项目 | Wiki 路径 |
|------|-----------|
| Contracts | [docs/contracts/wiki-v1.0.md](contracts/wiki-v1.0.md) |
| Editor | [docs/editor/wiki-v1.0.md](editor/wiki-v1.0.md) |
| ReportDataMaker | [docs/generators/wiki-v1.0.md](generators/wiki-v1.0.md) |

---

*本文档版本 1.0，基于代码实际状态编写，覆盖 v0.3.0 ~ v0.6.0*
