# 架构清理与打印预览修复 Spec

## Why

Phase 2 架构演进完成后，项目中残留了旧 Factory 体系（被 AdapterRegistry 替代）、过时 MVVM 基类（被 CommunityToolkit.Mvvm 替代）等死代码，以及断裂的打印预览功能（XAML 绑定了不存在的 Command）。需要清理死代码、修复断裂绑定，保持代码库整洁。

## What Changes

- 删除旧 Factory 体系：`ExcelAdapterFactory`、`DatabaseAdapterFactory`、`ContextAdapterFactory`、`AdapterServices` 及其 DI 注册
- 删除过时 MVVM 基类：`ViewModelBase`、`RelayCommand`、`AsyncRelayCommand`
- 修复断裂的打印预览绑定：移除 XAML 中不存在的 `PrintPreviewCommand` 绑定，或实现该命令
- 清理 `ReportDocumentPaginator` 半成品代码

## Impact

- Affected specs: architecture-evolution-phase2（I2 适配器注册表已替代旧 Factory）
- Affected code:
  - `ReportDataMaker.Core\Services\AdapterServices.cs`（删除）
  - `ReportDataMaker.Core\Services\ExcelAdapter\ExcelAdapterFactory.cs`（删除）
  - `ReportDataMaker.Core\Services\DatabaseAdapter\DatabaseAdapterFactory.cs`（删除）
  - `ReportDataMaker.Core\Services\ContextAdapter\ContextAdapterFactory.cs`（删除）
  - `ReportDataMaker.Core\Infrastructure\ViewModelBase.cs`（删除）
  - `ReportDataMaker\Infrastructure\RelayCommand.cs`（删除）
  - `ReportDataMaker\Infrastructure\AsyncRelayCommand.cs`（删除）
  - `ReportDataMaker\App.xaml.cs`（移除旧 Factory DI 注册）
  - `ReportDataMaker\Views\MainWindow.xaml`（修复 PrintPreviewCommand 绑定）
  - `ReportDataMaker\Views\Tabs\ExportTab.xaml`（修复 PrintPreviewCommand 绑定）
  - `ReportDataMaker\Services\PdfExport\ReportDocumentPaginator.cs`（清理或重做）

## ADDED Requirements

### Requirement: 死代码清理

系统 SHALL 不包含已被新体系替代的旧代码：
- 旧 Factory 体系（ExcelAdapterFactory/DatabaseAdapterFactory/ContextAdapterFactory/AdapterServices）已被 IAdapterPlugin/AdapterRegistry 替代，SHALL 被删除
- 旧 MVVM 基类（ViewModelBase/RelayCommand/AsyncRelayCommand）已被 CommunityToolkit.Mvvm 替代，SHALL 被删除
- App.xaml.cs 中对旧 Factory 的 DI 注册 SHALL 被移除

#### Scenario: 无旧 Factory 残留
- **WHEN** 搜索项目中 `ExcelAdapterFactory`、`DatabaseAdapterFactory`、`ContextAdapterFactory`、`AdapterServices` 类型名
- **THEN** 零搜索结果

#### Scenario: 无过时 MVVM 基类残留
- **WHEN** 搜索项目中 `ViewModelBase`、`RelayCommand`、`AsyncRelayCommand` 类型名
- **THEN** 零搜索结果（除 CommunityToolkit 自身的 AsyncRelayCommand）

### Requirement: 打印预览绑定修复

系统 SHALL 不包含断裂的 XAML 绑定：
- `PrintPreviewCommand` 在 XAML 中被绑定但在 ViewModel 中不存在，SHALL 移除 XAML 绑定
- `ReportDocumentPaginator` 半成品代码 SHALL 被清理（移除未使用的 `_pdfBytes` 字段和占位渲染逻辑）

#### Scenario: 无断裂 XAML 绑定
- **WHEN** 检查 `MainWindow.xaml` 和 `ExportTab.xaml` 中的 Command 绑定
- **THEN** 所有绑定的 Command 在对应 ViewModel 中均有定义

#### Scenario: ReportDocumentPaginator 无死代码
- **WHEN** 检查 `ReportDocumentPaginator.cs`
- **THEN** 不存在已渲染但未使用的 `_pdfBytes` 字段
