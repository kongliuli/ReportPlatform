# 架构演进 Phase 1 检查清单

## Wave 0: 编译基线

- [x] `dotnet build ReportPlatform.sln` 当前状态已记录
- [x] csproj 引用路径问题已修复（如有）

## Wave 1: 命名空间统一

- [x] Editor/Core 下所有 .cs 文件 namespace 为 `Xinglin.ReportEditor.Core`
- [x] Editor/Server 下所有 .cs 文件 namespace 为 `Xinglin.ReportEditor.Server`
- [x] Editor/Core csproj 的 AssemblyName 和 RootNamespace 已更新
- [x] Editor/Server csproj 的 AssemblyName 和 RootNamespace 已更新
- [x] `using Xinglin.WebReportEditor` 在 src/ 目录下零匹配
- [x] 命名空间变更后 `dotnet build ReportPlatform.sln` 编译通过

## Wave 2: E2 元素类型 Attribute + 反射注册

- [x] `ElementAdaptationGroupAttribute.cs` 存在于 `src/Contracts/Registry/`
- [x] Attribute 类允许标注在 class 上，AllowMultiple = false
- [x] 23 种元素类型均标注了 `[ElementAdaptationGroup(ElementAdaptationGroup.Xxx)]`
- [x] ElementGroupRegistry 不再包含手工 RegisterGroup 调用
- [x] ElementGroupRegistry 使用反射自动发现带 Attribute 的类型
- [x] `ElementGroupRegistry.GetGroup(typeof(TextElement))` 返回 `ElementAdaptationGroup.Form`
- [x] `ElementGroupRegistry.GetGroup(typeof(LineElement))` 返回 `ElementAdaptationGroup.Basic`
- [x] `ElementGroupRegistry.GetGroup(typeof(TableElement))` 返回 `ElementAdaptationGroup.Data`
- [x] `ElementGroupRegistry.GetGroup(typeof(ImageElement))` 返回 `ElementAdaptationGroup.Advanced`
- [x] `ElementGroupRegistry.GetAllElementTypes()` 返回 23 种类型
- [x] 公共 API 签名（GetGroup/GetElementsInGroup/GetAllElementTypes/GetAllGroups）未变
- [x] `dotnet build src/Contracts/Xinglin.WebReportEditor.Contracts.csproj` 编译通过

## Wave 3: M3 CommunityToolkit.Mvvm

- [x] `CommunityToolkit.Mvvm` NuGet 包已添加到 ReportDataMaker.csproj
- [x] 所有 ViewModel 类继承 `ObservableObject` 而非 `ViewModelBase`
- [x] ViewModel 属性使用 `[ObservableProperty]` 标注
- [x] 命令使用 `[RelayCommand]` 标注生成
- [x] `Infrastructure/ViewModelBase.cs` 已不再被引用（文件保留但不再使用）
- [x] `Infrastructure/RelayCommand.cs` 已不再被引用（文件保留但不再使用）
- [x] `Infrastructure/AsyncRelayCommand.cs` 已不再被引用（文件保留但不再使用）
- [x] `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过

## Wave 4: N3 SQLite 本地配置库

- [x] `Microsoft.Data.Sqlite` NuGet 包已存在于 ReportDataMaker.csproj
- [x] `SqliteDatabaseService.cs` 存在于 `Services/` 目录
- [x] SQLite 数据库文件路径为 `%AppData%/ReportDataMaker/config.db`
- [x] SQLite 启用 WAL 模式
- [x] adapter_configs 表结构正确（id, template_name, config_json, updated_at）
- [x] context_profiles 表结构正确（id, profile_name, config_json, updated_at）
- [x] AdapterConfigStore.Load 从 SQLite 读取
- [x] AdapterConfigStore.Save 写入 SQLite
- [x] AdapterConfigStore 公共 API 签名未变
- [x] ContextProfileStore.Load/Save/GetProfileNames 从 SQLite 操作
- [x] ContextProfileStore 公共 API 签名未变
- [x] JSON → SQLite 自动迁移逻辑存在
- [x] 迁移后原 JSON 文件保留但不再被读取
- [x] `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj` 编译通过

## Wave 5: API 测试修复 + PDF 标注

- [x] Editor.Server.Tests 编译通过（`dotnet build tests/Editor.Server.Tests/`）
- [x] Editor.Server.Tests 可运行（`dotnet test tests/Editor.Server.Tests/`）
- [x] PdfTemplateRenderer.cs 有 `[Obsolete]` 标注
- [x] IPdfSharpTemplateRenderer.cs 有 `[Obsolete]` 标注
- [x] PdfRenderService.cs 中使用 Obsolete 接口处有 `#pragma warning disable CS0618`

## Wave 6: 最终验证

- [x] `dotnet build ReportPlatform.sln` 零错误（12 个原有 nullable 警告，非本次引入）
- [x] ElementGroupRegistry 对所有 23 种元素类型返回正确分组
- [x] AdapterConfigStore Load/Save 功能正常
- [x] ContextProfileStore Load/Save/GetProfileNames 功能正常
- [x] 无 `Xinglin.WebReportEditor` 命名空间残留
- [x] 无手写 ViewModelBase/RelayCommand/AsyncRelayCommand 引用残留
