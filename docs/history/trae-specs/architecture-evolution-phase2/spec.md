# 架构演进 Phase 2 — 核心重构 Spec

## Why

Phase 1 基础加固已完成（命名空间统一、E2 反射注册、M3 CommunityToolkit.Mvvm、N3 SQLite）。现在项目最大的结构性问题是 **R-001 双重模型**：ReportDataMaker 中存在一套 `External*Element` 桥接模型（376 行），它通过 `new` 关键字隐藏了 Contracts 基类的属性，引入了属性类型不一致（`string?` vs `string`、`double?` vs `double`）和序列化混乱。消除这层影子模型是后续 H3（Core 类库提取）、I2（适配器工厂注册表）、J2（独立 Rendering 项目）等不可逆决策的前置条件。

## What Changes

- **O3: 消除双重模型**：删除 `ExternalExtendedElements.cs` 和 `ExternalTemplateModels.cs`，将 ReportDataMaker 中所有对 `External*Element` 和 `ExternalTemplateDefinition` 的引用改为直接使用 Contracts 中的 `TextElement`/`NumberElement`/... 和 `TemplateDefinition`
- **H3: ReportDataMaker.Core 类库提取**：将 ReportDataMaker 中的非 UI 逻辑（Services、Models、Infrastructure）提取到独立的 `ReportDataMaker.Core` 类库项目中，WPF 壳仅保留 Views/ViewModels/App.xaml
- **I2: 适配器工厂注册表**：将适配器创建逻辑从当前硬编码 Factory 模式（ExcelAdapterFactory、DatabaseAdapterFactory、ContextAdapterFactory 在各 Tab ViewModel 中直接 `new`）改为基于 `IAdapterPlugin` 接口的注册表模式，新增适配器无需修改现有 ViewModel

## Impact

- Affected specs: Phase 1 所有变更、v0.4.0 MVVM 重构、v0.5.0/v0.6.0/v0.7.0/v0.8.0 适配器
- Affected code:
  - `src/Generators/ReportDataMaker/Models/` — 删除整个目录（2 个文件）
  - `src/Generators/ReportDataMaker/Infrastructure/ReportExternalElementConverter.cs` — 重写为使用 Contracts ElementJsonConverter
  - `src/Generators/ReportDataMaker/Services/` — 所有引用 External*Element 的服务改为引用 Contracts 模型
  - `src/Generators/ReportDataMaker/ViewModels/` — 所有引用 External*Element 的 ViewModel 改为引用 Contracts 模型
  - `src/Generators/ReportDataMaker/Views/` — XAML 绑定可能需要调整
  - `src/Generators/ReportDataMaker/ReportDataMaker.csproj` — 拆分为 Core + WPF 壳
  - 新建 `src/Generators/ReportDataMaker.Core/` 项目

## ADDED Requirements

### Requirement: O3 消除双重模型

系统 SHALL 消除 ReportDataMaker 中的 `External*Element` 桥接模型，统一使用 Contracts 中的元素类型。

#### Scenario: 删除桥接模型文件
- **WHEN** 检查 `src/Generators/ReportDataMaker/Models/` 目录
- **THEN** `ExternalExtendedElements.cs` 和 `ExternalTemplateModels.cs` 已删除

#### Scenario: ReportDataMaker 直接使用 Contracts 模型
- **WHEN** 检查 ReportDataMaker 中任何引用元素类型的代码
- **THEN** 使用 `Xinglin.ReportEditor.Contracts.Models.Elements.TextElement` 等 Contracts 类型，而非 `ExternalTextElement`

#### Scenario: 模板定义统一
- **WHEN** 检查 ReportDataMaker 中引用模板定义的代码
- **THEN** 使用 `Xinglin.ReportEditor.Contracts.Models.Template.TemplateDefinition`，而非 `ExternalTemplateDefinition`

#### Scenario: 元素转换器统一
- **WHEN** 检查 ReportDataMaker 中的 JSON 反序列化逻辑
- **THEN** 使用 Contracts 中的 `ElementJsonConverter`，而非 `ReportExternalElementConverter`

#### Scenario: 属性类型兼容
- **WHEN** ReportDataMaker 的 ViewModel/Service 访问元素属性
- **THEN** 使用 Contracts 模型的原始属性类型（`string?` 而非 `string`、`double?` 而非 `double`），通过 null 检查或默认值处理

### Requirement: H3 ReportDataMaker.Core 类库提取

系统 SHALL 将 ReportDataMaker 的非 UI 逻辑提取到独立的 `ReportDataMaker.Core` 类库项目中。

#### Scenario: Core 项目创建
- **WHEN** 检查 `src/Generators/ReportDataMaker.Core/` 目录
- **THEN** 存在 `.csproj` 文件，目标框架为 `net8.0`（非 Windows 特定）

#### Scenario: Core 包含非 UI 逻辑
- **WHEN** 检查 ReportDataMaker.Core 项目内容
- **THEN** 包含 Services/、Infrastructure/（非 WPF 相关）、Models/（如仍有）目录

#### Scenario: WPF 壳仅包含 UI
- **WHEN** 检查 ReportDataMaker 项目内容
- **THEN** 仅包含 Views/、ViewModels/、App.xaml、MainWindow.xaml 等 UI 文件

#### Scenario: Core 项目被 WPF 壳引用
- **WHEN** 检查 ReportDataMaker.csproj 的项目引用
- **THEN** 包含对 ReportDataMaker.Core 的引用

### Requirement: I2 适配器工厂注册表

系统 SHALL 使用基于接口的适配器注册表替代当前硬编码 Factory 模式的适配器创建逻辑。

#### Scenario: IAdapterPlugin 接口定义
- **WHEN** 检查 Contracts 或 Core 项目
- **THEN** 存在 `IAdapterPlugin` 接口，定义 `AdapterType Type { get; }`、`string DisplayName { get; }`、`TabViewModelBase CreateTab(...)` 等成员

#### Scenario: 适配器注册表
- **WHEN** 检查 Core 项目
- **THEN** 存在 `AdapterRegistry` 类，支持 `Register(IAdapterPlugin)` 和 `GetByType(AdapterType)` 方法

#### Scenario: 适配器创建不再使用硬编码 Factory
- **WHEN** 检查各 Tab ViewModel 的适配器创建逻辑
- **THEN** 使用 `AdapterRegistry.GetByType(selectedType).CreateTab(...)` 而非直接 `new XxxAdapterFactory()`

#### Scenario: 新增适配器无需修改现有 ViewModel
- **WHEN** 开发者添加新的适配器类型
- **THEN** 只需实现 IAdapterPlugin 并注册到 AdapterRegistry，无需修改任何现有 ViewModel

## MODIFIED Requirements

### Requirement: ReportDataMaker 项目结构
原单一 WPF 项目拆分为 Core 类库 + WPF 壳两个项目。

### Requirement: 适配器创建方式
从硬编码 Factory 模式（各 Tab ViewModel 直接 `new XxxAdapterFactory()`）改为 AdapterRegistry 查找。

## REMOVED Requirements

### Requirement: External*Element 桥接模型
**Reason**: 被 Contracts 模型统一替代，消除属性类型不一致和 `new` 关键字隐藏
**Migration**: 所有引用 External*Element 的代码改为引用对应的 Contracts 元素类型

### Requirement: ExternalTemplateDefinition 桥接模型
**Reason**: 被 Contracts TemplateDefinition 替代
**Migration**: 所有引用 ExternalTemplateDefinition 的代码改为引用 TemplateDefinition，页面设置从扁平属性改为 PageSettings 子对象

### Requirement: ReportExternalElementConverter
**Reason**: 被 Contracts ElementJsonConverter 替代
**Migration**: 模板加载时使用 Contracts 的 TemplateSerializer.Deserialize()
