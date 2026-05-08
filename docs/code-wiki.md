# Xinglin.ReportPlatform 代码百科

基于契约驱动的报告单生成平台，支持在线模板编辑、多格式数据生产和可扩展的适配器架构。

> 文档版本：1.0.0  
> 更新日期：2026-05-08

---

## 目录

1. [项目概览](#1-项目概览)
2. [架构设计](#2-架构设计)
3. [模块详解](#3-模块详解)
4. [核心类与接口](#4-核心类与接口)
5. [依赖关系](#5-依赖关系)
6. [数据模型](#6-数据模型)
7. [API接口](#7-api接口)
8. [运行指南](#8-运行指南)

---

## 1. 项目概览

### 1.1 项目简介

**Xinglin.ReportPlatform（杏林报告平台）** 是一个面向医疗行业的报告单生成平台，采用契约驱动（Contract-Driven）架构，实现模板设计、版本管理和多格式报告生成的一体化解决方案。

### 1.2 技术栈

| 模块 | 技术框架 | 版本 | 说明 |
|------|---------|------|------|
| Editor.Server | ASP.NET Core | .NET 8 | Web API 服务端 |
| Editor.Core | .NET Class Library | .NET 8 | 核心业务逻辑库 |
| Contracts | .NET Class Library | .NET 8 | 数据传输契约定义 |
| ReportDataMaker | WPF | .NET 10 | Windows 桌面数据录入工具 |

### 1.3 解决方案结构

```
ReportPlatform.sln
├── Contracts/                              # 契约定义层
│   ├── DTOs/                              # 数据传输对象
│   ├── Requests/                          # 请求模型
│   └── Responses/                         # 响应模型
│
├── Editor/                                 # 在线编辑器模块
│   ├── Core/                              # 核心业务逻辑
│   │   ├── Data/                          # 数据实体
│   │   ├── Services/                      # 业务服务
│   │   ├── SharedInterfaces/              # 共享接口
│   │   └── Extensions/                    # 扩展方法
│   └── Server/                            # Web API 服务
│       ├── Controllers/                   # API 控制器
│       └── Middleware/                    # 中间件
│
└── Generators/                             # 生产器层
    └── ReportDataMaker/                   # WPF 数据录入工具
        ├── Models/                        # 数据模型
        ├── Services/                      # 业务服务
        ├── ViewModels/                    # 视图模型
        └── Views/                         # 视图组件
```

---

## 2. 架构设计

### 2.1 整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                    Xinglin.ReportPlatform                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────┐    ┌──────────┐    ┌──────────────┐          │
│  │  Editor  │◀──▶│ Contracts │◀──▶│  Generators  │          │
│  │ (编辑器) │    │ (契约层) │    │  (生产器层)  │          │
│  └──────────┘    └──────────┘    └──────────────┘          │
│         │              │                │                   │
│         ▼              ▼                ▼                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                    Data Layer                          │   │
│  │     (EF Core: SQLite / SQL Server)                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 核心设计原则

1. **契约优先（Contract-First）**：Editor 和 Generators 通过 Contracts 层定义的 DTO 进行解耦通信
2. **服务分离（Service Separation）**：业务逻辑封装在 Core 层，API 层仅负责请求路由
3. **依赖倒置（Dependency Inversion）**：服务通过接口定义，依赖注入容器统一管理

### 2.3 工作流程

```
1. 模板设计 ──▶ Editor 编辑模板 ──▶ 保存至数据库
                      │
                      ▼
2. 契约导出 ──▶ 生成标准 JSON 契约 ──▶ Templates 表 ContentJson 字段
                      │
                      ▼
3. 数据生产 ──▶ Generator 读取契约 ──▶ 数据录入 ──▶ 报告生成
                      │
                      ▼
4. 适配支持 ──▶ DatabaseAdapter / ExcelImportAdapter 获取数据
```

---

## 3. 模块详解

### 3.1 Contracts 模块

**职责**：定义 Editor 和 Generators 之间共享的数据结构和接口契约。

**项目路径**：`/Contracts/Xinglin.WebReportEditor.Contracts.csproj`

**命名空间**：`Xinglin.WebReportEditor.Contracts`

#### 3.1.1 子目录结构

| 目录 | 内容 |
|------|------|
| `DTOs/` | 数据传输对象（模板信息、用户信息、版本信息） |
| `Requests/` | API 请求模型（创建、筛选、更新） |
| `Responses/` | API 响应模型（统一响应、分页响应） |

#### 3.1.2 核心 DTO

- **TemplateDto** - 模板摘要信息
- **TemplateDetailDto** - 模板完整信息（含 ContentJson）
- **TemplateVersionDto** - 版本摘要
- **TemplateVersionDetailDto** - 版本详细信息（含 ContentJson）
- **LoginRequest/Response** - 登录请求响应
- **RefreshTokenRequest/Response** - Token 刷新

---

### 3.2 Editor.Core 模块

**职责**：封装核心业务逻辑，提供模板管理、版本控制、认证授权等服务。

**项目路径**：`/Editor/Core/Xinglin.WebReportEditor.Core.csproj`

**命名空间**：`Xinglin.WebReportEditor.Core`

#### 3.2.1 子目录结构

| 目录 | 内容 |
|------|------|
| `Data/` | Entity Framework 实体和 DbContext |
| `Services/` | 业务服务实现 |
| `SharedInterfaces/` | 跨模块共享的接口定义 |
| `Extensions/` | 依赖注入扩展方法 |
| `Migrations/` | EF Core 数据库迁移 |

#### 3.2.2 核心服务

| 服务类 | 接口 | 职责 |
|--------|------|------|
| TemplateService | ITemplateService | 模板 CRUD 操作 |
| VersionService | IVersionService | 模板版本管理、回滚、差异对比 |
| AuthService | IAuthService | 用户认证、Token 管理 |
| PdfRenderService | IPdfRenderService | PDF 渲染服务 |

#### 3.2.3 数据实体

- **TemplateEntity** - 模板主表
- **TemplateVersionEntity** - 模板版本历史
- **UserEntity** - 用户信息
- **RefreshTokenEntity** - 刷新令牌

---

### 3.3 Editor.Server 模块

**职责**：提供 RESTful API 接口，处理 HTTP 请求和响应。

**项目路径**：`/Editor/Server/Xinglin.WebReportEditor.Server.csproj`

**命名空间**：`Xinglin.WebReportEditor.Server`

#### 3.3.1 核心控制器

| 控制器 | 路由前缀 | 职责 |
|--------|---------|------|
| TemplatesController | `/api/templates` | 模板管理 API |
| VersionsController | `/api/versions` | 版本管理 API |
| AuthController | `/api/auth` | 认证授权 API |
| PreviewController | `/api/preview` | 模板预览 API |
| HealthController | `/api/health` | 健康检查 API |

#### 3.3.2 中间件

- **GlobalExceptionMiddleware** - 全局异常处理
- **ApiLoggingMiddleware** - API 请求日志记录

---

### 3.4 Generators.ReportDataMaker 模块

**职责**：基于契约的 WPF 桌面数据录入工具，支持模板加载、数据绑定和报告预览。

**项目路径**：`/Generators/ReportDataMaker/ReportDataMaker.csproj`

**目标框架**：`net10.0-windows`

#### 3.4.1 子目录结构

| 目录 | 内容 |
|------|------|
| `Models/` | 数据模型（模板定义、元素、数据上下文） |
| `Services/` | 业务服务（模板加载、数据绑定、导出） |
| `ViewModels/` | MVVM 视图模型 |
| `Views/` | XAML 视图 |
| `Configs/` | 配置文件 |
| `Templates/` | 内置模板 JSON |
| `Infrastructure/` | MVVM 基础设施 |

#### 3.4.2 核心服务

| 服务类 | 职责 |
|--------|------|
| JsonTemplateLoader | JSON 模板解析（支持新旧格式） |
| TemplateLoaderService | 模板加载和分类 |
| DataBindingService | 数据路径绑定 |
| DataExportService | 数据导出 |
| CanvasRenderer | 模板预览渲染 |
| DataEntryService | 数据录入管理 |

---

## 4. 核心类与接口

### 4.1 服务接口

#### ITemplateService

```csharp
public interface ITemplateService
{
    Task<PagedResponse<TemplateDto>> GetTemplatesAsync(TemplateFilterRequest filter);
    Task<TemplateDetailDto?> GetTemplateAsync(Guid id);
    Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateRequest request);
    Task<TemplateDetailDto?> UpdateTemplateAsync(Guid id, UpdateTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid id);
}
```

#### IVersionService

```csharp
public interface IVersionService
{
    Task<List<TemplateVersionDto>> GetVersionsAsync(Guid templateId);
    Task<TemplateVersionDetailDto?> GetVersionAsync(Guid templateId, Guid versionId);
    Task<TemplateVersionDto> RollbackAsync(Guid templateId, Guid versionId, string? createdBy);
    Task<VersionDiffResponse> DiffAsync(Guid templateId, Guid versionIdA, Guid versionIdB);
}
```

#### IAuthService

```csharp
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<UserDto?> GetUserByCredentialsAsync(string username, string password);
}
```

### 4.2 核心服务实现

#### TemplateService

| 方法 | 访问修饰符 | 说明 |
|------|----------|------|
| GetTemplatesAsync | public | 分页查询模板列表 |
| GetTemplateAsync | public | 根据 ID 获取模板详情 |
| CreateTemplateAsync | public | 创建新模板（同时创建初始版本） |
| UpdateTemplateAsync | public | 更新模板（内容变更时自动创建新版本） |
| DeleteTemplateAsync | public | 删除模板 |
| MapToDto | private static | 实体转 DTO |
| MapToDetailDto | private static | 实体转详细 DTO |

#### AuthService

| 方法 | 访问修饰符 | 说明 |
|------|----------|------|
| LoginAsync | public | 用户登录，验证密码，生成 Token |
| RefreshTokenAsync | public | 刷新访问令牌 |
| RevokeTokenAsync | public | 撤销刷新令牌 |
| GetUserByCredentialsAsync | public | 根据凭据获取用户 |
| GenerateAccessToken | private | 生成 JWT 访问令牌 |
| GenerateRefreshTokenAsync | private | 生成刷新令牌并存储 |
| MapToUserDto | private static | 用户实体转 DTO |

#### VersionService

| 方法 | 访问修饰符 | 说明 |
|------|----------|------|
| GetVersionsAsync | public | 获取模板的所有版本列表 |
| GetVersionAsync | public | 获取指定版本详情 |
| RollbackAsync | public | 回滚到指定版本（创建新版本） |
| DiffAsync | public | 对比两个版本的差异 |

### 4.3 数据模型

#### TemplateEntity

```csharp
[Table("Templates")]
public class TemplateEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }           // 模板名称
    public string Type { get; set; }            // 模板类型
    public int Version { get; set; }            // 当前版本号
    public string ContentJson { get; set; }    // 模板内容 JSON
    public string? HospitalId { get; set; }     // 医院标识
    public bool IsDefault { get; set; }        // 是否默认模板
    public bool IsPublished { get; set; }     // 是否已发布
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public string? CreatedBy { get; set; }
    
    public virtual ICollection<TemplateVersionEntity> Versions { get; set; }
}
```

#### TemplateVersionEntity

```csharp
[Table("TemplateVersions")]
public class TemplateVersionEntity
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }       // 关联模板 ID
    public int VersionNumber { get; set; }      // 版本号
    public string ContentJson { get; set; }    // 版本内容
    public string? ChangeDescription { get; set; }
    public DateTime CreateTime { get; set; }
    public string? CreatedBy { get; set; }
    
    [ForeignKey(nameof(TemplateId))]
    public virtual TemplateEntity Template { get; set; }
}
```

#### UserEntity

```csharp
[Table("Users")]
public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }   // BCrypt 哈希
    public string? DisplayName { get; set; }
    public string Role { get; set; }           // admin / editor
    public string? HospitalId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateTime { get; set; }
    
    public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; }
}
```

### 4.4 API 响应模型

#### ApiResponse<T>

```csharp
public class ApiResponse<T>
{
    public int Code { get; set; }              // 状态码
    public string Message { get; set; }        // 消息
    public T? Data { get; set; }               // 数据
    
    public static ApiResponse<T> Ok(T data, string message = "操作成功");
    public static ApiResponse<T> Fail(string message, int code = 500);
}
```

#### PagedResponse<T>

```csharp
public class PagedResponse<T>
{
    public List<T> Items { get; set; }         // 数据项列表
    public int TotalCount { get; set; }        // 总记录数
    public int Page { get; set; }              // 当前页码
    public int PageSize { get; set; }          // 每页条数
}
```

---

## 5. 依赖关系

### 5.1 项目依赖图

```
┌─────────────────────────────────────────────────────┐
│              Editor.Server (ASP.NET Core)           │
├─────────────────────────────────────────────────────┤
│  依赖: Editor.Core, Contracts                       │
│  NuGet: JwtBearer, Swagger, EF Core Design         │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│                 Editor.Core (.NET 8)                │
├─────────────────────────────────────────────────────┤
│  依赖: Contracts                                    │
│  NuGet: EF Core, EF Sqlite, EF SqlServer,          │
│         JwtBearer, Newtonsoft.Json, BCrypt.Net,    │
│         System.IdentityModel.Tokens.Jwt            │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│              Contracts (.NET 8)                     │
├─────────────────────────────────────────────────────┤
│  无外部依赖（纯数据定义）                            │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│           ReportDataMaker (WPF .NET 10)             │
├─────────────────────────────────────────────────────┤
│  NuGet: ClosedXML, Microsoft.Data.SqlClient,       │
│         MySqlConnector, Newtonsoft.Json,           │
│         ZXing.Net.Bindings.Windows.Compatibility   │
└─────────────────────────────────────────────────────┘
```

### 5.2 依赖注入配置

在 `ServiceCollectionExtensions.cs` 中注册：

```csharp
public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
{
    // 数据库上下文
    services.AddDbContext<TemplateDbContext>(...);
    
    // 业务服务
    services.AddScoped<ITemplateService, TemplateService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IVersionService, VersionService>();
    services.AddScoped<IPdfRenderService, PdfRenderService>();
    
    // 接口存根
    services.AddScoped<IJsonTemplateSerializer, JsonTemplateSerializerStub>();
    services.AddScoped<IDataBindingEngine, DataBindingEngineStub>();
    services.AddScoped<IPdfSharpTemplateRenderer, PdfSharpTemplateRendererStub>();
    
    return services;
}
```

### 5.3 数据库支持

支持双数据库配置（通过 `DatabaseProvider` 配置项切换）：

| Provider | 连接字符串键 | 说明 |
|----------|-------------|------|
| Sqlite | SqliteConnection | 默认，开发环境 |
| SqlServer | SqlServerConnection | 生产环境 |

---

## 6. 数据模型

### 6.1 数据库表结构

#### Templates 表

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | GUID | PK, Identity | 模板唯一标识 |
| Name | VARCHAR(100) | NOT NULL | 模板名称 |
| Type | VARCHAR(50) | NOT NULL | 模板类型 |
| Version | INT | NOT NULL | 当前版本号 |
| ContentJson | TEXT | NOT NULL | 模板内容 JSON |
| HospitalId | VARCHAR(50) | NULL | 医院标识 |
| IsDefault | BIT | NOT NULL | 是否默认 |
| IsPublished | BIT | NOT NULL | 是否已发布 |
| CreateTime | DATETIME | NOT NULL | 创建时间 |
| UpdateTime | DATETIME | NOT NULL | 更新时间 |
| CreatedBy | VARCHAR(100) | NULL | 创建人 |

#### TemplateVersions 表

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | GUID | PK, Identity | 版本唯一标识 |
| TemplateId | GUID | FK, NOT NULL | 关联模板 ID |
| VersionNumber | INT | NOT NULL | 版本号 |
| ContentJson | TEXT | NOT NULL | 版本内容 |
| ChangeDescription | VARCHAR(500) | NULL | 变更描述 |
| CreateTime | DATETIME | NOT NULL | 创建时间 |
| CreatedBy | VARCHAR(100) | NULL | 创建人 |

#### Users 表

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | GUID | PK, Identity | 用户唯一标识 |
| Username | VARCHAR(100) | UNIQUE, NOT NULL | 用户名 |
| PasswordHash | VARCHAR | NOT NULL | 密码哈希 |
| DisplayName | VARCHAR(100) | NULL | 显示名称 |
| Role | VARCHAR(50) | NOT NULL | 角色 |
| HospitalId | VARCHAR(50) | NULL | 医院标识 |
| IsActive | BIT | NOT NULL | 是否激活 |
| CreateTime | DATETIME | NOT NULL | 创建时间 |

#### RefreshTokens 表

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | GUID | PK, Identity | 令牌唯一标识 |
| Token | VARCHAR | UNIQUE, NOT NULL | 令牌值 |
| UserId | GUID | FK, NOT NULL | 关联用户 ID |
| ExpiryTime | DATETIME | NOT NULL | 过期时间 |
| IsRevoked | BIT | NOT NULL | 是否已撤销 |
| CreateTime | DATETIME | NOT NULL | 创建时间 |

### 6.2 索引设计

```csharp
// Templates 表
entity.HasIndex(e => e.Name);
entity.HasIndex(e => e.Type);
entity.HasIndex(e => e.HospitalId);
entity.HasIndex(e => new { e.Type, e.HospitalId });

// TemplateVersions 表
entity.HasIndex(e => e.TemplateId);
entity.HasIndex(e => new { e.TemplateId, e.VersionNumber }).IsUnique();

// Users 表
entity.HasIndex(e => e.Username).IsUnique();

// RefreshTokens 表
entity.HasIndex(e => e.Token);
entity.HasIndex(e => e.UserId);
```

---

## 7. API 接口

### 7.1 模板管理

#### GET /api/templates

分页查询模板列表。

**请求参数**：
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 否 | 模板名称（模糊匹配） |
| type | string | 否 | 模板类型 |
| hospitalId | string | 否 | 医院标识 |
| isPublished | bool | 否 | 是否已发布 |
| page | int | 否 | 页码（默认 1） |
| pageSize | int | 否 | 每页条数（默认 20） |

**响应**：`ApiResponse<PagedResponse<TemplateDto>>`

#### GET /api/templates/{id}

获取模板详情。

**响应**：`ApiResponse<TemplateDetailDto>`

#### POST /api/templates

创建新模板。

**请求体**：`CreateTemplateRequest`

**响应**：`ApiResponse<TemplateDetailDto>`

#### PUT /api/templates/{id}

更新模板。

**请求体**：`UpdateTemplateRequest`

**响应**：`ApiResponse<TemplateDetailDto>`

#### DELETE /api/templates/{id}

删除模板。

**响应**：`ApiResponse<bool>`

### 7.2 版本管理

#### GET /api/versions/{templateId}

获取模板的所有版本。

**响应**：`ApiResponse<List<TemplateVersionDto>>`

#### GET /api/versions/{templateId}/{versionId}

获取指定版本详情。

**响应**：`ApiResponse<TemplateVersionDetailDto>`

#### POST /api/versions/{templateId}/rollback

回滚到指定版本。

**请求体**：`RollbackRequest`

**响应**：`ApiResponse<TemplateVersionDto>`

#### GET /api/versions/{templateId}/diff

对比两个版本的差异。

**查询参数**：`versionA`（版本 ID A）, `versionB`（版本 ID B）

**响应**：`ApiResponse<VersionDiffResponse>`

### 7.3 认证授权

#### POST /api/auth/login

用户登录。

**请求体**：`LoginRequest`

**响应**：`ApiResponse<LoginResponse>`

#### POST /api/auth/refresh

刷新访问令牌。

**请求体**：`RefreshTokenRequest`

**响应**：`ApiResponse<RefreshTokenResponse>`

#### POST /api/auth/logout

用户登出。

**请求体**（可选）：`RefreshTokenRequest`

**响应**：`ApiResponse<object>`

### 7.4 预览功能

#### GET /api/preview/{templateId}

生成模板预览图片。

**响应**：`ApiResponse<string>`（Base64 编码的图片）

---

## 8. 运行指南

### 8.1 环境要求

| 组件 | 要求 |
|------|------|
| .NET SDK | .NET 8.0+ |
| 数据库 | SQLite 3+ 或 SQL Server 2016+ |
| Node.js | 可选（用于前端开发） |
| Visual Studio | 2022+（推荐） |

### 8.2 配置说明

#### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=xinglin_webreport.db"
  },
  
  "DatabaseProvider": "Sqlite",
  
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  },
  
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyHere_MinLength32Chars!",
    "Issuer": "Xinglin.ReportPlatform",
    "Audience": "Xinglin.ReportPlatform.Client",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 8.3 启动步骤

#### 1. 克隆项目

```bash
git clone <repository-url>
cd ReportPlatform
```

#### 2. 还原依赖

```bash
dotnet restore
```

#### 3. 配置数据库连接

编辑 `Editor/Server/appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=data/xinglin_webreport.db"
  }
}
```

#### 4. 应用数据库迁移

```bash
cd Editor/Server
dotnet ef database update
```

#### 5. 启动 Editor.Server

```bash
cd Editor/Server
dotnet run
```

服务将在 `http://localhost:5000` 启动。

#### 6. 访问 Swagger UI

打开浏览器访问：`http://localhost:5000/swagger`

### 8.4 默认用户

系统初始化时会创建两个默认用户：

| 用户名 | 密码 | 角色 | 显示名称 |
|--------|------|------|----------|
| admin | admin123 | admin | 系统管理员 |
| editor | editor123 | editor | 编辑员 |

### 8.5 启动 ReportDataMaker（WPF 应用）

```bash
cd Generators/ReportDataMaker
dotnet run
```

### 8.6 Docker 部署

#### Dockerfile 示例

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
RUN dotnet build "Xinglin.WebReportEditor.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Xinglin.WebReportEditor.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data
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

### B. 相关文档

- [架构设计文档](./docs/architecture/)
- [契约定义文档](./docs/contracts/)
- [适配器组件设计](./docs/adapters/adapter-components-design.md)

---

*本文档由 Code Wiki Generator 自动生成*
