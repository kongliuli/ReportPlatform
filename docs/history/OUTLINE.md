# 项目历史归档大纲

> 归档日期：2026-06-04
> 来源：`.archive/`、`.codebuddy/`、`.sisyphus/`、`.trae/specs/`

---

## 一、版本路线图（全部已实施）

| 版本 | 主题 | 状态 | 来源 |
|------|------|------|------|
| v0.2.0 | 项目总览与路线图 | ✅ 已完成 | `archive/roadmap/v0.2.0-roadmap.md` |
| v0.3.0 | 架构统一（Contracts 融合） | ✅ 已完成 | `archive/roadmap/v0.3.0-architecture-unification-plan.md` |
| v0.4.0 | WPF MVVM 重构 | ✅ 已完成 | `archive/roadmap/v0.4.0-wpf-mvvm-refactor-plan.md` |
| v0.5.0 | Excel 适配器 | ✅ 已完成 | `archive/roadmap/v0.5.0-excel-adapter-plan.md` |
| v0.6.0 | 数据库适配器 | ✅ 已完成 | `archive/roadmap/v0.6.0-database-adapter-plan.md` |
| v0.7.0 | 报告输出增强（PDF 导出/批量/打印） | ✅ 已完成 | `archive/roadmap/v0.7-v0.8-development-guide.md` |
| v0.8.0 | 上下文适配器 | ✅ 已完成 | `archive/roadmap/v0.7-v0.8-development-guide.md` |

---

## 二、架构演进（Phase 1-2 + 清理已完成）

### Phase 1：基础加固 ✅
- 命名空间统一为 `Xinglin.ReportEditor`
- 元素类型 Attribute + 反射注册（ElementGroupRegistry）
- CommunityToolkit.Mvvm 替换手写 MVVM
- SQLite 替代 JSON 配置存储
- API 测试修复 + PDF 渲染标注
- 来源：`trae-specs/architecture-evolution-phase1/`

### Phase 2：核心重构 ✅
- O3 消除双重模型（删除 External*Element 桥接模型，统一到 Contracts）
- H3 提取 ReportDataMaker.Core 类库（分离非 UI 逻辑）
- I2 适配器工厂注册表（IAdapterPlugin/AdapterRegistry 替代硬编码 Factory）
- 来源：`trae-specs/architecture-evolution-phase2/`

### 架构清理 ✅
- 删除旧 Factory 体系（AdapterServices/ExcelAdapterFactory/DatabaseAdapterFactory/ContextAdapterFactory）
- 删除过时 MVVM 基类（ViewModelBase/RelayCommand/AsyncRelayCommand）
- 修复断裂的 PrintPreviewCommand XAML 绑定
- 清理 ReportDocumentPaginator 半成品代码
- 来源：`trae-specs/architecture-cleanup-and-print-preview/`

### Phase 3-4：待实施
- Phase 3 模块整合（5 任务）：Editor.Core Stub→真实实现、共享 PDF 渲染层、IDataAdapter 统一接口、前端 CanvasEngine 完善、API 适配器
- Phase 4 远期演进（3 任务）：插件系统、多租户、性能优化
- 来源：`sisyphus/plans/architecture-evolution.md`

---

## 三、结构性风险（R-001 ~ R-004）

| 风险 | 描述 | 优先级 | 状态 | 来源 |
|------|------|--------|------|------|
| R-001 | 双重模型 `new` 隐藏（ReportExternalElementBase 隐藏 Contracts 14 个属性） | P1 | ✅ 已修复（O3 消除双重模型） | `archive/R-001-dual-model-new-hiding.md` |
| R-002 | IDataAdapter 接口缺失（三种适配器无统一接口） | P1 | ⚠️ 部分修复（IAdapterPlugin 已定义，但 IDataAdapter 在 Contracts 层仍缺失） | `archive/R-002-missing-adapter-interface.md` |
| R-003 | PDF 渲染双重实现（Editor.Core + Generators 各自独立） | P2 | ❌ 未修复 | `archive/R-003-dual-pdf-rendering.md` |
| R-004 | 扩展性瓶颈（MainViewModel God Object + 静态注册表） | P2 | ⚠️ 部分修复（AdapterRegistry 替代 switch-case，MainViewModel 仍较重） | `archive/R-004-extensibility-bottleneck.md` |

---

## 四、代码质量修复（全部已完成）

### 第一轮：21 个后端缺陷
- 来源：`codebuddy/plans/gitignore-supplement-and-code-defect-analysis_a451865d.md`
- 安全 S1-S8、架构 A1-A6、代码质量 B1-B8
- .gitignore 补充 6 条规则

### 第二轮：9 项修复
- 来源：`codebuddy/plans/remaining-code-quality-fixes_ea38535e.md`
- CACHE_SIZE Bug、WPF net10.0→net8.0、Nginx 反向代理、14 个空 catch 块、LoginView 开放重定向、废弃视图删除、LangVersion 12.0、Newtonsoft.Json 13.0.4、Docker 多阶段构建、ConnectionPoolManager SHA256 键

### 五个 UI/逻辑问题修复
- 来源：`archive/five-issues.md`
- A-下拉去重、B-日期/性别启发式、C-未配置字段隔离、D-面板拆分、E-滚轮传播、F-契约文档

---

## 五、仍有价值的待解决问题

### 高优先级
1. **IDataAdapter 统一接口**（R-002 延续）：Contracts 层仍缺少 IDataAdapter 接口
2. **SQL 注入风险**：SqlBuilder RawSQL 模式存在注入风险
3. **JWT 密钥硬编码**：docker-compose 中弱默认密钥

### 中优先级
4. **PDF 渲染双重实现**（R-003）：Editor.Core 和 Generators 各自独立实现，新增元素需双重维护
5. **前端 CanvasEngine 渲染器注册不完整**：13/24 元素类型缺失渲染器
6. **MainViewModel 仍较重**（R-004 延续）：虽已引入 AdapterRegistry，但职责仍多

### 低优先级
7. **QuestPDF Canvas API 已弃用**：应迁移到 SkiaSharp + .Svg() API
8. **ReportDocumentPaginator 占位实现**：打印预览功能仍为占位
9. **PdfElementRenderer nullable 警告**：19 个 CS8604 警告待修复

---

## 六、参考文档索引

| 文档 | 描述 | 位置 |
|------|------|------|
| 项目概览 | 最全面的项目概览（475 行） | `archive/260513.md` |
| 架构扩展评估 | 142 个类型的最终态分类 | `archive/Architecture_Extension_FinalState.md` |
| 优化分析 | 24 项优化点（CRITICAL→LOW） | `archive/drafts/optimization-analysis.md` |
| 契约层 Wiki | Contracts 枚举/元素/模板/适配器 | `archive/contracts/wiki-v1.0.md` |
| Editor Wiki | Editor.Core + Editor.Server | `archive/editor/wiki-v1.0.md` |
| Generators Wiki | ReportDataMaker 模块文档 | `archive/generators/wiki-v1.0.md` |
| 适配器组件设计 | IDataAdapter/Excel/Database/Context 设计 | `archive/generators/adapter-components-design.md` |
| 元素适配分组 | 四分体系 + Context 适配器设计 | `archive/contracts/element-adaptation-groups.md` |
| 契约版本历史 | v2.2.0 → v2.3.0 变更记录 | `archive/contracts/changelog.md` |
| 架构全景图 | 模块状态 + 依赖拓扑 | `sisyphus/plans/architecture-panorama.md` |
