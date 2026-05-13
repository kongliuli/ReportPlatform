using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.ViewModels.Tabs;

/// <summary>数据库适配器标签页视图模型，管理数据库连接、查询和字段映射</summary>
public class DatabaseAdapterTabViewModel : TabViewModelBase
{
    private readonly ExternalTemplateDefinition _template;
    private readonly DatabaseAdapterFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly AdapterConfigStore _configStore;

    /// <summary>初始化数据库适配器标签页视图模型</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="factory">数据库适配器工厂</param>
    /// <param name="dialogService">对话框服务</param>
    /// <param name="configStore">适配器配置存储</param>
    /// <param name="displayName">显示名称</param>
    public DatabaseAdapterTabViewModel(
        ExternalTemplateDefinition template,
        DatabaseAdapterFactory factory,
        IDialogService dialogService,
        AdapterConfigStore configStore,
        string displayName)
    {
        _template = template;
        _factory = factory;
        _dialogService = dialogService;
        _configStore = configStore;
        Title = $"数据库: {displayName}";
        IsClosable = true;

        Config = new DatabaseAdapterConfig
        {
            Type = AdapterType.Database,
            DisplayName = displayName,
            Provider = DatabaseProvider.SqlServer,
            ConnectionString = string.Empty,
            Query = new QueryConfig()
        };

        var schema = _factory.FlattenTemplate(_template);
        TemplateFields = schema.Fields;

        TestConnectionCommand = new AsyncRelayCommand(ExecuteTestConnection);
        LoadTablesCommand = new AsyncRelayCommand(ExecuteLoadTables);
        LoadColumnsCommand = new AsyncRelayCommand(ExecuteLoadColumns);
        ExecutePreviewCommand = new AsyncRelayCommand(ExecutePreview);
        ExecuteQueryCommand = new AsyncRelayCommand(ExecuteQuery);
        AddJoinCommand = new RelayCommand(_ => Joins.Add(new JoinDefinition()));
        RemoveJoinCommand = new RelayCommand(p => { if (p is JoinDefinition j) Joins.Remove(j); });
        AddParameterCommand = new RelayCommand(_ => Parameters.Add(new QueryParameter()));
        RemoveParameterCommand = new RelayCommand(p => { if (p is QueryParameter q) Parameters.Remove(q); });
        AddMappingCommand = new RelayCommand(_ => DbFieldMappings.Add(new DbFieldMapping()));
        RemoveMappingCommand = new RelayCommand(p => { if (p is DbFieldMapping m) DbFieldMappings.Remove(m); });
        AutoMatchCommand = new RelayCommand(_ => ExecuteAutoMatch());
        SaveConfigCommand = new RelayCommand(_ => ExecuteSaveConfig());
    }

    /// <summary>数据库适配器配置</summary>
    public DatabaseAdapterConfig Config { get; }
    /// <summary>模板字段列表</summary>
    public List<FlatField> TemplateFields { get; }
    /// <summary>数据库表集合</summary>
    public ObservableCollection<TableInfo> Tables { get; } = new();
    /// <summary>数据库列集合</summary>
    public ObservableCollection<ColumnInfo> Columns { get; } = new();
    /// <summary>选中的列名集合（用于 SELECT）</summary>
    public ObservableCollection<string> SelectedColumnNames { get; } = new();
    /// <summary>连接定义集合</summary>
    public ObservableCollection<JoinDefinition> Joins { get; } = new();
    /// <summary>查询参数集合</summary>
    public ObservableCollection<QueryParameter> Parameters { get; } = new();
    /// <summary>数据库字段映射集合</summary>
    public ObservableCollection<DbFieldMapping> DbFieldMappings { get; } = new();

    /// <summary>可用的数据库提供者列表</summary>
    public IReadOnlyList<IDatabaseProvider> Providers => _factory.GetAllProviders();
    /// <summary>连接类型列表</summary>
    public List<JoinType> JoinTypeList { get; } = Enum.GetValues(typeof(JoinType)).Cast<JoinType>().ToList();
    /// <summary>字段数据类型列表</summary>
    public List<FieldDataType> FieldDataTypeList { get; } = Enum.GetValues(typeof(FieldDataType)).Cast<FieldDataType>().ToList();
    /// <summary>参数来源列表</summary>
    public List<ParameterSource> ParameterSourceList { get; } = Enum.GetValues(typeof(ParameterSource)).Cast<ParameterSource>().ToList();
    /// <summary>模板数据路径列表</summary>
    public List<string> TemplateDataPathList => TemplateFields.Select(f => f.DataPath).ToList();

    private IDatabaseProvider? _selectedProviderInfo;
    /// <summary>选中的数据库提供者信息</summary>
    public IDatabaseProvider? SelectedProviderInfo
    {
        get => _selectedProviderInfo;
        set
        {
            if (SetProperty(ref _selectedProviderInfo, value))
            {
                if (value != null) SelectedProvider = value.ProviderType;
            }
        }
    }

    private DatabaseProvider _selectedProvider = DatabaseProvider.SqlServer;
    /// <summary>选中的数据库提供者类型</summary>
    public DatabaseProvider SelectedProvider
    {
        get => _selectedProvider;
        set { if (SetProperty(ref _selectedProvider, value)) Config.Provider = value; }
    }

    private string _connectionString = string.Empty;
    /// <summary>数据库连接字符串</summary>
    public string ConnectionString
    {
        get => _connectionString;
        set { if (SetProperty(ref _connectionString, value)) Config.ConnectionString = value; }
    }

    private bool _isConnected;
    /// <summary>是否已连接数据库</summary>
    public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }

    private string _connectionStatus = "未连接";
    /// <summary>连接状态文本</summary>
    public string ConnectionStatus { get => _connectionStatus; set => SetProperty(ref _connectionStatus, value); }

    private TableInfo? _selectedTable;
    /// <summary>选中的数据库表</summary>
    public TableInfo? SelectedTable
    {
        get => _selectedTable;
        set
        {
            if (SetProperty(ref _selectedTable, value))
            {
                Columns.Clear();
                if (value != null)
                    _ = LoadColumnsForTableAsync(value.Name);
            }
        }
    }

    private QueryMode _currentQueryMode = QueryMode.RawSql;
    /// <summary>当前查询模式</summary>
    public QueryMode CurrentQueryMode
    {
        get => _currentQueryMode;
        set
        {
            if (SetProperty(ref _currentQueryMode, value))
            {
                Config.Query.Mode = value;
                OnPropertyChanged(nameof(IsRawSqlMode));
                OnPropertyChanged(nameof(IsVisualBuilderMode));
            }
        }
    }

    /// <summary>是否为原始SQL模式</summary>
    public bool IsRawSqlMode
    {
        get => CurrentQueryMode == QueryMode.RawSql;
        set { if (value) CurrentQueryMode = QueryMode.RawSql; }
    }

    /// <summary>是否为可视化构建模式</summary>
    public bool IsVisualBuilderMode
    {
        get => CurrentQueryMode == QueryMode.VisualBuilder;
        set { if (value) CurrentQueryMode = QueryMode.VisualBuilder; }
    }

    private string _rawSql = string.Empty;
    /// <summary>原始SQL语句</summary>
    public string RawSql
    {
        get => _rawSql;
        set { if (SetProperty(ref _rawSql, value)) Config.Query.RawSql = value; }
    }

    private string _primaryTable = string.Empty;
    /// <summary>主表名称</summary>
    public string PrimaryTable
    {
        get => _primaryTable;
        set { if (SetProperty(ref _primaryTable, value)) Config.Query.PrimaryTable = value; }
    }

    private string _whereClause = string.Empty;
    /// <summary>WHERE条件子句</summary>
    public string WhereClause
    {
        get => _whereClause;
        set { if (SetProperty(ref _whereClause, value)) Config.Query.WhereClause = value; }
    }

    private string _orderBy = string.Empty;
    /// <summary>排序子句</summary>
    public string OrderBy
    {
        get => _orderBy;
        set { if (SetProperty(ref _orderBy, value)) Config.Query.OrderBy = value; }
    }

    private string _previewData = string.Empty;
    /// <summary>预览数据文本</summary>
    public string PreviewData
    {
        get => _previewData;
        set { if (SetProperty(ref _previewData, value)) OnPropertyChanged(nameof(HasPreviewData)); }
    }

    /// <summary>是否有预览数据</summary>
    public bool HasPreviewData => !string.IsNullOrEmpty(PreviewData);

    private string _statusText = "就绪";
    /// <summary>状态文本</summary>
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private bool _isBusy;
    /// <summary>是否正在执行操作</summary>
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    /// <summary>测试连接命令</summary>
    public AsyncRelayCommand TestConnectionCommand { get; }
    /// <summary>加载表列表命令</summary>
    public AsyncRelayCommand LoadTablesCommand { get; }
    /// <summary>加载列信息命令</summary>
    public AsyncRelayCommand LoadColumnsCommand { get; }
    /// <summary>执行预览命令</summary>
    public AsyncRelayCommand ExecutePreviewCommand { get; }
    /// <summary>执行查询命令</summary>
    public AsyncRelayCommand ExecuteQueryCommand { get; }
    /// <summary>添加连接命令</summary>
    public RelayCommand AddJoinCommand { get; }
    /// <summary>移除连接命令</summary>
    public RelayCommand RemoveJoinCommand { get; }
    /// <summary>添加参数命令</summary>
    public RelayCommand AddParameterCommand { get; }
    /// <summary>移除参数命令</summary>
    public RelayCommand RemoveParameterCommand { get; }
    /// <summary>添加映射命令</summary>
    public RelayCommand AddMappingCommand { get; }
    /// <summary>移除映射命令</summary>
    public RelayCommand RemoveMappingCommand { get; }
    /// <summary>自动匹配命令</summary>
    public RelayCommand AutoMatchCommand { get; }
    /// <summary>保存配置命令</summary>
    public RelayCommand SaveConfigCommand { get; }

    private async Task ExecuteTestConnection(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString)) return;
        IsBusy = true;
        StatusText = "正在测试连接...";
        try
        {
            var success = await _factory.TestConnectionAsync(SelectedProvider, ConnectionString);
            IsConnected = success;
            ConnectionStatus = success ? "已连接" : "连接失败";
            StatusText = success ? "连接成功" : "连接失败";
            if (success) _dialogService.ShowSuccess("数据库连接成功");
            else _dialogService.ShowError("无法连接到数据库", "连接失败");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = "连接错误";
            StatusText = $"连接错误: {ex.Message}";
            _dialogService.ShowError($"连接错误: {ex.Message}", "错误");
        }
        finally { IsBusy = false; }
    }

    private async Task ExecuteLoadTables(object? parameter)
    {
        if (!IsConnected) return;
        IsBusy = true;
        StatusText = "正在加载表列表...";
        try
        {
            var tables = await _factory.GetTablesAsync(SelectedProvider, ConnectionString);
            Tables.Clear();
            foreach (var t in tables) Tables.Add(t);
            StatusText = $"已加载 {tables.Count} 个表";
        }
        catch (Exception ex)
        {
            StatusText = $"加载表失败: {ex.Message}";
            _dialogService.ShowError($"加载表失败: {ex.Message}", "错误");
        }
        finally { IsBusy = false; }
    }

    private async Task ExecuteLoadColumns(object? parameter)
    {
        if (SelectedTable == null || !IsConnected) return;
        await LoadColumnsForTableAsync(SelectedTable.Name);
    }

    private async Task LoadColumnsForTableAsync(string tableName)
    {
        IsBusy = true;
        StatusText = $"正在加载 {tableName} 的列信息...";
        try
        {
            var columns = await _factory.GetColumnsAsync(SelectedProvider, ConnectionString, tableName);
            Columns.Clear();
            foreach (var c in columns) Columns.Add(c);
            StatusText = $"已加载 {columns.Count} 列";
        }
        catch (Exception ex)
        {
            StatusText = $"加载列失败: {ex.Message}";
            _dialogService.ShowError($"加载列失败: {ex.Message}", "错误");
        }
        finally { IsBusy = false; }
    }

    private void ExecuteAutoMatch()
    {
        if (Columns.Count == 0 || TemplateFields.Count == 0) return;
        DbFieldMappings.Clear();
        foreach (var col in Columns)
        {
            var match = TemplateFields.FirstOrDefault(f =>
                string.Equals(f.DataPath, col.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.Label, col.Name, StringComparison.OrdinalIgnoreCase) ||
                f.DataPath.EndsWith($".{col.Name}", StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                DbFieldMappings.Add(new DbFieldMapping
                {
                    ColumnName = col.Name,
                    TargetDataPath = match.DataPath
                });
            }
        }
        StatusText = $"自动匹配完成: {DbFieldMappings.Count}/{Columns.Count} 列";
    }

    private async Task ExecutePreview(object? parameter)
    {
        SyncConfig();
        IsBusy = true;
        StatusText = "正在预览数据...";
        try
        {
            var result = await _factory.PreviewAsync(Config);
            if (result.Success)
            {
                PreviewData = FormatPreviewData(result.BatchData);
                StatusText = $"预览完成, {result.BatchData.Count} 行";
            }
            else
            {
                PreviewData = string.Empty;
                StatusText = $"预览失败: {result.ErrorMessage}";
                _dialogService.ShowError(result.ErrorMessage ?? "预览失败", "错误");
            }
        }
        catch (Exception ex)
        {
            PreviewData = string.Empty;
            StatusText = $"预览失败: {ex.Message}";
            _dialogService.ShowError($"预览失败: {ex.Message}", "错误");
        }
        finally { IsBusy = false; }
    }

    private async Task ExecuteQuery(object? parameter)
    {
        SyncConfig();
        IsBusy = true;
        StatusText = "正在执行查询...";
        try
        {
            var result = await _factory.ExecuteQueryAsync(Config);
            if (result.Success)
            {
                var rowCount = result.Data.Count > 0 ? 1 : 0;
                _dialogService.ShowSuccess($"查询成功, 返回 {rowCount} 行数据");
                StatusText = $"查询完成, {rowCount} 行";
            }
            else
            {
                _dialogService.ShowError(result.ErrorMessage ?? "查询失败", "错误");
                StatusText = $"查询失败: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"查询失败: {ex.Message}", "错误");
            StatusText = $"查询失败: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    private void ExecuteSaveConfig()
    {
        SyncConfig();
        try
        {
            var existing = _configStore.Load(_template.Name);
            var idx = existing.FindIndex(c => c.AdapterId == Config.AdapterId);
            if (idx >= 0) existing[idx] = Config;
            else existing.Add(Config);
            _configStore.Save(_template.Name, existing);
            StatusText = "配置已保存";
            _dialogService.ShowSuccess("配置已保存");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"保存失败: {ex.Message}", "错误");
        }
    }

    private void SyncConfig()
    {
        Config.Provider = SelectedProvider;
        Config.ConnectionString = ConnectionString;
        Config.Query.Mode = CurrentQueryMode;
        Config.Query.RawSql = RawSql;
        Config.Query.PrimaryTable = PrimaryTable;
        Config.Query.SelectedColumns = SelectedColumnNames.Any() ? SelectedColumnNames.ToList() : new List<string>();
        Config.Query.WhereClause = WhereClause;
        Config.Query.OrderBy = OrderBy;
        Config.Joins = Joins.ToList();
        Config.Parameters = Parameters.ToList();
        Config.DbFieldMappings = DbFieldMappings.ToList();
    }

    private static string FormatPreviewData(List<Dictionary<string, object>> batchData)
    {
        if (batchData.Count == 0) return "(无数据)";
        var lines = new List<string>();
        foreach (var row in batchData)
        {
            var parts = row.Select(kv => $"{kv.Key}={kv.Value}");
            lines.Add(string.Join(", ", parts));
        }
        return string.Join(Environment.NewLine, lines);
    }
}
