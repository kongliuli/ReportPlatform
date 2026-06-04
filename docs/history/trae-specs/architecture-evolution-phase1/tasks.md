# Tasks

## Wave 0: 编译基线确认

- [x] Task 0.1: 全量编译验证
  - [x] 0.1.1: 执行 `dotnet build ReportPlatform.sln`，记录当前错误和警告
  - [x] 0.1.2: 修复 csproj 引用路径问题（如有）
  - [x] 0.1.3: 确认编译通过或记录阻塞项

## Wave 1: 命名空间统一（独立，无前置依赖）

- [x] Task 1.1: Editor.Core 命名空间变更
  - [x] 1.1.1: 将 `src/Editor/Core/` 下所有 `.cs` 文件中的 `namespace Xinglin.WebReportEditor.Core` 替换为 `namespace Xinglin.ReportEditor.Core`
  - [x] 1.1.2: 将 `src/Editor/Core/` 下所有 `.cs` 文件中的 `using Xinglin.WebReportEditor.Core` 替换为 `using Xinglin.ReportEditor.Core`
  - [x] 1.1.3: 更新 `Xinglin.WebReportEditor.Core.csproj` 的 AssemblyName 和 RootNamespace

- [x] Task 1.2: Editor.Server 命名空间变更
  - [x] 1.2.1: 将 `src/Editor/Server/` 下所有 `.cs` 文件中的 `namespace Xinglin.WebReportEditor.Server` 替换为 `namespace Xinglin.ReportEditor.Server`
  - [x] 1.2.2: 将 `src/Editor/Server/` 下所有 `.cs` 文件中的 `using Xinglin.WebReportEditor.Server` 替换为 `using Xinglin.ReportEditor.Server`
  - [x] 1.2.3: 更新 `Xinglin.WebReportEditor.Server.csproj` 的 AssemblyName 和 RootNamespace

- [x] Task 1.3: 命名空间变更后编译验证
  - [x] 1.3.1: 执行 `dotnet build ReportPlatform.sln`
  - [x] 1.3.2: 修复因命名空间变更导致的引用错误
  - [x] 1.3.3: 确认编译通过

## Wave 2: E2 元素类型 Attribute + 反射注册（独立，无前置依赖）

- [x] Task 2.1: 创建 ElementAdaptationGroupAttribute
  - [x] 2.1.1: 在 `src/Contracts/Registry/` 创建 `ElementAdaptationGroupAttribute.cs`，定义 Attribute 类
  - [x] 2.1.2: Attribute 允许标注在 class 上，AllowMultiple = false

- [x] Task 2.2: 为所有 23 种元素类型添加 Attribute 标注
  - [x] 2.2.1: Basic 组：LineElement, DividerElement, ShapeElement → `[ElementAdaptationGroup(ElementAdaptationGroup.Basic)]`
  - [x] 2.2.2: Form 组：TextElement, NumberElement, DateElement, DropdownElement, CheckboxElement, RadioElement → `[ElementAdaptationGroup(ElementAdaptationGroup.Form)]`
  - [x] 2.2.3: Data 组：TableElement, RepeatElement, ChartElement → `[ElementAdaptationGroup(ElementAdaptationGroup.Data)]`
  - [x] 2.2.4: Advanced 组：ImageElement, BarcodeElement, QrCodeElement, SignatureElement, HyperlinkElement, IconElement, ContainerElement, HeaderElement, FooterElement, PageNumberElement, WatermarkElement → `[ElementAdaptationGroup(ElementAdaptationGroup.Advanced)]`

- [x] Task 2.3: 重构 ElementGroupRegistry 为反射注册
  - [x] 2.3.1: 删除静态构造函数中的手工 RegisterGroup 调用
  - [x] 2.3.2: 实现 `EnsureInitialized()` 懒加载方法，通过 `Assembly.GetExecutingAssembly()` 扫描所有带 Attribute 的类型
  - [x] 2.3.3: GetGroup/GetElementsInGroup/GetAllElementTypes/GetAllGroups 调用前触发 EnsureInitialized
  - [x] 2.3.4: 保持公共 API 签名不变

- [x] Task 2.4: E2 编译验证
  - [x] 2.4.1: `dotnet build src/Contracts/Xinglin.WebReportEditor.Contracts.csproj`
  - [x] 2.4.2: 确认 ElementGroupRegistry.GetGroup 返回值与重构前一致

## Wave 3: M3 CommunityToolkit.Mvvm 替换（独立，无前置依赖）

- [x] Task 3.1: 添加 CommunityToolkit.Mvvm NuGet 包
  - [x] 3.1.1: 在 `ReportDataMaker.csproj` 中添加 `CommunityToolkit.Mvvm` 包引用

- [x] Task 3.2: 迁移 ViewModelBase 子类到 ObservableObject
  - [x] 3.2.1: TabViewModelBase → 继承 ObservableObject，Title/IsClosable 用 [ObservableProperty] 标注
  - [x] 3.2.2: MainTabViewModel → 继承 ObservableObject，属性用 [ObservableProperty] 标注
  - [x] 3.2.3: DataEntryTabViewModel → 继承 ObservableObject，属性用 [ObservableProperty] 标注
  - [x] 3.2.4: PreviewTabViewModel → 继承 ObservableObject
  - [x] 3.2.5: ExcelAdapterTabViewModel → 继承 ObservableObject
  - [x] 3.2.6: DatabaseAdapterTabViewModel → 继承 ObservableObject
  - [x] 3.2.7: ContextAdapterTabViewModel → 继承 ObservableObject
  - [x] 3.2.8: ExportTabViewModel → 继承 ObservableObject
  - [x] 3.2.9: MainViewModel → 继承 ObservableObject
  - [x] 3.2.10: SidePanelViewModel → 继承 ObservableObject
  - [x] 3.2.11: TemplateLoadViewModel → 继承 ObservableObject
  - [x] 3.2.12: AddAdapterDialogViewModel → 继承 ObservableObject

- [x] Task 3.3: 迁移命令到 [RelayCommand] 标注
  - [x] 3.3.1: 扫描所有 ViewModel 中的 ICommand 属性和对应的 Execute/CanExecute 方法
  - [x] 3.3.2: 将手写 `new RelayCommand(ExecuteXxx, CanXxx)` 替换为 `[RelayCommand(CanExecute = nameof(CanXxx))]` 标注 + `partial void Xxx()` 方法
  - [x] 3.3.3: 将手写 `new AsyncRelayCommand(ExecuteXxxAsync)` 替换为 `[RelayCommand]` 标注 + `partial async Task XxxAsync()` 方法

- [x] Task 3.4: 删除手写 MVVM 基础设施
  - [x] 3.4.1: 删除 `Infrastructure/ViewModelBase.cs`（保留文件但不再使用）
  - [x] 3.4.2: 删除 `Infrastructure/RelayCommand.cs`（保留文件但不再使用）
  - [x] 3.4.3: 删除 `Infrastructure/AsyncRelayCommand.cs`（保留文件但不再使用）

- [x] Task 3.5: M3 编译验证
  - [x] 3.5.1: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`
  - [x] 3.5.2: 确认所有 ViewModel 属性变更通知正常工作

## Wave 4: N3 SQLite 本地配置库（独立，无前置依赖）

- [x] Task 4.1: 添加 Microsoft.Data.Sqlite NuGet 包
  - [x] 4.1.1: 在 `ReportDataMaker.csproj` 中添加 `Microsoft.Data.Sqlite` 包引用（已存在）

- [x] Task 4.2: 创建 SQLiteDatabaseService
  - [x] 4.2.1: 创建 `Services/SqliteDatabaseService.cs`，封装 SQLite 连接管理
  - [x] 4.2.2: 实现数据库初始化（建表：adapter_configs, context_profiles）
  - [x] 4.2.3: 启用 WAL 模式
  - [x] 4.2.4: 实现通用 CRUD 方法

- [x] Task 4.3: 重写 AdapterConfigStore
  - [x] 4.3.1: 修改 `AdapterConfigStore.cs`，内部使用 SQLiteDatabaseService
  - [x] 4.3.2: Load 方法从 SQLite adapter_configs 表读取
  - [x] 4.3.3: Save 方法写入 SQLite adapter_configs 表
  - [x] 4.3.4: 保持公共 API（Load/Save）签名不变

- [x] Task 4.4: 重写 ContextProfileStore
  - [x] 4.4.1: 修改 `ContextProfileStore.cs`，内部使用 SQLiteDatabaseService
  - [x] 4.4.2: Load/Save/GetProfileNames 从 SQLite context_profiles 表操作
  - [x] 4.4.3: 保持公共 API 签名不变

- [x] Task 4.5: JSON → SQLite 数据迁移
  - [x] 4.5.1: 在 SQLiteDatabaseService 初始化时检测旧 JSON 文件
  - [x] 4.5.2: 如果 adapters.json 存在且 SQLite 表为空，自动迁移数据
  - [x] 4.5.3: 如果 context-profiles.json 存在且 SQLite 表为空，自动迁移数据
  - [x] 4.5.4: 迁移完成后保留原 JSON 文件（不删除，仅不再读取）

- [x] Task 4.6: N3 编译验证
  - [x] 4.6.1: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`
  - [x] 4.6.2: 确认 AdapterConfigStore.Load/Save 正常工作

## Wave 5: API 测试修复 + PDF 渲染标注

- [x] Task 5.1: 修复 Editor.Server.Tests 编译错误
  - [x] 5.1.1: 逐个检查 5 个测试文件，修复 DTO 属性名不匹配
  - [x] 5.1.2: 修复 Assert 条件
  - [x] 5.1.3: `dotnet build tests/Editor.Server.Tests/` 编译通过

- [x] Task 5.2: PDF 渲染过渡标注
  - [x] 5.2.1: 为 `Editor/Core/Services/PdfTemplateRenderer.cs` 添加 `[Obsolete]` 标注
  - [x] 5.2.2: 为 `Editor/Core/SharedInterfaces/IPdfSharpTemplateRenderer.cs` 添加 `[Obsolete]` 标注
  - [x] 5.2.3: 为 `Editor/Core/Services/PdfRenderService.cs` 中使用 IPdfSharpTemplateRenderer 的地方添加 `#pragma warning disable CS0618`

## Wave 6: 最终验证

- [x] Task 6.1: 全量编译验证
  - [x] 6.1.1: `dotnet build ReportPlatform.sln` 零错误
  - [x] 6.1.2: 检查无新增警告

- [x] Task 6.2: 功能回归验证
  - [x] 6.2.1: 确认 ElementGroupRegistry.GetGroup 对所有 23 种元素类型返回正确分组
  - [x] 6.2.2: 确认 AdapterConfigStore Load/Save 功能正常
  - [x] 6.2.3: 确认 ContextProfileStore Load/Save/GetProfileNames 功能正常
  - [x] 6.2.4: 确认 Editor.Server.Tests 可运行

# Task Dependencies

- [Task 1.3] depends on [Task 1.1, Task 1.2]（命名空间变更后才能验证编译）
- [Task 2.4] depends on [Task 2.1, Task 2.2, Task 2.3]
- [Task 3.5] depends on [Task 3.1, Task 3.2, Task 3.3, Task 3.4]
- [Task 4.6] depends on [Task 4.1, Task 4.2, Task 4.3, Task 4.4, Task 4.5]
- [Task 6.1] depends on [Task 0.1, Task 1.3, Task 2.4, Task 3.5, Task 4.6, Task 5.1, Task 5.2]
- [Task 6.2] depends on [Task 6.1]

# Parallelizable Work

- Wave 1（命名空间）、Wave 2（E2）、Wave 3（M3）、Wave 4（N3）**可并行执行**，互不依赖
- Wave 5 也可与 Wave 1-4 并行
- Wave 6 必须在所有其他 Wave 完成后执行
