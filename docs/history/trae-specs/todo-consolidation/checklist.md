# 待办事项汇总与代码比对检查清单

## 代码质量修复计划验证

- [x] CACHE_SIZE bug 已修复（文件重构为 templateCache.js）
- [x] WPF 目标框架为 net8.0-windows
- [ ] Nginx /assets/ 反向代理配置已生效（无法验证，文件不在仓库中）
- [x] 空 catch 块已添加 Debug.WriteLine 日志
- [x] LoginView 开放重定向已添加路径校验
- [x] VersionsView.vue 和 SettingsView.vue 已删除
- [x] Directory.Build.props LangVersion 为 12.0
- [x] Newtonsoft.Json 版本为 13.0.4
- [ ] Docker 多阶段构建已生效（无法验证，文件不在仓库中）
- [x] ConnectionPoolManager 使用 SHA256 替代 GetHashCode

## Phase 1 架构演进验证

- [x] 命名空间统一为 Xinglin.ReportEditor
- [x] ElementGroupRegistry 使用反射注册
- [x] CommunityToolkit.Mvvm 替换手写 MVVM
- [x] SQLite 替代 JSON 配置存储
- [x] API 测试修复
- [x] PDF 渲染标注

## Phase 2 架构演进 — 代码状态比对

- [x] O3：ExternalExtendedElements.cs 仍存在（与计划一致，未开始）
- [x] O3：ExternalTemplateModels.cs 仍存在（与计划一致，未开始）
- [x] O3：ReportExternalElementConverter.cs 仍存在（与计划一致，未开始）
- [x] H3：ReportDataMaker.Core 目录不存在（与计划一致，未开始）
- [x] I2：IAdapterPlugin 接口不存在（与计划一致，未开始）
- [x] I2：AdapterRegistry 类不存在（与计划一致，未开始）
- [x] I2 偏差已识别：当前使用硬编码 Factory 模式而非 switch-case

## Phase 2 Spec 更新

- [x] Phase 2 tasks.md 中 I2 任务描述已更新（从 switch-case 改为硬编码 Factory）
- [x] Phase 2 checklist.md 中 I2 检查项已更新
- [x] Phase 2 spec.md 中 I2 场景描述已更新
