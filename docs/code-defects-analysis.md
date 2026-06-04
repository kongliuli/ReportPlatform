# 杏林报告平台 — 代码缺陷分析报告

> 分析日期：2026-06-04 | 分析范围：全项目（Contracts / Editor / Generators）

---

## 一、安全缺陷

### 🔴 S1 — Newtonsoft.Json TypeNameHandling.Auto 反序列化风险

| 属性 | 详情 |
|------|------|
| **严重程度** | 高危 |
| **文件** | `src/Editor/Server/Program.cs:18` |
| **问题** | `.AddNewtonsoftJson(options => options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto)` 开启了类型自动解析，攻击者可通过构造包含 `$type` 的恶意 JSON 载荷进行远程代码执行（RCE）。 |
| **风险** | 这是 Newtonsoft.Json 已知的反序列化漏洞载体，参见 [CWE-502](https://cwe.mitre.org/data/definitions/502.html)。 |
| **修复建议** | 改用 `System.Text.Json`（ASP.NET Core 默认），或至少将 `TypeNameHandling` 设为 `None`，通过自定义 `ElementJsonConverter` 处理多态序列化。 |

### 🔴 S2 — JWT SecretKey 硬编码在源码中

| 属性 | 详情 |
|------|------|
| **严重程度** | 高危 |
| **文件** | `src/Editor/Server/appsettings.json:15`、`appsettings.Development.json:13` |
| **问题** | JWT 签名密钥以明文形式写在配置文件中并提交到代码仓库，任何能访问代码库的人都可以伪造 JWT Token。 |
| **修复建议** | 从配置文件中移除 SecretKey，改用环境变量、User Secrets（开发环境）或密钥管理服务（如 Azure Key Vault）。`production` 配置文件中的密钥已经是占位符，但 `appsettings.json` 和 `Development.json` 中仍有有效密钥。 |

### 🔴 S3 — 默认管理员密码硬编码

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Data/TemplateSeedData.cs:448-451` |
| **问题** | 种子数据中硬编码了 `admin/admin123` 和 `editor/editor123` 两套默认凭据。虽然使用了 BCrypt 加密存储，但所有部署实例的默认密码相同。 |
| **修复建议** | 首次部署时强制要求修改密码，或从环境变量读取初始密码，或在种子后标记 `PasswordChangeRequired` 标志。 |

### 🟡 S4 — RefreshToken 使用非密码学安全随机数

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Services/AuthService.cs:135` |
| **代码** | `var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");` |
| **问题** | `Guid.NewGuid()` 生成的虽然是唯一标识符但并非密码学安全随机数。RFC 6819 建议刷新令牌使用密码学安全随机生成器。 |
| **修复建议** | 改用 `RandomNumberGenerator.GetBytes(32)` 生成：<br>`var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));` |

### 🟡 S5 — 刷新令牌接口未返回新的 RefreshToken

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Services/AuthService.cs:71-74` |
| **问题** | `RefreshTokenAsync` 内部生成了 `newRefreshToken` 并存入了数据库，但 `RefreshTokenResponse` 只包含 `AccessToken`，未将新的 RefreshToken 返回给客户端。客户端无法获知新令牌，可能导致后续刷新失败。 |
| **修复建议** | 在 `RefreshTokenResponse` 中添加 `RefreshToken` 属性并赋值。 |

### 🟡 S6 — 多个 API 端点缺少 [Authorize] 保护

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `TemplatesController.cs`、`VersionsController.cs`、`PreviewController.cs`、`ContextController.cs` |
| **问题** | 模板 CRUD、版本管理、预览、上下文值等 API 均未添加 `[Authorize]` 属性。虽然在 `Program.cs` 中调用了 `UseAuthorization()`，但若无全局 FallbackPolicy 配置，这些端点对所有匿名请求开放。 |
| **修复建议** | 在控制器或全局添加 `[Authorize]`，或在 `Program.cs` 中配置：<br>`builder.Services.AddAuthorization(options => { options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); });` |

### 🟡 S7 — ContextService 暴露系统信息

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Core/Services/ContextService.cs:38-39` |
| **问题** | `GetAllBuiltInValues()` 通过 `Environment.UserName` 和 `Environment.MachineName` 返回服务器操作系统敏感信息。该端点目前无认证限制。 |
| **修复建议** | 移除 `System.UserName` 和 `System.MachineName`，或在生产环境中配置受限访问。 |

### 🟢 S8 — 生产配置中开发 CORS 未收紧

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Server/appsettings.json:23-26` |
| **问题** | `Cors.AllowedOrigins` 包含 `localhost:5173/5174/3000`，这些是开发环境的 URL，不应出现在生产配置中。 |
| **修复建议** | 将开发环境的 CORS 配置移到 `appsettings.Development.json`，生产配置使用实际域名。 |

---

## 二、架构缺陷

### 🔴 A1 — 启动时无条件执行数据库迁移（生产环境风险）

| 属性 | 详情 |
|------|------|
| **严重程度** | 高危 |
| **文件** | `src/Editor/Server/Program.cs:95-103` |
| **问题** | `db.Database.Migrate()` 在应用启动时无条件执行，无论环境是开发还是生产。EF Core 迁移可能包含破坏性变更（如删表、改列类型），生产环境自动迁移可能导致数据丢失或服务不可用。 |
| **修复建议** | 仅开发环境执行自动迁移，生产环境使用独立的迁移工具（如 `dotnet ef database update`）。或至少加环境判断：<br>`if (app.Environment.IsDevelopment()) { db.Database.Migrate(); }` |

### 🔴 A2 — PdfRenderService 为 Stub 实现，核心功能不可用

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Services/PdfRenderService.cs:44-54` |
| **问题** | `RenderToImageAsync` 返回一个硬编码的"预览占位图" SVG，`RenderToPdfAsync` 调用已被标记 `[Obsolete]` 的 `IPdfSharpTemplateRenderer`。用户调用 PDF/图片预览时得到的不是真实内容。 |
| **修复建议** | 尽快将 `PdfTemplateRenderer` 迁移出 `[Obsolete]` 状态，或实现 `PdfRenderService` 的完整逻辑。 |

### 🟡 A3 — FindSolutionRoot 查找错误的 sln 文件名

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Extensions/ServiceCollectionExtensions.cs:80` |
| **代码** | `File.Exists(Path.Combine(dir, "Xinglin.WebReportEditor.sln"))` |
| **问题** | 解决方案文件实际名为 `ReportPlatform.sln`，而非 `Xinglin.WebReportEditor.sln`。这意味着 `FindSolutionRoot()` 在非开发环境下永远找不到根目录，回退到 `AppContext.BaseDirectory`，SQLite 数据库可能创建在 `bin/Debug/` 下，每次构建后丢失。 |
| **修复建议** | 将 `"Xinglin.WebReportEditor.sln"` 改为 `"ReportPlatform.sln"`。 |

### 🟡 A4 — 缺少 HTTPS 重定向和 HSTS 配置

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Server/Program.cs` |
| **问题** | 未调用 `app.UseHttpsRedirection()` 和 `app.UseHsts()`，生产环境下 HTTP 请求不会被升级到 HTTPS。 |
| **修复建议** | 在认证中间件之前添加：<br>`if (!app.Environment.IsDevelopment()) { app.UseHsts(); } app.UseHttpsRedirection();` |

### 🟡 A5 — 使用已标记 [Obsolete] 的接口和实现

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `IPdfSharpTemplateRenderer.cs`、`PdfTemplateRenderer.cs`、`ServiceCollectionExtensions.cs` |
| **问题** | `IPdfSharpTemplateRenderer` 和 `PdfTemplateRenderer` 被标记为 `[Obsolete("后续由 Rendering 项目的 ITemplateRenderer 替代")]` 但仍通过 `#pragma warning disable CS0618` 强制注册和使用。这是技术债务信号。 |
| **修复建议** | 按计划迁移到新接口，或移除 `[Obsolete]` 标记并正式化该接口。 |

### 🟡 A6 — 无速率限制保护

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `Program.cs`、`AuthController.cs` |
| **问题** | 登录 (`/api/auth/login`) 和令牌刷新 (`/api/auth/refresh`) 端点无速率限制，易受暴力破解和 DoS 攻击。 |
| **修复建议** | 添加 ASP.NET Core Rate Limiting 中间件（.NET 7+ 内置），对认证端点实施限制（如 5次/分钟）。 |

---

## 三、代码质量与潜在 Bug

### 🔴 B1 — UpdateTemplateAsync 存在并发竞争条件

| 属性 | 详情 |
|------|------|
| **严重程度** | 高危 |
| **文件** | `src/Editor/Core/Services/TemplateService.cs:102-140` |
| **问题** | `UpdateTemplateAsync` 中先设置 `entity.Version += 1`，再异步查询 `MaxAsync(v => (int?)v.VersionNumber)` 计算版本号。在高并发下，`entity.Version` 可能被多次递增而 `TemplateVersionEntity` 只插入一条，导致 Template 的 Version 字段与库中的版本记录数不一致。 |
| **修复建议** | 使用乐观锁或数据库事务包裹整个操作。使用 `MaxAsync` 的结果同时赋值给 `entity.Version` 和 `TemplateVersionEntity.VersionNumber`。 |

### 🔴 B2 — VersionService.RollbackAsync 全量加载版本到内存

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Services/VersionService.cs:59-68` |
| **代码** | `var allVersions = await _dbContext.TemplateVersions.Where(...).ToListAsync();`<br>`var maxVersion = allVersions.Max(v => v.VersionNumber);` |
| **问题** | 将模板的所有版本加载到内存，再用 LINQ `Max()` 计算最大值。当版本数量增长到数百个以上时，会有显著的性能和内存开销。 |
| **修复建议** | 用数据库端聚合：<br>`var maxVersion = await _dbContext.TemplateVersions.Where(v => v.TemplateId == templateId).MaxAsync(v => (int?)v.VersionNumber) ?? 0;` |

### 🟡 B3 — GetTemplatesAsync 直接修改输入参数（副作用）

| 属性 | 详情 |
|------|------|
| **严重程度** | 中危 |
| **文件** | `src/Editor/Core/Services/TemplateService.cs:22-25` |
| **代码** | `if (filter.Page < 1) filter.Page = 1;` |
| **问题** | 方法直接修改了调用者传入的 `TemplateFilterRequest filter` 对象的 `Page` 和 `PageSize` 属性。这种副作用可能导致调用方在后续代码中获取到被修改过的值。 |
| **修复建议** | 将参数规范化到局部变量中：<br>`var page = Math.Max(1, filter.Page); var pageSize = Math.Clamp(filter.PageSize, 1, 100);` |

### 🟡 B4 — GlobalExceptionMiddleware 异常映射语义不当

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Server/Middleware/GlobalExceptionMiddleware.cs:42` |
| **问题** | 所有 `InvalidOperationException` 一律返回 409 Conflict。但这类异常可能是数据库连接失败、DI 配置错误等，返回 409 会误导客户端。 |
| **修复建议** | 对 `InvalidOperationException` 返回 500 Internal Server Error，或根据消息内容细分。 |

### 🟢 B5 — DiffAsync 静默吞掉所有异常

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Core/Services/VersionService.cs:190-198` |
| **问题** | JSON 解析失败时使用空的 `catch` 块吞掉所有异常，仅返回 "解析失败" 提示，导致实际错误原因不可追踪。 |
| **修复建议** | 至少应记录异常日志：`catch (Exception ex) { _logger.LogWarning(ex, "版本对比解析失败"); ... }` |

### 🟢 B6 — DTO 缺少输入验证

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Contracts/Requests/CreateTemplateRequest.cs`、`UpdateTemplateRequest.cs` |
| **问题** | 请求 DTO 缺少 DataAnnotation 验证（如 `[Required]`、`[MaxLength]`、`[StringLength]`），可能导致空名称、超长 ContentJson 等无效数据进入服务层。 |
| **修复建议** | 在请求模型上添加验证特性：<br>`[Required] public string Name { get; set; }`、`[MaxLength(1_000_000)] public string ContentJson { get; set; }` |

### 🟢 B7 — 日志记录可能暴露敏感数据

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Server/Middleware/ApiLoggingMiddleware.cs:24-31` |
| **问题** | 请求日志记录了完整的 `Path` 和 `QueryString`，如果 URL 中包含敏感参数（如 token），会被写入日志。 |
| **修复建议** | 对 QueryString 中的敏感参数（如 `token`、`password`）进行脱敏处理后再记录。 |

### 🟢 B8 — 生产配置文件中的数据库连接串缺失

| 属性 | 详情 |
|------|------|
| **严重程度** | 低危 |
| **文件** | `src/Editor/Server/appsettings.Production.json` |
| **问题** | 生产配置文件中的 `SqlServerConnection` 为 `YOUR_SERVER_ADDRESS` 占位符，未提供有效的数据库连接串。部署到生产环境时若不替换会导致运行时错误。 |
| **修复建议** | 从配置文件中移除占位符，通过环境变量注入生产环境的连接字符串。 |

---

## 四、总结

### 按严重程度统计

| 级别 | 数量 | 关键项 |
|------|------|--------|
| 🔴 高危 | 4 | S1（反序列化RCE）、A1（生产环境自动迁移）、B1（并发竞争条件）、S2（硬编码JWT密钥） |
| 🟡 中危 | 9 | S3-S6、A2-A3、B2-B3 |
| 🟢 低危 | 8 | S7-S8、A4-A6、B4-B8 |

### 优先修复建议

1. **立即修复**：S1（TypeNameHandling.Auto）和 S2（硬编码 JWT SecretKey）是直接安全风险
2. **尽快修复**：A1（生产环境自动迁移）、A3（错误的 sln 文件名）、B1（并发竞争条件）
3. **计划修复**：添加 `[Authorize]`、实现完整的 PDF 渲染、RefreshToken 安全性提升
4. **技术债务清理**：解决 `[Obsolete]` 接口、添加输入验证、修复语义问题
