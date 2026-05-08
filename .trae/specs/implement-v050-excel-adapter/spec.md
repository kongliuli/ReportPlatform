# v0.5.0 Excel 适配器 Spec

## Why

现有 ExcelImportAdapter 基于列头文本匹配，不可靠且无类型校验。需要实现基于扁平化契约驱动的 Excel 适配器，支持隐藏契约行精确匹配、强类型校验和批量导入。

## What Changes

- 新建 TemplateFlattenService：将 Editable 元素扁平化为 FlatField 列表
- 新建 ExcelSchemaExporter：导出含隐藏契约行（DataPath）的 xlsx 模板
- 新建 ExcelContractReader：按契约行精确匹配导入数据
- 新建 ExcelDataValidator：Number/Date/Dropdown/Boolean 强类型校验
- 新建 ExcelAdapterConfig 和 ExcelTemplateSchema 配置模型
- 新建 ExcelAdapterFactory 工厂类
- 新建 ExcelAdapterTabViewModel 和 ExcelAdapterTab 可视化配置面板
- 修改 MainViewModel 支持 Excel 适配器 Tab 创建
- 修改 App.xaml.cs 注册 ExcelAdapterFactory
- **BREAKING**: 导入方式从列头文本匹配改为契约行精确匹配

## Impact

- Affected specs: v0.4.0 WPF MVVM 重构
- Affected code:
  - `Generators/ReportDataMaker/Services/ExcelAdapter/` - 新建目录
  - `Generators/ReportDataMaker/ViewModels/Tabs/` - 新增 ExcelAdapterTabViewModel
  - `Generators/ReportDataMaker/Views/Tabs/` - 新增 ExcelAdapterTab
  - `Generators/ReportDataMaker/ViewModels/MainViewModel.cs` - 修改
  - `Generators/ReportDataMaker/App.xaml.cs` - 修改

## ADDED Requirements

### Requirement: 模板扁平化

系统 SHALL 将模板中 ElementGroup.Editable 的元素扁平化为 FlatField 列表，包含 DataPath、Label、DataType、Format、Options、IsRequired、MinValue、MaxValue、DecimalPlaces。

#### Scenario: 简单元素扁平化
- **WHEN** 模板包含 TextElement (Editable, DataPath="Patient.Name", Label="姓名")
- **THEN** 扁平化结果包含 FlatField { DataPath="Patient.Name", Label="姓名", DataType=Text }

#### Scenario: Table Cell 扁平化
- **WHEN** 模板包含 3行4列 TableElement（第1行为表头），Cell[1][1] 可编辑
- **THEN** 扁平化结果包含 FlatField { DataPath=Cell.DataPath, DataType=Number }

#### Scenario: 跳过非 Editable 元素
- **WHEN** 模板包含 ElementGroup.Fixed 或 ElementGroup.DataAdapter 元素
- **THEN** 扁平化结果不包含这些元素

### Requirement: Excel 契约模板导出

系统 SHALL 导出 xlsx 文件，包含 4 行结构：隐藏 DataPath 契约行、人类可读列头行、隐藏数据类型行、示例数据行。

#### Scenario: 导出模板结构
- **WHEN** 用户点击"导出 Excel 模板"
- **THEN** 生成 xlsx 文件，Row 1 为隐藏的 DataPath 行，Row 2 为 Label 列头行，Row 3 为隐藏的 DataType 行，Row 4 为示例数据行

#### Scenario: 契约行隐藏
- **WHEN** 用户在 Excel 中打开导出的模板
- **THEN** Row 1 和 Row 3 默认隐藏，用户仅看到列头和数据区

### Requirement: 契约行精确导入

系统 SHALL 读取 xlsx 中隐藏的 DataPath 契约行，按 DataPath 精确匹配数据列，不依赖列头文本。

#### Scenario: 单行导入
- **WHEN** 用户选择 xlsx 文件并选择"单行导入"
- **THEN** 读取 DataStartRow 的数据，按契约行 DataPath 匹配，返回 Dictionary<string, object>

#### Scenario: 批量导入
- **WHEN** 用户选择 xlsx 文件并选择"批量导入"
- **THEN** 读取 DataStartRow 到最后一行的所有数据，返回 List<Dictionary<string, object>>

#### Scenario: 契约行缺失
- **WHEN** 导入的 xlsx 文件没有契约行
- **THEN** 返回错误信息"未找到契约行，请确认使用正确的模板文件"

### Requirement: 强类型校验

系统 SHALL 在导入前对数据进行类型校验，返回 ValidationReport。

#### Scenario: Number 校验
- **WHEN** FlatField.DataType=Number，值为 "abc"
- **THEN** 返回 ValidationError { Message="期望数字，实际值 \"abc\"" }

#### Scenario: Date 校验
- **WHEN** FlatField.DataType=Date，Format="yyyy-MM-dd"，值为 "2026/05/08"
- **THEN** 返回 ValidationError { Message="日期格式不匹配，期望 yyyy-MM-dd" }

#### Scenario: Dropdown 校验
- **WHEN** FlatField.DataType=Dropdown，Options=["男","女"]，值为 "其他"
- **THEN** 返回 ValidationError { Message="值 \"其他\" 不在选项列表中 [男,女]" }

#### Scenario: 校验通过
- **WHEN** 所有字段值均符合类型约束
- **THEN** ValidationReport.IsValid = true

### Requirement: ExcelAdapterTab 可视化配置

系统 SHALL 提供 ExcelAdapterTab 作为 TabControl 的 Tab 页，包含模板导出、数据导入和校验结果三个区域。

#### Scenario: 添加 Excel 适配器
- **WHEN** 用户通过 AddAdapterDialog 选择 Excel 类型
- **THEN** 在 TabControl 中创建 ExcelAdapterTab

#### Scenario: 预览扁平化结果
- **WHEN** ExcelAdapterTab 加载
- **THEN** 显示当前模板的可编辑字段数量

#### Scenario: 校验结果展示
- **WHEN** 用户点击"校验数据"
- **THEN** 在校验结果区域显示所有 ValidationError

## MODIFIED Requirements

### Requirement: MainViewModel 适配器创建

MainViewModel.ExecuteAddAdapter() SHALL 根据选择的适配器类型创建对应的 TabViewModel。

## REMOVED Requirements

### Requirement: ExcelImportAdapter 列头文本匹配
**Reason**: 替换为契约行精确匹配
**Migration**: ExcelImportAdapter 标记为 Obsolete，新代码使用 ExcelContractReader
