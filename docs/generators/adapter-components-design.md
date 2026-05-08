# 适配器组件设计文档

> 目标：为 ReportDataMaker (WPF) 设计可插拔的数据适配器体系，支持 Excel 导入、数据库查询、API 查询三种数据源
> 日期：2026-05-08（修订）

---

## 1. 设计原则

- **契约优先**：适配器通过 `IDataAdapter` 接口与宿主解耦，可独立开发和测试
- **模板驱动**：适配器的字段映射基于已加载模板的 `DataPath` 定义，而非硬编码
- **配置持久化**：每个模板可保存多个适配器配置，下次打开自动恢复
- **枚举约束**：适配器类型通过枚举定义，编译期可检查，避免字符串魔法值
- **WPF 兼容**：适配器的配置 UI 以 TabItem 形式加载到主界面 TabControl 中

---

## 2. 核心接口设计

### 2.1 适配器类型枚举

```csharp
public enum AdapterType
{
    Context,   // 上下文/配置适配器（基础，自动执行）
    Excel,
    Database,
    Api
}

public enum DatabaseProvider
{
    SqlServer,
    MySql,
    Sqlite,
    PostgreSql
}
```

### 2.2 适配器接口

```csharp
public interface IDataAdapter
{
    string AdapterName { get; }
    AdapterType Type { get; }
    IReadOnlyList<string> TargetDataPaths { get; }
    
    Task<AdapterResult> ReadDataAsync();
    Task<AdapterResult> ReadBatchDataAsync();
    Task<ValidationResult> ValidateConfigAsync();
}

public class AdapterResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public List<Dictionary<string, object>> BatchData { get; set; }
}
```

### 2.3 适配器配置基类

```csharp
public abstract class AdapterConfigBase
{
    public string AdapterId { get; set; } = Guid.NewGuid().ToString();
    public AdapterType Type { get; set; }
    public string DisplayName { get; set; }
    public Dictionary<string, string> FieldMappings { get; set; } = new();
}
```

### 2.4 适配器工厂

```csharp
public interface IAdapterFactory
{
    AdapterType Type { get; }
    IDataAdapter Create(AdapterConfigBase config);
    FrameworkElement CreateConfigView(AdapterConfigBase config, TemplateFieldSchema schema);
}
```

---

## 3. Excel 适配器 — 小型适配工厂

### 3.1 核心思路

Excel 适配器本质上是一套**小型适配工厂**：

1. **仅针对可输入项**：只处理模板中标记为 Editable 的元素，Fixed 和 DataAdapter 元素不参与
2. **扁平化**：将所有可输入项（含 Table 中的可编辑 Cell）扁平化为单行 Key-Value 结构
3. **表格行列固定**：Table 元素的行数和列数由模板定义决定，不支持动态增减行列。扁平化时按模板定义的固定行列展开
4. **强类型约束**：每个可输入项的输入类型（Text/Number/Date/Dropdown）由模板严格定义，导入时必须校验类型匹配
5. **导出契约**：基于扁平化结果生成 xlsx 模板，隐藏行存储 DataPath + DataType 作为契约
6. **契约读取**：导入时依据 xlsx 中的契约行（DataPath）精确匹配，而非列头文本

### 3.2 扁平化规则

```csharp
public class TemplateFieldSchema
{
    public List<FlatField> Fields { get; set; } = new();
}

public class FlatField
{
    public string DataPath { get; set; }      // "Patient.Name", "Table1.Cell[0][2]"
    public string Label { get; set; }         // "姓名", "白细胞计数"
    public FieldDataType DataType { get; set; }
    public string Format { get; set; }        // 日期格式、数字格式
    public List<string> Options { get; set; } // Dropdown 的固定选项列表
    public bool IsRequired { get; set; }
    public double? MinValue { get; set; }     // Number 约束
    public double? MaxValue { get; set; }     // Number 约束
    public int? DecimalPlaces { get; set; }   // Number 精度约束
}

public enum FieldDataType
{
    Text,
    Number,
    Date,
    Dropdown,
    Boolean
}
```

扁平化策略（仅处理 Editable 元素）：

| 元素类型 | 扁平化方式 | 约束继承 |
|---------|-----------|---------|
| TextElement (Editable) | 直接取 DataPath + Label | 无特殊约束 |
| NumberElement (Editable) | DataPath + Label + Min/Max/Decimal | 数值范围和精度 |
| DateElement (Editable) | DataPath + Label + Format | 日期格式约束 |
| DropdownElement (Editable) | DataPath + Label + Options | 值必须在选项列表内 |
| TableElement 可编辑 Cell | 按固定行列展开为 `Table.Cell[row][col]` | 继承 Cell 的类型约束 |
| CheckboxElement (Editable) | DataPath + Label | Boolean 约束 |

**不参与扁平化的元素**：
- `ElementGroup.Fixed` — 固定内容，不可编辑
- `ElementGroup.DataAdapter` — 由其他适配器填充
- Table 的不可编辑 Cell（如表头行）
- 所有非 Editable 的元素

### 3.3 表格扁平化示例

模板定义一个 3行4列 的检验结果表（第1行为表头，不可编辑）：

```
扁平化结果：
- Table1.Cell[1][0] → "项目名称" (Dropdown, Options: ["白细胞","红细胞",...])
- Table1.Cell[1][1] → "结果" (Number, DecimalPlaces: 2)
- Table1.Cell[1][2] → "单位" (Text)
- Table1.Cell[1][3] → "参考范围" (Text)
- Table1.Cell[2][0] → "项目名称" (Dropdown, Options: [...])
- Table1.Cell[2][1] → "结果" (Number, DecimalPlaces: 2)
- Table1.Cell[2][2] → "单位" (Text)
- Table1.Cell[2][3] → "参考范围" (Text)
```

行数固定为模板定义的 2 行数据行（不含表头），不可增减。

### 3.4 导入校验规则

```csharp
public class ImportValidationRule
{
    public static ValidationResult Validate(FlatField field, object value)
    {
        return field.DataType switch
        {
            FieldDataType.Number => ValidateNumber(field, value),
            FieldDataType.Date => ValidateDate(field, value),
            FieldDataType.Dropdown => ValidateDropdown(field, value),
            FieldDataType.Boolean => ValidateBoolean(value),
            _ => ValidationResult.Success
        };
    }
    
    private static ValidationResult ValidateNumber(FlatField field, object value)
    {
        // 1. 必须可解析为数字
        // 2. 必须在 MinValue ~ MaxValue 范围内
        // 3. 小数位数不超过 DecimalPlaces
    }
    
    private static ValidationResult ValidateDropdown(FlatField field, object value)
    {
        // 值必须存在于 field.Options 列表中
    }
}
```

### 3.3 导出模板结构

```
┌─────────────────────────────────────────────────────────────┐
│ Row 1 (隐藏): DataPath 契约行                                │
│ Patient.Name │ Patient.Age │ Report.Date │ Items[0].Result  │
├─────────────────────────────────────────────────────────────┤
│ Row 2: 人类可读列头 (Label)                                  │
│ 姓名         │ 年龄        │ 检验日期     │ 检验结果         │
├─────────────────────────────────────────────────────────────┤
│ Row 3: 数据类型提示 (隐藏)                                   │
│ text         │ number      │ date        │ text             │
├─────────────────────────────────────────────────────────────┤
│ Row 4+: 用户填写数据区                                       │
│              │             │             │                  │
└─────────────────────────────────────────────────────────────┘
```

### 3.4 配置模型

```csharp
public class ExcelAdapterConfig : AdapterConfigBase
{
    public string FilePath { get; set; }
    public string SheetName { get; set; }
    public ExcelTemplateSchema ExportedSchema { get; set; }  // 导出时生成的契约快照
    public ImportMode Mode { get; set; } = ImportMode.Single;
}

public class ExcelTemplateSchema
{
    public int ContractRow { get; set; } = 1;   // DataPath 契约行
    public int LabelRow { get; set; } = 2;      // 人类可读列头行
    public int TypeRow { get; set; } = 3;       // 数据类型行
    public int DataStartRow { get; set; } = 4;  // 数据起始行
    public List<FlatField> Fields { get; set; } // 字段定义快照
}

public enum ImportMode
{
    Single,   // 单行导入
    Batch     // 批量导入（每行一份报告）
}
```

### 3.5 工作流程

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ 1. 加载模板       │────▶│ 2. 扁平化 Editable│────▶│ 3. 导出 xlsx 模板 │
│ ExternalTemplate │     │ → FlatField[]    │     │ 含契约行+类型行  │
└──────────────────┘     └──────────────────┘     └──────────────────┘
                                                          │
                                                          ▼ 用户填写
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ 5. 数据填充       │◀────│ 4. 按契约行解析   │◀────│ 用户导入 xlsx     │
│ DataPath→Value   │     │ DataPath 精确匹配 │     │                  │
└──────────────────┘     └──────────────────┘     └──────────────────┘
```

### 3.6 Excel 适配工厂内部结构

```csharp
public class ExcelAdapterFactory : IAdapterFactory
{
    public AdapterType Type => AdapterType.Excel;
    
    // 内部组件
    private readonly TemplateFlattenService _flattener;      // 模板扁平化
    private readonly ExcelSchemaExporter _exporter;          // 导出 xlsx 契约模板
    private readonly ExcelContractReader _reader;            // 按契约读取数据
    private readonly ExcelDataValidator _validator;          // 数据类型校验
}

public class TemplateFlattenService
{
    public TemplateFieldSchema Flatten(ExternalTemplateDefinition template);
    // 递归遍历所有 Editable 元素，展开嵌套结构为扁平 FlatField 列表
}

public class ExcelSchemaExporter
{
    public void ExportTemplate(string filePath, TemplateFieldSchema schema);
    // 生成带契约行的 xlsx 模板
}

public class ExcelContractReader
{
    public AdapterResult ReadByContract(string filePath, ExcelTemplateSchema schema);
    // 读取第一行隐藏的 DataPath 契约，按 DataPath 精确匹配数据列
    
    public AdapterResult ReadBatchByContract(string filePath, ExcelTemplateSchema schema);
    // 批量读取所有数据行
}
```

### 3.7 配置 UI 设计

```
┌─────────────────────────────────────────────────────────────┐
│ Excel 适配器                                    [TabItem]    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─ 模板导出 ──────────────────────────────────────────────┐ │
│ │ 当前模板可编辑字段: 12 个                                │ │
│ │ [预览扁平化结果]  [导出 Excel 模板]                      │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─ 数据导入 ──────────────────────────────────────────────┐ │
│ │ 文件: [________________________] [浏览...]               │ │
│ │ 模式: ○ 单行导入  ● 批量导入                             │ │
│ │                                                         │ │
│ │ 契约校验: ✅ DataPath 匹配 12/12                         │ │
│ │ 数据行数: 45 行                                         │ │
│ │                                                         │ │
│ │ [校验数据]  [导入]                                      │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─ 校验结果 ──────────────────────────────────────────────┐ │
│ │ ⚠ 第 5 行: "年龄" 期望 Number，实际值 "二十"            │ │
│ │ ⚠ 第 12 行: "检验日期" 格式不匹配                       │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. 数据库查询适配器 — Base + Provider 组件架构

### 4.1 设计思路

数据库适配器采用 **Base 抽象 + Provider 组件** 的分层架构：

- `DatabaseAdapterBase`：定义通用的查询执行、字段映射、参数管理逻辑
- `IDatabaseProvider`：各数据库的连接、元数据获取、SQL 方言差异由 Provider 组件实现
- 可视化操作通过 Provider 提供的元数据（表列表、列信息、数据类型）驱动

```
┌─────────────────────────────────────────────────┐
│           DatabaseAdapterBase                    │
│  - 查询执行引擎                                  │
│  - 字段映射管理                                  │
│  - 参数化查询                                    │
│  - 结果转换                                      │
├─────────────────────────────────────────────────┤
│         IDatabaseProvider                        │
├──────────┬──────────┬──────────┬────────────────┤
│SqlServer │  MySql   │  Sqlite  │  PostgreSql    │
│Provider  │ Provider │ Provider │  Provider      │
└──────────┴──────────┴──────────┴────────────────┘
```

### 4.2 Provider 接口

```csharp
public interface IDatabaseProvider
{
    DatabaseProvider ProviderType { get; }
    string DisplayName { get; }
    
    // 连接管理
    Task<bool> TestConnectionAsync(string connectionString);
    DbConnection CreateConnection(string connectionString);
    
    // 元数据获取（驱动可视化操作）
    Task<List<TableInfo>> GetTablesAsync(string connectionString);
    Task<List<ColumnInfo>> GetColumnsAsync(string connectionString, string tableName);
    Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string tableName);
    
    // SQL 方言
    string BuildPagedQuery(string baseSql, int offset, int limit);
    string QuoteIdentifier(string identifier);
    string GetParameterPrefix();  // @ for SqlServer/Sqlite, @ for MySql, : for PostgreSql
}

public class TableInfo
{
    public string Schema { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }  // "TABLE", "VIEW"
}

public class ColumnInfo
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public int? MaxLength { get; set; }
}

public class ForeignKeyInfo
{
    public string ColumnName { get; set; }
    public string ReferencedTable { get; set; }
    public string ReferencedColumn { get; set; }
}
```

### 4.3 Base 适配器

```csharp
public class DatabaseAdapterBase : IDataAdapter
{
    private readonly IDatabaseProvider _provider;
    private readonly DatabaseAdapterConfig _config;
    
    public string AdapterName => _config.DisplayName;
    public AdapterType Type => AdapterType.Database;
    
    public DatabaseAdapterBase(IDatabaseProvider provider, DatabaseAdapterConfig config)
    {
        _provider = provider;
        _config = config;
    }
    
    public async Task<AdapterResult> ReadDataAsync()
    {
        using var conn = _provider.CreateConnection(_config.ConnectionString);
        await conn.OpenAsync();
        
        var sql = BuildSql();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        ApplyParameters(cmd);
        
        using var reader = await cmd.ExecuteReaderAsync();
        return MapResult(reader);
    }
    
    private string BuildSql() { /* 根据 QueryConfig 构建 SQL */ }
    private void ApplyParameters(DbCommand cmd) { /* 参数化 */ }
    private AdapterResult MapResult(DbDataReader reader) { /* 按 FieldMappings 转换 */ }
}
```

### 4.4 Provider 实现示例

```csharp
public class SqlServerProvider : IDatabaseProvider
{
    public DatabaseProvider ProviderType => DatabaseProvider.SqlServer;
    public string DisplayName => "SQL Server";
    
    public DbConnection CreateConnection(string connectionString)
        => new SqlConnection(connectionString);
    
    public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
    {
        // SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES
    }
    
    public string QuoteIdentifier(string id) => $"[{id}]";
    public string GetParameterPrefix() => "@";
}

public class MySqlProvider : IDatabaseProvider { /* ... */ }
public class SqliteProvider : IDatabaseProvider { /* ... */ }
public class PostgreSqlProvider : IDatabaseProvider { /* ... */ }
```

### 4.5 配置模型

```csharp
public class DatabaseAdapterConfig : AdapterConfigBase
{
    public DatabaseProvider Provider { get; set; }
    public string ConnectionString { get; set; }
    public QueryConfig Query { get; set; }
    public List<DbFieldMapping> DbFieldMappings { get; set; } = new();
    public List<QueryParameter> Parameters { get; set; } = new();
    public List<JoinDefinition> Joins { get; set; } = new();
}

public class QueryConfig
{
    public QueryMode Mode { get; set; }
    public string RawSql { get; set; }
    public string PrimaryTable { get; set; }
    public List<string> SelectedColumns { get; set; }
    public string WhereClause { get; set; }
    public string OrderBy { get; set; }
}

public enum QueryMode { RawSql, VisualBuilder }

public class DbFieldMapping
{
    public string ColumnName { get; set; }
    public string TargetDataPath { get; set; }
    public string Transform { get; set; }
}

public class QueryParameter
{
    public string Name { get; set; }
    public string Label { get; set; }
    public FieldDataType DataType { get; set; }
    public string DefaultValue { get; set; }
    public ParameterSource Source { get; set; }
}

public enum ParameterSource { Manual, FromTemplate, FromContext }

public class JoinDefinition
{
    public string LeftTable { get; set; }
    public string LeftColumn { get; set; }
    public string RightTable { get; set; }
    public string RightColumn { get; set; }
    public JoinType Type { get; set; }
}

public enum JoinType { Inner, Left, Right }
```

### 4.6 可视化操作流程

Provider 的元数据能力驱动 UI 的可视化操作：

```
用户选择 Provider
    │
    ▼ GetTables()
┌──────────────┐
│ 表/视图列表   │ ← 树形展示，可展开查看列
└──────┬───────┘
       │ 选择表 → GetColumns()
       ▼
┌──────────────┐
│ 列信息列表    │ ← 显示类型、是否主键、可空
└──────┬───────┘
       │ GetForeignKeys()
       ▼
┌──────────────┐
│ 关联关系      │ ← 自动推荐 JOIN 条件
└──────┬───────┘
       │ 用户确认映射
       ▼
┌──────────────┐
│ 生成 SQL 预览 │ ← 可手动微调
└──────────────┘
```

### 4.7 配置 UI 设计

```
┌─────────────────────────────────────────────────────────────┐
│ 数据库适配器                                    [TabItem]    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─ 连接配置 ──────────────────────────────────────────────┐ │
│ │ 数据库类型: [SQL Server ▼] [MySQL ▼] [SQLite ▼] [PgSQL]│ │
│ │                                                         │ │
│ │ ┌─ SQL Server 连接参数 ───────────────────────────────┐ │ │
│ │ │ 服务器: [localhost\SQLEXPRESS___]                    │ │ │
│ │ │ 数据库: [HIS_DB__________________]                  │ │ │
│ │ │ 认证:   ○ Windows  ● SQL Server                     │ │ │
│ │ │ 用户名: [sa_______]  密码: [********]               │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ │ [测试连接]  状态: ● 已连接                               │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─ 查询构建 ──────────────────────────────────────────────┐ │
│ │ 模式: ○ 可视化构建  ● SQL 语句                           │ │
│ │                                                         │ │
│ │ [可视化模式时:]                                          │ │
│ │ 主表: [Patients ▼]                                      │ │
│ │ 关联: [+ 添加 JOIN]                                     │ │
│ │   └ LEFT JOIN Reports ON Patients.Id = Reports.PatientId│ │
│ │ 条件: [Patients.Id = @patientId]                        │ │
│ │                                                         │ │
│ │ [SQL模式时:]                                            │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │ SELECT p.Name, p.Age, r.Result                      │ │ │
│ │ │ FROM Patients p                                     │ │ │
│ │ │ LEFT JOIN Reports r ON p.Id = r.PatientId           │ │ │
│ │ │ WHERE p.Id = @patientId                             │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ │ [执行预览 (前10行)]                                      │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─ 参数定义 ──────────────────────────────────────────────┐ │
│ │ @patientId  类型:[String ▼]  来源:[手动输入 ▼]  默认:[] │ │
│ │ [+ 添加参数]                                            │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─ 字段映射 ──────────────────────────────────────────────┐ │
│ │ ┌────────────┬──────────────┬────────────┐              │ │
│ │ │ 查询结果列  │ 模板字段      │ 转换       │              │ │
│ │ ├────────────┼──────────────┼────────────┤              │ │
│ │ │ Name       │ Patient.Name ▼│            │              │ │
│ │ │ Age        │ Patient.Age  ▼│            │              │ │
│ │ │ Result     │ Items[0].Val ▼│ Trim()     │              │ │
│ │ └────────────┴──────────────┴────────────┘              │ │
│ │ [自动匹配]                                              │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ [保存配置]  [执行查询并填充]                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. API 查询适配器（预留设计）

### 5.1 预期能力

| 功能 | 说明 | 状态 |
|------|------|------|
| HTTP 请求配置 | GET/POST，URL，Headers，Body | 待设计 |
| 认证方式 | Bearer Token / Basic / API Key / OAuth2 | 待设计 |
| 响应解析 | JSONPath / XPath 提取字段 | 待设计 |
| 字段映射 | 响应字段 → 模板 DataPath | 待设计 |
| 轮询/WebHook | 定时拉取或被动接收 | 待设计 |
| 链式调用 | 多个 API 串联，前一个的结果作为后一个的参数 | 待设计 |

### 5.2 配置模型骨架

```csharp
public class ApiAdapterConfig : AdapterConfigBase
{
    public string BaseUrl { get; set; }
    public HttpMethod Method { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public AuthConfig Authentication { get; set; }
    public string RequestBodyTemplate { get; set; }
    public string ResponseDataPath { get; set; }  // JSONPath
    public List<ApiFieldMapping> ApiFieldMappings { get; set; }
}
```

### 5.3 设计约束

API 适配器的复杂度显著高于 Excel 和数据库：
- 需要处理异步、超时、重试
- 认证流程多样
- 响应结构不可预测
- 可能需要多步编排

**建议**：在 Excel 和数据库适配器稳定后再启动 API 适配器的详细设计。

---

## 6. Context 适配器 — 基础配置数据源

### 6.1 定位

Context 适配器是所有适配器中最基础的一个，负责从本地配置和系统上下文自动填充固定信息。它与其他适配器的区别：

| 特性 | Context | Excel/Database/API |
|------|---------|-------------------|
| 执行时机 | 模板加载后自动执行 | 用户手动触发 |
| 配置频率 | 一次配置，全局共享 | 每个模板独立配置 |
| 数据来源 | 本地配置文件 + 系统时间 | 外部文件/数据库/网络 |
| 用户交互 | 无（静默填充） | 需要用户操作 |

### 6.2 认领的 DataPath

Context 适配器通过 DataPath 前缀 `Context.` 认领元素：

```
Context.Hospital.Name       → 医院名称
Context.Hospital.Address    → 医院地址
Context.Hospital.Phone      → 联系电话
Context.Hospital.Logo       → 医院 Logo
Context.Department.Name     → 科室名称
Context.Department.Code     → 科室代码
Context.Doctor.Name         → 报告医生
Context.Doctor.Id           → 医生工号
Context.Doctor.Title        → 医生职称
Context.Reviewer.Name       → 审核医生
Context.Report.Date         → 报告日期（自动生成）
Context.Report.Time         → 报告时间（自动生成）
Context.Report.PrintTime    → 打印时间（导出时生成）
Context.Report.SerialNo     → 报告编号（规则生成）
```

### 6.3 配置存储

Context 配置是全局的，存储在应用配置目录：

```
Configs/
├── institution.json    # 机构信息（医院名称、地址等）
├── department.json     # 科室信息
├── user.json           # 当前用户信息
└── auto-fields.json    # 自动字段规则（日期格式、编号规则）
```

### 6.4 实现

```csharp
public class ContextAdapter : IDataAdapter
{
    public string AdapterName => "上下文配置";
    public AdapterType Type => AdapterType.Context;
    
    private readonly ContextAdapterConfig _config;
    
    public IReadOnlyList<string> TargetDataPaths => 
        _config.GetAllPaths();  // 返回所有 Context.* 路径
    
    public Task<AdapterResult> ReadDataAsync()
    {
        var data = new Dictionary<string, object>();
        
        // 机构信息
        data["Context.Hospital.Name"] = _config.Institution.HospitalName;
        data["Context.Hospital.Address"] = _config.Institution.HospitalAddress;
        // ...
        
        // 自动字段
        foreach (var rule in _config.AutoFields)
        {
            data[rule.DataPath] = rule.Type switch
            {
                AutoFieldType.CurrentDate => DateTime.Now.ToString(rule.Format),
                AutoFieldType.CurrentDateTime => DateTime.Now.ToString(rule.Format),
                AutoFieldType.SerialNumber => GenerateSerial(rule.Format),
                _ => ""
            };
        }
        
        return Task.FromResult(new AdapterResult { Success = true, Data = data });
    }
}
```

### 6.5 在 WPF 中的呈现

Context 适配器不需要独立 Tab 页签（因为配置稳定），而是作为「设置」的一部分：
- 首次使用时通过设置页面配置
- 模板加载后自动执行，填充结果在数据录入 Tab 中以灰色/只读形式展示
- 用户可在设置中修改，修改后自动刷新所有已填充的 Context 字段

---

## 7. 适配器执行顺序与数据优先级

```
模板加载完成
    │
    ▼ (自动)
┌──────────────┐
│ 1. Context   │ 填充：医院、科室、医生、日期
└──────┬───────┘
       │
       ▼ (用户触发)
┌──────────────┐
│ 2. DataAdapter│ 填充：外部数据源字段
└──────┬───────┘
       │
       ▼ (用户手动)
┌──────────────┐
│ 3. Editable  │ 填充：手动录入字段
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ 全部就绪      │ → 预览 / 导出
└──────────────┘
```

**冲突规则**：后执行的覆盖先执行的。如果 DataAdapter 返回了一个 Context 已填充的字段，以 DataAdapter 为准（外部数据优先于本地配置）。

---

## 8. 适配器配置持久化

### 6.1 存储结构

每个模板的适配器配置保存为独立 JSON 文件：

```
Templates/
├── 检验报告单.json              # 模板定义
├── 检验报告单.adapters.json     # 适配器配置集合
└── ...
```

### 6.2 配置文件格式

```json
{
  "templateId": "检验报告单",
  "templateVersion": "v2.2.0",
  "adapters": [
    {
      "adapterId": "guid-1",
      "adapterType": "Excel",
      "displayName": "批量导入-检验数据",
      "config": {
        "exportedSchema": { "fields": [...] }
      }
    },
    {
      "adapterId": "guid-2",
      "adapterType": "Database",
      "displayName": "LIS系统查询",
      "config": {
        "provider": "SqlServer",
        "connectionString": "...",
        "query": { "mode": "RawSql", "rawSql": "..." },
        "dbFieldMappings": [...]
      }
    }
  ]
}
```

---

## 9. 适配器注册与发现

```csharp
public class AdapterRegistry
{
    private readonly Dictionary<AdapterType, IAdapterFactory> _factories = new();

    public void Register(AdapterType type, IAdapterFactory factory);
    public IDataAdapter Create(AdapterConfigBase config);
    public FrameworkElement CreateConfigView(AdapterConfigBase config, TemplateFieldSchema schema);
    public IReadOnlyList<AdapterType> GetRegisteredTypes();
}
```

启动时注册：
```csharp
var registry = new AdapterRegistry();
registry.Register(AdapterType.Context, new ContextAdapterFactory());  // 基础适配器
registry.Register(AdapterType.Excel, new ExcelAdapterFactory());
registry.Register(AdapterType.Database, new DatabaseAdapterFactory());
// registry.Register(AdapterType.Api, new ApiAdapterFactory());  // 后续
```

Database Provider 注册：
```csharp
var dbProviderRegistry = new DatabaseProviderRegistry();
dbProviderRegistry.Register(new SqlServerProvider());
dbProviderRegistry.Register(new MySqlProvider());
dbProviderRegistry.Register(new SqliteProvider());
dbProviderRegistry.Register(new PostgreSqlProvider());
```

---

## 10. 与现有代码的关系

| 现有实现 | 重构方向 |
|---------|---------|
| `ExcelImportAdapter` | 拆分为 TemplateFlattenService + ExcelSchemaExporter + ExcelContractReader |
| `DatabaseAdapter` | 拆分为 DatabaseAdapterBase + IDatabaseProvider 实现 |
| `DataAdapterService` | 演化为 `AdapterRegistry` |
| `DbAdapterConfig` | 演化为 `DatabaseAdapterConfig`（枚举化 Provider） |
| `IDataAdapter`（现有） | 扩展为新版接口（增加 Batch、Validate、枚举 Type） |
