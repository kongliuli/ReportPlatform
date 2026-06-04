# WPF 项目 MVVM 重构设计文档

> 目标：将 ReportDataMaker 重构为标准 MVVM 架构，引入 HandyControl 美化，TabControl 驱动适配器页签，可折叠左面板
> 日期：2026-05-08（修订）

---

## 1. 现状分析

### 1.1 当前架构问题

| 问题 | 影响 |
|------|------|
| MainWindow 使用 code-behind 事件处理 | ViewModel 的 Commands 未被使用，逻辑分散 |
| MainViewModel 直接调用 `MessageBox.Show()` | 不可测试，违反 MVVM |
| MainViewModel 直接 `new OpenFileDialog()` | UI 逻辑混入 ViewModel |
| 服务在构造函数中直接 new | 无法替换实现，不可测试 |
| TemplatePreviewService 返回 UIElement | 渲染逻辑与视图层耦合 |
| 无 DI 容器 | 依赖关系硬编码 |
| 双模型体系共存 | 仅使用 External 格式，Legacy 代码冗余 |
| 左侧面板固定宽度 | 权宜之计，无法适配不同分辨率 |

### 1.2 当前做得好的部分

| 优点 | 位置 |
|------|------|
| DataEntryWindow 完整 MVVM | 隐式 DataTemplate 多态绑定 |
| ViewModelBase + RelayCommand 基础设施 | Infrastructure/ |
| 元素 ViewModel 继承体系完整 | ViewModels/ 下 20+ 子类 |
| 适配器接口已定义 | IDataAdapter |

---

## 2. 目标架构

### 2.1 技术选型变更

| 项目 | 选择 | 原因 |
|------|------|------|
| UI 框架 | HandyControl | 现代化 WPF 控件库，开箱即用的主题和动画 |
| 布局模式 | TabControl 驱动 | 每个适配器独立 Tab 页签，清晰隔离 |
| 左面板 | 可展开/折叠 | 适配不同分辨率，节省工作区空间 |
| DI | Microsoft.Extensions.DependencyInjection | .NET 标准 |

### 2.2 整体结构

**设计原则**：每个独立的业务能力拆分为单独的 View + ViewModel 对，便于独立开发、测试和维护。

```
ReportDataMaker/
├── App.xaml                    # 启动 + DI + HandyControl 主题
├── Infrastructure/             # MVVM 基础设施
│   ├── ViewModelBase.cs
│   ├── RelayCommand.cs
│   ├── AsyncRelayCommand.cs
│   ├── IDialogService.cs
│   ├── INavigationService.cs
│   └── ServiceLocator.cs
├── Models/                     # 数据模型（引用 Contracts 后可精简）
├── Services/                   # 业务服务
│   ├── Adapters/
│   │   ├── Excel/
│   │   │   ├── TemplateFlattenService.cs
│   │   │   ├── ExcelSchemaExporter.cs
│   │   │   └── ExcelContractReader.cs
│   │   ├── Database/
│   │   │   ├── DatabaseAdapterBase.cs
│   │   │   ├── IDatabaseProvider.cs
│   │   │   ├── SqlServerProvider.cs
│   │   │   ├── MySqlProvider.cs
│   │   │   └── SqliteProvider.cs
│   │   └── AdapterRegistry.cs
│   ├── TemplateLoaderService.cs
│   ├── TemplatePreviewService.cs
│   ├── DataBindingService.cs
│   └── AdapterConfigStore.cs
├── ViewModels/
│   ├── MainViewModel.cs            # Shell 编排
│   ├── TemplateLoadViewModel.cs    # 模板加载对话框
│   ├── SidePanelViewModel.cs       # 左侧面板状态
│   ├── Tabs/                       # 各 Tab 页 ViewModel（独立业务单元）
│   │   ├── DataEntryTabViewModel.cs        # 数据录入
│   │   ├── PreviewTabViewModel.cs          # 模板预览
│   │   ├── ExcelAdapterTabViewModel.cs     # Excel 适配器配置
│   │   └── DatabaseAdapterTabViewModel.cs  # 数据库适配器配置
│   ├── Components/                 # 可复用业务组件 ViewModel
│   │   ├── FieldMappingViewModel.cs        # 字段映射（Excel/DB 共用）
│   │   ├── ConnectionTestViewModel.cs      # 数据库连接测试
│   │   ├── QueryBuilderViewModel.cs        # 可视化查询构建
│   │   ├── SqlEditorViewModel.cs           # SQL 编辑
│   │   ├── DataPreviewViewModel.cs         # 数据预览表格
│   │   ├── ValidationResultViewModel.cs    # 校验结果展示
│   │   └── FlatFieldListViewModel.cs       # 扁平化字段列表
│   ├── Elements/                   # 元素 ViewModel（保持现有）
│   └── Dialogs/                    # 对话框 ViewModel
│       ├── AddAdapterDialogViewModel.cs
│       └── ParameterInputDialogViewModel.cs
├── Views/
│   ├── MainWindow.xaml             # Shell 布局
│   ├── Dialogs/                    # 对话框视图
│   │   ├── TemplateLoadDialog.xaml
│   │   ├── AddAdapterDialog.xaml
│   │   └── ParameterInputDialog.xaml
│   ├── Panels/                     # 面板视图
│   │   └── SidePanel.xaml
│   ├── Tabs/                       # 各 Tab 页视图（独立业务单元）
│   │   ├── DataEntryTab.xaml
│   │   ├── PreviewTab.xaml
│   │   ├── ExcelAdapterTab.xaml
│   │   └── DatabaseAdapterTab.xaml
│   └── Components/                 # 可复用业务组件视图
│       ├── FieldMappingView.xaml           # 字段映射控件
│       ├── ConnectionTestView.xaml         # 连接测试控件
│       ├── QueryBuilderView.xaml           # 可视化查询构建控件
│       ├── SqlEditorView.xaml              # SQL 编辑控件
│       ├── DataPreviewView.xaml            # 数据预览表格控件
│       ├── ValidationResultView.xaml       # 校验结果控件
│       └── FlatFieldListView.xaml          # 扁平化字段列表控件
└── Converters/
```

**拆分原则**：
- 每个 Tab 页是一个独立业务单元（View + ViewModel）
- Tab 内部的子功能进一步拆分为 Components（可复用的 View + ViewModel 对）
- Components 可跨 Tab 复用（如 FieldMappingView 在 Excel 和 Database Tab 中都使用）
- 对话框独立为 Dialogs 目录
- 每个 View 只依赖自己的 ViewModel，通过 DI 获取服务

### 2.3 依赖注入配置

```csharp
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        
        // 基础设施
        services.AddSingleton<IDialogService, WpfDialogService>();
        
        // 业务服务
        services.AddSingleton<TemplateLoaderService>();
        services.AddSingleton<TemplatePreviewService>();
        services.AddSingleton<DataBindingService>();
        services.AddSingleton<TemplateFlattenService>();
        services.AddSingleton<AdapterRegistry>();
        services.AddSingleton<AdapterConfigStore>();
        services.AddSingleton<DatabaseProviderRegistry>();
        
        // Database Providers
        services.AddSingleton<IDatabaseProvider, SqlServerProvider>();
        services.AddSingleton<IDatabaseProvider, MySqlProvider>();
        services.AddSingleton<IDatabaseProvider, SqliteProvider>();
        
        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<TemplateLoadViewModel>();
        services.AddTransient<SidePanelViewModel>();
        services.AddTransient<AdapterTabsViewModel>();
        
        Services = services.BuildServiceProvider();
        
        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}
```

---

## 3. 主界面布局设计

### 3.1 启动流程

```
App 启动
    │
    ▼
┌──────────────────────────┐
│ TemplateLoadDialog        │  ← HandyControl Dialog 样式
│                           │
│  ┌─ 最近使用 ──────────┐ │
│  │ ● 检验报告单.json    │ │
│  │ ○ 门诊病历.json      │ │
│  └─────────────────────┘ │
│                           │
│  [浏览文件...]  [从服务器] │
│                           │
│       [加载]  [取消]      │
└────────────┬─────────────┘
             │ 加载成功
             ▼
┌──────────────────────────────────────────────────────────────┐
│ MainWindow                                                    │
└──────────────────────────────────────────────────────────────┘
```

### 3.2 主界面三栏布局

```
┌──────────────────────────────────────────────────────────────────┐
│ [菜单栏]  文件 | 适配器 | 视图 | 帮助                             │
├────┬─────────────────────────────────────────────────────────────┤
│ ◀  │                                                             │
│    │  ┌─────────────────────────────────────────────────────┐    │
│ 模 │  │ [数据录入] [Excel适配器] [数据库适配器] [预览]  [+]  │    │
│ 板 │  ├─────────────────────────────────────────────────────┤    │
│ 信 │  │                                                     │    │
│ 息 │  │          当前 Tab 页内容区                            │    │
│    │  │                                                     │    │
│ ── │  │  (每个适配器配置独立一个 Tab)                         │    │
│    │  │  (数据录入是固定 Tab)                                │    │
│ 适 │  │  (预览是固定 Tab)                                    │    │
│ 配 │  │                                                     │    │
│ 器 │  │                                                     │    │
│ 列 │  │                                                     │    │
│ 表 │  │                                                     │    │
│    │  │                                                     │    │
│ ── │  └─────────────────────────────────────────────────────┘    │
│    │                                                             │
│ 操 │                                                             │
│ 作 │                                                             │
├────┴─────────────────────────────────────────────────────────────┤
│ [状态栏] ● 就绪 | 模板: 检验报告单 v2.2.0 | 适配器: 2 已配置      │
└──────────────────────────────────────────────────────────────────┘
```

### 3.3 左侧面板 — 可展开/折叠

左侧面板使用 HandyControl 的 `SideMenu` 或自定义折叠面板：

```xml
<!-- 左侧可折叠面板 -->
<Grid Grid.Column="0">
    <Grid.Width>
        <Binding Path="IsSidePanelExpanded" 
                 Converter="{StaticResource BoolToWidthConverter}"
                 ConverterParameter="240,48"/>
    </Grid.Width>
    
    <!-- 折叠状态：仅显示图标按钮 -->
    <StackPanel Visibility="{Binding IsSidePanelExpanded, 
                Converter={StaticResource InverseBoolToVisibility}}">
        <Button Command="{Binding ToggleSidePanelCommand}" 
                Style="{StaticResource IconButtonStyle}">
            <hc:Icon Kind="ChevronRight"/>
        </Button>
        <!-- 图标模式的适配器快捷按钮 -->
    </StackPanel>
    
    <!-- 展开状态：完整面板 -->
    <DockPanel Visibility="{Binding IsSidePanelExpanded, 
               Converter={StaticResource BoolToVisibility}}">
        
        <!-- 折叠按钮 -->
        <Button DockPanel.Dock="Top" 
                Command="{Binding ToggleSidePanelCommand}"
                HorizontalAlignment="Right">
            <hc:Icon Kind="ChevronLeft"/>
        </Button>
        
        <!-- 模板信息区（可折叠） -->
        <hc:Expander DockPanel.Dock="Top" Header="模板信息" IsExpanded="True">
            <StackPanel>
                <TextBlock Text="{Binding TemplateName}"/>
                <TextBlock Text="{Binding TemplateVersion}"/>
                <TextBlock Text="{Binding FieldSummary}"/>
            </StackPanel>
        </hc:Expander>
        
        <!-- 适配器列表区（可折叠） -->
        <hc:Expander DockPanel.Dock="Top" Header="已配置适配器" IsExpanded="True">
            <StackPanel>
                <ItemsControl ItemsSource="{Binding Adapters}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Button Content="{Binding DisplayName}"
                                    Command="{Binding DataContext.SwitchToAdapterTabCommand, 
                                             RelativeSource={RelativeSource AncestorType=Window}}"
                                    CommandParameter="{Binding}"/>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
                <Button Content="+ 添加适配器" 
                        Command="{Binding AddAdapterCommand}"
                        Style="{StaticResource DashedBorderButton}"/>
            </StackPanel>
        </hc:Expander>
        
        <!-- 操作区（可折叠） -->
        <hc:Expander DockPanel.Dock="Top" Header="操作" IsExpanded="True">
            <StackPanel>
                <Button Content="保存所有配置" Command="{Binding SaveCommand}"/>
                <Button Content="导出报告" Command="{Binding ExportReportCommand}"/>
                <Button Content="批量生成" Command="{Binding BatchExportCommand}"/>
            </StackPanel>
        </hc:Expander>
    </DockPanel>
</Grid>
```

### 3.4 中央 TabControl — 适配器页签管理

核心设计：每个适配器配置作为独立 TabItem 加载，用户可自由切换。

```xml
<hc:TabControl Style="{StaticResource TabControlCapsule}"
               ItemsSource="{Binding Tabs}"
               SelectedItem="{Binding ActiveTab}">
    <hc:TabControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <hc:Icon Kind="{Binding IconKind}" Margin="0,0,4,0"/>
                <TextBlock Text="{Binding Title}"/>
                <!-- 可关闭按钮（适配器 Tab 可关闭，固定 Tab 不可） -->
                <Button Command="{Binding CloseCommand}" 
                        Visibility="{Binding IsClosable, Converter=...}"
                        Style="{StaticResource TabCloseButton}">
                    <hc:Icon Kind="Close" Width="10"/>
                </Button>
            </StackPanel>
        </DataTemplate>
    </hc:TabControl.ItemTemplate>
    <hc:TabControl.ContentTemplate>
        <DataTemplate>
            <ContentControl Content="{Binding}"/>
        </DataTemplate>
    </hc:TabControl.ContentTemplate>
</hc:TabControl>
```

### 3.5 Tab 页签模型

```csharp
public abstract class TabViewModelBase : ViewModelBase
{
    public string Title { get; set; }
    public string IconKind { get; set; }
    public bool IsClosable { get; set; }
    public ICommand CloseCommand { get; }
}

public class DataEntryTabViewModel : TabViewModelBase
{
    // Title = "数据录入", IconKind = "Edit", IsClosable = false
    public DataEntryViewModel DataEntry { get; }
}

public class PreviewTabViewModel : TabViewModelBase
{
    // Title = "预览", IconKind = "Eye", IsClosable = false
    public TemplatePreviewViewModel Preview { get; }
}

public class ExcelAdapterTabViewModel : TabViewModelBase
{
    // Title = "Excel: {DisplayName}", IconKind = "FileExcel", IsClosable = true
    public ExcelConfigViewModel Config { get; }
}

public class DatabaseAdapterTabViewModel : TabViewModelBase
{
    // Title = "DB: {DisplayName}", IconKind = "Database", IsClosable = true
    public DatabaseConfigViewModel Config { get; }
}
```

### 3.6 添加适配器流程

```
用户点击 "+ 添加适配器"
    │
    ▼
┌──────────────────────────┐
│ 选择适配器类型 (Dialog)    │
│                           │
│  ┌─────┐ ┌─────┐ ┌─────┐│
│  │Excel│ │数据库│ │ API ││
│  │     │ │     │ │(禁用)││
│  └─────┘ └─────┘ └─────┘│
│                           │
│  名称: [________________] │
│       [确定]  [取消]      │
└────────────┬─────────────┘
             │
             ▼
TabControl 新增一个对应类型的 TabItem
自动切换到新 Tab
```

---

## 4. HandyControl 集成与 Component 组合

### 4.1 Tab 页内部的 Component 组合

每个 Tab 页由多个独立 Component 组合而成，每个 Component 是独立的 View + ViewModel 对。

以 DatabaseAdapterTab 为例：

```xml
<!-- Views/Tabs/DatabaseAdapterTab.xaml -->
<UserControl>
    <ScrollViewer>
        <StackPanel>
            <!-- 连接配置组件 -->
            <ContentControl Content="{Binding ConnectionTest}"/>
            
            <!-- 查询构建组件 -->
            <ContentControl Content="{Binding QueryBuilder}"/>
            
            <!-- 数据预览组件 -->
            <ContentControl Content="{Binding DataPreview}"/>
            
            <!-- 字段映射组件（与 Excel Tab 共用同一个 Component） -->
            <ContentControl Content="{Binding FieldMapping}"/>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

```csharp
public class DatabaseAdapterTabViewModel : TabViewModelBase
{
    public ConnectionTestViewModel ConnectionTest { get; }
    public QueryBuilderViewModel QueryBuilder { get; }
    public DataPreviewViewModel DataPreview { get; }
    public FieldMappingViewModel FieldMapping { get; }  // 与 ExcelAdapterTab 共用
}

public class ExcelAdapterTabViewModel : TabViewModelBase
{
    public FlatFieldListViewModel FlatFields { get; }       // 扁平化字段预览
    public FieldMappingViewModel FieldMapping { get; }      // 共用字段映射组件
    public ValidationResultViewModel Validation { get; }    // 共用校验结果组件
    public DataPreviewViewModel DataPreview { get; }        // 共用数据预览组件
}
```

**收益**：
- `FieldMappingView` 开发一次，Excel 和 Database Tab 都能用
- `DataPreviewView` 开发一次，所有需要预览数据的场景都能用
- 每个 Component 可独立单元测试
- 新增适配器类型时，只需组合已有 Components + 开发特有 Components

### 4.2 App.xaml 主题配置

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml"/>
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/Theme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
        
        <!-- 隐式 DataTemplate 注册 -->
        <DataTemplate DataType="{x:Type vm:DataEntryTabViewModel}">
            <views:DataEntryTab/>
        </DataTemplate>
        <DataTemplate DataType="{x:Type vm:ExcelAdapterTabViewModel}">
            <views:ExcelAdapterTab/>
        </DataTemplate>
        <DataTemplate DataType="{x:Type vm:DatabaseAdapterTabViewModel}">
            <views:DatabaseAdapterTab/>
        </DataTemplate>
        <DataTemplate DataType="{x:Type vm:PreviewTabViewModel}">
            <views:PreviewTab/>
        </DataTemplate>
    </ResourceDictionary>
</Application.Resources>
```

### 4.2 使用的 HandyControl 组件

| 组件 | 用途 |
|------|------|
| `hc:TabControl` (Capsule style) | 主工作区 Tab 页签 |
| `hc:Expander` | 左侧面板可折叠区域 |
| `hc:Dialog` | 模板加载、适配器选择对话框 |
| `hc:Growl` | 操作成功/失败通知 |
| `hc:Loading` | 异步操作加载指示器 |
| `hc:StepBar` | Excel 导入步骤指示 |
| `hc:Transfer` | 字段映射（左右穿梭） |
| `hc:Tag` | 适配器类型标签 |
| `hc:Badge` | 适配器状态指示 |
| `hc:Watermark` (TextBox) | 输入框占位提示 |
| `hc:NumericUpDown` | 数字参数输入 |
| `hc:DatePicker` | 日期参数输入 |
| `hc:SearchBar` | 字段搜索过滤 |

### 4.3 NuGet 依赖新增

```xml
<PackageReference Include="HandyControl" Version="3.5.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
```

---

## 5. 核心 ViewModel 重构

### 5.1 MainViewModel — Shell 编排器

```csharp
public class MainViewModel : ViewModelBase
{
    // 模板状态
    public ExternalTemplateDefinition CurrentTemplate { get; set; }
    public bool IsTemplateLoaded { get; set; }
    public string TemplateName { get; set; }
    public string TemplateVersion { get; set; }
    public string FieldSummary { get; set; }  // "Fixed:5 Editable:12 Adapter:3"
    
    // 左侧面板
    public bool IsSidePanelExpanded { get; set; } = true;
    public ObservableCollection<AdapterItemViewModel> Adapters { get; }
    
    // Tab 管理
    public ObservableCollection<TabViewModelBase> Tabs { get; }
    public TabViewModelBase ActiveTab { get; set; }
    
    // 命令
    public ICommand LoadTemplateCommand { get; }
    public ICommand ToggleSidePanelCommand { get; }
    public ICommand AddAdapterCommand { get; }
    public ICommand SwitchToAdapterTabCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExportReportCommand { get; }
    public ICommand BatchExportCommand { get; }
    
    private void OnTemplateLoaded(ExternalTemplateDefinition template)
    {
        CurrentTemplate = template;
        IsTemplateLoaded = true;
        TemplateName = template.Name;
        
        // 初始化固定 Tab
        Tabs.Clear();
        Tabs.Add(new DataEntryTabViewModel(template, _dataBindingService));
        Tabs.Add(new PreviewTabViewModel(template));
        ActiveTab = Tabs[0];
        
        // 加载已保存的适配器配置 → 每个适配器创建一个 Tab
        var savedConfigs = _configStore.Load(template.Name);
        foreach (var config in savedConfigs)
        {
            var tab = CreateAdapterTab(config);
            Tabs.Insert(Tabs.Count - 1, tab);  // 插入到预览 Tab 之前
            Adapters.Add(new AdapterItemViewModel(config));
        }
    }
    
    private TabViewModelBase CreateAdapterTab(AdapterConfigBase config)
    {
        return config.Type switch
        {
            AdapterType.Excel => new ExcelAdapterTabViewModel(config, _flattenService, _schema),
            AdapterType.Database => new DatabaseAdapterTabViewModel(config, _providerRegistry),
            _ => throw new NotSupportedException()
        };
    }
    
    private void ExecuteAddAdapter(object parameter)
    {
        // 弹出选择对话框，创建新 Tab
        var adapterType = /* dialog result */;
        var config = new AdapterConfigBase { Type = adapterType, DisplayName = "新适配器" };
        var tab = CreateAdapterTab(config);
        Tabs.Insert(Tabs.Count - 1, tab);
        ActiveTab = tab;
        Adapters.Add(new AdapterItemViewModel(config));
    }
}
```

### 5.2 TemplateLoadViewModel — 启动加载框

```csharp
public class TemplateLoadViewModel : ViewModelBase
{
    public LoadMode SelectedMode { get; set; }
    public ObservableCollection<RecentTemplate> RecentTemplates { get; }
    public ObservableCollection<TemplateDto> ServerTemplates { get; }
    
    public ICommand BrowseFileCommand { get; }
    public ICommand LoadFromServerCommand { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }
    
    public ExternalTemplateDefinition LoadedTemplate { get; }
    public event Action<ExternalTemplateDefinition> TemplateLoaded;
}

public enum LoadMode { FromFile, FromServer, Recent }
```

---

## 6. 对话框服务抽象

```csharp
public interface IDialogService
{
    string OpenFile(string filter, string title);
    string SaveFile(string filter, string title, string defaultName);
    bool Confirm(string message, string title);
    void ShowInfo(string message, string title);
    void ShowError(string message, string title);
    void ShowGrowl(string message, GrowlType type);  // HandyControl Growl 通知
    Task<T> ShowDialogAsync<T>(ViewModelBase dialogViewModel);
}
```

---

## 7. 异步命令支持

```csharp
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object, Task> _execute;
    private readonly Func<object, bool> _canExecute;
    private bool _isExecuting;

    public bool IsExecuting
    {
        get => _isExecuting;
        set { _isExecuting = value; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
    }

    public async void Execute(object parameter)
    {
        if (_isExecuting) return;
        IsExecuting = true;
        try { await _execute(parameter); }
        finally { IsExecuting = false; }
    }
}
```

---

## 8. 重构步骤

| 阶段 | 内容 | 预计工作量 |
|------|------|-----------|
| Step 1 | 引入 HandyControl + DI 容器，配置主题 | 0.5 天 |
| Step 2 | 实现 IDialogService、AsyncRelayCommand | 0.5 天 |
| Step 3 | 实现可折叠左面板 (SidePanel) | 1 天 |
| Step 4 | 实现 TabControl 页签管理 (AdapterTabsViewModel) | 1 天 |
| Step 5 | 重构 MainViewModel 为 Shell 编排器 | 1 天 |
| Step 6 | 重写 MainWindow.xaml（HandyControl 样式） | 1.5 天 |
| Step 7 | 实现 TemplateLoadDialog | 0.5 天 |
| Step 8 | 实现 DataEntryTab（迁移现有 DataEntryWindow） | 1 天 |
| Step 9 | 实现 ExcelAdapterTab（扁平化+契约导出+导入） | 2 天 |
| Step 10 | 实现 DatabaseAdapterTab（Base+Provider 可视化） | 2.5 天 |
| Step 11 | 实现 PreviewTab（迁移现有预览） | 0.5 天 |
| Step 12 | 适配器配置持久化 | 1 天 |
| Step 13 | 清理 Legacy 模型代码 | 0.5 天 |

**总计约 13.5 个工作日**

---

## 9. 数据流总览

```
┌─────────────┐    启动加载    ┌──────────────┐
│ 模板 JSON    │──────────────▶│ MainViewModel │
└─────────────┘                └──────┬───────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
                    ▼                 ▼                 ▼
         ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
         │ 左侧面板      │  │  TabControl  │  │  状态栏      │
         │ (折叠/展开)   │  │  (页签管理)  │  │              │
         └──────────────┘  └──────┬───────┘  └──────────────┘
                                  │
              ┌───────────────────┼───────────────────┐
              ▼                   ▼                   ▼
    ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
    │ 数据录入 Tab  │   │ Excel Tab    │   │ 数据库 Tab   │
    │ (手动编辑)   │   │ (扁平化契约) │   │ (Base+Provider)│
    └──────┬───────┘   └──────┬───────┘   └──────┬───────┘
           │                  │                   │
           │◀─────────────────┴───────────────────┘
           │         适配器数据填充到录入层
           ▼
    ┌──────────────┐
    │ 预览 Tab     │
    │ (实时渲染)   │
    └──────────────┘
```

适配器 Tab 执行数据获取后，结果自动合并到数据录入 Tab 的对应字段。用户可在数据录入 Tab 查看、微调所有数据，预览 Tab 实时反映最终效果。
