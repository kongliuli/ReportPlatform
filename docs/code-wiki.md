# Xinglin.ReportPlatform 代码百科

基于契约驱动的医疗报告单生成平台，支持在线模板编辑、多格式数据生产和可扩展的适配器架构。

> 文档版本：2.0.0
> 更新日期：2026-05-08
> 覆盖版本：v0.3.0 ~ v0.6.0

---

## 目录

1. [项目概览](#1-项目概览)
2. [架构设计](#2-架构设计)
3. [Contracts 契约层](#3-contracts-契约层)
4. [ReportDataMaker 数据录入工具](#4-reportdatamaker-数据录入工具)
5. [Editor 编辑器模块](#5-editor-编辑器模块)
6. [适配器体系](#6-适配器体系)
7. [依赖关系](#7-依赖关系)
8. [版本演进](#8-版本演进)
9. [运行指南](#9-运行指南)

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
├── Contracts/                              # 契约定义层（v0.3.0 重构）
│   ├── Enums/                              # 枚举定义
│   ├── Models/Elements/                    # 23种元素类型
│   ├── Models/Template/                    # 模板定义
│   ├── Models/Adapters/                    # 适配器接口与模型
│   ├── Converters/                         # JSON 转换器
│   ├── Registry/                           # 元素注册表
│   ├── DTOs/                               # 数据传输对象
│   ├── Requests/                           # 请求模型
│   ├── Responses/                          # 响应模型
│   └── TemplateSerializer.cs               # 模板序列化器
│
├── Editor/                                 # 在线编辑器模块
│   ├── Core/                               # 核心业务逻辑
│   │   ├── Data/                           # EF Core 实体和 DbContext
│   │   ├── Services/                       # 业务服务实现
│   │   ├── SharedInterfaces/               # 跨模块共享接口
│   │   └── Extensions/                     # 依赖注入扩展
│   └── Server/                             # Web API 服务
│       ├── Controllers/                    # API 控制器
│       └── Middleware/                     # 中间件
│
└── Generators/                             # 生产器层
    └── ReportDataMaker/                    # WPF 数据录入工具（v0.4.0 MVVM重构）
        ├── Models/                         # 桥接模型层
        ├── Infrastructure/                 # MVVM 基础设施
        ├── Services/                       # 业务服务
        │   ├── ExcelAdapter/               # v0.5.0 Excel 适配器
        │   └── DatabaseAdapter/            # v0.6.0 数据库适配器
        ├── ViewModels/                     # MVVM 视图模型
        │   ├── Tabs/                       # Tab 页签 ViewModel
        │   └── Dialogs/                    # 对话框 ViewModel
        ├── Views/                          # XAML 视图
        │   └── Tabs/                       # Tab 页签 View
        └── Converters/                     # 值转换器
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

1. **契约优先（Contract-First）**：Contracts 层作为单一数据源（Single Source of Truth），Editor 和 Generators 通过契约解耦
2. **Bridge 模式**：ReportDataMaker 通过 `ReportExternalElementBase` 桥接类继承 Contracts `ExternalElementBase` 并添加 WPF 渲染属性
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

### 3.1 概述

**职责**：定义 Editor 和 Generators 之间共享的数据结构、接口契约和枚举类型。作为整个平台的单一数据源。

**项目路径**：`/Contracts/Xinglin.WebReportEditor.Contracts.csproj`
**命名空间**：`Xinglin.ReportEditor.Contracts`
**目标框架**：`net8.0`
**NuGet 依赖**：`Newtonsoft.Json 13.0.3`

### 3.2 枚举定义

| 枚举 | 命名空间 | 值 | 说明 |
|------|---------|-----|------|
| `ElementGroup` | `Contracts.Enums` | Fixed, Context, Editable, DataAdapter | 元素分组 |
| `ElementAdaptationGroup` | `Contracts.Enums` | Basic, Form, Data, Advanced | 元素适配分组 |
| `AdapterType` | `Contracts.Enums` | Context, Excel, Database, Api | 适配器类型 |
| `DatabaseProvider` | `Contracts.Enums` | SqlServer, MySql, Sqlite, PostgreSql | 数据库提供程序 |
| `FieldDataType` | `Contracts.Models.Adapters` | Text, Number, Date, Dropdown, Boolean | 字段数据类型 |

### 3.3 元素类型体系

```
ElementBase (抽象基类)
├── 属性: Id, X, Y, Width, Height, Rotation, ZIndex, Tooltip, IsLocked
├── ID 生成: Guid.NewGuid().ToString("N") (32字符无连字符)
│
└── ExternalElementBase (抽象基类, 支持数据绑定)
    ├── 属性: Label, DataPath, IsDataBound, IsRequired, Group, AdapterId
    │
    ├── Fixed 组 (不可编辑)
    │   ├── LineElement        线条
    │   ├── TextElement        文本
    │   ├── ShapeElement       形状
    │   ├── ImageElement       图片
    │   ├── DividerElement     分隔线
    │   ├── HeaderElement      页眉
    │   ├── FooterElement      页脚
    │   ├── PageNumberElement  页码
    │   ├── WatermarkElement   水印
    │   └── IconElement        图标
    │
    ├── Context 组 (上下文自动填充)
    │   └── HyperlinkElement   超链接
    │
    ├── Editable 组 (可编辑录入)
    │   ├── NumberElement      数字
    │   ├── DateElement        日期
    │   ├── DropdownElement    下拉框
    │   ├── CheckboxElement    复选框
    │   ├── RadioElement       单选
    │   ├── SignatureElement   签名
    │   └── BarcodeElement     条形码
    │
    └── DataAdapter 组 (适配器填充)
        ├── TableElement       表格
        ├── ContainerElement   容器
        ├── RepeatElement      重复区域
        ├── QrCodeElement      二维码
        └── ChartElement       图表
```

### 3.4 模板定义

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `TemplateDefinition` | `Contracts.Models.Template` | 模板定义（Id, Name, Version, Elements, PageSettings, DataBindings） |
| `PageSettings` | `Contracts.Models.Template` | 页面设置（PageSize, Orientation, Margins） |
| `DataBindingDefinition` | `Contracts.Models.Template` | 数据绑定定义（DataPath, Source, AdapterId, Priority） |

### 3.5 适配器模型

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `IDataAdapter` | `Contracts.Models.Adapters` | 适配器接口（ReadDataAsync, ReadBatchDataAsync, ValidateConfigAsync） |
| `AdapterConfigBase` | `Contracts.Models.Adapters` | 配置基类（AdapterId, Type, DisplayName, IsEnabled） |
| `AdapterResult` | `Contracts.Models.Adapters` | 执行结果（Success, ErrorMessage, Data, BatchData） |
| `ValidationResult` | `Contracts.Models.Adapters` | 验证结果（IsValid, Errors, Warnings） |
| `FieldSchema` | `Contracts.Models.Adapters` | 字段模式（DataPath, Label, DataType, Format, Options, IsRequired, MinValue, MaxValue, DecimalPlaces） |

### 3.6 关键组件

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `ElementJsonConverter` | `Contracts.Converters` | 双格式 JSON 转换器（Web 短格式 + WPF 全格式） |
| `ElementGroupRegistry` | `Contracts.Registry` | 元素类型 → 分组映射注册表 |
| `TemplateSerializer` | `Contracts` | 统一模板序列化/反序列化工具 |

### 3.7 Web API 契约

| 目录 | 内容 |
|------|------|
| `DTOs/` | TemplateDto, TemplateDetailDto, TemplateVersionDto, AuthDtos |
| `Requests/` | CreateTemplateRequest, UpdateTemplateRequest, TemplateFilterRequest, RollbackRequest |
| `Responses/` | ApiResponse\<T\>, PagedResponse\<T\> |

---

## 4. ReportDataMaker 数据录入工具

### 4.1 概述

**职责**：基于契约的 WPF 桌面数据录入工具，支持模板加载、数据绑定、适配器数据填充和报告预览。

**项目路径**：`/Generators/ReportDataMaker/ReportDataMaker.csproj`
**目标框架**：`net10.0-windows`
**UI 框架**：HandyControl

### 4.2 NuGet 依赖

| 包 | 版本 | 用途 |
|----|------|------|
| HandyControl | 3.5.1 | WPF UI 控件库 |
| ClosedXML | 0.104.2 | Excel 文件操作 |
| Microsoft.Data.SqlClient | 6.0.1 | SQL Server 数据库 |
| Microsoft.Data.Sqlite | 9.0.4 | SQLite 数据库 |
| MySqlConnector | 2.5.0 | MySQL 数据库 |
| Npgsql | 9.0.3 | PostgreSQL 数据库 |
| Newtonsoft.Json | 13.0.4 | JSON 序列化 |
| Microsoft.Extensions.DependencyInjection | 9.0.4 | 依赖注入 |
| ZXing.Net.Bindings.Windows.Compatibility | 0.16.12 | 条形码/二维码生成 |

### 4.3 桥接模型层

`ReportExternalElementBase` 继承 Contracts `ExternalElementBase`，添加 WPF 渲染属性：

```csharp
public abstract class ReportExternalElementBase : ExternalElementBase
{
    // 可见性
    public bool IsVisible { get; set; } = true;
    public double Opacity { get; set; } = 1;

    // 边框
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public double BorderWidth { get; set; }
    public string BorderStyle { get; set; } = string.Empty;
    public double CornerRadius { get; set; }

    // 文字
    public string FontFamily { get; set; } = string.Empty;
    public double FontSize { get; set; }
    public string FontWeight { get; set; } = string.Empty;
    public string FontStyle { get; set; } = string.Empty;
    public string ForegroundColor { get; set; } = "#000000";
    public string TextAlignment { get; set; } = string.Empty;
    public double LabelWidth { get; set; }

    // 数据
    public string DefaultValue { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string FormatString { get; set; } = string.Empty;
    public string? ElementType { get; set; }
}
```

20 个 WPF 元素类型均继承 `ReportExternalElementBase`：
ExternalTextElement, ExternalLineElement, ExternalDropdownElement, ExternalNumberElement, ExternalDateElement, ExternalTableElement, ExternalImageElement, ExternalShapeElement, ExternalCheckboxElement, ExternalRadioElement, ExternalSignatureElement, ExternalBarcodeElement, ExternalQrCodeElement, ExternalHyperlinkElement, ExternalIconElement, ExternalDividerElement, ExternalRepeatElement, ExternalContainerElement, ExternalChartElement, ExternalWatermarkElement

### 4.4 MVVM 基础设施

| 类 | 命名空间 | 说明 |
|----|---------|------|
| `ViewModelBase` | `ReportDataMaker.Infrastructure` | INotifyPropertyChanged + SetProperty |
| `RelayCommand` | `ReportDataMaker.Infrastructure` | 同步 ICommand 实现 |
| `AsyncRelayCommand` | `ReportDataMaker.Infrastructure` | 异步 ICommand + IsExecuting 守卫 |
| `IDialogService` | `ReportDataMaker.Infrastructure` | 对话框接口（OpenFile, SaveFile, Confirm, ShowInfo, ShowError, ShowSuccess） |
| `DialogService` | `ReportDataMaker.Infrastructure` | WPF MessageBox 实现 |
| `ServiceLocator` | `ReportDataMaker.Infrastructure` | DI 容器包装 |

### 4.5 服务层

#### 4.5.1 核心服务

| 接口 | 实现 | 说明 |
|------|------|------|
| `ITemplateLoaderService` | `TemplateLoaderService` | 模板加载和分类 |
| `ITemplatePreviewService` | `TemplatePreviewService` | 模板预览渲染 |
| `IDataBindingService` | `DataBindingService` | 数据路径绑定 |
| - | `AdapterConfigStore` | 适配器配置持久化（%APPDATA%/ReportDataMaker/adapters.json） |

#### 4.5.2 Excel 适配器服务（v0.5.0）

| 类 | 说明 |
|----|------|
| `TemplateFlattenService` | 将 Editable 元素展平为 FlatField 列表 |
| `ExcelSchemaExporter` | 导出 xlsx（隐藏 DataPath 契约行 + 标签行 + 数据行） |
| `ExcelContractReader` | 基于契约行的精确导入（单条/批量） |
| `ExcelDataValidator` | 数据验证（Number/Date/Dropdown/Boolean） |
| `ExcelAdapterConfig` | Excel 适配器配置（FilePath, SheetName, Mode, ExportedSchema） |
| `ExcelAdapterFactory` | 工厂（整合 Flattener + Exporter + Reader + Validator） |

`FlatField` 继承 `FieldSchema`，添加 `ElementId` 属性用于回溯元素。

Excel 契约驱动导入原理：
- Row1: DataPath（隐藏行，精确匹配列）
- Row2: Label（用户可见的列标题）
- Row3: DataType（隐藏行，类型校验依据）
- Row4+: 数据行

#### 4.5.3 数据库适配器服务（v0.6.0）

| 类 | 说明 |
|----|------|
| `IDatabaseProvider` | Provider 接口（连接/元数据/SQL 方言） |
| `DatabaseProviderRegistry` | Provider 注册表 + CreateDefault() |
| `SqlServerProvider` | SQL Server（[id]引用, @参数, OFFSET...FETCH分页） |
| `MySqlProvider` | MySQL（\`id\`引用, @参数, LIMIT...OFFSET分页） |
| `SqliteProvider` | SQLite（"id"引用, @参数, PRAGMA元数据） |
| `PostgreSqlProvider` | PostgreSQL（"id"引用, :参数, LIMIT...OFFSET分页） |
| `DatabaseAdapterConfig` | 配置（Provider, ConnectionString, Query, DbFieldMappings, Parameters, Joins） |
| `SqlBuilder` | 可视化模式 → SQL 生成器 |
| `DatabaseAdapterBase` | 查询执行 + 字段映射 + 参数化 + 结果转换 |
| `DatabaseAdapterFactory` | 工厂（整合 Provider + AdapterBase + Flattener） |

元数据模型：`TableInfo`（Schema, Name, Type）、`ColumnInfo`（Name, DataType, IsNullable, IsPrimaryKey, MaxLength）、`ForeignKeyInfo`（ColumnName, ReferencedTable, ReferencedColumn）

配置子模型：
- `QueryConfig`（Mode: RawSql/VisualBuilder, RawSql, PrimaryTable, SelectedColumns, WhereClause, OrderBy）
- `DbFieldMapping`（ColumnName, TargetDataPath, Transform）
- `QueryParameter`（Name, Label, DataType, DefaultValue, Source: Manual/FromTemplate/FromContext）
- `JoinDefinition`（LeftTable, LeftColumn, RightTable, RightColumn, Type: Inner/Left/Right）

### 4.6 ViewModel 层

| 类 | 继承 | 说明 |
|----|------|------|
| `MainViewModel` | ViewModelBase | 主 ViewModel，管理模板加载、适配器 Tab、侧边栏 |
| `TabViewModelBase` | ViewModelBase | Tab 页签基类（Title, IsClosable, CloseRequested） |
| `DataEntryTabViewModel` | TabViewModelBase | 数据录入 Tab |
| `PreviewTabViewModel` | TabViewModelBase | 预览 Tab |
| `ExcelAdapterTabViewModel` | TabViewModelBase | Excel 适配器 Tab（导出/导入/验证） |
| `DatabaseAdapterTabViewModel` | TabViewModelBase | 数据库适配器 Tab（连接/查询/参数/映射/预览） |
| `TemplateLoadViewModel` | ViewModelBase | 模板加载对话框 |
| `SidePanelViewModel` | ViewModelBase | 侧边栏 |
| `AddAdapterDialogViewModel` | ViewModelBase | 添加适配器对话框 |

MainViewModel 依赖注入：
- `ITemplateLoaderService`
- `IDataBindingService`
- `ITemplatePreviewService`
- `IDialogService`
- `AdapterConfigStore`
- `ExcelAdapterFactory`
- `DatabaseAdapterFactory`

### 4.7 View 层

| XAML | 说明 |
|------|------|
| `MainWindow.xaml` | 三栏布局（可折叠侧边栏 + TabControl + 状态栏） |
| `DataEntryTab.xaml` | 数据录入界面 |
| `PreviewTab.xaml` | 预览界面 |
| `ExcelAdapterTab.xaml` | Excel 适配器三区布局（导出/导入/验证） |
| `DatabaseAdapterTab.xaml` | 数据库适配器四区布局（连接/查询/参数/映射） |

### 4.8 DI 注册

```csharp
// App.xaml.cs
services.AddSingleton<IDialogService, DialogService>();
services.AddSingleton<ITemplateLoaderService, TemplateLoaderService>();
services.AddSingleton<ITemplatePreviewService, TemplatePreviewService>();
services.AddSingleton<IDataBindingService, DataBindingService>();
services.AddSingleton<AdapterConfigStore>();
services.AddSingleton<ExcelAdapterFactory>();
services.AddSingleton<DatabaseProviderRegistry>(sp => DatabaseProviderRegistry.CreateDefault());
services.AddSingleton<DatabaseAdapterFactory>();
```

### 4.9 值转换器

| 类 | 说明 |
|----|------|
| `BoolToVisibilityConverter` | Bool → Visibility（含 Inverse 和 BoolToWidth 变体） |

---

## 5. Editor 编辑器模块

### 5.1 Editor.Core

**职责**：封装核心业务逻辑，提供模板管理、版本控制、认证授权等服务。

**命名空间**：`Xinglin.WebReportEditor.Core`

#### 核心服务

| 服务类 | 接口 | 职责 |
|--------|------|------|
| TemplateService | ITemplateService | 模板 CRUD 操作 |
| VersionService | IVersionService | 模板版本管理、回滚、差异对比 |
| AuthService | IAuthService | 用户认证、Token 管理 |
| PdfRenderService | IPdfRenderService | PDF 渲染服务 |

#### 数据实体

- **TemplateEntity** - 模板主表（Id, Name, Type, Version, ContentJson, HospitalId, IsDefault, IsPublished）
- **TemplateVersionEntity** - 模板版本历史（Id, TemplateId, VersionNumber, ContentJson, ChangeDescription）
- **UserEntity** - 用户信息（Id, Username, PasswordHash, DisplayName, Role, HospitalId, IsActive）
- **RefreshTokenEntity** - 刷新令牌（Id, Token, UserId, ExpiryTime, IsRevoked）

### 5.2 Editor.Server

**职责**：提供 RESTful API 接口，处理 HTTP 请求和响应。

**命名空间**：`Xinglin.WebReportEditor.Server`

#### 核心控制器

| 控制器 | 路由前缀 | 职责 |
|--------|---------|------|
| TemplatesController | `/api/templates` | 模板管理 API |
| VersionsController | `/api/versions` | 版本管理 API |
| AuthController | `/api/auth` | 认证授权 API |
| PreviewController | `/api/preview` | 模板预览 API |
| HealthController | `/api/health` | 健康检查 API |

#### 中间件

- **GlobalExceptionMiddleware** - 全局异常处理
- **ApiLoggingMiddleware** - API 请求日志记录

---

## 6. 适配器体系

### 6.1 适配器架构

```
┌─────────────────────────────────────────────────────────┐
│                    适配器体系                             │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Context    │  │    Excel     │  │  Database    │  │
│  │   适配器     │  │    适配器    │  │    适配器    │  │
│  │ (上下文填充) │  │ (导入导出)   │  │ (查询映射)   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│                                                          │
│  ┌──────────────┐                                       │
│  │     API      │        执行顺序:                      │
│  │   适配器     │        Context → DataAdapter → Editable│
│  │ (接口对接)   │                                       │
│  └──────────────┘                                       │
│                                                          │
├─────────────────────────────────────────────────────────┤
│              Contracts (IDataAdapter, AdapterConfigBase) │
└─────────────────────────────────────────────────────────┘
```

### 6.2 Excel 适配器工作流

```
模板加载 → TemplateFlattenService 展平 → FlatField 列表
                                              │
                    ┌─────────────────────────┤
                    ▼                         ▼
            ExcelSchemaExporter        ExcelContractReader
            (导出带契约行的xlsx)        (基于契约行精确读取)
                    │                         │
                    ▼                         ▼
            用户填写数据              ExcelDataValidator
                                              │
                                              ▼
                                      AdapterResult → 数据绑定
```

### 6.3 数据库适配器工作流

```
选择 Provider → 输入连接字符串 → 测试连接
                                        │
                                        ▼
                              获取表/列元数据
                                        │
                    ┌───────────────────┤
                    ▼                   ▼
            SQL 模式              可视化构建模式
            (直接输入SQL)         (选表+选列+JOIN+WHERE)
                    │                   │
                    └───────┬───────────┘
                            ▼
                      SqlBuilder 生成 SQL
                            │
                            ▼
                      参数化查询执行
                            │
                            ▼
                      DbFieldMapping 列→DataPath 映射
                            │
                            ▼
                      AdapterResult → 数据绑定
```

### 6.4 自动匹配策略

DatabaseAdapterTabViewModel.AutoMatch 使用三种策略匹配列名到 DataPath：
1. **精确匹配 DataPath**：列名 == DataPath
2. **精确匹配 Label**：列名 == Label
3. **DataPath 后缀匹配**：DataPath 以列名结尾（如 `Patient.Name` 匹配 `Name`）

---

## 7. 依赖关系

### 7.1 项目依赖图

```
┌─────────────────────────────────────────────────────┐
│              Editor.Server (ASP.NET Core 8)          │
│  依赖: Editor.Core, Contracts                       │
│  NuGet: JwtBearer, Swagger, EF Core Design          │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│                 Editor.Core (.NET 8)                 │
│  依赖: Contracts                                    │
│  NuGet: EF Core, EF Sqlite, EF SqlServer,           │
│         JwtBearer, Newtonsoft.Json, BCrypt.Net,     │
│         System.IdentityModel.Tokens.Jwt             │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│              Contracts (.NET 8)                      │
│  NuGet: Newtonsoft.Json 13.0.3                      │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│           ReportDataMaker (WPF .NET 10)              │
│  依赖: Contracts                                     │
│  NuGet: HandyControl 3.5.1, ClosedXML 0.104.2,      │
│         Microsoft.Data.SqlClient 6.0.1,              │
│         Microsoft.Data.Sqlite 9.0.4,                 │
│         MySqlConnector 2.5.0, Npgsql 9.0.3,         │
│         Newtonsoft.Json 13.0.4,                      │
│         Microsoft.Extensions.DependencyInjection,    │
│         ZXing.Net.Bindings.Windows.Compatibility     │
└─────────────────────────────────────────────────────┘
```

### 7.2 数据库支持

| Provider | NuGet 包 | 连接类 | 标识符引用 | 参数前缀 | 分页语法 |
|----------|---------|--------|-----------|---------|---------|
| SqlServer | Microsoft.Data.SqlClient | SqlConnection | `[name]` | `@` | OFFSET...FETCH |
| MySql | MySqlConnector | MySqlConnection | `` `name` `` | `@` | LIMIT...OFFSET |
| Sqlite | Microsoft.Data.Sqlite | SqliteConnection | `"name"` | `@` | LIMIT...OFFSET |
| PostgreSql | Npgsql | NpgsqlConnection | `"name"` | `:` | LIMIT...OFFSET |

---

## 8. 版本演进

### 8.1 版本路线图

| 版本 | 主题 | 状态 | 核心变更 |
|------|------|------|---------|
| v0.3.0 | 架构统一 | ✅ 完成 | Contracts 契约层 + 23种元素类型 + ElementJsonConverter + ElementGroupRegistry |
| v0.4.0 | WPF MVVM 重构 | ✅ 完成 | ViewModelBase + DI + HandyControl + MainViewModel + Tab 体系 |
| v0.5.0 | Excel 适配器 | ✅ 完成 | 契约驱动导入导出 + TemplateFlattenService + ExcelDataValidator |
| v0.6.0 | 数据库适配器 | ✅ 完成 | IDatabaseProvider + 4个Provider + DatabaseAdapterBase + SqlBuilder |
| v0.7.0 | API 适配器 | 📋 计划中 | RESTful API 数据源对接 |
| v0.8.0 | 上下文适配器 | 📋 计划中 | 自动填充医院/医生/日期字段 |
| v0.9.0 | 模板编辑器增强 | 📋 计划中 | 可视化模板编辑 |
| v1.0.0 | 生产就绪 | 📋 计划中 | 性能优化 + 安全加固 + 部署方案 |

### 8.2 设计文档索引

| 文档 | 路径 |
|------|------|
| 总路线图 | `docs/reportplatform/v0.2.0-roadmap.md` |
| v0.3.0 架构统一计划 | `docs/reportplatform/v0.3.0-architecture-unification-plan.md` |
| v0.4.0 MVVM 重构计划 | `docs/reportplatform/v0.4.0-wpf-mvvm-refactor-plan.md` |
| v0.5.0 Excel 适配器计划 | `docs/reportplatform/v0.5.0-excel-adapter-plan.md` |
| v0.6.0 数据库适配器计划 | `docs/reportplatform/v0.6.0-database-adapter-plan.md` |
| 适配器组件设计 | `docs/adapters/adapter-components-design.md` |
| Contracts 变更日志 | `docs/contracts/changelog.md` |

### 8.3 Spec 文档索引

| Spec | 路径 |
|------|------|
| v0.3.0 验证 + v0.4.0 实施 | `.trae/specs/verify-v030-and-implement-v040/` |
| v0.5.0 Excel 适配器实施 | `.trae/specs/implement-v050-excel-adapter/` |
| v0.6.0 数据库适配器实施 | `.trae/specs/implement-v060-database-adapter/` |

---

## 9. 运行指南

### 9.1 环境要求

| 组件 | 要求 |
|------|------|
| .NET SDK | .NET 8.0+ (Editor) / .NET 10.0+ (ReportDataMaker) |
| 数据库 | SQLite 3+ 或 SQL Server 2016+ |
| Visual Studio | 2022+（推荐） |

### 9.2 启动 Editor.Server

```bash
cd Editor/Server
dotnet restore
dotnet ef database update
dotnet run
# 服务将在 http://localhost:5000 启动
# Swagger UI: http://localhost:5000/swagger
```

### 9.3 启动 ReportDataMaker

```bash
cd Generators/ReportDataMaker
dotnet restore
dotnet run
```

### 9.4 默认用户

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | admin123 | admin |
| editor | editor123 | editor |

### 9.5 Docker 部署

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Editor/Server/Xinglin.WebReportEditor.Server.csproj", "Editor/Server/"]
COPY ["Editor/Core/Xinglin.WebReportEditor.Core.csproj", "Editor/Core/"]
COPY ["Contracts/Xinglin.WebReportEditor.Contracts.csproj", "Contracts/"]
RUN dotnet restore "Editor/Server/Xinglin.WebReportEditor.Server.csproj"
COPY . .
WORKDIR "/src/Editor/Server"
RUN dotnet publish "Xinglin.WebReportEditor.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Xinglin.WebReportEditor.Server.dll"]
```

---

## 附录

### A. 错误码对照表

| HTTP 状态码 | 错误类型 | 说明 |
|------------|----------|------|
| 200 | Success | 操作成功 |
| 400 | BadRequest | 请求参数错误 |
| 401 | Unauthorized | 未授权 |
| 404 | NotFound | 资源不存在 |
| 409 | Conflict | 资源冲突 |
| 500 | InternalServerError | 服务器内部错误 |

### B. ID 生成策略

| 场景 | 方式 | 长度 | 示例 |
|------|------|------|------|
| 元素 ID | `Guid.NewGuid().ToString("N")` | 32字符 | `a1b2c3d4e5f6...` |
| 适配器 ID | `Guid.NewGuid().ToString("N")` | 32字符 | `f6e5d4c3b2a1...` |

### C. TODO 清单

- [ ] 短唯一ID：考虑使用更短的 ID 方案替代 Guid（参见 v0.3.0 TODO）
- [ ] 连接字符串加密：敏感信息不应明文存储
- [ ] 可视化查询构建器：当前仅支持 SQL 模式，可视化模式后续迭代
- [ ] 连接池管理：多适配器共享连接
- [ ] 编译验证：所有版本需 `dotnet build` 验证

---

*本文档版本 2.0.0，覆盖 v0.3.0 ~ v0.6.0 全部变更*
