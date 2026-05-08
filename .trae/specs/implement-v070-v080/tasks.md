# v0.8.0 & v0.7.0 任务清单

> 执行顺序：v0.8.0（上下文适配器）→ v0.7.0（报告输出增强）
> 原因：上下文填充是数据准备阶段，输出是最终环节

## Phase 1: v0.8.0 上下文适配器

- [x] Task 1: 创建 ContextAdapterConfig + ContextDataSource 模型
  - [x] 1.1: 创建 `Services/ContextAdapter/ContextAdapterConfig.cs`（继承 AdapterConfigBase，含 ProfileName、StaticValues、DynamicRules）
  - [x] 1.2: 创建 `Services/ContextAdapter/ContextDataSource.cs`（DynamicContextRule、ContextValueSource 枚举）

- [x] Task 2: 实现 ContextAdapterService 核心填充逻辑
  - [x] 2.1: 创建 `Services/ContextAdapter/ContextAdapterService.cs`（FillContext 方法：遍历 Context 元素 → StaticValues → DynamicRules → 内置默认规则）
  - [x] 2.2: 实现 ResolveDynamic（按 ContextValueSource 解析 CurrentDate/CurrentTime/CurrentDateTime/CurrentUser/MachineName）
  - [x] 2.3: 实现 ResolveBuiltIn（内置 Context.DateTime.* 默认规则）

- [x] Task 3: 实现 ContextProfileStore 配置持久化
  - [x] 3.1: 创建 `Services/ContextAdapter/ContextProfileStore.cs`（读写 `%AppData%/ReportDataMaker/context-profiles.json`）
  - [x] 3.2: 实现 Load/Save/GetProfileNames 方法

- [x] Task 4: 实现 ContextAdapterFactory
  - [x] 4.1: 创建 `Services/ContextAdapter/ContextAdapterFactory.cs`（整合 ContextAdapterService + ContextProfileStore）
  - [x] 4.2: 实现 Fill 方法（加载 Profile → 调用 ContextAdapterService.FillContext）

- [x] Task 5: 创建 ContextAdapterTabViewModel
  - [x] 5.1: 创建 `ViewModels/Tabs/ContextAdapterTabViewModel.cs`
  - [x] 5.2: 实现 Profile 选择/新建/删除命令
  - [x] 5.3: 实现静态值编辑（ObservableCollection 键值对）
  - [x] 5.4: 实现动态规则编辑
  - [x] 5.5: 实现自动检测未配置 Context 字段
  - [x] 5.6: 实现预览功能
  - [x] 5.7: 实现保存配置命令

- [x] Task 6: 创建 ContextAdapterTab.xaml UI
  - [x] 6.1: 创建 `Views/Tabs/ContextAdapterTab.xaml` + `.xaml.cs`
  - [x] 6.2: Profile 选择区（ComboBox + 新建/删除按钮）
  - [x] 6.3: 静态值 DataGrid
  - [x] 6.4: 动态规则 DataGrid
  - [x] 6.5: 预览区域
  - [x] 6.6: 未配置字段提示区域

- [x] Task 7: 集成到 MainViewModel
  - [x] 7.1: MainViewModel 添加 ContextAdapterFactory 和 ContextProfileStore 依赖
  - [x] 7.2: LoadTemplate 方法中添加 Context 自动填充逻辑
  - [x] 7.3: 添加 EditContextCommand 命令
  - [x] 7.4: MainWindow.xaml 添加"上下文"菜单项
  - [x] 7.5: MainWindow.xaml 添加 ContextAdapterTabViewModel DataTemplate
  - [x] 7.6: App.xaml.cs 注册 ContextProfileStore 和 ContextAdapterFactory

## Phase 2: v0.7.0 报告输出增强

- [x] Task 8: 添加 QuestPDF NuGet 依赖
  - [x] 8.1: ReportDataMaker.csproj 添加 QuestPDF 包引用

- [x] Task 9: 实现 PdfElementRenderer 元素渲染映射
  - [x] 9.1: 创建 `Services/PdfExport/PdfElementRenderer.cs`
  - [x] 9.2: 实现 Text/Number/Date 元素渲染（文本块+字体+颜色+对齐）
  - [x] 9.3: 实现 Image/Signature 元素渲染（图片嵌入）
  - [x] 9.4: 实现 Table 元素渲染（表格+边框）
  - [x] 9.5: 实现 Barcode/QrCode 元素渲染（ZXing 生成图片嵌入）
  - [x] 9.6: 实现 Line/Shape/Divider 元素渲染（线条+图形）
  - [x] 9.7: 实现 Checkbox/Radio 元素渲染（☑/☐/◉/○ 符号）
  - [x] 9.8: 实现 Header/Footer/PageNumber 元素渲染
  - [x] 9.9: 实现 Watermark 元素渲染（半透明水印层）
  - [x] 9.10: 实现 Container/Repeat/Hyperlink/Icon/Chart 元素渲染

- [x] Task 10: 实现 PdfPageLayoutEngine 页面布局引擎
  - [x] 10.1: 创建 `Services/PdfExport/PdfPageLayoutEngine.cs`
  - [x] 10.2: 实现页面尺寸、边距、方向的布局计算
  - [x] 10.3: 实现元素坐标到 PDF 页面坐标的映射

- [x] Task 11: 实现 PdfExportService 单份导出
  - [x] 11.1: 创建 `Services/PdfExport/IPdfExportService.cs` 接口
  - [x] 11.2: 创建 `Services/PdfExport/PdfExportService.cs`
  - [x] 11.3: 实现 RenderToPdf 方法（整合 LayoutEngine + ElementRenderer）

- [x] Task 12: 实现 BatchExportService 批量导出
  - [x] 12.1: 创建 `Services/PdfExport/BatchExportService.cs`
  - [x] 12.2: 实现 ExportAsync 方法（逐行填充+渲染+保存+进度报告）
  - [x] 12.3: 实现 BatchExportResult 和 BatchExportOptions 模型

- [x] Task 13: 实现 ExportHistoryStore 导出历史
  - [x] 13.1: 创建 `Services/PdfExport/ExportHistoryStore.cs`
  - [x] 13.2: 实现记录添加和查询方法

- [x] Task 14: 创建 ExportTabViewModel
  - [x] 14.1: 创建 `ViewModels/Tabs/ExportTabViewModel.cs`
  - [x] 14.2: 实现单份导出命令（预览→导出→选择路径）
  - [x] 14.3: 实现批量导出命令（数据源选择→命名规则→输出目录→开始）
  - [x] 14.4: 实现进度报告（进度条+当前/总数）
  - [x] 14.5: 实现打印预览命令
  - [x] 14.6: 实现导出历史查看

- [x] Task 15: 创建 ExportTab.xaml UI
  - [x] 15.1: 创建 `Views/Tabs/ExportTab.xaml` + `.xaml.cs`
  - [x] 15.2: 单份导出区域
  - [x] 15.3: 批量导出区域（数据源选择+命名规则+输出目录+进度条）
  - [x] 15.4: 打印按钮区域
  - [x] 15.5: 导出历史列表区域

- [x] Task 16: 实现打印预览
  - [x] 16.1: 创建 `Services/PdfExport/ReportDocumentPaginator.cs`（DocumentPaginator 子类）
  - [x] 16.2: 实现 PDF 页面到 WPF Visual 的转换
  - [x] 16.3: 集成 WPF PrintDialog

- [x] Task 17: 集成到 MainViewModel + 菜单
  - [x] 17.1: MainViewModel 添加 IPdfExportService、BatchExportService、ExportHistoryStore 依赖
  - [x] 17.2: 添加 ExportPdfCommand、BatchExportCommand、PrintPreviewCommand 命令
  - [x] 17.3: 模板加载时自动创建 ExportTab
  - [x] 17.4: MainWindow.xaml 添加"导出"菜单项
  - [x] 17.5: MainWindow.xaml 添加 ExportTabViewModel DataTemplate
  - [x] 17.6: App.xaml.cs 注册 IPdfExportService、BatchExportService、ExportHistoryStore

## Task Dependencies
- Task 2 depends on Task 1
- Task 4 depends on Task 2, Task 3
- Task 5 depends on Task 4
- Task 6 depends on Task 5
- Task 7 depends on Task 5, Task 6
- Task 9 depends on Task 8
- Task 10 depends on Task 8
- Task 11 depends on Task 9, Task 10
- Task 12 depends on Task 11
- Task 14 depends on Task 11, Task 12, Task 13
- Task 15 depends on Task 14
- Task 16 depends on Task 11
- Task 17 depends on Task 14, Task 15, Task 16
- Phase 2 (Task 8-17) starts after Phase 1 (Task 1-7) completes
