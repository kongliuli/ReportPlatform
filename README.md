# Xinglin.ReportPlatform 报告平台

基于契约驱动的医疗报告单生成平台，支持在线模板编辑、多源数据适配和可扩展的适配器架构。

## 架构总览

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

## 解决方案结构

```
ReportPlatform.sln
├── Contracts/                          # 契约定义层（共享库）
│   ├── Models/
│   │   ├── Elements/                   # 22种元素类型定义
│   │   ├── Adapters/                   # 适配器接口与配置模型
│   │   └── Template/                   # 模板定义与数据绑定
│   ├── DTOs/                           # 数据传输对象
│   ├── Requests/                       # API请求模型
│   ├── Responses/                      # 统一响应模型
│   ├── Enums/                          # 枚举定义
│   ├── Registry/                       # 元素分组注册表
│   ├── Converters/                     # JSON多态序列化转换器
│   ├── IdGenerator.cs                  # Nanoid唯一ID生成
│   └── TemplateSerializer.cs           # 模板序列化器
│
├── Editor/                             # Web编辑器
│   ├── Core/                           # 编辑器核心逻辑
│   │   ├── Data/                       # EF Core实体与数据库上下文
│   │   ├── Services/                   # 业务服务实现
│   │   ├── SharedInterfaces/           # 共享接口定义
│   │   └── Extensions/                 # DI注册扩展
│   └── Server/                         # ASP.NET Core API服务端
│       ├── Controllers/                # REST API控制器
│       └── Middleware/                  # 全局异常与日志中间件
│
├── Generators/                         # 数据生产器层
│   └── ReportDataMaker/                # WPF桌面应用
│       ├── Models/                     # 外部模板与扩展元素模型
│       ├── Services/
│       │   ├── ExcelAdapter/           # Excel适配器实现
│       │   ├── DatabaseAdapter/        # 数据库适配器实现
│       │   ├── DataBindingService.cs   # 数据绑定服务
│       │   ├── TemplateLoaderService.cs # 模板加载服务
│       │   ├── AdapterConfigStore.cs   # 适配器配置持久化
│       │   └── ConfigProtector.cs      # 连接字符串加密
│       ├── ViewModels/                 # MVVM视图模型
│       │   ├── Tabs/                   # 标签页视图模型
│       │   └── Dialogs/               # 对话框视图模型
│       ├── Views/                      # XAML视图
│       ├── Infrastructure/             # MVVM基础设施
│       ├── Templates/                  # 内置医疗模板
│       └── Configs/                    # 适配器配置文件
│
└── docs/                               # 文档中心
    ├── contracts/                      # 契约文档与版本化模板
    ├── decisions/                      # 架构决策记录
    ├── editor/                         # 编辑器文档
    ├── generators/                     # 生产器文档
    └── roadmap/                        # 版本路线图
```

## 核心模块详解

### 1. Contracts — 契约层

契约层是整个平台的基石，定义了Editor和Generators之间的共享数据结构，确保两端对模板的理解一致。

#### 元素模型体系

```
ElementBase (抽象基类)
├── Id, X, Y, Width, Height    # 布局属性
├── Rotation, ZIndex            # 变换属性
└── Tooltip, IsLocked           # 交互属性
    │
    └── ExternalElementBase (外部元素基类，支持数据绑定)
        ├── Label, DataPath     # 数据绑定属性
        ├── IsRequired, Group   # 校验与分组
        └── AdapterId           # 关联适配器
            │
            ├── TextElement     ├── NumberElement
            ├── LineElement     ├── DateElement
            ├── TableElement    ├── DropdownElement
            ├── ShapeElement    ├── CheckboxElement
            ├── DividerElement  ├── RadioElement
            ├── BarcodeElement  ├── ImageElement
            ├── QrCodeElement   ├── IconElement
            ├── SignatureElement ├── HyperlinkElement
            ├── ContainerElement ├── ChartElement
            ├── RepeatElement   ├── HeaderElement
            ├── FooterElement   ├── PageNumberElement
            └── WatermarkElement
```

**元素分组机制**：通过 `ElementGroup` 枚举将元素分为四类：
- **Fixed** — 固定元素，无数据绑定
- **Context** — 上下文元素，DataPath以 `Context.` 开头
- **Editable** — 可编辑元素，有数据绑定但无适配器
- **DataAdapter** — 适配器元素，关联数据适配器或为表格/重复/图表类型

**适配分组**：`ElementAdaptationGroup` 将22种元素按功能分为四组：
- **Basic** — Line, Divider, Shape
- **Form** — Text, Number, Date, Dropdown, Checkbox, Radio
- **Data** — Table, Repeat, Chart
- **Advanced** — Image, Barcode, QrCode, Signature, Hyperlink, Icon, Container, Header, Footer, PageNumber, Watermark

#### 适配器契约

```csharp
public interface IDataAdapter
{
    string AdapterId { get; }
    string AdapterName { get; }
    AdapterType Type { get; }           // Context | Excel | Database | Api
    IReadOnlyList<string> TargetDataPaths { get; }
    Task<AdapterResult> ReadDataAsync();
    Task<AdapterResult> ReadBatchDataAsync();
    Task<ValidationResult> ValidateConfigAsync();
}
```

适配器配置继承自 `AdapterConfigBase`，包含 AdapterId、Type、DisplayName、IsEnabled 等通用字段。

`FieldSchema` 定义了字段的数据模式（DataPath、Label、DataType、Format、Options、IsRequired、MinValue、MaxValue、DecimalPlaces），用于模板扁平化和数据校验。

#### 模板定义

`TemplateDefinition` 是模板的完整结构：
- **基本信息** — Id, Name, Version, Type, HospitalId
- **页面设置** — PageSettings（宽高、边距、方向、背景色）
- **数据绑定** — List\<DataBindingDefinition\>（ElementId、DataPath、BindingType、FormatString、DefaultValue、Transform）
- **元素列表** — List\<ExternalElementBase\>（多态序列化）
- **全局字体** — EnableGlobalFontSize, GlobalFontSize

#### 序列化机制

- `ElementJsonConverter`：基于 Newtonsoft.Json 的自定义转换器，通过 `$type` 鉴别符（格式 `template.element.{shortType}`）实现 `ExternalElementBase` 的多态序列化
- `TemplateSerializer`：封装序列化配置（CamelCase、忽略Null、枚举转字符串），提供模板和元素列表的序列化/反序列化
- `IdGenerator`：基于 Nanoid 生成21位唯一标识

### 2. Editor — Web编辑器

#### 数据层

| 实体 | 说明 | 关键关系 |
|------|------|----------|
| TemplateEntity | 模板主表 | 1:N → TemplateVersionEntity |
| TemplateVersionEntity | 版本快照 | N:1 → TemplateEntity |
| UserEntity | 用户账户 | 1:N → RefreshTokenEntity |
| RefreshTokenEntity | 刷新令牌 | N:1 → UserEntity |

数据库支持 SQL Server 和 SQLite，通过 `ServiceCollectionExtensions` 按配置切换。种子数据预置 admin/editor 两个账户。

#### 服务层

| 服务 | 职责 |
|------|------|
| TemplateService | 模板CRUD，更新时自动创建版本快照 |
| VersionService | 版本列表查询、版本详情、版本回滚、版本差异对比 |
| AuthService | JWT认证（登录/刷新令牌/撤销令牌），BCrypt密码哈希 |
| PdfRenderService | 模板PDF/图片渲染（当前为Stub实现） |

#### API端点

| 控制器 | 路由前缀 | 功能 |
|--------|----------|------|
| AuthController | `/api/auth` | 登录、刷新令牌 |
| TemplatesController | `/api/templates` | 模板列表、详情、创建、更新、删除 |
| VersionsController | `/api/templates/{id}/versions` | 版本列表、版本详情、版本回滚 |
| PreviewController | `/api/preview` | 模板预览（PDF/图片） |
| HealthController | `/api/health` | 健康检查 |

#### 中间件管道

```
Request → GlobalExceptionMiddleware → ApiLoggingMiddleware → CORS → Authentication → Authorization → Controller
```

### 3. Generators — 数据生产器

#### ReportDataMaker（WPF桌面应用）

基于 MVVM 架构的桌面数据录入工具，核心功能包括模板加载、数据录入、数据适配和预览。

**MVVM架构**：

```
ViewModelBase (INotifyPropertyChanged)
├── MainViewModel              # 主窗口：模板加载、适配器管理、标签页切换
├── SidePanelViewModel         # 侧边面板：展开/折叠
├── AdapterItemViewModel       # 适配器条目：侧边栏适配器列表项
└── TabViewModelBase (抽象)    # 标签页基类
    ├── DataEntryTabViewModel  # 数据录入：字段编辑与适配器数据应用
    ├── PreviewTabViewModel    # 模板预览：可视化预览与缩放
    ├── ExcelAdapterTabViewModel   # Excel适配器：导出/导入/校验
    └── DatabaseAdapterTabViewModel # 数据库适配器：连接/查询/映射
```

**标签页工作流**：

1. 加载模板 → 自动创建「数据录入」和「预览」标签页
2. 添加适配器 → 在预览标签前插入适配器标签页
3. 适配器数据 → 通过 `ApplyDataFromAdapter` 回填到数据录入标签页

#### Excel适配器

Excel适配器实现了完整的「契约行」驱动的Excel数据导入导出：

```
模板 → TemplateFlattenService → TemplateFieldSchema → ExcelSchemaExporter → .xlsx文件
                                                                    ↓ (用户填写)
.xlsx文件 → ExcelContractReader → AdapterResult → DataBindingService → 模板
```

**Excel文件结构**（4行模式）：
- **第1行**（隐藏）：契约行，存储 DataPath
- **第2行**：标签行，显示字段名称
- **第3行**（隐藏）：类型行，标注数据类型与约束
- **第4行起**：数据行

**核心组件**：
- `TemplateFlattenService` — 将模板元素树扁平化为字段列表，过滤 `ElementGroup.Editable` 元素
- `ExcelSchemaExporter` — 导出带契约行的Excel模板，含填写说明页
- `ExcelContractReader` — 按契约行读取Excel数据（单条/批量）
- `ExcelDataValidator` — 校验数据类型、范围、选项、格式

#### 数据库适配器

数据库适配器支持四种数据库的查询与数据映射：

```
DatabaseProviderRegistry → IDatabaseProvider
├── SqlServerProvider    (Microsoft.Data.SqlClient)
├── MySqlProvider        (MySqlConnector)
├── SqliteProvider       (Microsoft.Data.Sqlite)
└── PostgreSqlProvider   (Npgsql)
```

**核心组件**：
- `DatabaseAdapterBase` — 适配器基类，封装查询执行、参数绑定、结果映射、数据转换
- `DatabaseAdapterFactory` — 工厂类，提供连接测试、表/列浏览、查询执行、预览
- `SqlBuilder` — SQL构建器，支持 RawSQL 和 VisualBuilder 两种模式，自动处理JOIN
- `ConnectionPoolManager` — 连接池管理，基于 SemaphoreSlim 的并发控制
- `DatabaseAdapterConfig` — 配置模型，包含 Provider、ConnectionString、Query、DbFieldMappings、Parameters、Joins

**查询模式**：
- **RawSQL** — 直接编写SQL语句
- **VisualBuilder** — 可视化构建：选择主表、列、WHERE条件、排序，自动生成SQL

**字段映射**：`DbFieldMapping` 将数据库列名映射到模板 DataPath，支持 Trim/Upper/Lower 数据转换。`AutoMatchCommand` 自动按列名匹配模板字段。

#### 配置安全

- `AdapterConfigStore` — 适配器配置持久化到 `%AppData%/ReportDataMaker/adapters.json`
- `ConfigProtector` — 使用 DPAPI（Current User scope）加密连接字符串，加密值以 `ENC:` 前缀标识

## 数据流

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
                    │  │ Excel适配器  │  │  数据库适配器     │  │
                    │  │ 导出→填写→读 │  │ 连接→查询→映射   │  │
                    │  └──────┬──────┘  └────────┬─────────┘  │
                    │         ↓                  ↓             │
                    │       AdapterResult (Dictionary)         │
                    │              ↓                           │
                    │     DataBindingService.ApplyData         │
                    │              ↓                           │
                    │     填充后的模板 → 预览/PDF              │
                    └─────────────────────────────────────────┘
```

## 技术栈

| 组件 | 框架 | 关键依赖 |
|------|------|----------|
| Contracts | .NET 8 Class Library | Newtonsoft.Json, Nanoid |
| Editor.Core | .NET 8 Class Library | EF Core 8 (SQLite/SqlServer), JWT, BCrypt |
| Editor.Server | ASP.NET Core 8 | Swagger, Newtonsoft.Json, JWT Bearer |
| ReportDataMaker | WPF (.NET 10) | ClosedXML, HandyControl, 4个数据库驱动, ZXing, DPAPI |

## 内置医疗模板

| 模板 | 文件 |
|------|------|
| 住院病历 | 住院病历.json |
| 门诊病历 | 门诊病历.json |
| 体温单 | 体温单.json |
| 检验报告单 | 检验报告单.json |
| 影像报告单 | 影像报告单.json |
| 处方单 | 处方单.json |
| 护理记录单 | 护理记录单.json |

## API认证

系统使用 JWT Bearer 认证，预置账户：

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | admin123 | admin |
| editor | editor123 | editor |

## 扩展能力

- **新增元素类型**：在 `Contracts/Models/Elements/` 继承 `ExternalElementBase`，在 `ElementJsonConverter.WebShortTypeMap` 注册类型映射，在 `ElementGroupRegistry` 注册适配分组
- **新增适配器类型**：在 `Contracts/Enums/AdapterType` 添加枚举值，实现 `IDataAdapter` 接口，在 Generators 中创建适配器工厂和标签页
- **新增数据库提供者**：实现 `IDatabaseProvider` 接口，在 `DatabaseProviderRegistry.CreateDefault()` 中注册
- **新增生产器**：在 `Generators/` 目录下添加新项目，引用 Contracts 层
