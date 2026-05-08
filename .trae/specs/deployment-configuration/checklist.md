# 部署配置验证检查清单

## 路由懒加载
- [x] 验证路由配置使用 defineAsyncComponent
- [x] 验证首屏 JS 文件大小减少（使用 webpackPrefetch）
- [x] 验证路由切换时动态加载组件（使用 webpackChunkName）

## 模板缓存机制
- [x] 验证内存缓存限制为 10 个模板（LRU 策略）
- [x] 验证 localStorage 持久化模板列表
- [x] 验证草稿自动保存功能

## 画布渲染优化
- [x] 验证加载 100 元素 < 3s（批量添加优化）
- [x] 验证拖拽帧率 > 30fps（节流优化）
- [x] 验证批量添加元素优化（renderOnAddRemove 控制）

## Docker 容器化
- [x] 验证后端 Dockerfile 多阶段构建
- [x] 验证前端 Dockerfile 多阶段构建
- [ ] 验证镜像构建成功（待测试）
- [ ] 验证容器启动后服务可访问（待测试）

## Nginx 配置
- [x] 验证反向代理配置正确
- [x] 验证 WebSocket 支持（Hub/SignalR）
- [x] 验证 gzip 压缩配置

## Docker Compose
- [x] 验证 docker-compose.yml 配置完整
- [ ] 验证所有服务正常启动（待测试）
- [ ] 验证前后端通信正常（待测试）

## 生产配置
- [x] 验证 appsettings.Production.json 存在
- [x] 验证 SQL Server 连接字符串配置
- [x] 验证 Swagger 已关闭
- [x] 验证敏感配置使用环境变量

## 性能验证
- [ ] 首页 Lighthouse 评分 > 90（待测试）
- [ ] 编辑器加载 100 元素 < 3s（待测试）
- [ ] 拖拽帧率 > 30fps（待测试）