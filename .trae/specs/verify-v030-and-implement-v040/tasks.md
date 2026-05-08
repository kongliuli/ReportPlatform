# Tasks

## Phase 1: v0.3.0 修复

- [x] Task 1: 修复 ReportDataMaker 模型层与 Contracts 的冲突
  - [x] 1.1: 删除 ExternalTemplateModels.cs 中重复定义的 ExternalElementBase 类，改为引用 Contracts 中的定义
  - [x] 1.2: 修改 ExternalExtendedElements.cs 中所有元素类，继承自 Contracts.Models.Elements.ExternalElementBase
  - [x] 1.3: 修改 ExternalTemplateModels.cs 中 ExternalTemplateDefinition 的 Elements 属性类型为 Contracts 中的类型
  - [x] 1.4: 修改 ElementBase.cs，删除与 Contracts 重复的定义，统一引用
  - [x] 1.5: 修改 TextElement.cs、TableElement.cs 等本地模型，统一引用 Contracts 或删除重复定义
  - [x] 1.6: 修改 LabelInputBoxElement.cs，迁移为 Contracts 中的 TextElement 或保留为兼容别名
  - [x] 1.7: 修改 IDataAdapter.cs，与 Contracts 中的 IDataAdapter 对齐
  - [x] 1.8: 修改 DbAdapterConfig.cs，继承 Contracts 中的 AdapterConfigBase

- [x] Task 2: 修复 Services 层引用
  - [x] 2.1: 修改 ExternalElementConverter.cs，使用 Contracts 中的 ElementJsonConverter
  - [x] 2.2: 修改 JsonTemplateLoader.cs，使用 Contracts 中的 TemplateSerializer
  - [x] 2.3: 修改 TemplateLoaderService.cs，引用 Contracts 模型
  - [x] 2.4: 修改 DataBindingService.cs，引用 Contracts 模型
  - [x] 2.5: 修改 DataExportService.cs，引用 Contracts 模型

- [x] Task 3: 修复 ViewModels 层引用
  - [x] 3.1: 修改 MainViewModel.cs，引用 Contracts 模型
  - [x] 3.2: 修改 DataEntryViewModel.cs，引用 Contracts 模型
  - [x] 3.3: 修改所有元素 ViewModel，引用 Contracts 模型

- [x] Task 4: 验证 v0.3.0 修复
  - [x] 4.1: 确认无重复类型定义
  - [x] 4.2: 确认所有命名空间引用一致
  - [x] 4.3: 确认 Contracts 是唯一的模型定义来源

## Phase 2: v0.4.0 MVVM 基础设施

- [x] Task 5: 添加 NuGet 依赖和项目配置
  - [x] 5.1: 在 ReportDataMaker.csproj 中添加 HandyControl 3.5.1
  - [x] 5.2: 在 ReportDataMaker.csproj 中添加 Microsoft.Extensions.DependencyInjection 8.0.1

- [x] Task 6: 创建 MVVM 基础设施
  - [x] 6.1: 创建 Infrastructure/ViewModelBase.cs
  - [x] 6.2: 创建 Infrastructure/RelayCommand.cs
  - [x] 6.3: 创建 Infrastructure/AsyncRelayCommand.cs
  - [x] 6.4: 创建 Infrastructure/IDialogService.cs
  - [x] 6.5: 创建 Infrastructure/DialogService.cs
  - [x] 6.6: 创建 Infrastructure/ServiceLocator.cs

## Phase 3: v0.4.0 服务层

- [x] Task 7: 创建服务接口和实现
  - [x] 7.1: 创建 Services/ITemplateLoaderService.cs 接口
  - [x] 7.2: 创建 Services/TemplateLoaderService.cs 实现
  - [x] 7.3: 创建 Services/ITemplatePreviewService.cs 接口
  - [x] 7.4: 创建 Services/TemplatePreviewService.cs 实现
  - [x] 7.5: 创建 Services/IDataBindingService.cs 接口
  - [x] 7.6: 创建 Services/DataBindingService.cs 实现
  - [x] 7.7: 创建 Services/AdapterConfigStore.cs

## Phase 4: v0.4.0 ViewModel 层

- [x] Task 8: 创建 Tab 基类和核心 ViewModel
  - [x] 8.1: 创建 ViewModels/Tabs/TabViewModelBase.cs
  - [x] 8.2: 创建 ViewModels/Tabs/DataEntryTabViewModel.cs
  - [x] 8.3: 创建 ViewModels/Tabs/PreviewTabViewModel.cs
  - [x] 8.4: 创建 ViewModels/MainViewModel.cs
  - [x] 8.5: 创建 ViewModels/TemplateLoadViewModel.cs
  - [x] 8.6: 创建 ViewModels/SidePanelViewModel.cs
  - [x] 8.7: 创建 ViewModels/Dialogs/AddAdapterDialogViewModel.cs

## Phase 5: v0.4.0 View 层

- [x] Task 9: 创建主窗口和布局
  - [x] 9.1: 重写 Views/MainWindow.xaml
  - [x] 9.2: 更新 Views/MainWindow.xaml.cs
  - [x] 9.3: 创建 Converters/BoolToVisibilityConverter.cs

- [x] Task 10: 创建 Tab 页视图
  - [x] 10.1: 创建 Views/Tabs/DataEntryTab.xaml
  - [x] 10.2: 创建 Views/Tabs/PreviewTab.xaml

- [x] Task 11: 创建对话框视图
  - [ ] 11.1: 创建 Views/Dialogs/TemplateLoadDialog.xaml（待后续版本实现）
  - [ ] 11.2: 创建 Views/Dialogs/AddAdapterDialog.xaml（待后续版本实现）

## Phase 6: v0.4.0 App 配置和集成

- [x] Task 12: 配置 App.xaml 和 DI 容器
  - [x] 12.1: 更新 App.xaml（添加 HandyControl 主题资源字典）
  - [x] 12.2: 更新 App.xaml.cs（DI 容器初始化，注册所有服务和 ViewModel）
  - [x] 12.3: 添加隐式 DataTemplate 注册

## Phase 7: 验证

- [ ] Task 13: 编译和功能验证
  - [ ] 13.1: dotnet build 编译通过
  - [ ] 13.2: 启动显示 TemplateLoadDialog
  - [ ] 13.3: 从文件加载模板成功
  - [ ] 13.4: 主界面三栏布局正常显示
  - [ ] 13.5: 左面板折叠/展开正常
  - [ ] 13.6: TabControl 页签切换正常
  - [ ] 13.7: 添加适配器对话框正常
  - [ ] 13.8: ViewModel 无直接 View 引用
