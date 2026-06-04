---
name: remaining-code-quality-fixes
overview: 修复项目中第二轮深度排查发现的代码质量问题，涵盖前端运行时 Bug、WPF 线程安全、空 catch 块、死代码清理、安全加固、部署配置修正和测试完善。
todos:
  - id: fix-cache-size-bug
    content: 修复 template.js store CACHE_SIZE 未定义的运行时 Bug：在 import 语句中添加 CACHE_SIZE 常量导入
    status: completed
  - id: fix-wpf-target-framework
    content: 修复 ReportDataMaker 目标框架和包版本兼容性：net10.0-windows 降级到 net8.0-windows，修复 Microsoft.Data.Sqlite/Npgsql/ProtectedData 包版本到 8.0 兼容版本
    status: completed
  - id: fix-nginx-static-files
    content: 修复 Nginx 配置：/assets/ 从静态文件改为反向代理到 editor-server，添加 /hub SignalR WebSocket 端点，添加安全头和 client_max_body_size
    status: completed
  - id: fix-empty-catch-blocks
    content: 修复 14 个空 catch 块：在 SqliteDatabaseService、CanvasRenderer、AdapterConfigStore、ContextProfileStore、ReportDocumentPaginator 中添加 Debug.WriteLine 日志记录
    status: completed
  - id: fix-login-open-redirect
    content: 修复 LoginView 开放重定向：添加 redirect 路径校验，仅允许以 '/' 开头的相对路径
    status: completed
  - id: delete-abandoned-views
    content: 删除废弃的 VersionsView.vue 和 SettingsView.vue 页面文件，清理 router 中对应路由
    status: completed
  - id: fix-build-props-and-nuget
    content: 修复构建配置：Directory.Build.props LangVersion 固定为 12.0，升级 Newtonsoft.Json 从 13.0.3 到 13.0.4
    status: completed
  - id: fix-docker-deploy
    content: 修复 Docker 部署配置：Dockerfile 改为多阶段构建，docker-compose.yml 移除弱默认 JWT 密钥，修复 depends_on 与 profiles 的冲突
    status: completed
    dependencies:
      - fix-nginx-static-files
      - fix-wpf-target-framework
  - id: fix-connection-pool-key
    content: 修复 ConnectionPoolManager 使用 connection string 本身替代 GetHashCode() 作为字典键
    status: completed
---

## 产品概述

第一轮 21 个后端缺陷已全部修复。本轮针对项目剩余问题进行第二轮修复，涵盖前端运行时 Bug、构建兼容性、部署损坏、空异常处理、安全漏洞、死代码清理、包升级和构建配置修正。

## 核心修复项

### 严重问题（运行时 Bug + 构建兼容性）

- **template.js store CACHE_SIZE 未定义**：`_addToCache` 方法引用了未导入的 `CACHE_SIZE` 常量，导致运行时 ReferenceError
- **ReportDataMaker 使用 net10.0-windows**：与其他所有项目的 net8.0 不兼容，需降级并修复依赖包版本
- **Nginx 无法访问 editor-server 容器中的静态文件**：`/assets/` 的 `root /app/wwwroot` 指向 nginx 自身文件系统，需改为反向代理

### 高优先级（空 catch 块 + 安全 + 死代码）

- **14 个空 catch 块静默吞掉异常**：分布在 SqliteDatabaseService、CanvasRenderer、AdapterConfigStore、ContextProfileStore、ReportDocumentPaginator 中
- **LoginView 开放重定向**：`router.push(redirect)` 未验证 redirect 是否为本域路径
- **废弃页面 VersionsView.vue 和 SettingsView.vue**：死代码应删除

### 中优先级（构建配置 + 包升级 + 部署）

- **Directory.Build.props LangVersion=latest**：应固定为 12.0
- **Newtonsoft.Json 13.0.3 升级到 13.0.4**：修复已知安全漏洞
- **Dockerfile 缺少多阶段构建**：期望宿主机预发布文件，应在 Docker 内完成 dotnet publish
- **docker-compose JWT 弱默认密钥 + depends_on profiles 冲突**

### 低优先级（线程安全 + 连接池）

- **ConnectionPoolManager 使用 GetHashCode() 作为字典键**：hash 冲突风险
- **SqliteDatabaseService 单例长连接线程不安全**：记录 TODO 注释