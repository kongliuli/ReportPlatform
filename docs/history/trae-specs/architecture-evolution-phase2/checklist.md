# 架构演进 Phase 2 检查清单

## Wave 1: O3 消除双重模型

- [x] ReportExternalElementBase 的独有属性已迁移到 Contracts ExternalElementBase（Shadow, LabelWidth, DefaultValue, Options 已存在；ElementType 使用 [ElementType] attribute）
- [x] 各 External*Element 的独有属性已迁移到对应 Contracts 元素类型（Contracts 已包含所有属性）
- [x] 属性类型与 Contracts 风格一致（nullable 引用类型）
- [x] `Models/ExternalExtendedElements.cs` 已删除
- [x] `Models/ExternalTemplateModels.cs` 已删除
- [x] `Infrastructure/ReportExternalElementConverter.cs` 已删除
- [x] ReportDataMaker 中无 `ExternalTextElement`/`ExternalNumberElement` 等类型引用
- [x] ReportDataMaker 中无 `ExternalTemplateDefinition` 类型引用
- [x] ReportDataMaker 中无 `ReportExternalElementBase` 类型引用
- [x] 模板加载使用 Contracts TemplateSerializer.Deserialize()
- [x] 页面设置通过 TemplateDefinition.PageSettings 子对象访问
- [x] `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过

## Wave 2: H3 ReportDataMaker.Core 类库提取

- [x] `src/Generators/ReportDataMaker.Core/ReportDataMaker.Core.csproj` 存在
- [x] Core 项目目标框架为 `net8.0`
- [x] Core 项目引用了 Contracts 项目
- [x] Core 项目包含 Services/ 目录（所有适配器服务）
- [x] Core 项目包含 Infrastructure/ 目录（FileLogger, IDialogService, ViewModelBase）
- [x] WPF 壳项目引用了 Core 项目
- [x] WPF 壳项目仅包含 Views/、ViewModels/、App.xaml、Converters/（WPF 特定）、Services/（WPF 特定）、Infrastructure/（WPF 特定）
- [x] ReportPlatform.sln 包含 Core 项目
- [x] `dotnet build Generators/ReportDataMaker.Core/ReportDataMaker.Core.csproj` 编译通过
- [x] `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过

## Wave 3: I2 适配器工厂注册表

- [x] `IAdapterPlugin` 接口存在于 Core 项目
- [x] `AdapterRegistry` 类存在于 Core 项目
- [x] `ExcelAdapterPlugin` 实现了 IAdapterPlugin
- [x] `DatabaseAdapterPlugin` 实现了 IAdapterPlugin
- [x] `ContextAdapterPlugin` 实现了 IAdapterPlugin
- [x] `ExportAdapterPlugin` 实现了 IAdapterPlugin
- [x] 各 Tab ViewModel 不再直接 `new XxxAdapterFactory()`（使用 AdapterRegistry 查找）
- [x] App.xaml.cs 注册了所有 IAdapterPlugin 实现
- [x] `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过

## Wave 4: 最终验证

- [x] `dotnet build ReportPlatform.sln` 零错误
- [x] 无 External*Element 类型残留
- [x] 无 ExternalTemplateDefinition 类型残留
- [x] 模板加载功能正常（使用 Contracts TemplateSerializer）
- [x] 适配器创建功能正常（通过 AdapterRegistry）
- [x] ReportDataMaker.Core 可独立编译
