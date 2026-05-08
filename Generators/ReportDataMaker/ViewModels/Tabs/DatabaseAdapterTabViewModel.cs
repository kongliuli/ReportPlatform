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

public class DatabaseAdapterTabViewModel : TabViewModelBase
{
    private readonly ExternalTemplateDefinition _template;
    private readonly DatabaseAdapterFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly AdapterConfigStore _configStore;

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

    public DatabaseAdapterConfig Config { get; }
    public List<FlatField> TemplateFields { get; }
    public ObservableCollection<TableInfo> Tables { get; } = new();
    public ObservableCollection<ColumnInfo> Columns { get; } = new();
    public ObservableCollection<JoinDefinition> Joins { get; } = new();
    public ObservableCollection<QueryParameter> Parameters { get; } = new();
    public ObservableCollection<DbFieldMapping> DbFieldMappings { get; } = new();

    public IReadOnlyList<IDatabaseProvider> Providers => _factory.GetAllProviders();
    public List<JoinType> JoinTypeList { get; } = Enum.GetValues(typeof(JoinType)).Cast<JoinType>().ToList();
    public List<FieldDataType> FieldDataTypeList { get; } = Enum.GetValues(typeof(FieldDataType)).Cast<FieldDataType>().ToList();
    public List<ParameterSource> ParameterSourceList { get; } = Enum.GetValues(typeof(ParameterSource)).Cast<ParameterSource>().ToList();
    public List<string> TemplateDataPathList => TemplateFields.Select(f => f.DataPath).ToList();

    private IDatabaseProvider? _selectedProviderInfo;
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
    public DatabaseProvider SelectedProvider
    {
        get => _selectedProvider;
        set { if (SetProperty(ref _selectedProvider, value)) Config.Provider = value; }
    }

    private string _connectionString = string.Empty;
    public string ConnectionString
    {
        get => _connectionString;
        set { if (SetProperty(ref _connectionString, value)) Config.ConnectionString = value; }
    }

    private bool _isConnected;
    public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }

    private string _connectionStatus = "未连接";
    public string ConnectionStatus { get => _connectionStatus; set => SetProperty(ref _connectionStatus, value); }

    private TableInfo? _selectedTable;
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

    public bool IsRawSqlMode
    {
        get => CurrentQueryMode == QueryMode.RawSql;
        set { if (value) CurrentQueryMode = QueryMode.RawSql; }
    }

    public bool IsVisualBuilderMode
    {
        get => CurrentQueryMode == QueryMode.VisualBuilder;
        set { if (value) CurrentQueryMode = QueryMode.VisualBuilder; }
    }

    private string _rawSql = string.Empty;
    public string RawSql
    {
        get => _rawSql;
        set { if (SetProperty(ref _rawSql, value)) Config.Query.RawSql = value; }
    }

    private string _primaryTable = string.Empty;
    public string PrimaryTable
    {
        get => _primaryTable;
        set { if (SetProperty(ref _primaryTable, value)) Config.Query.PrimaryTable = value; }
    }

    private string _whereClause = string.Empty;
    public string WhereClause
    {
        get => _whereClause;
        set { if (SetProperty(ref _whereClause, value)) Config.Query.WhereClause = value; }
    }

    private string _orderBy = string.Empty;
    public string OrderBy
    {
        get => _orderBy;
        set { if (SetProperty(ref _orderBy, value)) Config.Query.OrderBy = value; }
    }

    private string _previewData = string.Empty;
    public string PreviewData
    {
        get => _previewData;
        set { if (SetProperty(ref _previewData, value)) OnPropertyChanged(nameof(HasPreviewData)); }
    }

    public bool HasPreviewData => !string.IsNullOrEmpty(PreviewData);

    private string _statusText = "就绪";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand TestConnectionCommand { get; }
    public AsyncRelayCommand LoadTablesCommand { get; }
    public AsyncRelayCommand LoadColumnsCommand { get; }
    public AsyncRelayCommand ExecutePreviewCommand { get; }
    public AsyncRelayCommand ExecuteQueryCommand { get; }
    public RelayCommand AddJoinCommand { get; }
    public RelayCommand RemoveJoinCommand { get; }
    public RelayCommand AddParameterCommand { get; }
    public RelayCommand RemoveParameterCommand { get; }
    public RelayCommand AddMappingCommand { get; }
    public RelayCommand RemoveMappingCommand { get; }
    public RelayCommand AutoMatchCommand { get; }
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
