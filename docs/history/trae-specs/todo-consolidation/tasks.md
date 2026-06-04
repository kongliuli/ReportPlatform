# Tasks

## Wave 0: 更新 Phase 2 spec 偏差

- [x] Task 0.1: 更新 I2 相关任务描述
  - [x] 0.1.1: 更新 Phase 2 tasks.md 中 Task 3.4，将"替换 switch-case"改为"替换硬编码 Factory 模式"
  - [x] 0.1.2: 更新 Phase 2 checklist.md 中对应检查项
  - [x] 0.1.3: 更新 Phase 2 spec.md 中 I2 场景描述

## Wave 1: O3 消除双重模型（沿用 Phase 2 原有任务，无偏差）

- [ ] Task 1.1: 分析 External*Element 独有属性，确定迁移映射
- [ ] Task 1.2: 将独有属性合并到 Contracts 模型
- [ ] Task 1.3: 重写 ReportExternalElementConverter
- [ ] Task 1.4: 迁移 Services 层引用
- [ ] Task 1.5: 迁移 ViewModels 层引用
- [ ] Task 1.6: 迁移 Views 层绑定
- [ ] Task 1.7: 删除桥接模型文件
- [ ] Task 1.8: O3 编译验证

## Wave 2: H3 ReportDataMaker.Core 类库提取（沿用 Phase 2 原有任务，无偏差）

- [ ] Task 2.1: 创建 ReportDataMaker.Core 项目
- [ ] Task 2.2: 迁移 Services 到 Core
- [ ] Task 2.3: 迁移 Infrastructure 到 Core
- [ ] Task 2.4: 迁移 Converters 到 Core
- [ ] Task 2.5: 更新 WPF 壳项目引用
- [ ] Task 2.6: H3 编译验证

## Wave 3: I2 适配器注册表（需更新描述）

- [ ] Task 3.1: 定义 IAdapterPlugin 接口
- [ ] Task 3.2: 实现 AdapterRegistry
- [ ] Task 3.3: 实现各适配器插件（替换现有硬编码 Factory）
- [ ] Task 3.4: 重构适配器创建逻辑（从硬编码 Factory 改为 AdapterRegistry 查找）
- [ ] Task 3.5: I2 编译验证

## Wave 4: 最终验证

- [ ] Task 4.1: 全量编译验证
- [ ] Task 4.2: 功能回归验证

# Task Dependencies

- [Task 0.1] 无前置依赖，应首先执行
- [Wave 1] 无前置依赖（但建议在 0.1 之后执行）
- [Wave 2] depends on [Wave 1]
- [Wave 3] depends on [Wave 2]
- [Wave 4] depends on [Wave 1, Wave 2, Wave 3]
