# v0.8.0 上下文适配器 & v0.7.0 报告输出增强 Spec

## Why
当前模板中 `DataPath` 以 `Context.` 开头的元素（医院名称、医生、日期等）需要手动填写，缺乏自动填充机制；同时填充完成的模板无法导出为 PDF 或打印，缺少报告输出能力。

## What Changes
- 新增 Context 适配器：自动填充 `Context.*` 元素，支持静态值、动态规则和多 Profile
- 新增 PDF 导出：基于 QuestPDF 将填充后的模板渲染为 PDF
- 新增批量导出：从 Excel/数据库适配器结果批量生成 PDF
- 新增打印预览：WPF PrintDialog 打印支持
- 新增导出 Tab：统一的导出操作界面
- 新增上下文配置 Tab：Context Profile 编辑界面

## Impact
- Affected specs: v0.5.0 Excel 适配器、v0.6.0 数据库适配器（批量数据可流向导出）
- Affected code: MainViewModel（执行链扩展）、App.xaml.cs（DI 注册）、MainWindow.xaml（菜单+DataTemplate）

## ADDED Requirements

### Requirement: Context 适配器自动填充
系统 SHALL 在模板加载后自动执行 Context 适配器，将 `ElementGroup.Context` 元素按配置填充数据。

#### Scenario: 模板加载后自动填充 Context 字段
- **WHEN** 用户加载包含 `Context.*` DataPath 元素的模板
- **THEN** 系统自动从当前 Profile 的 StaticValues 和 DynamicRules 解析值，填充到模板元素

#### Scenario: 日期时间字段实时更新
- **WHEN** DynamicRule 的 Source 为 CurrentDate/CurrentTime/CurrentDateTime
- **THEN** 系统在填充时取当前系统时间，按 Format 格式化

#### Scenario: 未配置的 Context 字段
- **WHEN** 模板中存在 Context 元素但 Profile 中无对应配置
- **THEN** 系统使用内置默认规则解析，无法解析时填充空字符串（不报错）

### Requirement: Context Profile 管理
系统 SHALL 支持多 Profile 的创建、切换和持久化。

#### Scenario: 多 Profile 切换
- **WHEN** 用户在 ContextAdapterTab 中选择不同 Profile
- **THEN** 系统加载对应 Profile 的 StaticValues 和 DynamicRules，重新填充模板

#### Scenario: 配置持久化
- **WHEN** 用户编辑并保存 Profile
- **THEN** 配置写入 `%AppData%/ReportDataMaker/context-profiles.json`

### Requirement: ContextAdapterTab 交互
系统 SHALL 提供 Context 配置编辑界面。

#### Scenario: 自动检测未配置字段
- **WHEN** 用户打开 ContextAdapterTab
- **THEN** 系统扫描模板中所有 `Context.*` 元素，列出未配置项

#### Scenario: 静态值编辑
- **WHEN** 用户在 DataGrid 中编辑 DataPath-Value 键值对
- **THEN** 修改实时反映到预览区域

### Requirement: PDF 单份导出
系统 SHALL 将填充完成的模板渲染为 PDF 文件。

#### Scenario: 导出单份 PDF
- **WHEN** 用户点击"导出 PDF"并选择保存路径
- **THEN** 系统将当前模板数据渲染为 PDF，布局与模板设计一致

### Requirement: PDF 元素渲染
系统 SHALL 支持所有元素类型的 PDF 渲染。

#### Scenario: 渲染各元素类型
- **WHEN** 模板包含 Text/Number/Date/Image/Table/Barcode/QrCode/Line/Shape/Divider/Checkbox/Radio/Signature/Header/Footer/PageNumber/Watermark 元素
- **THEN** 每种元素按其类型正确渲染到 PDF 对应位置

### Requirement: 批量 PDF 导出
系统 SHALL 支持从 Excel/数据库适配器结果批量生成 PDF。

#### Scenario: 批量导出
- **WHEN** 用户选择数据源和输出目录，设置命名规则后点击"开始"
- **THEN** 系统逐行填充模板并渲染 PDF，按命名规则保存文件，显示进度

### Requirement: 打印预览
系统 SHALL 提供 WPF 打印预览功能。

#### Scenario: 打印预览
- **WHEN** 用户点击"打印预览"
- **THEN** 系统显示打印预览窗口，用户可选择打印机进行打印

### Requirement: 导出历史记录
系统 SHALL 记录导出操作历史。

#### Scenario: 查看导出历史
- **WHEN** 用户查看导出历史区域
- **THEN** 显示最近导出记录（时间、文件名、路径）

### Requirement: ExportTab 交互
系统 SHALL 提供统一的导出操作界面。

#### Scenario: 单份导出流程
- **WHEN** 用户在 ExportTab 中预览当前数据并点击导出
- **THEN** 系统渲染 PDF 并让用户选择保存路径

#### Scenario: 批量导出流程
- **WHEN** 用户选择数据源、命名规则、输出目录后开始批量导出
- **THEN** 系统显示进度条、当前/总数、预计剩余时间

### Requirement: MainViewModel 执行链扩展
系统 SHALL 在模板加载后按顺序执行 Context 填充。

#### Scenario: 完整执行链
- **WHEN** 模板加载完成
- **THEN** 按 ContextAdapter.Fill → 用户操作 DataAdapter → 用户手动填写 → ExportService.RenderToPdf 顺序执行

## MODIFIED Requirements

### Requirement: MainViewModel 适配器管理
MainViewModel 新增 Context 适配器和导出 Tab 的创建逻辑，模板加载后自动执行 Context 填充。

### Requirement: MainWindow 菜单
MainWindow 新增"导出"和"上下文"菜单项。

### Requirement: App.xaml.cs DI 注册
App.xaml.cs 新增 ContextProfileStore、ContextAdapterFactory、IPdfExportService、BatchExportService、ExportHistoryStore 的 DI 注册。

## REMOVED Requirements

### Requirement: IPdfSharpTemplateRenderer Stub
**Reason**: 被 QuestPDF 方案替代
**Migration**: PdfExportService 替代 PdfSharpTemplateRendererStub，IPdfSharpTemplateRenderer 接口保留但不再作为主要渲染路径
