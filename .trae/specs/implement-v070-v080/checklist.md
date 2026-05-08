# v0.8.0 & v0.7.0 检查清单

## v0.8.0 上下文适配器

- [x] ContextAdapterConfig 继承 AdapterConfigBase，包含 ProfileName、StaticValues、DynamicRules
- [x] ContextValueSource 枚举包含 Static/CurrentDate/CurrentTime/CurrentDateTime/CurrentUser/MachineName/Custom
- [x] ContextAdapterService.FillContext 正确遍历 ElementGroup.Context 元素
- [x] StaticValues 优先级高于 DynamicRules，DynamicRules 优先级高于内置默认规则
- [x] ResolveDynamic 按 ContextValueSource 正确解析日期时间和系统信息
- [x] ResolveBuiltIn 正确处理 Context.DateTime.Now/Date/Time/Year
- [x] 未配置的 Context 字段填充空字符串，不抛异常
- [x] ContextProfileStore 持久化到 %AppData%/ReportDataMaker/context-profiles.json
- [x] ContextProfileStore 支持 Load/Save/GetProfileNames
- [x] ContextAdapterFactory 整合 ContextAdapterService + ContextProfileStore
- [x] ContextAdapterTabViewModel 支持 Profile 选择/新建/删除
- [x] ContextAdapterTabViewModel 支持静态值和动态规则编辑
- [x] ContextAdapterTabViewModel 自动检测未配置的 Context 字段
- [x] ContextAdapterTabViewModel 提供预览功能
- [x] ContextAdapterTab.xaml 包含 Profile 选择区、静态值 DataGrid、动态规则 DataGrid、预览区、未配置提示
- [x] MainViewModel 模板加载后自动执行 Context 填充
- [x] MainViewModel 添加 EditContextCommand
- [x] MainWindow.xaml 添加"上下文"菜单项和 ContextAdapterTabViewModel DataTemplate
- [x] App.xaml.cs 注册 ContextProfileStore 和 ContextAdapterFactory

## v0.7.0 报告输出增强

- [x] QuestPDF NuGet 包已添加到 ReportDataMaker.csproj
- [x] PdfElementRenderer 支持 Text/Number/Date 元素渲染
- [x] PdfElementRenderer 支持 Image/Signature 元素渲染
- [x] PdfElementRenderer 支持 Table 元素渲染
- [x] PdfElementRenderer 支持 Barcode/QrCode 元素渲染（ZXing）
- [x] PdfElementRenderer 支持 Line/Shape/Divider 元素渲染
- [x] PdfElementRenderer 支持 Checkbox/Radio 元素渲染
- [x] PdfElementRenderer 支持 Header/Footer/PageNumber 元素渲染
- [x] PdfElementRenderer 支持 Watermark 元素渲染
- [x] PdfElementRenderer 支持 Container/Repeat/Hyperlink/Icon/Chart 元素渲染
- [x] PdfPageLayoutEngine 正确计算页面尺寸、边距和元素坐标映射
- [x] IPdfExportService 接口定义 RenderToPdf 和 BatchExportAsync
- [x] PdfExportService.RenderToPdf 输出布局与模板设计一致的 PDF
- [x] BatchExportService 支持逐行填充+渲染+保存+进度报告
- [x] BatchExportOptions 包含 OutputDir、FileNamePattern、Parallelism
- [x] ExportHistoryStore 记录导出操作（时间、文件名、路径）
- [x] ExportTabViewModel 支持单份导出、批量导出、打印预览、历史查看
- [x] ExportTab.xaml 包含单份导出区、批量导出区（含进度条）、打印按钮、历史列表
- [x] ReportDocumentPaginator 实现 DocumentPaginator，支持打印预览
- [x] 打印预览集成 WPF PrintDialog
- [x] MainViewModel 添加 ExportPdfCommand、BatchExportCommand、PrintPreviewCommand
- [x] 模板加载时自动创建 ExportTab
- [x] MainWindow.xaml 添加"导出"菜单项和 ExportTabViewModel DataTemplate
- [x] App.xaml.cs 注册 IPdfExportService、BatchExportService、ExportHistoryStore
