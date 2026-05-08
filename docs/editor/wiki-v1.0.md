# Editor 编辑器模块 Wiki v1.0

> 版本：v1.0 | 更新日期：2026-05-08 | 覆盖版本：v0.3.0 ~ v0.6.0

---

## 1. 概述

Editor 模块分为两个子项目：
- **Editor.Core** — 核心业务逻辑库
- **Editor.Server** — ASP.NET Core Web API 服务端

---

## 2. Editor.Core

**项目路径**：`/Editor/Core/Xinglin.WebReportEditor.Core.csproj`
**命名空间**：`Xinglin.WebReportEditor.Core`
**目标框架**：`net8.0`

### 2.1 NuGet 依赖

| 包 | 版本 | 用途 |
|----|------|------|
| Microsoft.EntityFrameworkCore | 8.0.0 | ORM |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 | SQLite Provider |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server Provider |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT 认证 |
| Newtonsoft.Json | 13.0.3 | JSON 序列化 |
| System.IdentityModel.Tokens.Jwt | 8.0.0 | JWT Token 生成 |
| BCrypt.Net-Next | 4.0.3 | 密码哈希 |

### 2.2 项目引用

- Contracts（路径：`..\..\Contracts\Xinglin.WebReportEditor.Contracts.csproj`）
- 条件引用：Xinglin.Core.dll, Xinglin.Infrastructure.dll, Xinglin.Security.dll（from ../libs/）

### 2.3 数据实体

| 实体 | 主要属性 |
|------|---------|
| TemplateEntity | Id, Name, Type, Version, ContentJson, HospitalId, IsDefault, IsPublished |
| TemplateVersionEntity | Id, TemplateId, VersionNumber, ContentJson, ChangeDescription |
| UserEntity | Id, Username, PasswordHash, DisplayName, Role, HospitalId, IsActive |
| RefreshTokenEntity | Id, Token, UserId, ExpiryTime, IsRevoked |

### 2.4 DbContext

- **TemplateDbContext** — EF Core 上下文
- **TemplateDbContextFactory** — 设计时工厂（用于 Migrations）
- **TemplateSeedData** — 种子数据（默认用户 admin/editor）

### 2.5 Migrations

| 迁移 | 说明 |
|------|------|
| 20260426055921_InitialCreate | 初始表结构（Templates, TemplateVersions） |
| 20260426080903_AddAuthEntities | 认证实体（Users, RefreshTokens） |

### 2.6 服务接口与实现

| 接口 | 实现 | 方法 |
|------|------|------|
| ITemplateService | TemplateService | GetTemplatesAsync, GetTemplateAsync, CreateTemplateAsync, UpdateTemplateAsync, DeleteTemplateAsync |
| IVersionService | VersionService | GetVersionsAsync, GetVersionAsync, RollbackAsync, DiffAsync |
| IAuthService | AuthService | LoginAsync, RefreshTokenAsync, RevokeTokenAsync, GetUserByCredentialsAsync |
| IPdfRenderService | PdfRenderService | PDF 渲染（Stub 实现） |

### 2.7 SharedInterfaces（跨模块共享接口）

| 接口 | 说明 |
|------|------|
| IDataBindingEngine | 数据绑定引擎（ApplyDataBinding） |
| IJsonTemplateSerializer | JSON 模板序列化（Serialize/Deserialize） |
| IPdfSharpTemplateRenderer | PDF 渲染（RenderToPdf） |

当前均为 Stub 实现（DataBindingEngineStub, JsonTemplateSerializerStub, PdfSharpTemplateRendererStub）。

### 2.8 DI 扩展

`ServiceCollectionExtensions` 提供 `AddEditorCore()` 扩展方法注册所有 Core 服务。

---

## 3. Editor.Server

**项目路径**：`/Editor/Server/Xinglin.WebReportEditor.Server.csproj`
**命名空间**：`Xinglin.WebReportEditor.Server`
**目标框架**：`net8.0`

### 3.1 NuGet 依赖

| 包 | 版本 | 用途 |
|----|------|------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT 认证 |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 8.0.0 | JSON 序列化 |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | EF Core 迁移工具 |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI |

### 3.2 项目引用

- Editor.Core
- Contracts

### 3.3 控制器

| 控制器 | 路由前缀 | 职责 |
|--------|---------|------|
| TemplatesController | `/api/templates` | 模板 CRUD + 分页查询 |
| VersionsController | `/api/versions` | 版本管理 + 回滚 + Diff |
| AuthController | `/api/auth` | 登录 + Token 刷新 + 注销 |
| PreviewController | `/api/preview` | 模板预览渲染 |
| HealthController | `/api/health` | 健康检查 |

### 3.4 中间件

| 中间件 | 说明 |
|--------|------|
| GlobalExceptionMiddleware | 全局异常处理，统一错误响应格式 |
| ApiLoggingMiddleware | API 请求/响应日志记录 |

### 3.5 Program.cs 配置

- Swagger/OpenAPI
- JWT Bearer 认证
- EF Core + 自动迁移
- CORS 跨域策略
- SignalR 支持
- 数据库种子数据初始化

### 3.6 配置文件

| 文件 | 说明 |
|------|------|
| appsettings.json | 基础配置 |
| appsettings.Development.json | 开发环境 |
| appsettings.Production.json | 生产环境 |

---

## 4. 部署

### 4.1 Docker 部署

部署文件位于 `/deploy/` 目录：

| 文件 | 说明 |
|------|------|
| docker-compose.yml | 编排（editor-server + nginx） |
| docker-compose.override.yml | 开发覆盖配置 |
| .env.example | 环境变量模板 |
| editor-server/Dockerfile | Editor.Server 镜像构建 |
| nginx/nginx.conf | Nginx 反向代理配置 |

服务端口：
- editor-server: 5000（内部）
- nginx: 80（外部）

### 4.2 默认用户

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | admin123 | admin |
| editor | editor123 | editor |

---

## 5. 已知问题

- csproj 中项目引用路径不正确（使用旧路径 `..\Xinglin.WebReportEditor.Contracts\` 而非 `..\..\Contracts\`），但不影响 Solution 级别构建
- SharedInterfaces 中的 PDF 渲染、数据绑定引擎均为 Stub 实现，待后续版本完善

---

*文档版本 1.0，基于代码实际状态编写*
