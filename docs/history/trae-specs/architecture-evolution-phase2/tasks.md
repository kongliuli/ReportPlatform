# Tasks

## Wave 1: O3 消除双重模型

- [x] Task 1.1: 分析 External*Element 独有属性，确定迁移映射
  - [x] 1.1.1: 对比 ReportExternalElementBase 与 Contracts ExternalElementBase，列出 `new` 隐藏属性和独有属性（Shadow, LabelWidth, DefaultValue, Options, ElementType）
  - [x] 1.1.2: 对比每个 External*Element 与对应 Contracts 元素，列出独有属性
  - [x] 1.1.3: 确定每个独有属性的迁移策略：加入 Contracts 模型 / 在 ViewModel 层处理 / 丢弃

- [x] Task 1.2: 将独有属性合并到 Contracts 模型
  - [x] 1.2.1: 在 ExternalElementBase 中添加 `Shadow`、`LabelWidth`、`DefaultValue`、`Options` 属性（已存在）
  - [x] 1.2.2: 在 ExternalElementBase 中添加 `ElementType` 属性（使用 [ElementType] attribute 替代）
  - [x] 1.2.3: 在各 Contracts 元素类型中补充缺失属性（如 RichText/IsRichText/StyleRef → TextElement）（已存在）
  - [x] 1.2.4: 修复属性类型不一致（已使用 Contracts nullable 风格）

- [x] Task 1.3: 重写 ReportExternalElementConverter
  - [x] 1.3.1: TemplateLoaderService 已使用 Contracts 的 TemplateSerializer.Deserialize()
  - [x] 1.3.2: 删除 `Infrastructure/ReportExternalElementConverter.cs`
  - [x] 1.3.3: ReportElementContractResolver 已随转换器一起删除

- [x] Task 1.4: 迁移 Services 层引用
  - [x] 1.4.1: Services 层已使用 TemplateDefinition（无需替换）
  - [x] 1.4.2: Services 层已使用 Contracts 元素类型（无需替换）
  - [x] 1.4.3: Services 层已使用 ExternalElementBase（无需替换）
  - [x] 1.4.4: 属性访问已兼容 Contracts nullable 风格
  - [x] 1.4.5: 页面设置已通过 PageSettings 子对象访问

- [x] Task 1.5: 迁移 ViewModels 层引用
  - [x] 1.5.1: MainTabViewModel 已使用 Contracts 模型
  - [x] 1.5.2: DataEntryTabViewModel 已使用 Contracts 模型
  - [x] 1.5.3: 其他 Tab ViewModel 已使用 Contracts 模型

- [x] Task 1.6: 迁移 Views 层绑定
  - [x] 1.6.1: XAML 中无 External*Element 绑定
  - [x] 1.6.2: 无 DataTemplate DataType 引用需修复

- [x] Task 1.7: 删除桥接模型文件
  - [x] 1.7.1: 删除 `Models/ExternalExtendedElements.cs`
  - [x] 1.7.2: 删除 `Models/ExternalTemplateModels.cs`
  - [x] 1.7.3: Models 目录已删除（为空）

- [x] Task 1.8: O3 编译验证
  - [x] 1.8.1: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过
  - [x] 1.8.2: 确认无 `ExternalTextElement`/`ExternalTemplateDefinition` 等类型残留引用

## Wave 2: H3 ReportDataMaker.Core 类库提取

- [x] Task 2.1: 创建 ReportDataMaker.Core 项目
  - [x] 2.1.1: 在 `src/Generators/ReportDataMaker.Core/` 创建 `.csproj`，目标框架 `net8.0`
  - [x] 2.1.2: 添加对 Contracts 项目的引用
  - [x] 2.1.3: 添加必要的 NuGet 包（Microsoft.Data.Sqlite, ClosedXML, QuestPDF, ZXing 等）
  - [x] 2.1.4: 在 ReportPlatform.sln 中添加项目

- [x] Task 2.2: 迁移 Services 到 Core
  - [x] 2.2.1: 移动非 WPF 的 Services 文件到 Core 项目（41 个文件）
  - [x] 2.2.2: 移动 `Services/ContextAdapter/` 到 Core
  - [x] 2.2.3: 移动 `Services/DatabaseAdapter/` 到 Core
  - [x] 2.2.4: 移动 `Services/ExcelAdapter/` 到 Core
  - [x] 2.2.5: 移动 `Services/PdfExport/` 到 Core（ReportDocumentPaginator 保留在 WPF 壳）
  - [x] 2.2.6: 移动 `Services/AdapterConfigStore.cs` 和 `SqliteDatabaseService.cs` 到 Core

- [x] Task 2.3: 迁移 Infrastructure 到 Core
  - [x] 2.3.1: 移动 `IDialogService.cs` 到 Core（DialogService 保留在 WPF 壳）
  - [x] 2.3.2: 移动 `Infrastructure/FileLogger.cs` 到 Core
  - [x] 2.3.3: ConfigProtector.cs 已在 Services/ 中，随 Services 一起迁移
  - [x] 2.3.4: FieldDataTemplateSelector.cs 保留在 WPF 壳（WPF 特定）
  - [x] 2.3.5: ViewModelBase/RelayCommand/AsyncRelayCommand 保留在 WPF 壳（使用 CommandManager）

- [x] Task 2.4: 迁移 Converters 到 Core
  - [x] 2.4.1: 无非 WPF 特定的 Converters 需要迁移
  - [x] 2.4.2: 所有 Converters 保留在 WPF 壳

- [x] Task 2.5: 更新 WPF 壳项目引用
  - [x] 2.5.1: 在 ReportDataMaker.csproj 中添加对 ReportDataMaker.Core 的项目引用
  - [x] 2.5.2: SDK 风格 csproj 自动排除已迁移文件
  - [x] 2.5.3: 命名空间引用保持不变

- [x] Task 2.6: H3 编译验证
  - [x] 2.6.1: `dotnet build Generators/ReportDataMaker.Core/ReportDataMaker.Core.csproj` 编译通过
  - [x] 2.6.2: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过
  - [x] 2.6.3: `dotnet build ReportPlatform.sln` 编译通过

## Wave 3: I2 适配器工厂注册表

- [x] Task 3.1: 定义 IAdapterPlugin 接口
  - [x] 3.1.1: 在 Core 项目中创建 `IAdapterPlugin.cs`，定义 `AdapterType`、`DisplayName`、`CreateService()` 成员

- [x] Task 3.2: 实现 AdapterRegistry
  - [x] 3.2.1: 在 Core 项目中创建 `AdapterRegistry.cs`，实现 `Register(IAdapterPlugin)` 和 `GetByType(string)` 方法
  - [x] 3.2.2: 实现 `GetAllTypes()` 和 `GetAllPlugins()` 返回所有已注册的适配器

- [x] Task 3.3: 实现各适配器插件
  - [x] 3.3.1: 创建 `ExcelAdapterPlugin : IAdapterPlugin`
  - [x] 3.3.2: 创建 `DatabaseAdapterPlugin : IAdapterPlugin`
  - [x] 3.3.3: 创建 `ContextAdapterPlugin : IAdapterPlugin`
  - [x] 3.3.4: 创建 `ExportAdapterPlugin : IAdapterPlugin`

- [x] Task 3.4: 重构适配器创建逻辑
  - [x] 3.4.1: 替换各 Tab ViewModel 中直接 `new XxxAdapterFactory()` 的硬编码为 AdapterRegistry 查找
  - [x] 3.4.2: 在 App.xaml.cs DI 中注册所有 IAdapterPlugin 实现
  - [x] 3.4.3: 旧 Factory 类保留（仍被 AdapterServices 引用），Tab ViewModel 已改用注册表

- [x] Task 3.5: I2 编译验证
  - [x] 3.5.1: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过
  - [x] 3.5.2: 确认各 Tab ViewModel 不再直接 `new XxxAdapterFactory()`

## Wave 4: 最终验证

- [x] Task 4.1: 全量编译验证
  - [x] 4.1.1: `dotnet build ReportPlatform.sln` 零错误

- [x] Task 4.2: 功能回归验证
  - [x] 4.2.1: 确认模板加载功能正常（使用 Contracts 模型反序列化）
  - [x] 4.2.2: 确认适配器创建功能正常（通过 AdapterRegistry）
  - [x] 4.2.3: 确认无 External*Element 类型残留
  - [x] 4.2.4: 确认 ReportDataMaker.Core 项目可独立编译

# Task Dependencies

- [Task 1.2] depends on [Task 1.1]（确定映射后才能修改 Contracts）
- [Task 1.3] depends on [Task 1.2]（Contracts 模型完善后才能替换转换器）
- [Task 1.4] depends on [Task 1.2]（Contracts 模型完善后才能迁移引用）
- [Task 1.5] depends on [Task 1.4]
- [Task 1.6] depends on [Task 1.5]
- [Task 1.7] depends on [Task 1.6]
- [Task 1.8] depends on [Task 1.7]
- [Wave 2] depends on [Wave 1]（O3 完成后才能提取 Core，因为 Core 需要使用统一的 Contracts 模型）
- [Wave 3] depends on [Wave 2]（I2 需要 Core 项目存在）
- [Wave 4] depends on [Wave 1, Wave 2, Wave 3]

# Parallelizable Work

- Wave 1 内部 Task 1.4/1.5/1.6 可部分并行（不同层修改互不依赖，但都依赖 1.2）
- Wave 2 和 Wave 3 不可并行（I2 依赖 H3 的 Core 项目）
