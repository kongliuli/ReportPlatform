# v0.6.0 数据库适配器实施规格

## 概述
将现有 DatabaseAdapter 重构为 Base + Provider 分层架构，支持 SqlServer/MySql/Sqlite/PostgreSql 四种数据库，实现可视化查询构建与原生 SQL 双模式。

## 实施范围

### 1. Provider 层
- IDatabaseProvider 接口：连接管理、元数据获取、SQL 方言
- 元数据模型：TableInfo, ColumnInfo, ForeignKeyInfo
- SqlServerProvider / MySqlProvider / SqliteProvider / PostgreSqlProvider
- DatabaseProviderRegistry

### 2. 适配器核心
- DatabaseAdapterConfig（QueryConfig, DbFieldMapping, QueryParameter, JoinDefinition）
- SqlBuilder（可视化模式 → SQL 生成）
- DatabaseAdapterBase（查询执行 + 字段映射 + 参数化 + 结果转换）

### 3. 工厂与集成
- DatabaseAdapterFactory
- MainViewModel 添加数据库适配器 Tab 创建逻辑
- App.xaml.cs DI 注册

### 4. ViewModel
- DatabaseAdapterTabViewModel（连接/查询/参数/映射/预览/保存）

### 5. View
- DatabaseAdapterTab.xaml + .cs（四区布局：连接/查询/参数/映射）

## 约束
- 使用 Guid.NewGuid().ToString("N") 生成短 ID
- 不添加注释
- 遵循现有代码风格和命名空间约定
- DatabaseProvider 枚举已在 Contracts/Enums/AdapterType.cs 中定义
- Npgsql 需新增到 csproj
