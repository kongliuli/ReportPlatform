# Tasks

## Wave 1: 死代码清理

- [x] Task 1.1: 删除旧 Factory 体系
  - [x] 1.1.1: 删除 `src\Generators\ReportDataMaker.Core\Services\AdapterServices.cs`
  - [x] 1.1.2: 删除 `src\Generators\ReportDataMaker.Core\Services\ExcelAdapter\ExcelAdapterFactory.cs`
  - [x] 1.1.3: 删除 `src\Generators\ReportDataMaker.Core\Services\DatabaseAdapter\DatabaseAdapterFactory.cs`
  - [x] 1.1.4: 删除 `src\Generators\ReportDataMaker.Core\Services\ContextAdapter\ContextAdapterFactory.cs`
  - [x] 1.1.5: 从 `App.xaml.cs` 中移除三个 Factory 的 DI 注册（`AddSingleton<ExcelAdapterFactory>()`、`AddSingleton<DatabaseAdapterFactory>()`、`AddSingleton<ContextAdapterFactory>()`）

- [x] Task 1.2: 删除过时 MVVM 基类
  - [x] 1.2.1: 删除 `src\Generators\ReportDataMaker.Core\Infrastructure\ViewModelBase.cs`
  - [x] 1.2.2: 删除 `src\Generators\ReportDataMaker\Infrastructure\RelayCommand.cs`
  - [x] 1.2.3: 删除 `src\Generators\ReportDataMaker\Infrastructure\AsyncRelayCommand.cs`

- [x] Task 1.3: 编译验证 Wave 1
  - [x] 1.3.1: `dotnet build ReportPlatform.sln` 零错误
  - [x] 1.3.2: 搜索确认无旧 Factory/AdapterServices/ViewModelBase/RelayCommand 残留引用

## Wave 2: 打印预览绑定修复

- [x] Task 2.1: 修复断裂的 PrintPreviewCommand 绑定
  - [x] 2.1.1: 从 `MainWindow.xaml` 中移除 `PrintPreviewCommand` 绑定的 MenuItem
  - [x] 2.1.2: 从 `ExportTab.xaml` 中移除 `PrintPreviewCommand` 绑定的 Button

- [x] Task 2.2: 清理 ReportDocumentPaginator 半成品代码
  - [x] 2.2.1: 移除 `ReportDocumentPaginator.cs` 中未使用的 `_pdfBytes` 字段及其赋值
  - [x] 2.2.2: 移除 `PrintHelper.ShowPrintPreview()` 中对 `RenderToPdf()` 的无用调用
  - [x] 2.2.3: 保留 `ReportDocumentPaginator` 的基本结构（占位渲染），但清理死代码

- [x] Task 2.3: 编译验证 Wave 2
  - [x] 2.3.1: `dotnet build ReportPlatform.sln` 零错误
  - [x] 2.3.2: 确认 XAML 中无断裂的 Command 绑定

# Task Dependencies

- [Task 1.2] 无前置依赖，可与 [Task 1.1] 并行
- [Task 1.3] depends on [Task 1.1, Task 1.2]
- [Task 2.1] 无前置依赖，可与 Wave 1 并行
- [Task 2.2] 无前置依赖，可与 Wave 1 并行
- [Task 2.3] depends on [Task 2.1, Task 2.2]
