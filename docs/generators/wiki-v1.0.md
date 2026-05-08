# ReportDataMaker 数据录入工具 Wiki v1.0

> 版本：v1.0 | 更新日期：2026-05-08 | 覆盖版本：v0.4.0 ~ v0.6.0

---

## 1. 概述

**职责**：基于契约的 WPF 桌面数据录入工具，支持模板加载、数据绑定、适配器数据填充和报告预览。

**项目路径**：`/Generators/ReportDataMaker/ReportDataMaker.csproj`
**目标框架**：`net10.0-windows`
**UI 框架**：HandyControl 3.5.1
**输出类型**：WinExe

---

## 2. NuGet 依赖

| 包 | 版本 | 用途 |
|----|------|------|
| HandyControl | 3.5.1 | WPF UI 控件库 |
| ClosedXML | 0.105.0 | Excel 文件操作 |
| Microsoft.Data.SqlClient | 7.0.1 | SQL Server 数据库 |
| Microsoft.Data.Sqlite | 10.0.7 | SQLite 数据库 |
| MySqlConnector | 2.5.0 | MySQL 数据库 |
| Npgsql | 9.0.3 | PostgreSQL 数据库 |
| Newtonsoft.Json | 13.0.4 | JSON 序列化 |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | 依赖注入 |
| ZXing.Net.Bindings.Windows.Compatibility | 0.16.13 | 条形码/二维码生成 |

---

## 3. 桥接模型层

### 3.1 ReportExternalElementBase

继承 Contracts `ExternalElementBase`，添加 WPF 渲染属性：

| 属性分类 | 属性 |
|---------|------|
| 可见性 | IsVisible, Opacity |
| 边框 | BackgroundColor, BorderColor, BorderWidth, BorderStyle, CornerRadius |
| 阴影 | Shadow |
| 文字 | FontFamily, FontSize, FontWeight, FontStyle, ForegroundColor, TextAlignment, LabelWidth |
| 数据 | DefaultValue, Options, FormatString, ElementType |

### 3.2 WPF 元素类型（23 个）

所有继承 `ReportExternalElementBase`：

ExternalTextElement, ExternalLineElement, ExternalDropdownElement, ExternalNumberElement,
ExternalDateElement, ExternalTableElement, ExternalImageElement, ExternalShapeElement,
ExternalDividerElement, ExternalCheckboxElement, ExternalRadioElement, ExternalSignatureElement,
ExternalBarcodeElement, ExternalQrCodeElement, ExternalChartElement, ExternalContainerElement,
ExternalRepeatElement, ExternalHeaderElement, ExternalFooterElement, ExternalPageNumberElement,
ExternalWatermarkElement, ExternalIconElement, ExternalHyperlinkElement

### 3.3 ExternalTemplateDefinition

WPF 端模板定义模型，包含：
- 基本信息：Id, Name, Type, Version, HospitalId
- 页面设置：PageWidth, PageHeight, Orientation, Margins (Left/Right/Top/Bottom)
- 样式：BackgroundColor, GlobalFontSize, EnableGlobalFontSize
- 数据：Elements (List\<ReportExternalElementBase\>), DataBindings (List\<LegacyDataBindingDefinition\>)
- 文件：FilePath

---

## 4. MVVM 基础设施

| 类 | 说明 |
|----|------|
| `ViewModelBase` | INotifyPropertyChanged + SetProperty 辅助方法 |
| `RelayCommand` | 同步 ICommand 实现 |
| `AsyncRelayCommand` | 异步 ICommand + IsExecuting 守卫防重入 |
| `IDialogService` | 对话框接口（OpenFile, SaveFile, Confirm, ShowInfo, ShowError, ShowSuccess） |
| `DialogService` | WPF MessageBox 实现 |
| `ServiceLocator` | DI 容器包装，提供静态访问 |

---

## 5. 服务层

### 5.1 核心服务

| 接口 | 实现 | 说明 |
|------|------|------|
| `ITemplateLoaderService` | `TemplateLoaderService` | 模板 JSON 加载和分类 |
| `ITemplatePreviewService` | `TemplatePreviewService` | 模板预览渲染 |
| `IDataBindingService` | `DataBindingService` | 数据路径绑定和填充 |
| — | `AdapterConfigStore` | 适配器配置持久化（%APPDATA%/ReportDataMaker/adapters.json） |

### 5.2 Excel 适配器服务（v0.5.0）

命名空间：`ReportDataMaker.Services.ExcelAdapter`

| 类 | 说明 |
|----|------|
| `TemplateFlattenService` | 将 Editable 元素展平为 FlatField 列表 |
| `ExcelSchemaExporter` | 导出 xlsx（隐藏契约行 + 标签行 + 数据行 + 说明） |
| `ExcelContractReader` | 基于契约行的精确导入（单条/批量） |
| `ExcelDataValidator` | 数据验证（Number/Date/Dropdown/Boolean），返回 ValidationReport |
| `ExcelAdapterConfig` | 继承 AdapterConfigBase，含 FilePath, SheetName, Mode, ExportedSchema |
| `ExcelAdapterFactory` | 工厂（整合 Flattener + Exporter + Reader + Validator） |
| `TemplateFieldSchema` | 模板字段模式（TemplateName, TemplateVersion, Fields, GeneratedAt） |
| `FlatField` | 继承 FieldSchema，添加 ElementId 属性 |

**Excel 契约行结构：**
- Row1: DataPath（隐藏行，精确匹配列）
- Row2: Label（用户可见的列标题）
- Row3: DataType（隐藏行，类型校验依据）
- Row4+: 数据行

**ImportMode 枚举：** Single / Batch

### 5.3 数据库适配器服务（v0.6.0）

命名空间：`ReportDataMaker.Services.DatabaseAdapter`

| 类 | 说明 |
|----|------|
| `IDatabaseProvider` | Provider 接口（连接/元数据/SQL 方言） |
| `DatabaseProviderRegistry` | Provider 注册表 + CreateDefault() |
| `SqlServerProvider` | SQL Server（[id]引用, @参数, OFFSET...FETCH 分页） |
| `MySqlProvider` | MySQL（\`id\`引用, @参数, LIMIT...OFFSET 分页） |
| `SqliteProvider` | SQLite（"id"引用, @参数, PRAGMA 元数据） |
| `PostgreSqlProvider` | PostgreSQL（"id"引用, :参数, LIMIT...OFFSET 分页） |
| `DatabaseAdapterConfig` | 继承 AdapterConfigBase，含 Provider, ConnectionString, Query, DbFieldMappings, Parameters, Joins |
| `SqlBuilder` | 可视化模式 → SQL 生成器 |
| `DatabaseAdapterBase` | 查询执行 + 字段映射 + 参数化 + 结果转换（全异步） |
| `DatabaseAdapterFactory` | 工厂（整合 Provider + AdapterBase + Flattener） |
| `TableInfo` | 表元数据（Schema, Name, Type） |

**配置子模型：**
- `Query`（Mode: RawSql/VisualBuilder, RawSql, PrimaryTable, SelectedColumns, WhereClause, OrderBy）
- `DbFieldMapping`（ColumnName, TargetDataPath, Transform）
- `QueryParameter`（Name, Label, DataType, DefaultValue, Source: Manual/FromTemplate/FromContext）
- `JoinDefinition`（LeftTable, LeftColumn, RightTable, RightColumn, Type: Inner/Left/Right）

---

## 6. ViewModel 层

| 类 | 继承 | 说明 |
|----|------|------|
| `MainViewModel` | ViewModelBase | 主 ViewModel，管理模板加载、适配器 Tab、侧边栏 |
| `TabViewModelBase` | ViewModelBase | Tab 页签基类（Title, IsClosable, CloseRequested） |
| `DataEntryTabViewModel` | TabViewModelBase | 数据录入 Tab |
| `PreviewTabViewModel` | TabViewModelBase | 预览 Tab |
| `ExcelAdapterTabViewModel` | TabViewModelBase | Excel 适配器 Tab（导出/导入/验证） |
| `DatabaseAdapterTabViewModel` | TabViewModelBase | 数据库适配器 Tab（连接/查询/参数/映射/预览） |
| `TemplateLoadViewModel` | ViewModelBase | 模板加载对话框 |
| `SidePanelViewModel` | ViewModelBase | 侧边栏 |
| `AddAdapterDialogViewModel` | ViewModelBase | 添加适配器对话框 |

### MainViewModel 依赖注入

- ITemplateLoaderService
- IDataBindingService
- ITemplatePreviewService
- IDialogService
- AdapterConfigStore
- ExcelAdapterFactory
- DatabaseAdapterFactory

### 数据库适配器自动匹配策略

DatabaseAdapterTabViewModel.AutoMatch 使用三种策略：
1. 精确匹配 DataPath：列名 == DataPath
2. 精确匹配 Label：列名 == Label
3. DataPath 后缀匹配：DataPath 以列名结尾（如 `Patient.Name` 匹配 `Name`）

---

## 7. View 层

| XAML | 说明 |
|------|------|
| `MainWindow.xaml` | 三栏布局（可折叠侧边栏 + TabControl + 状态栏） |
| `DataEntryTab.xaml` | 数据录入界面 |
| `PreviewTab.xaml` | 预览界面 |
| `ExcelAdapterTab.xaml` | Excel 适配器三区布局（导出/导入/验证） |
| `DatabaseAdapterTab.xaml` | 数据库适配器四区布局（连接/查询/参数/映射） |

---

## 8. DI 注册（App.xaml.cs）

```csharp
services.AddSingleton<IDialogService, DialogService>();
services.AddSingleton<ITemplateLoaderService, TemplateLoaderService>();
services.AddSingleton<ITemplatePreviewService, TemplatePreviewService>();
services.AddSingleton<IDataBindingService, DataBindingService>();
services.AddSingleton<AdapterConfigStore>();
services.AddSingleton<ExcelAdapterFactory>();
services.AddSingleton<DatabaseProviderRegistry>(sp => DatabaseProviderRegistry.CreateDefault());
services.AddSingleton<DatabaseAdapterFactory>();
services.AddTransient<MainViewModel>();
services.AddTransient<TemplateLoadViewModel>();
services.AddTransient<SidePanelViewModel>();
```

---

## 9. 值转换器

| 类 | 说明 |
|----|------|
| `BoolToVisibilityConverter` | Bool → Visibility（含 Inverse 和 BoolToWidth 变体） |

---

## 10. 适配器执行顺序

```
Context 适配器 ──▶ DataAdapter 适配器 ──▶ Editable 元素手动录入
   (自动填充)         (批量导入)              (人工补充)
```

---

*文档版本 1.0，基于代码实际状态编写*
