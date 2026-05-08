# 杏林 Web 报告编辑器 - 部署配置实现计划

## [x] Task 1: 实现路由懒加载
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 修改 `src/router/index.js`
  - 使用 Vue Router 的 defineAsyncComponent 实现懒加载
  - 添加 prefetch 配置
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-1.1: 验证首屏加载的 JS 文件大小减少
  - `programmatic` TR-1.2: 验证路由切换时动态加载组件
- **Notes**: 参考 Vue 3 官方文档的懒加载最佳实践

## [x] Task 2: 实现模板缓存机制
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 修改 `src/stores/template.js`
  - 实现内存缓存（最多 10 个模板）
  - 实现 localStorage 缓存模板列表
  - 实现自动保存草稿功能
- **Acceptance Criteria Addressed**: AC-2
- **Test Requirements**:
  - `programmatic` TR-2.1: 验证内存缓存大小限制为 10
  - `programmatic` TR-2.2: 验证 localStorage 持久化模板列表
  - `programmatic` TR-2.3: 验证草稿自动保存功能
- **Notes**: 使用 LRU 缓存策略

## [ ] Task 3: 优化画布渲染性能
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 修改 `src/engine/CanvasEngine.js`
  - 实现批量添加元素优化
  - 实现对象缓存机制
  - 实现 rAF 节流
  - 实现大模板虚拟化（可选）
- **Acceptance Criteria Addressed**: AC-3
- **Test Requirements**:
  - `programmatic` TR-3.1: 验证加载 100 元素 < 3s
  - `programmatic` TR-3.2: 验证拖拽帧率 > 30fps
- **Notes**: 使用 Fabric.js 的 batchRender 方法

## [ ] Task 4: 创建后端 Dockerfile
- **Priority**: P1
- **Depends On**: None
- **Description**: 
  - 创建 `docker/backend.Dockerfile`
  - 使用多阶段构建
  - 第一阶段：dotnet build
  - 第二阶段：运行时镜像
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic` TR-4.1: 验证镜像构建成功
  - `programmatic` TR-4.2: 验证容器启动后 API 可访问
- **Notes**: 使用 mcr.microsoft.com/dotnet/aspnet:8.0 作为基础镜像

## [ ] Task 5: 创建前端 Dockerfile
- **Priority**: P1
- **Depends On**: None
- **Description**: 
  - 创建 `docker/frontend.Dockerfile`
  - 第一阶段：Node.js 构建
  - 第二阶段：Nginx 运行时
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic` TR-5.1: 验证镜像构建成功
  - `programmatic` TR-5.2: 验证容器启动后前端可访问
- **Notes**: 使用 node:20-alpine 和 nginx:alpine

## [ ] Task 6: 创建 Nginx 配置
- **Priority**: P1
- **Depends On**: Task 4, Task 5
- **Description**: 
  - 创建 `docker/nginx.conf`
  - 配置反向代理到后端 API
  - 配置 WebSocket 支持
  - 配置 gzip 压缩
- **Acceptance Criteria Addressed**: AC-5
- **Test Requirements**:
  - `programmatic` TR-6.1: 验证 API 请求正确路由
  - `programmatic` TR-6.2: 验证 WebSocket 连接成功
- **Notes**: 需要配置 Connection: upgrade 和 Upgrade 头

## [ ] Task 7: 创建 docker-compose.yml
- **Priority**: P1
- **Depends On**: Task 4, Task 5, Task 6
- **Description**: 
  - 创建 `docker-compose.yml`
  - 定义 frontend、backend、nginx 服务
  - 配置网络
- **Acceptance Criteria Addressed**: AC-4, AC-5
- **Test Requirements**:
  - `programmatic` TR-7.1: 验证 docker-compose up 成功启动所有服务
  - `programmatic` TR-7.2: 验证前后端通信正常
- **Notes**: 使用 Docker 网络别名

## [x] Task 8: 创建生产配置文件
- **Priority**: P1
- **Depends On**: None
- **Description**: 
  - 创建 `Server/appsettings.Production.json`
  - 配置 SQL Server 连接字符串
  - 关闭 Swagger
  - 设置生产环境日志级别
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `human-judgement` TR-8.1: 验证配置文件格式正确
  - `human-judgement` TR-8.2: 验证敏感配置项未硬编码
- **Notes**: 使用环境变量注入敏感配置

## [ ] Task 9: 性能验证与优化
- **Priority**: P2
- **Depends On**: Task 1, Task 2, Task 3
- **Description**: 
  - 运行 Lighthouse 性能测试
  - 分析并优化性能瓶颈
- **Acceptance Criteria Addressed**: AC-1, AC-2, AC-3
- **Test Requirements**:
  - `programmatic` TR-9.1: 首页 Lighthouse 评分 > 90
  - `programmatic` TR-9.2: 编辑器加载 100 元素 < 3s
- **Notes**: 使用 Chrome DevTools 进行性能分析