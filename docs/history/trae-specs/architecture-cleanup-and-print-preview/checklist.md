# 架构清理与打印预览修复检查清单

## 死代码清理

- [x] `AdapterServices.cs` 已删除
- [x] `ExcelAdapterFactory.cs` 已删除
- [x] `DatabaseAdapterFactory.cs` 已删除
- [x] `ContextAdapterFactory.cs` 已删除
- [x] `ViewModelBase.cs` 已删除
- [x] `RelayCommand.cs` 已删除
- [x] `AsyncRelayCommand.cs` 已删除
- [x] App.xaml.cs 中无旧 Factory 的 DI 注册
- [x] 项目中无 `ExcelAdapterFactory`/`DatabaseAdapterFactory`/`ContextAdapterFactory`/`AdapterServices` 残留引用
- [x] 项目中无自定义 `ViewModelBase`/`RelayCommand`/`AsyncRelayCommand` 残留引用

## 打印预览绑定修复

- [x] MainWindow.xaml 中无 `PrintPreviewCommand` 绑定
- [x] ExportTab.xaml 中无 `PrintPreviewCommand` 绑定
- [x] ReportDocumentPaginator.cs 中无未使用的 `_pdfBytes` 字段
- [x] ReportDocumentPaginator.cs 中无对 `RenderToPdf()` 的无用调用

## 编译验证

- [x] `dotnet build ReportPlatform.sln` 零错误
- [x] XAML 中所有 Command 绑定在 ViewModel 中均有定义
