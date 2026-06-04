---
name: gitignore-supplement-and-code-defect-analysis
overview: 补充 .gitignore 中缺失的忽略规则，然后对项目代码进行全面的缺陷分析，包括安全、架构、代码质量、潜在 Bug 等维度。
todos:
  - id: update-gitignore
    content: 补充 .gitignore 缺失的忽略规则（.sisyphus/、.trae/、.codebuddy/、.vs/、*.orig、*.db）
    status: completed
  - id: write-defects-report
    content: 深入分析并撰写代码缺陷报告，输出到 docs/code-defects-analysis.md，覆盖安全、架构、代码质量、潜在 Bug 四个维度
    status: completed
---

## 任务一：补充 .gitignore 缺失规则

在上轮分析中发现了 6 项未列入 `.gitignore` 的内容，需要在文件末尾追加以下规则：

- `.sisyphus/` — Sisyphus AI 编排器内部工作文件（boulder.json、plans、run-continuation），属于工具缓存
- `.trae/` — Trae AI 助手的内部 spec 文件，属于 AI 辅助开发临时产物
- `.codebuddy/` — CodeBuddy 工作记忆与内部数据目录
- `.vs/` — Visual Studio 工作区设置（项目缓存、.suo 等），标准 .NET 项目都应忽略
- `*.orig` — git merge 冲突产生的备份文件
- `*.db` — SQLite 数据库运行时文件（项目中已配置 SQLite 数据源，运行时会生成 .db 文件）

## 任务二：深入代码缺陷分析并输出报告

对项目进行全面的代码审查，覆盖以下维度，并将结果写入 `docs/code-defects-analysis.md`：

### 安全缺陷

- Newtonsoft.Json TypeNameHandling.Auto 反序列化风险（Program.cs 第18行）
- JWT SecretKey 硬编码在源码中（appsettings.json / appsettings.Development.json）
- 默认管理员密码硬编码在 TemplateSeedData.cs 中
- RefreshToken 使用 Guid 拼接而非密码学安全随机数生成
- 多个控制器缺少 `[Authorize]` 属性保护
- ContextService 暴露系统信息（Environment.UserName、Environment.MachineName）

### 架构缺陷

- 启动时自动执行 `db.Database.Migrate()`，生产环境有数据丢失风险
- PdfRenderService 为 Stub 实现，核心功能不可用
- FindSolutionRoot 硬编码了错误的 sln 文件名（实际为 ReportPlatform.sln）
- 缺少 HTTPS 重定向和 HSTS 配置
- 使用已标记 `[Obsolete]` 的 `IPdfSharpTemplateRenderer`

### 代码质量与潜在 Bug

- TemplateService.UpdateTemplateAsync 存在并发竞争条件（version 计算可能不准确）
- VersionService.RollbackAsync 加载全部版本到内存后再取 Max，而非使用数据库端 MaxAsync
- TemplateService.GetTemplatesAsync 直接修改 filter 参数（副作用）
- AuthService.RefreshTokenAsync 未返回新的 RefreshToken 到客户端
- GlobalExceptionMiddleware 对 InvalidOperationException 一律返回 409 Conflict，语义不当
- 缺少输入验证（CreateTemplateRequest 等 DTO 仅有少量 DataAnnotation）

## 实现方式

### 任务一：更新 .gitignore

直接在现有 `.gitignore` 文件末尾追加新的忽略规则，格式与现有风格一致。

### 任务二：生成代码缺陷报告

通过全面审查已读取的关键代码文件，按照安全缺陷、架构缺陷、代码质量、潜在 Bug 四个维度分类输出 Markdown 报告。每个缺陷条目包含：严重程度、文件位置、问题描述、风险说明、修复建议。