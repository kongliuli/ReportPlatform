# Draft: ReportPlatform 优化分析

## 项目概况
- Xinglin.ReportPlatform - 契约驱动的医疗报告单生成平台
- 技术栈: .NET 8/10, ASP.NET Core 8, Vue 3 + Vite, WPF (.NET 10)
- 当前版本: v0.6.0, v0.7.0/v0.8.0 开发中
- Editor/ (41 C# + 96 JS/Vue) + Generators/ (78 C#) + Contracts (共享层)

---

## 🔴 CRITICAL (安全/数据风险)

### C1. PdfTemplateRenderer & DataBindingEngine - 过度使用 object 类型
- 文件: `Editor/Core/Services/PdfTemplateRenderer.cs:19`, `DataBindingEngine.cs:15`
- `RenderToPdf(object templateDefinition)` 和 `ApplyDataBinding(object template, object sampleData)`
- 编译时无类型检查，运行时易出错

### C2. TemplateDbContext.SeedData - 硬编码凭证
- 文件: `Editor/Core/Data/TemplateDbContext.cs:67`
- `admin/admin123` 硬编码在迁移中，每次迁移都会插入
- 建议: 仅开发环境生效，或使用 user-secrets

### C3. .NET 10 SDK 预览版
- 文件: `global.json`
- 锁定 SDK 10.0.200 (预览版)，生产环境稳定性风险
- Editor 项目实际 target net8.0，但 SDK 是 10.0 预览

---

## 🟠 HIGH (性能/架构)

### H1. VersionService.RollbackAsync - 三次分步DB查询
- 文件: `Editor/Core/Services/VersionService.cs:54-67`
- 分别查 targetVersion、template、maxVersion，可合并为 1-2 次查询

### H2. CanvasEngine 渲染器注册重复但不完整
- 文件: `Editor/frontend/src/engine/CanvasEngine.js:44` vs `engine/index.js`
- CanvasEngine 注册了所有 24 个渲染器 (完整)
- 但 `engine/index.js` 只导出了 11 个 (不完整)
- 缺少: Checkbox, Container, Barcode, Hyperlink, Icon, Chart, Footer, Header, Divider, Signature, Watermark, PageNumber, Repeat 共 13 个

### H3. Mixed serialization libraries
- Contracts 用 Newtonsoft.Json，Middleware 用 System.Text.Json
- 两套序列化器共存，增加内存开销和潜在的行为不一致

### H4. PdfRenderService.RenderToImageAsync 实现有误
- 文件: `Editor/Core/Services/PdfRenderService.cs:32-45`
- 返回 PDF 字节的 Base64，而非真正的图片
- 方法名误导，且 PDF 不能直接当图片显示

### H5. Auth store 动态导入性能开销
- 文件: `Editor/frontend/src/stores/auth.js:18,34,47`
- login/refresh/logout 每次调用 `await import('@/api/auth')` 
- 应改为顶层静态导入

### H6. Template store 同样使用动态导入
- 文件: `Editor/frontend/src/stores/template.js:48,68,83,90,102`
- fetchTemplates/createTemplate/updateTemplate/deleteTemplate 都使用动态 import

---

## 🟡 MEDIUM (代码质量/维护性)

### M1. MainViewModel 构造器过载 (12个依赖)
- 文件: `Generators/ReportDataMaker/ViewModels/MainViewModel.cs:41-52`
- 12 个构造参数，违反单一职责
- 建议: 按功能拆分为 facade 或 service aggregator

### M2. PdfElementRenderer 重复的方法签名
- 文件: `Generators/ReportDataMaker/Services/PdfExport/PdfElementRenderer.cs`
- 两个 `RenderElement` 重载 (IContainer vs SKCanvas)，switch-case 逻辑几乎重复

### M3. ContextService 不可测试
- 文件: `Editor/Core/Services/ContextService.cs`
- 直接使用 `DateTime.Now`、`Environment.UserName`，无法 mock
- 建议: 注入 `TimeProvider` ( .NET 8 已内置)

### M4. 前端 elements.js clone() 实现低效
- 文件: `Editor/frontend/src/models/elements.js:55-58`
- `JSON.parse(JSON.stringify(this))` 丢失原型链和 getter
- ID 生成使用 `Date.now()` + 计数器，快速循环可能碰撞

### M5. 前端 router 使用 Vite 不支持的 webpack 魔法注释
- 文件: `Editor/frontend/src/router/index.js:4-9`
- `webpackPrefetch`、`webpackChunkName` 在 Vite 下无作用

### M6. Program.cs 在非开发环境也执行 SeedData
- 文件: `Editor/Server/Program.cs:96`
- `TemplateSeedData.SeedTemplates(db)` 每次启动都执行
- 建议: 加环境判断或幂等检查

### M7. TemplateService 缺少分页参数验证
- 文件: `Editor/Core/Services/TemplateService.cs:21`
- Page/PageSize 未校验，可能为 0 或负数

### M8. WPF ViewModels 大量使用 ServiceLocator 反模式
- 多个 ViewModel 直接通过静态 ServiceLocator 获取服务
- 应通过构造器注入

---

## 🟢 LOW (小改进)

### L1. pdfGenerator.js 弹窗可能被浏览器拦截
- 文件: `Editor/frontend/src/utils/pdfGenerator.js:48`
- `window.open('', '_blank')` 非用户触发时可能被拦截

### L2. Nginx 配置过于简陋
- 文件: `deploy/nginx/nginx.conf`
- 无静态文件服务、无 gzip、无 HTTPS、无缓存配置

### L3. Docker Compose 缺少数据库服务
- 文件: `deploy/docker-compose.yml`
- SQLite 适合开发，生产需外部 SQL Server

### L4. QuestPDF License 每次调用都设置
- 文件: `Editor/Core/Services/PdfTemplateRenderer.cs:21` 和 `Generators/.../PdfExportService.cs:16`
- `QuestPDF.Settings.License = LicenseType.Community` 只需设置一次

### L5. ApiLoggingMiddleware 捕获异常后又抛出
- 文件: `Editor/Server/Middleware/ApiLoggingMiddleware.cs:35-41`
- 和外层 GlobalExceptionMiddleware 重复捕获

### L6. Login 类型未导入
- 文件: `Editor/Server/Controllers/AuthController.cs:22`
- `LoginRequest` 类型未 using，可能编译错误

### L7. engine/index.js 导出不完整
- 文件: `Editor/frontend/src/engine/index.js`
- 只导出 11/24 个渲染器

---

## 📊 统计
- CRITICAL: 3 项
- HIGH: 6 项
- MEDIUM: 8 项
- LOW: 7 项
- 总计: 24 项优化点

## 覆盖范围
- Editor/Server (C#): C1, C2, H1, H4, M3, M6, M7, L4, L5, L6
- Editor/Core (C#): C1, H4, M3
- Editor Frontend (JS/Vue): H2, H5, H6, M4, M5, L1, L7
- Generators WPF (C#): M1, M2, M8, L4
- Contracts: C1, H3
- Deploy/Infra: C3, L2, L3

