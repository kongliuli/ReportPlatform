# 架构演进 Phase 1 — 基础加固 Spec

## Why

项目经过 v0.3.0 ~ v0.8.0 的快速迭代，功能框架已基本搭建完成，但存在以下结构性问题：命名空间不一致（`Xinglin.ReportEditor` vs `Xinglin.WebReportEditor` 两套并存）、手写 MVVM 基础设施维护成本高、JSON 配置存储缺乏事务安全、ElementGroupRegistry 使用静态注册无法扩展、PDF 渲染逻辑在 Editor.Core 和 ReportDataMaker 中重复实现、多个规划项目目录仍为空壳。需要在进入核心重构（Phase 2）之前先完成基础加固，为后续 O3/H3/I2/J2 等不可逆决策铺路。

## What Changes

- **命名空间统一**：将 `Xinglin.WebReportEditor.Core` 和 `Xinglin.WebReportEditor.Server` 统一为 `Xinglin.ReportEditor.Core` 和 `Xinglin.ReportEditor.Server`，与 Contracts 的 `Xinglin.ReportEditor.Contracts` 保持一致
- **E2: 元素类型 Attribute + 反射注册**：为每种元素类型添加 `ElementAdaptationGroupAttribute`，替换 ElementGroupRegistry 中的静态手工注册
- **M3: 引入 CommunityToolkit.Mvvm**：替换手写 ViewModelBase/RelayCommand/AsyncRelayCommand，使用 Source Generator 减少样板代码
- **N3: SQLite 本地配置库**：替换 AdapterConfigStore 和 ContextProfileStore 的 JSON 文件存储为 SQLite，提供事务安全和并发友好
- **编译验证**：确保所有项目 `dotnet build` 通过，修复 csproj 引用路径问题
- **API 测试修复**：修复 Editor.Server.Tests 中因 DTO 属性变更导致的编译错误
- **PDF 渲染去重**：将 Editor.Core 中的 PdfTemplateRenderer 标记为过渡实现，明确 ReportDataMaker 中的 PdfExportService 为权威实现，为后续 J2（独立 Rendering 项目）做准备

## Impact

- Affected specs: v0.3.0 架构统一、v0.4.0 MVVM 重构、v0.7.0/v0.8.0 报告输出与上下文适配器
- Affected code:
  - `src/Editor/Core/` — 命名空间变更（45 个文件）
  - `src/Editor/Server/` — 命名空间变更
  - `src/Contracts/Registry/ElementGroupRegistry.cs` — 重构为反射注册
  - `src/Contracts/Models/Elements/*.cs` — 添加 Attribute 标注
  - `src/Generators/ReportDataMaker/Infrastructure/` — 删除手写 MVVM 基类
  - `src/Generators/ReportDataMaker/ViewModels/` — 迁移到 CommunityToolkit.Mvvm
  - `src/Generators/ReportDataMaker/Services/AdapterConfigStore.cs` — 重写为 SQLite
  - `src/Generators/ReportDataMaker/Services/ContextAdapter/ContextProfileStore.cs` — 重写为 SQLite
  - `tests/Editor.Server.Tests/` — 修复编译错误

## ADDED Requirements

### Requirement: 命名空间统一

系统 SHALL 使用统一的 `Xinglin.ReportEditor` 命名空间根，消除 `Xinglin.WebReportEditor` 命名空间。

#### Scenario: Editor.Core 命名空间变更
- **WHEN** 检查 Editor/Core 下所有 .cs 文件的 namespace 声明
- **THEN** 所有 `Xinglin.WebReportEditor.Core` 替换为 `Xinglin.ReportEditor.Core`

#### Scenario: Editor.Server 命名空间变更
- **WHEN** 检查 Editor/Server 下所有 .cs 文件的 namespace 声明
- **THEN** 所有 `Xinglin.WebReportEditor.Server` 替换为 `Xinglin.ReportEditor.Server`

#### Scenario: Contracts 命名空间保持不变
- **WHEN** 检查 Contracts 下所有 .cs 文件
- **THEN** 命名空间仍为 `Xinglin.ReportEditor.Contracts.*`，无需变更

#### Scenario: csproj 程序集名称更新
- **WHEN** 检查 Editor/Core 和 Editor/Server 的 csproj 文件
- **THEN** AssemblyName 和 RootNamespace 更新为新的命名空间根

### Requirement: 元素类型 Attribute + 反射注册（E2）

系统 SHALL 使用 `ElementAdaptationGroupAttribute` 标注元素类型所属适配分组，ElementGroupRegistry 在首次访问时通过反射自动发现和注册。

#### Scenario: Attribute 标注元素类型
- **WHEN** 检查任意元素类型（如 TextElement）
- **THEN** 该类上有 `[ElementAdaptationGroup(ElementAdaptationGroup.Form)]` 标注

#### Scenario: 反射自动注册
- **WHEN** 调用 `ElementGroupRegistry.GetGroup(typeof(TextElement))`
- **THEN** 返回 `ElementAdaptationGroup.Form`，无需手工在静态构造函数中注册

#### Scenario: 新增元素类型无需修改注册表
- **WHEN** 开发者添加新的元素类型并标注 Attribute
- **THEN** ElementGroupRegistry 自动识别，无需修改 Registry 代码

### Requirement: CommunityToolkit.Mvvm 替换手写 MVVM（M3）

系统 SHALL 使用 CommunityToolkit.Mvvm 替代手写的 ViewModelBase、RelayCommand 和 AsyncRelayCommand。

#### Scenario: ViewModelBase 替换
- **WHEN** 检查 ReportDataMaker 中所有 ViewModel 类
- **THEN** 它们继承 `ObservableObject` 而非 `ViewModelBase`，使用 `[ObservableProperty]` 标注自动生成属性

#### Scenario: RelayCommand 替换
- **WHEN** 检查 ReportDataMaker 中所有命令属性
- **THEN** 使用 `[RelayCommand]` 标注生成 ICommand 实现，而非手写 RelayCommand/AsyncRelayCommand

#### Scenario: 手写基础设施文件移除
- **WHEN** 检查 Infrastructure 目录
- **THEN** ViewModelBase.cs、RelayCommand.cs、AsyncRelayCommand.cs 已删除

### Requirement: SQLite 本地配置库（N3）

系统 SHALL 使用 SQLite 替代 JSON 文件存储适配器配置和上下文 Profile。

#### Scenario: AdapterConfigStore 使用 SQLite
- **WHEN** 用户保存适配器配置
- **THEN** 数据写入 `%AppData%/ReportDataMaker/config.db` SQLite 数据库

#### Scenario: ContextProfileStore 使用 SQLite
- **WHEN** 用户保存上下文 Profile
- **THEN** 数据写入同一 SQLite 数据库的 context_profiles 表

#### Scenario: 数据迁移
- **WHEN** 首次启动新版本且存在旧的 JSON 配置文件
- **THEN** 系统自动将 adapters.json 和 context-profiles.json 的数据迁移到 SQLite，迁移完成后保留原文件但不再使用

#### Scenario: 事务安全
- **WHEN** 并发读写配置
- **THEN** SQLite WAL 模式确保数据一致性，不出现损坏

### Requirement: 编译验证

系统 SHALL 确保所有项目在 `dotnet build` 后零错误零警告。

#### Scenario: 全量编译
- **WHEN** 执行 `dotnet build ReportPlatform.sln`
- **THEN** 输出 "Build succeeded"，0 error，0 warning

### Requirement: API 测试修复

系统 SHALL 修复 Editor.Server.Tests 中所有编译错误，使测试可运行。

#### Scenario: 测试编译通过
- **WHEN** 执行 `dotnet build tests/Editor.Server.Tests/`
- **THEN** 编译通过，0 error

#### Scenario: 测试可运行
- **WHEN** 执行 `dotnet test tests/Editor.Server.Tests/`
- **THEN** 所有测试可执行（不要求全部通过，但不应因编译错误无法运行）

### Requirement: PDF 渲染职责明确化

系统 SHALL 明确 Editor.Core 中的 PdfTemplateRenderer 为过渡实现，ReportDataMaker 中的 PdfExportService 为权威实现。

#### Scenario: Editor.Core PdfTemplateRenderer 标注
- **WHEN** 检查 Editor/Core/Services/PdfTemplateRenderer.cs
- **THEN** 类上有 `[Obsolete("过渡实现，后续迁移到独立 Rendering 项目")]` 标注

#### Scenario: IPdfSharpTemplateRenderer 接口标注
- **WHEN** 检查 Editor/Core/SharedInterfaces/IPdfSharpTemplateRenderer.cs
- **THEN** 接口上有 `[Obsolete("后续由 Rendering 项目的 ITemplateRenderer 替代")]` 标注

## MODIFIED Requirements

### Requirement: ElementGroupRegistry 注册机制
原静态手工注册变更为 Attribute + 反射自动注册，公共 API（GetGroup/GetElementsInGroup/GetAllElementTypes/GetAllGroups）签名不变。

### Requirement: ReportDataMaker ViewModel 基类
所有 ViewModel 从继承 ViewModelBase 改为继承 ObservableObject，属性通知从 SetProperty 改为 [ObservableProperty] Source Generator。

### Requirement: 适配器配置持久化
AdapterConfigStore 从 JSON 文件读写改为 SQLite 读写，公共 API（Load/Save）签名不变。

## REMOVED Requirements

### Requirement: 手写 MVVM 基础设施
**Reason**: 被 CommunityToolkit.Mvvm 替代
**Migration**: ViewModelBase → ObservableObject，RelayCommand → [RelayCommand]，AsyncRelayCommand → [RelayCommand]（异步方法自动生成 AsyncRelayCommand）

### Requirement: JSON 配置文件直接读写
**Reason**: 被 SQLite 替代，提供事务安全和并发友好
**Migration**: 首次启动时自动从 JSON 迁移数据到 SQLite
