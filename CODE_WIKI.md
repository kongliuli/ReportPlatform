# Xinglin.ReportPlatform — Code Wiki

> 基于契约驱动的医疗报告单生成平台，支持在线模板编辑、多源数据适配和可扩展的适配器架构。

---

## 目录

1. [项目概述](#1-项目概述)
2. [架构总览](#2-架构总览)
3. [解决方案结构](#3-解决方案结构)
4. [模块详解](#4-模块详解)
   - 4.1 [Contracts — 契约层](#41-contracts--契约层)
   - 4.2 [Editor — Web编辑器](#42-editor--web编辑器)
   - 4.3 [Generators — 数据生产器](#43-generators--数据生产器)
5. [数据流](#5-数据流)
6. [依赖关系](#6-依赖关系)
7. [项目运行方式](#7-项目运行方式)
8. [扩展指南](#8-扩展指南)

---

## 1. 项目概述

Xinglin.ReportPlatform（杏林报告平台）是一个面向医疗行业的报告单生成平台，采用**契约驱动**架构，将模板设计（Editor）与数据生产（Generators）解耦。核心设计理念：

- **契约层（Contracts）** 作为共享基石，确保编辑端与生产端对模板数据结构的一致理解
- **适配器模式** 统一多种数据源（Excel、数据库、上下文变量）的接入方式
- **22种元素类型** 覆盖医疗报告单的各类可视化需求
- **版本化管理** 模板支持版本快照、回滚和差异对比

### 技术栈

| 组件 | 框架 | 关键依赖 |
|------|------|----------|
| Contracts | .NET 8 Class Library | Newtonsoft.Json 13.0.3, Nanoid 3.1.0 |
| Editor.Core | .NET 8 Class Library | EF Core 8 (SQLite/SqlServer), JWT, BCrypt, QuestPDF, SkiaSharp |
| Editor.Server | ASP.NET Core 8 | Swagger, Newtonsoft.Json, JWT Bearer |
| Editor.Server.Tests | .NET 8 | xUnit 2.6, Moq 4.20, Microsoft.AspNetCore.Mvc.Testing 8.0 |
| ReportDataMaker | WPF (.NET 10) | ClosedXML 0.105, HandyControl 3.5, 4个数据库驱动, ZXing, QuestPDF, SkiaSharp, DPAPI |

---

## 2. 架构总览

```
┌──────────────────────────────────────────────────────────────────────┐
│                      Xinglin.ReportPlatform                         │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌───────────────┐     ┌───────────────┐     ┌───────────────────┐  │
│  │    Editor     │────▶│   Contracts   │◀────│    Generators     │  │
│  │  (Web编辑器)  │     │  (契约层)     │     │  (数据生产器)     │  │
│  └───────────────┘     └───────────────┘     └───────────────────┘  │
│         │                     │                      │               │
│         │              ┌──────┴──────┐               │               │
│         │              │             │               │               │
│         ▼              ▼             ▼               ▼               │
│  ┌────────────┐  ┌──────────┐ ┌──────────┐  ┌──────────────────┐   │
│  │  ASP.NET   │  │  元素    │ │  适配器  │  │  ReportDataMaker │   │
│  │  Core API  │  │  模型    │ │  契约    │  │  (WPF桌面应用)   │   │
│  └────────────┘  └──────────┘ └──────────┘  └──────────────────┘   │
│                                                      │             │
│                                              ┌───────┴───────┐     │
│                                              │               │     │
│                                              ▼               ▼     │
│                                      ┌────────────┐ ┌──────────┐  │
│                                      │   Excel    │ │  数据库  │  │
│                                      │   适配器   │ │  适配器  │  │
│                                      └────────────┘ └──────────┘  │
└──────────────────────────────────────────────────────────────────────┘
```

**架构核心原则**：

- **单向依赖**：Editor 和 Generators 均依赖 Contracts，二者之间无直接依赖
- **契约先行**：所有共享数据结构定义在 Contracts 层，保证两端一致性
- **适配器抽象**：通过 `IDataAdapter` 接口统一数据源接入，新增数据源只需实现接口

---

## 3. 解决方案结构

```
ReportPlatform.sln
├── Contracts/                              # 契约定义层（共享库）
│   ├── Models/
│   │   ├── Elements/                       # 22种元素类型定义
│   │   ├── Adapters/                       # 适配器接口与配置模型
│   │   └── Template/                       # 模板定义与数据绑定
│   ├── DTOs/                               # 数据传输对象
│   ├── Requests/                           # API请求模型
│   ├── Responses/                          # 统一响应模型
│   ├── Enums/                              # 枚举定义
│   ├── Registry/                           # 元素分组注册表
│   ├── Converters/                         # JSON多态序列化转换器
│   ├── IdGenerator.cs                      # Nanoid唯一ID生成
│   └── TemplateSerializer.cs               # 模板序列化器
│
├── Editor/                                 # Web编辑器
│   ├── Core/                               # 编辑器核心逻辑
│   │   ├── Data/                           # EF Core实体与数据库上下文
│   │   ├── Services/                       # 业务服务实现
│   │   ├── SharedInterfaces/               # 共享接口定义
│   │   └── Extensions/                     # DI注册扩展
│   ├── Server/                             # ASP.NET Core API服务端
│   │   ├── Controllers/                    # REST API控制器
│   │   └── Middleware/                     # 全局异常与日志中间件
│   └── Server.Tests/                       # 单元测试
│
└── Generators/                             # 数据生产器层
    └── ReportDataMaker/                    # WPF桌面应用
        ├── Models/                         # 外部模板与扩展元素模型
        ├── Services/
        │   ├── ExcelAdapter/               # Excel适配器实现
        │   ├── DatabaseAdapter/            # 数据库适配器实现
        │   ├── ContextAdapter/             # 上下文适配器实现
        │   ├── PdfExport/                  # PDF导出服务
        │   ├── DataBindingService.cs       # 数据绑定服务
        │   ├── TemplateLoaderService.cs    # 模板加载服务
        │   ├── AdapterConfigStore.cs       # 适配器配置持久化
        │   └── ConfigProtector.cs          # 连接字符串加密
        ├── ViewModels/                     # MVVM视图模型
        │   ├── Tabs/                       # 标签页视图模型
        │   └── Dialogs/                    # 对话框视图模型
        ├── Views/                          # XAML视图
        ├── Infrastructure/                 # MVVM基础设施
        ├── Converters/                     # WPF值转换器
        ├── Templates/                      # 内置医疗模板（7个）
        └── Configs/                        # 适配器配置文件
```

---

## 4. 模块详解

### 4.1 Contracts — 契约层

> 命名空间：`Xinglin.ReportEditor.Contracts`
> 项目文件：[Xinglin.WebReportEditor.Contracts.csproj](Contracts/Xinglin.WebReportEditor.Contracts.csproj)
> 目标框架：.NET 8

契约层是整个平台的基石，定义了 Editor 和 Generators 之间的共享数据结构。

#### 4.1.1 元素模型体系

**继承层次**：

```
ElementBase (抽象基类)
│   属性: Id, X, Y, Width, Height, Rotation, ZIndex, Tooltip, IsLocked,
│         IsVisible, BackgroundColor, BorderColor, BorderWidth, BorderStyle,
│         CornerRadius, Opacity, ForegroundColor, FontFamily, FontSize,
│         FontWeight, FontStyle, TextAlignment, Label, DataPath, FormatString
│
└── ExternalElementBase (外部元素基类)
    │   属性: IsDataBound, IsRequired, Group (ElementGroup), AdapterId
    │
    ├── TextElement          ├── NumberElement
    ├── LineElement          ├── DateElement
    ├── TableElement         ├── DropdownElement
    ├── ShapeElement         ├── CheckboxElement
    ├── DividerElement       ├── RadioElement
    ├── BarcodeElement       ├── ImageElement
    ├── QrCodeElement        ├── IconElement
    ├── SignatureElement     ├── HyperlinkElement
    ├── ContainerElement     ├── ChartElement
    ├── RepeatElement        ├── HeaderElement
    ├── FooterElement        ├── PageNumberElement
    └── WatermarkElement
```

**关键类说明**：

| 类名 | 文件 | 说明 |
|------|------|------|
| `ElementBase` | [ElementBase.cs](Contracts/Models/Elements/ElementBase.cs) | 所有元素的抽象基类，定义布局、样式、字体等公共属性。Id 使用 `IdGenerator.NewId()` 懒初始化 |
| `ExternalElementBase` | [ExternalElementBase.cs](Contracts/Models/Elements/ExternalElementBase.cs) | 继承 ElementBase，增加数据绑定（`IsDataBound`、`IsRequired`、`Group`）和适配器关联（`AdapterId`）属性 |
| `TextElement` | [TextElement.cs](Contracts/Models/Elements/TextElement.cs) | 文本元素，最常用的元素类型 |
| `TableElement` | [TableElement.cs](Contracts/Models/Elements/TableElement.cs) | 表格元素，支持行列定义和单元格数据绑定 |
| `ContainerElement` | [ContainerElement.cs](Contracts/Models/Elements/ContainerElement.cs) | 容器元素，可嵌套子元素 |
| `RepeatElement` | [RepeatElement.cs](Contracts/Models/Elements/RepeatElement.cs) | 重复元素，用于动态列表渲染 |
| `ChartElement` | [ChartElement.cs](Contracts/Models/Elements/ChartElement.cs) | 图表元素，支持多种图表类型 |

#### 4.1.2 元素分组机制

**`ElementGroup` 枚举**（[ElementGroup.cs](Contracts/Enums/ElementGroup.cs)）：

| 值 | 说明 | 判定规则 |
|----|------|----------|
| `Fixed` | 固定元素，无数据绑定 | `DataPath` 为空 |
| `Context` | 上下文元素 | `DataPath` 以 `Context.` 开头 |
| `Editable` | 可编辑元素 | 有 `DataPath` 但无 `AdapterId`，且非 Table/Repeat/Chart |
| `DataAdapter` | 适配器元素 | 有 `AdapterId` 或为 Table/Repeat/Chart 类型 |

**`ElementAdaptationGroup` 枚举**（[ElementAdaptationGroup.cs](Contracts/Enums/ElementAdaptationGroup.cs)）：

| 值 | 包含元素 |
|----|----------|
| `Basic` | Line, Divider, Shape |
| `Form` | Text, Number, Date, Dropdown, Checkbox, Radio |
| `Data` | Table, Repeat, Chart |
| `Advanced` | Image, Barcode, QrCode, Signature, Hyperlink, Icon, Container, Header, Footer, PageNumber, Watermark |

#### 4.1.3 模板定义

**`TemplateDefinition`**（[TemplateDefinition.cs](Contracts/Models/Template/TemplateDefinition.cs)）：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `string?` | 模板唯一标识 |
| `Name` | `string` | 模板名称 |
| `Version` | `int` | 模板版本号 |
| `Type` | `string` | 模板类型 |
| `HospitalId` | `string?` | 所属医院标识 |
| `PageSettings` | `PageSettings` | 页面设置（宽高、边距、方向、背景色） |
| `DataBindings` | `List<DataBindingDefinition>` | 数据绑定定义列表 |
| `Elements` | `List<ExternalElementBase>` | 模板元素列表（多态序列化） |
| `EnableGlobalFontSize` | `bool` | 是否启用全局字体大小 |
| `GlobalFontSize` | `double?` | 全局字体大小 |

**`PageSettings`**（[PageSettings.cs](Contracts/Models/Template/PageSettings.cs)）：

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `PageWidth` | 210 | 页面宽度（毫米） |
| `PageHeight` | 297 | 页面高度（毫米） |
| `MarginLeft/Right/Top/Bottom` | 20 | 页面边距（毫米） |
| `Orientation` | Portrait | 页面方向（Portrait/Landscape） |
| `BackgroundColor` | #FFFFFF | 背景颜色 |

**`DataBindingDefinition`**（[DataBindingDefinition.cs](Contracts/Models/Template/DataBindingDefinition.cs)）：

| 属性 | 说明 |
|------|------|
| `ElementId` | 关联的元素标识 |
| `DataPath` | 数据路径 |
| `BindingType` | 绑定类型（Text/Image/Visibility/Repeat/Style） |
| `FormatString` | 格式化字符串 |
| `DefaultValue` | 默认值 |
| `Transform` | 数据转换方式 |

#### 4.1.4 适配器契约

**`AdapterType` 枚举**（[AdapterType.cs](Contracts/Enums/AdapterType.cs)）：

| 值 | 说明 |
|----|------|
| `Context` | 上下文数据适配器 |
| `Excel` | Excel数据适配器 |
| `Database` | 数据库数据适配器 |
| `Api` | API数据适配器（预留） |

**`DatabaseProvider` 枚举**：

| 值 | 说明 |
|----|------|
| `SqlServer` | SQL Server |
| `MySql` | MySQL |
| `Sqlite` | SQLite |
| `PostgreSql` | PostgreSQL |

**适配器配置模型**（`Contracts/Models/Adapters/`）：

- `AdapterConfigBase` — 适配器配置基类，包含 `AdapterId`、`Type`、`DisplayName`、`IsEnabled` 等通用字段
- `FieldSchema` — 字段数据模式（DataPath、Label、DataType、Format、Options、IsRequired、MinValue、MaxValue、DecimalPlaces）
- `AdapterResult` — 适配器读取结果（Success、Data、BatchData、ErrorMessage）
- `ValidationResult` — 配置校验结果

#### 4.1.5 序列化机制

**`ElementJsonConverter`**（[ElementJsonConverter.cs](Contracts/Converters/ElementJsonConverter.cs)）：

基于 Newtonsoft.Json 的自定义转换器，实现 `ExternalElementBase` 的多态序列化：

- **序列化**：通过 `ReverseTypeMap` 将元素类型映射为 `$type` 鉴别符（格式 `template.element.{shortType}`）
- **反序列化**：通过 `WebShortTypeMap` 将 `$type` 映射回具体元素类型，同时调用 `ClassifyElement()` 自动分类元素分组
- **兼容性**：`MapType()` 方法支持旧版类型名称映射（如 `LabelElement` → `TextElement`，`RectangleElement` → `ShapeElement`）

**`TemplateSerializer`**（[TemplateSerializer.cs](Contracts/TemplateSerializer.cs)）：

封装序列化配置的静态工具类：

| 方法 | 说明 |
|------|------|
| `Deserialize(string json)` | 从 JSON 反序列化 `TemplateDefinition` |
| `Serialize(TemplateDefinition template)` | 将 `TemplateDefinition` 序列化为 JSON |
| `DeserializeElements(string json)` | 从 JSON 反序列化元素列表 |
| `SerializeElements(IEnumerable<ExternalElementBase> elements)` | 将元素列表序列化为 JSON |

序列化配置：CamelCase 命名、忽略 Null 值、枚举转字符串、注册 `ElementJsonConverter`。

**`IdGenerator`**（[IdGenerator.cs](Contracts/IdGenerator.cs)）：

基于 Nanoid 生成 21 位唯一标识符。

#### 4.1.6 元素分组注册表

**`ElementGroupRegistry`**（[ElementGroupRegistry.cs](Contracts/Registry/ElementGroupRegistry.cs)）：

静态注册表，管理元素类型与 `ElementAdaptationGroup` 的映射关系：

| 方法 | 说明 |
|------|------|
| `GetGroup(Type elementType)` | 根据元素类型获取所属适配分组 |
| `GetElementsInGroup(ElementAdaptationGroup group)` | 获取指定分组中的所有元素类型 |
| `GetAllElementTypes()` | 获取所有已注册的元素类型 |
| `GetAllGroups()` | 获取所有适配分组 |

#### 4.1.7 DTO 与请求/响应模型

**DTOs**（`Contracts/DTOs/`）：

| 类 | 说明 |
|----|------|
| `TemplateDto` | 模板列表项 DTO（Id、Name、Type、Version、HospitalId 等） |
| `TemplateDetailDto` | 模板详情 DTO，额外包含 `ContentJson` |
| `TemplateVersionDto` | 版本列表项 DTO |
| `TemplateVersionDetailDto` | 版本详情 DTO，额外包含 `ContentJson` |
| `AuthDtos` | 认证相关 DTO（LoginRequest/Response、RefreshTokenRequest/Response、UserDto） |

**统一响应**（`Contracts/Responses/`）：

- `ApiResponse<T>` — 统一 API 响应包装，包含 `Code`、`Message`、`Data`，提供 `Ok()` 和 `Fail()` 静态工厂方法
- `PagedResponse<T>` — 分页响应，包含 `Items`、`TotalCount`、`Page`、`PageSize`、`TotalPages`、`HasPrevious`、`HasNext`

---

### 4.2 Editor — Web编辑器

> 命名空间：`Xinglin.WebReportEditor.Core` / `Xinglin.WebReportEditor.Server`
> 项目文件：[Xinglin.WebReportEditor.Core.csproj](Editor/Core/Xinglin.WebReportEditor.Core.csproj) / [Xinglin.WebReportEditor.Server.csproj](Editor/Server/Xinglin.WebReportEditor.Server.csproj)
> 目标框架：.NET 8

#### 4.2.1 数据层

**`TemplateDbContext`**（[TemplateDbContext.cs](Editor/Core/Data/TemplateDbContext.cs)）：

EF Core 数据库上下文，管理以下实体：

| 实体 | DbSet | 说明 |
|------|-------|------|
| `TemplateEntity` | `Templates` | 模板主表 |
| `TemplateVersionEntity` | `TemplateVersions` | 版本快照 |
| `UserEntity` | `Users` | 用户账户 |
| `RefreshTokenEntity` | `RefreshTokens` | 刷新令牌 |

**实体关系**：

```
TemplateEntity 1:N ── TemplateVersionEntity
UserEntity     1:N ── RefreshTokenEntity
```

**`TemplateEntity`** 关键属性：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `Guid` | 主键 |
| `Name` | `string` | 模板名称 |
| `Type` | `string` | 模板类型 |
| `Version` | `int` | 当前版本号 |
| `ContentJson` | `string` | 模板内容 JSON |
| `HospitalId` | `string?` | 医院标识 |
| `IsDefault` | `bool` | 是否为默认模板 |
| `IsPublished` | `bool` | 是否已发布 |
| `CreatedBy` | `string?` | 创建者 |
| `CreateTime` / `UpdateTime` | `DateTime` | 创建/更新时间 |

**`TemplateSeedData`**（[TemplateSeedData.cs](Editor/Core/Data/TemplateSeedData.cs)）：

种子数据初始化，预置 admin/editor 两个账户和默认模板数据。

#### 4.2.2 服务层

**`ServiceCollectionExtensions`**（[ServiceCollectionExtensions.cs](Editor/Core/Extensions/ServiceCollectionExtensions.cs)）：

DI 注册扩展方法 `AddCoreServices()`，注册以下服务：

| 注册 | 生命周期 | 说明 |
|------|----------|------|
| `TemplateDbContext` | Scoped | 按 `DatabaseProvider` 配置切换 SQLite/SqlServer |
| `ITemplateService → TemplateService` | Scoped | 模板 CRUD |
| `IAuthService → AuthService` | Scoped | JWT 认证 |
| `IVersionService → VersionService` | Scoped | 版本管理 |
| `IPdfRenderService → PdfRenderService` | Scoped | PDF 渲染 |
| `IJsonTemplateSerializer → JsonTemplateSerializer` | Scoped | JSON 序列化 |
| `IDataBindingEngine → DataBindingEngine` | Scoped | 数据绑定引擎 |
| `IPdfSharpTemplateRenderer → PdfTemplateRenderer` | Scoped | PDF 模板渲染器 |
| `ContextService` | Scoped | 上下文服务 |
| `TimeProvider.System` | Singleton | 时间提供者 |

**`TemplateService`**（[TemplateService.cs](Editor/Core/Services/TemplateService.cs)）：

| 方法 | 说明 |
|------|------|
| `GetTemplatesAsync(TemplateFilterRequest)` | 分页查询模板列表，支持按 Name/Type/HospitalId/IsPublished 过滤 |
| `GetTemplateAsync(Guid id)` | 获取模板详情 |
| `CreateTemplateAsync(CreateTemplateRequest)` | 创建模板，同时创建初始版本快照 |
| `UpdateTemplateAsync(Guid id, UpdateTemplateRequest)` | 更新模板，ContentJson 变更时自动创建新版本快照 |
| `DeleteTemplateAsync(Guid id)` | 删除模板 |

**`AuthService`**（[AuthService.cs](Editor/Core/Services/AuthService.cs)）：

| 方法 | 说明 |
|------|------|
| `LoginAsync(LoginRequest)` | 用户登录，BCrypt 密码验证，生成 JWT AccessToken + RefreshToken |
| `RefreshTokenAsync(string)` | 刷新令牌，旧令牌标记为已撤销 |
| `RevokeTokenAsync(string)` | 撤销刷新令牌 |
| `GetUserByCredentialsAsync(string, string)` | 根据用户名密码获取用户信息 |

JWT Token 包含 Claims：Sub（用户ID）、Name（用户名）、Role（角色）、HospitalId（医院ID）。

**`VersionService`**（[VersionService.cs](Editor/Core/Services/VersionService.cs)）：

| 方法 | 说明 |
|------|------|
| `GetVersionsAsync(Guid templateId)` | 获取模板所有版本（按版本号降序） |
| `GetVersionAsync(Guid templateId, Guid versionId)` | 获取版本详情（含 ContentJson） |
| `RollbackAsync(Guid templateId, Guid versionId, string? createdBy)` | 回滚到指定版本，创建新版本快照 |
| `DiffAsync(Guid templateId, Guid vidA, Guid vidB)` | 版本差异对比，比较元素增删改和页面属性变更 |

**`ContextService`**（[ContextService.cs](Editor/Core/Services/ContextService.cs)）：

提供模板渲染所需的上下文信息（时间、系统信息等）。

**`PdfRenderService`** / **`PdfTemplateRenderer`**：

PDF 渲染服务，基于 QuestPDF + SkiaSharp 将模板渲染为 PDF 或图片。

**`DataBindingEngine`**（[DataBindingEngine.cs](Editor/Core/Services/DataBindingEngine.cs)）：

数据绑定引擎，将样本数据注入模板元素，用于编辑器预览。

#### 4.2.3 API 端点

| 控制器 | 路由前缀 | 端点 | 方法 | 说明 |
|--------|----------|------|------|------|
| `AuthController` | `/api/auth` | `POST /login` | `Login` | 用户登录 |
| | | `POST /refresh` | `RefreshToken` | 刷新令牌 |
| | | `POST /logout` | `Logout` | 用户登出 |
| `TemplatesController` | `/api/templates` | `GET /` | `GetTemplates` | 分页获取模板列表 |
| | | `GET /{id}` | `GetTemplate` | 获取模板详情 |
| | | `POST /` | `CreateTemplate` | 创建模板 |
| | | `PUT /{id}` | `UpdateTemplate` | 更新模板 |
| | | `DELETE /{id}` | `DeleteTemplate` | 删除模板 |
| `VersionsController` | `/api/templates/{templateId}/versions` | `GET /` | `GetVersions` | 获取版本列表 |
| | | `GET /{versionId}` | `GetVersion` | 获取版本详情 |
| | | `POST /{versionId}/rollback` | `Rollback` | 回滚到指定版本 |
| | | `GET /diff` | `Diff` | 版本差异对比 |
| `PreviewController` | `/api/templates/{templateId}/preview` | `GET /image` | `GetPreviewImage` | 获取预览图片（Base64） |
| | | `GET /pdf` | `GetPdf` | 获取 PDF 文件 |
| `ContextController` | `/api/context` | `GET /values` | `GetContextValues` | 获取所有内置上下文值 |
| | | `GET /resolve` | `ResolveContextValue` | 解析指定路径的上下文值 |
| `HealthController` | `/api/health` | `GET /` | `Get` | 健康检查 |

#### 4.2.4 中间件管道

```
Request → GlobalExceptionMiddleware → ApiLoggingMiddleware → CORS → Authentication → Authorization → Controller
```

**`GlobalExceptionMiddleware`**（[GlobalExceptionMiddleware.cs](Editor/Server/Middleware/GlobalExceptionMiddleware.cs)）：

统一异常处理中间件，将异常映射为标准 `ApiResponse`：

| 异常类型 | HTTP 状态码 |
|----------|-------------|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `ArgumentException` | 400 Bad Request |
| `InvalidOperationException` | 409 Conflict |
| 其他 | 500 Internal Server Error |

**`ApiLoggingMiddleware`**（[ApiLoggingMiddleware.cs](Editor/Server/Middleware/ApiLoggingMiddleware.cs)）：

API 日志中间件，记录每个请求的方法、路径、查询参数、客户端 IP、响应状态码和耗时。

#### 4.2.5 配置

**`appsettings.json`** 关键配置：

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `DatabaseProvider` | 数据库提供者 | `Sqlite` |
| `ConnectionStrings:SqliteConnection` | SQLite 连接字符串 | `Data Source=xinglin_webreport.db` |
| `ConnectionStrings:SqlServerConnection` | SQL Server 连接字符串 | — |
| `Jwt:SecretKey` | JWT 签名密钥 | — |
| `Jwt:AccessTokenExpirationMinutes` | AccessToken 过期时间（分钟） | 30 |
| `Jwt:RefreshTokenExpirationDays` | RefreshToken 过期时间（天） | 7 |
| `Cors:AllowedOrigins` | CORS 允许的源 | `http://localhost:5173` 等 |

---

### 4.3 Generators — 数据生产器

> 命名空间：`ReportDataMaker`
> 项目文件：[ReportDataMaker.csproj](Generators/ReportDataMaker/ReportDataMaker.csproj)
> 目标框架：.NET 10 (WPF)

#### 4.3.1 应用入口

**`App.xaml.cs`**（[App.xaml.cs](Generators/ReportDataMaker/App.xaml.cs)）：

WPF 应用程序入口，负责 DI 容器配置和启动：

1. 初始化 QuestPDF 社区许可证
2. 初始化 `FileLogger`
3. 注册所有服务到 `ServiceCollection`
4. 构建 `ServiceProvider`
5. 执行模板加载测试（`TestTemplateLoading`）
6. 创建 `MainViewModel` 并绑定到 `MainWindow`

**DI 注册清单**：

| 注册 | 生命周期 | 说明 |
|------|----------|------|
| `IDialogService → DialogService` | Singleton | 对话框服务 |
| `ITemplateLoaderService → TemplateLoaderService` | Singleton | 模板加载 |
| `ITemplatePreviewService → TemplatePreviewService` | Singleton | 模板预览 |
| `IDataBindingService → DataBindingService` | Singleton | 数据绑定 |
| `AdapterConfigStore` | Singleton | 适配器配置存储 |
| `ExcelAdapterFactory` | Singleton | Excel 适配器工厂 |
| `DatabaseProviderRegistry` | Singleton | 数据库提供者注册表 |
| `ConnectionPoolManager` | Singleton | 连接池管理 |
| `DatabaseAdapterFactory` | Singleton | 数据库适配器工厂 |
| `ContextProfileStore` | Singleton | 上下文配置存储 |
| `ContextAdapterService` | Singleton | 上下文适配器服务 |
| `ContextAdapterFactory` | Singleton | 上下文适配器工厂 |
| `IPdfExportService → PdfExportService` | Singleton | PDF 导出 |
| `BatchExportService` | Singleton | 批量导出 |
| `ExportHistoryStore` | Singleton | 导出历史 |
| `MainViewModel` | Transient | 主视图模型 |

#### 4.3.2 MVVM 架构

**基础设施层**（`Infrastructure/`）：

| 类 | 文件 | 说明 |
|----|------|------|
| `ViewModelBase` | [ViewModelBase.cs](Generators/ReportDataMaker/Infrastructure/ViewModelBase.cs) | MVVM 基类，实现 `INotifyPropertyChanged`，提供 `SetProperty<T>()` 泛型属性设置方法 |
| `RelayCommand` | [RelayCommand.cs](Generators/ReportDataMaker/Infrastructure/RelayCommand.cs) | 同步命令实现，`ICommand` 的委托封装，支持 `CanExecute` 谓词 |
| `AsyncRelayCommand` | [AsyncRelayCommand.cs](Generators/ReportDataMaker/Infrastructure/AsyncRelayCommand.cs) | 异步命令实现，支持 `Task` 返回的执行函数，内置 `IsExecuting` 防重入 |
| `IDialogService` / `DialogService` | [IDialogService.cs](Generators/ReportDataMaker/Infrastructure/IDialogService.cs) | 对话框服务接口/实现，提供文件打开、消息提示、错误提示等 UI 交互 |
| `FileLogger` | [FileLogger.cs](Generators/ReportDataMaker/Infrastructure/FileLogger.cs) | 文件日志记录器，单例模式，日志写入 `%AppData%/ReportDataMaker/` |
| `FieldDataTemplateSelector` | [FieldDataTemplateSelector.cs](Generators/ReportDataMaker/Infrastructure/FieldDataTemplateSelector.cs) | 字段数据模板选择器，根据字段类型选择 WPF DataTemplate |
| `ReportExternalElementConverter` | [ReportExternalElementConverter.cs](Generators/ReportDataMaker/Infrastructure/ReportExternalElementConverter.cs) | 外部元素类型转换器 |

**ViewModel 层次**：

```
ViewModelBase (INotifyPropertyChanged)
├── MainViewModel              # 主窗口：模板加载、适配器管理、标签页切换
├── SidePanelViewModel         # 侧边面板：展开/折叠
├── TemplateLoadViewModel      # 模板加载
├── AdapterItemViewModel       # 适配器条目：侧边栏适配器列表项
└── TabViewModelBase (抽象)    # 标签页基类
    ├── MainTabViewModel       # 主标签页：数据录入 + 预览
    ├── DataEntryTabViewModel  # 数据录入：字段编辑与适配器数据应用
    ├── PreviewTabViewModel    # 模板预览：可视化预览与缩放
    ├── ExcelAdapterTabViewModel   # Excel适配器：导出/导入/校验
    ├── DatabaseAdapterTabViewModel # 数据库适配器：连接/查询/映射
    ├── ContextAdapterTabViewModel  # 上下文适配器：静态值/动态规则
    └── ExportTabViewModel     # 导出：PDF导出/批量导出/打印预览
```

**`MainViewModel`**（[MainViewModel.cs](Generators/ReportDataMaker/ViewModels/MainViewModel.cs)）：

主视图模型，核心协调者：

| 命令 | 说明 | 启用条件 |
|------|------|----------|
| `LoadTemplateCommand` | 加载模板文件 | 始终 |
| `ToggleSidePanelCommand` | 切换侧边面板 | 始终 |
| `AddExcelAdapterCommand` | 添加 Excel 适配器 | 已加载模板 |
| `AddDbAdapterCommand` | 添加数据库适配器 | 已加载模板 |
| `EditContextCommand` | 编辑上下文 | 已加载模板 |
| `ExportPdfCommand` | 导出 PDF | 已加载模板 |
| `BatchExportCommand` | 批量导出 | 已加载模板 |
| `PrintPreviewCommand` | 打印预览 | 已加载模板 |
| `SaveCommand` | 保存配置 | 已加载模板 |
| `ExitCommand` | 退出应用 | 始终 |

**标签页工作流**：

1. 加载模板 → 自动创建「主标签页」（数据录入+预览）和「导出标签页」
2. 添加适配器 → 在导出标签页前插入适配器标签页
3. 适配器数据 → 通过 `ApplyDataFromAdapter` 回填到数据录入标签页

#### 4.3.3 外部模板模型

**`ExternalTemplateDefinition`**（[ExternalTemplateModels.cs](Generators/ReportDataMaker/Models/ExternalTemplateModels.cs)）：

ReportDataMaker 使用的模板定义模型，与 Contracts 层的 `TemplateDefinition` 对应但结构扁平化：

| 属性 | 说明 |
|------|------|
| `Id`, `Name`, `Type`, `Version`, `HospitalId` | 基本信息 |
| `PageWidth`, `PageHeight`, `Orientation` | 页面尺寸 |
| `MarginLeft/Right/Top/Bottom`, `BackgroundColor` | 页面边距与背景 |
| `GlobalFontSize`, `EnableGlobalFontSize` | 全局字体 |
| `Elements` | `List<ReportExternalElementBase>` 元素列表 |
| `DataBindings` | `List<LegacyDataBindingDefinition>` 数据绑定 |
| `FilePath` | 模板文件路径 |

**`ReportExternalElementBase`**（[ExternalExtendedElements.cs](Generators/ReportDataMaker/Models/ExternalExtendedElements.cs)）：

继承自 Contracts 层的 `ExternalElementBase`，使用 `new` 关键字重新定义样式属性为非空类型，并增加：

| 属性 | 说明 |
|------|------|
| `Shadow` | 阴影效果 |
| `LabelWidth` | 标签宽度 |
| `DefaultValue` | 默认值 |
| `Options` | 选项列表 |
| `ElementType` | 元素类型标识 |

具体元素类型（共 18 种）：`ExternalTextElement`、`ExternalLineElement`、`ExternalDropdownElement`、`ExternalNumberElement`、`ExternalDateElement`、`ExternalTableElement`、`ExternalImageElement`、`ExternalShapeElement`、`ExternalDividerElement`、`ExternalCheckboxElement`、`ExternalRadioElement`、`ExternalSignatureElement`、`ExternalBarcodeElement`、`ExternalQrCodeElement`、`ExternalChartElement`、`ExternalContainerElement`、`ExternalRepeatElement`、`ExternalHeaderElement`、`ExternalFooterElement`、`ExternalPageNumberElement`、`ExternalWatermarkElement`、`ExternalIconElement`、`ExternalHyperlinkElement`。

#### 4.3.4 数据绑定服务

**`DataBindingService`**（[DataBindingService.cs](Generators/ReportDataMaker/Services/DataBindingService.cs)）：

| 方法 | 说明 |
|------|------|
| `ApplyData(ExternalTemplateDefinition, Dictionary<string, object>)` | 将适配器数据应用到模板元素。普通元素设置 `DefaultValue`；表格元素按单元格 DataPath 或 `R{row}C{col}` 模式填充 `CellData` |
| `ExtractData(ExternalTemplateDefinition)` | 从模板元素提取数据为字典。表格元素同时提取单元格级别数据和整体数据 |

**表格数据处理策略**：

1. 优先检查单元格级别数据（`cell.DataPath` 或 `{tableDataPath}.R{row}C{col}`）
2. 若无单元格级数据，尝试整体数据（`List<List<string>>` 或 `TableDataValue`）
3. 自动初始化 `CellData` 矩阵以匹配表格行列数

#### 4.3.5 Excel 适配器

**数据流**：

```
模板 → TemplateFlattenService → TemplateFieldSchema → ExcelSchemaExporter → .xlsx文件
                                                                    ↓ (用户填写)
.xlsx文件 → ExcelContractReader → AdapterResult → DataBindingService → 模板
```

**Excel 文件结构**（4行模式）：

| 行 | 可见性 | 内容 |
|----|--------|------|
| 第1行 | 隐藏 | 契约行，存储 DataPath |
| 第2行 | 可见 | 标签行，显示字段名称 |
| 第3行 | 隐藏 | 类型行，标注数据类型与约束 |
| 第4行起 | 可见 | 数据行 |

**核心组件**：

| 类 | 文件 | 说明 |
|----|------|------|
| `ExcelAdapterFactory` | [ExcelAdapterFactory.cs](Generators/ReportDataMaker/Services/ExcelAdapter/ExcelAdapterFactory.cs) | 工厂类，聚合 Flatten/Export/Read/Validate 功能 |
| `TemplateFlattenService` | [TemplateFlattenService.cs](Generators/ReportDataMaker/Services/ExcelAdapter/TemplateFlattenService.cs) | 将模板元素树扁平化为字段列表，过滤 `ElementGroup.Editable` 元素，处理表格单元格展开 |
| `ExcelSchemaExporter` | [ExcelSchemaExporter.cs](Generators/ReportDataMaker/Services/ExcelAdapter/ExcelSchemaExporter.cs) | 导出带契约行的 Excel 模板，含填写说明页 |
| `ExcelContractReader` | [ExcelContractReader.cs](Generators/ReportDataMaker/Services/ExcelAdapter/ExcelContractReader.cs) | 按契约行读取 Excel 数据（单条/批量） |
| `ExcelDataValidator` | [ExcelDataValidator.cs](Generators/ReportDataMaker/Services/ExcelAdapter/ExcelDataValidator.cs) | 校验数据类型、范围、选项、格式 |
| `ExcelAdapterConfig` | [ExcelAdapterConfig.cs](Generators/ReportDataMaker/Services/ExcelAdapter/ExcelAdapterConfig.cs) | Excel 适配器配置模型 |
| `TemplateFieldSchema` | [TemplateFieldSchema.cs](Generators/ReportDataMaker/Services/ExcelAdapter/TemplateFieldSchema.cs) | 模板字段模式定义 |

#### 4.3.6 数据库适配器

**架构**：

```
DatabaseProviderRegistry → IDatabaseProvider
├── SqlServerProvider    (Microsoft.Data.SqlClient)
├── MySqlProvider        (MySqlConnector)
├── SqliteProvider       (Microsoft.Data.Sqlite)
└── PostgreSqlProvider   (Npgsql)
```

**核心组件**：

| 类 | 文件 | 说明 |
|----|------|------|
| `IDatabaseProvider` | [IDatabaseProvider.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/IDatabaseProvider.cs) | 数据库提供者接口，定义连接测试、表/列浏览、分页查询等契约 |
| `DatabaseAdapterBase` | [DatabaseAdapterBase.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/DatabaseAdapterBase.cs) | 适配器基类，封装查询执行、参数绑定、结果映射、数据转换（Trim/Upper/Lower） |
| `DatabaseAdapterFactory` | [DatabaseAdapterFactory.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/DatabaseAdapterFactory.cs) | 工厂类，提供连接测试、表/列浏览、查询执行、预览、校验 |
| `SqlBuilder` | [SqlBuilder.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/SqlBuilder.cs) | SQL 构建器，支持 RawSQL 和 VisualBuilder 两种模式，自动处理 JOIN |
| `ConnectionPoolManager` | [ConnectionPoolManager.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/ConnectionPoolManager.cs) | 连接池管理，基于 `SemaphoreSlim` 的并发控制 |
| `DatabaseProviderRegistry` | [DatabaseProviderRegistry.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/DatabaseProviderRegistry.cs) | 提供者注册表，`CreateDefault()` 注册四种数据库提供者 |
| `DatabaseAdapterConfig` | [DatabaseAdapterConfig.cs](Generators/ReportDataMaker/Services/DatabaseAdapter/DatabaseAdapterConfig.cs) | 配置模型，包含 Provider、ConnectionString、Query、DbFieldMappings、Parameters、Joins |

**查询模式**：

| 模式 | 说明 |
|------|------|
| `RawSQL` | 直接编写 SQL 语句 |
| `VisualBuilder` | 可视化构建：选择主表、列、WHERE 条件、排序，自动生成 SQL |

**字段映射**：`DbFieldMapping` 将数据库列名映射到模板 DataPath，支持 `Trim()`/`Upper()`/`Lower()` 数据转换。

**`IDatabaseProvider` 接口方法**：

| 方法 | 说明 |
|------|------|
| `TestConnectionAsync(string)` | 测试数据库连接 |
| `CreateConnection(string)` | 创建数据库连接 |
| `GetTablesAsync(string)` | 获取表列表 |
| `GetColumnsAsync(string, string)` | 获取列信息 |
| `GetForeignKeysAsync(string, string)` | 获取外键信息 |
| `BuildPagedQuery(string, int, int)` | 构建分页查询 |
| `QuoteIdentifier(string)` | 引用标识符 |
| `GetParameterPrefix()` | 获取参数前缀 |

#### 4.3.7 上下文适配器

**核心组件**：

| 类 | 文件 | 说明 |
|----|------|------|
| `ContextAdapterService` | [ContextAdapterService.cs](Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs) | 上下文数据填充服务，扫描模板 DataPath，按优先级填充：静态值 → 动态规则 → 内置值 |
| `ContextAdapterFactory` | [ContextAdapterFactory.cs](Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterFactory.cs) | 工厂类，提供 Fill/Profile 管理/未配置字段检测 |
| `ContextProfileStore` | [ContextProfileStore.cs](Generators/ReportDataMaker/Services/ContextAdapter/ContextProfileStore.cs) | 上下文配置持久化 |
| `ContextAdapterConfig` | [ContextAdapterConfig.cs](Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterConfig.cs) | 配置模型，包含 StaticValues 和 DynamicRules |

**内置上下文路径**：

| DataPath | 值 |
|----------|----|
| `Context.DateTime.Now` | 当前日期时间 |
| `Context.DateTime.Date` | 当前日期 |
| `Context.DateTime.Time` | 当前时间 |
| `Context.DateTime.Year` | 当前年份 |
| `Context.DateTime.Month` | 当前月份 |
| `Context.DateTime.Day` | 当前日期 |

**动态规则源**：`CurrentDate`、`CurrentTime`、`CurrentDateTime`、`CurrentUser`、`MachineName`、`Static`

#### 4.3.8 PDF 导出

**核心组件**：

| 类 | 文件 | 说明 |
|----|------|------|
| `IPdfExportService` | [IPdfExportService.cs](Generators/ReportDataMaker/Services/PdfExport/IPdfExportService.cs) | PDF 导出服务接口 |
| `PdfExportService` | [PdfExportService.cs](Generators/ReportDataMaker/Services/PdfExport/PdfExportService.cs) | 基于 QuestPDF + SkiaSharp 的 PDF 渲染实现，支持单条和批量导出 |
| `PdfElementRenderer` | [PdfElementRenderer.cs](Generators/ReportDataMaker/Services/PdfExport/PdfElementRenderer.cs) | 元素渲染器，将各类型元素绘制到 SKCanvas |
| `PdfPageLayoutEngine` | [PdfPageLayoutEngine.cs](Generators/ReportDataMaker/Services/PdfExport/PdfPageLayoutEngine.cs) | 页面布局引擎，计算毫米到像素的转换 |
| `BatchExportService` | [BatchExportService.cs](Generators/ReportDataMaker/Services/PdfExport/BatchExportService.cs) | 批量导出服务，支持并行导出（SemaphoreSlim 控制并发度） |
| `ExportHistoryStore` | [ExportHistoryStore.cs](Generators/ReportDataMaker/Services/PdfExport/ExportHistoryStore.cs) | 导出历史记录存储（最多 100 条），持久化到 `%AppData%/ReportDataMaker/export-history.json` |

**`PdfExportService.RenderToPdf()`** 渲染流程：

1. 创建 `PdfPageLayoutEngine` 计算页面布局
2. 分离 Header/Footer/Content 元素
3. 使用 QuestPDF 构建 Document，分别渲染到 page.Header()、page.Footer()、page.Content()
4. 通过 `PdfElementRenderer` 将每个元素绘制到 SkiaSharp Canvas
5. 生成 PDF 字节数组

**批量导出文件名模式**：支持 `{index}`、`{date}`、`{name}` 占位符。

#### 4.3.9 配置安全

**`AdapterConfigStore`**（[AdapterConfigStore.cs](Generators/ReportDataMaker/Services/AdapterConfigStore.cs)）：

适配器配置持久化到 `%AppData%/ReportDataMaker/adapters.json`，按模板名称分组存储。读写时自动处理连接字符串的加密/解密。

**`ConfigProtector`**（[ConfigProtector.cs](Generators/ReportDataMaker/Services/ConfigProtector.cs)）：

| 方法 | 说明 |
|------|------|
| `Protect(string)` | 使用 DPAPI（Current User scope）加密，加密值以 `ENC:` 前缀标识 |
| `Unprotect(string)` | 解密 DPAPI 加密的值 |
| `IsProtected(string)` | 检查值是否已加密（`ENC:` 前缀） |

#### 4.3.10 内置医疗模板

| 模板 | 文件 |
|------|------|
| 住院病历 | `Templates/住院病历.json` |
| 门诊病历 | `Templates/门诊病历.json` |
| 体温单 | `Templates/体温单.json` |
| 检验报告单 | `Templates/检验报告单.json` |
| 影像报告单 | `Templates/影像报告单.json` |
| 处方单 | `Templates/处方单.json` |
| 护理记录单 | `Templates/护理记录单.json` |

---

## 5. 数据流

### 5.1 模板设计阶段

```
Editor Web UI → TemplateDefinition (JSON)
       ↓ 保存
TemplateEntity + TemplateVersionEntity (数据库)
```

### 5.2 数据生产阶段

```
ReportDataMaker 加载契约 JSON
       ↓
ExternalTemplateDefinition
       ↓
┌─────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ 上下文适配器  │  │  Excel适配器     │  │  数据库适配器     │
│ 自动填充     │  │ 导出→填写→读     │  │ 连接→查询→映射   │
└──────┬──────┘  └────────┬─────────┘  └────────┬─────────┘
       ↓                  ↓                     ↓
       └────────── AdapterResult (Dictionary) ──┘
                          ↓
              DataBindingService.ApplyData
                          ↓
                 填充后的模板 → 预览 / PDF导出
```

### 5.3 完整数据流图

```
                    ┌─────────────────────────────────────────┐
                    │            模板设计阶段                  │
                    │  Editor Web UI → TemplateDefinition     │
                    │       ↓ 保存                             │
                    │  TemplateEntity + TemplateVersionEntity │
                    └──────────────────┬──────────────────────┘
                                       │ 导出契约JSON
                    ┌──────────────────▼──────────────────────┐
                    │            数据生产阶段                  │
                    │  ReportDataMaker 加载契约JSON            │
                    │       ↓                                  │
                    │  ExternalTemplateDefinition              │
                    │       ↓                                  │
                    │  ┌─────────────┐  ┌──────────────────┐  │
                    │  │ 上下文适配器  │  │  Excel适配器     │  │
                    │  │ 自动填充     │  │ 导出→填写→读     │  │
                    │  └──────┬──────┘  └────────┬─────────┘  │
                    │         ↓                  ↓             │
                    │  ┌─────────────────────────────────┐    │
                    │  │      数据库适配器                 │    │
                    │  │      连接→查询→映射              │    │
                    │  └──────────────┬──────────────────┘    │
                    │                ↓                        │
                    │       AdapterResult (Dictionary)        │
                    │              ↓                           │
                    │     DataBindingService.ApplyData        │
                    │              ↓                           │
                    │     填充后的模板 → 预览/PDF             │
                    └─────────────────────────────────────────┘
```

---

## 6. 依赖关系

### 6.1 项目引用关系

```
Editor.Server ──▶ Editor.Core ──▶ Contracts
     │                                  ▲
     └──────────────────────────────────┘

Editor.Server.Tests ──▶ Editor.Server
                     ──▶ Editor.Core
                     ──▶ Contracts

ReportDataMaker ──▶ Contracts
```

### 6.2 NuGet 包依赖

**Contracts**：

| 包 | 版本 | 用途 |
|----|------|------|
| Newtonsoft.Json | 13.0.3 | JSON 序列化（多态转换器） |
| Nanoid | 3.1.0 | 唯一 ID 生成 |

**Editor.Core**：

| 包 | 版本 | 用途 |
|----|------|------|
| Microsoft.EntityFrameworkCore | 8.0.0 | ORM 框架 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 | SQLite 提供者 |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server 提供者 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT 认证 |
| Newtonsoft.Json | 13.0.3 | JSON 序列化 |
| System.IdentityModel.Tokens.Jwt | 8.0.0 | JWT Token 处理 |
| BCrypt.Net-Next | 4.0.3 | 密码哈希 |
| QuestPDF | 2025.1.0 | PDF 生成 |
| SkiaSharp | 3.116.1 | 图形渲染 |

**Editor.Server**：

| 包 | 版本 | 用途 |
|----|------|------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT 认证中间件 |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 8.0.0 | MVC Newtonsoft 集成 |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | EF Core 工具（迁移） |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger API 文档 |
| Swashbuckle.AspNetCore.Newtonsoft | 6.5.0 | Swagger Newtonsoft 支持 |

**Editor.Server.Tests**：

| 包 | 版本 | 用途 |
|----|------|------|
| xunit | 2.6.0 | 测试框架 |
| xunit.runner.visualstudio | 2.5.0 | 测试运行器 |
| Moq | 4.20.69 | Mock 框架 |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | 集成测试 |
| Microsoft.NET.Test.Sdk | 17.8.0 | 测试 SDK |

**ReportDataMaker**：

| 包 | 版本 | 用途 |
|----|------|------|
| ClosedXML | 0.105.0 | Excel 文件操作 |
| Microsoft.Data.SqlClient | 7.0.1 | SQL Server 驱动 |
| Microsoft.Data.Sqlite | 10.0.7 | SQLite 驱动 |
| MySqlConnector | 2.5.0 | MySQL 驱动 |
| Npgsql | 9.0.3 | PostgreSQL 驱动 |
| Newtonsoft.Json | 13.0.4 | JSON 序列化 |
| ZXing.Net.Bindings.Windows.Compatibility | 0.16.13 | 条形码/二维码生成 |
| HandyControl | 3.5.1 | WPF UI 控件库 |
| QuestPDF | 2025.1.0 | PDF 生成 |
| SkiaSharp | 3.116.1 | 图形渲染 |
| System.Security.Cryptography.ProtectedData | 10.0.7 | DPAPI 加密 |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | DI 容器 |

---

## 7. 项目运行方式

### 7.1 环境要求

- **.NET SDK**：8.0+（global.json 指定 8.0.0，rollForward 为 latestMajor）
- **ReportDataMaker**：需要 .NET 10 SDK（目标框架 `net10.0-windows`）
- **操作系统**：Windows（WPF 应用需要 Windows）
- **IDE**：Visual Studio 2022 v17.5+ 或 JetBrains Rider

### 7.2 构建项目

```bash
# 还原依赖
dotnet restore

# 构建整个解决方案
dotnet build

# 发布 Editor.Server
dotnet publish Editor/Server/Xinglin.WebReportEditor.Server.csproj -c Release

# 发布 ReportDataMaker
dotnet publish Generators/ReportDataMaker/ReportDataMaker.csproj -c Release
```

### 7.3 运行 Editor.Server（Web API）

```bash
cd Editor/Server
dotnet run
```

- 默认监听：`http://localhost:5000` / `https://localhost:5001`（由 `launchSettings.json` 配置）
- Swagger UI：`http://localhost:5000/swagger`
- 数据库：默认使用 SQLite，数据库文件位于解决方案根目录的 `data/` 文件夹
- 首次启动自动执行数据库迁移和种子数据初始化

### 7.4 运行 ReportDataMaker（WPF 桌面应用）

```bash
cd Generators/ReportDataMaker
dotnet run
```

- 启动后自动加载 DI 容器和内置模板测试
- 通过「文件 → 加载模板」加载 JSON 模板文件
- 模板文件位于运行目录的 `Templates/` 文件夹

### 7.5 运行测试

```bash
cd Editor/Server.Tests
dotnet test
```

测试覆盖 AuthController、TemplatesController、PreviewController、HealthController 等控制器。

### 7.6 数据库迁移

```bash
cd Editor/Server
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### 7.7 API 认证

系统使用 JWT Bearer 认证，预置账户：

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | admin123 | admin |
| editor | editor123 | editor |

**获取 Token**：

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

---

## 8. 扩展指南

### 8.1 新增元素类型

1. 在 `Contracts/Models/Elements/` 继承 `ExternalElementBase` 创建新元素类
2. 在 `ElementJsonConverter.WebShortTypeMap` 注册类型映射（`shortName → Type`）
3. 在 `ElementGroupRegistry` 静态构造函数中注册适配分组
4. 在 `ReportDataMaker/Models/ExternalExtendedElements.cs` 创建对应的外部元素类
5. 在 `ReportExternalElementConverter` 中添加类型转换逻辑
6. 在 `PdfElementRenderer` 中添加渲染逻辑

### 8.2 新增适配器类型

1. 在 `Contracts/Enums/AdapterType` 添加枚举值
2. 在 `Contracts/Models/Adapters/` 创建适配器配置模型
3. 在 `ReportDataMaker/Services/` 创建适配器服务、工厂和配置类
4. 在 `ReportDataMaker/ViewModels/Tabs/` 创建标签页 ViewModel
5. 在 `ReportDataMaker/Views/Tabs/` 创建标签页 View
6. 在 `MainViewModel` 中添加适配器命令和创建逻辑
7. 在 `App.xaml.cs` 中注册 DI 服务

### 8.3 新增数据库提供者

1. 实现 `IDatabaseProvider` 接口
2. 在 `DatabaseProviderRegistry.CreateDefault()` 中注册新提供者
3. 在 `Contracts/Enums/AdapterType.cs` 的 `DatabaseProvider` 枚举中添加值
4. 添加对应 NuGet 包依赖

### 8.4 新增生产器

1. 在 `Generators/` 目录下创建新项目
2. 引用 `Contracts` 层项目
3. 实现 `IDataAdapter` 接口
4. 根据目标平台选择 UI 框架
