# 待办事项汇总与代码比对 Spec

## Why

项目存在多个阶段的待办事项（Phase 2 架构演进 3 项需求、代码质量修复计划 2 项未验证项），需要汇总所有待办事项并与当前代码库进行比对，确认哪些已完成、哪些未开始、哪些可能因代码变更而需要调整。

## What Changes

- 汇总所有待办事项状态
- 比对代码确认实际完成情况
- 识别因代码变更导致的任务偏差

## 汇总结果

### 一、代码质量修复计划（remaining-code-quality-fixes）— 9/9 已完成

| # | 任务 | 计划状态 | 代码验证 |
|---|------|----------|----------|
| 1 | CACHE_SIZE 未定义 Bug | ✅ 已完成 | ✅ 已确认（文件已重构为 templateCache.js，CACHE_SIZE 本地定义） |
| 2 | WPF 目标框架 net10.0→net8.0 | ✅ 已完成 | ✅ 已确认（net8.0-windows） |
| 3 | Nginx /assets/ 反向代理 | ✅ 已完成 | ⚠️ 无法验证（仓库中无 nginx 配置文件） |
| 4 | 14 个空 catch 块添加日志 | ✅ 已完成 | ✅ 已确认（SqliteDatabaseService 等） |
| 5 | LoginView 开放重定向 | ✅ 已完成 | ✅ 已确认（三重验证：存在性+类型+startsWith('/')） |
| 6 | 删除废弃视图 | ✅ 已完成 | ✅ 已确认（VersionsView/SettingsView 不存在） |
| 7 | LangVersion 固定 12.0 | ✅ 已完成 | ✅ 已确认 |
| 8 | Newtonsoft.Json 13.0.4 | ✅ 已完成 | ✅ 已确认（3 个 csproj 均为 13.0.4） |
| 9 | Docker 多阶段构建 | ✅ 已完成 | ⚠️ 无法验证（仓库中无 Dockerfile） |
| 10 | ConnectionPoolManager key | ✅ 已完成 | ✅ 已确认（使用 SHA256 替代 GetHashCode） |

**结论**：8 项已确认在代码中落实，2 项（Nginx、Docker）因相关文件不在仓库中无法验证。

### 二、Phase 1 架构演进 — 全部已完成 ✅

6 个 Wave 全部完成，代码验证与计划一致，无偏差。

### 三、Phase 2 架构演进 — 全部未开始，代码与计划一致

| 需求项 | 目标 | 当前代码状态 | 完成度 | 与计划偏差 |
|--------|------|-------------|--------|-----------|
| **O3** 消除双重模型 | 移除 External*Element，统一到 Contracts | 桥接模型完整存在，转换器完整存在，Contracts 目标模型已就绪 | 0% | 无偏差 |
| **H3** 提取 Core 类库 | 分离非 UI 逻辑到 Core 项目 | 仅有空 Core/.gitkeep 占位目录 | ~2% | 无偏差 |
| **I2** 适配器注册表 | 注册表模式替代 switch-case | 使用硬编码 Factory 模式，IAdapterPlugin/AdapterRegistry 均不存在 | 0% | ⚠️ 有偏差（见下） |

### I2 偏差说明

Phase 2 spec 中描述 MainViewModel 使用 **switch-case** 创建适配器，但实际代码中：
- MainViewModel 本身不包含适配器创建的 switch/case 语句
- 当前使用的是 **硬编码 Factory 模式**（ExcelAdapterFactory、DatabaseAdapterFactory、ContextAdapterFactory）
- 这三个 Factory 在 App.xaml.cs 中注册为 DI 单例
- 各 Tab ViewModel 直接 `new` 各自的 Factory

这意味着 I2 的实施路径需要调整：不是从 switch-case 改为注册表，而是从硬编码 Factory 改为注册表。任务描述需要更新以反映实际代码状态。

### 四、待执行事项汇总

**需要执行的待办事项（Phase 2）**：

1. **O3 消除双重模型**（Wave 1，8 个 Task）
   - 分析独有属性 → 合并到 Contracts → 重写转换器 → 迁移 Services/ViewModels/Views → 删除桥接模型 → 编译验证

2. **H3 提取 Core 类库**（Wave 2，6 个 Task）
   - 创建 Core 项目 → 迁移 Services/Infrastructure/Converters → 更新 WPF 壳引用 → 编译验证

3. **I2 适配器注册表**（Wave 3，5 个 Task）⚠️ 需更新
   - 定义 IAdapterPlugin → 实现 AdapterRegistry → 实现各适配器插件 → 重构 Factory 模式为注册表 → 编译验证

4. **最终验证**（Wave 4，2 个 Task）

**执行顺序**：Wave 1 → Wave 2 → Wave 3 → Wave 4（串行依赖）

## ADDED Requirements

### Requirement: I2 任务描述更新

I2 的 tasks.md 和 checklist.md SHALL 更新以反映实际代码状态：当前使用硬编码 Factory 模式而非 switch-case。

#### Scenario: I2 任务描述准确
- **WHEN** 读取 Phase 2 的 tasks.md
- **THEN** Task 3.4 描述的是从硬编码 Factory 模式迁移到注册表模式，而非从 switch-case 迁移
