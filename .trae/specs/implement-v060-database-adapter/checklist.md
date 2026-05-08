# v0.6.0 检查清单

- [ ] IDatabaseProvider 接口完整（TestConnection, CreateConnection, GetTables, GetColumns, GetForeignKeys, BuildPagedQuery, QuoteIdentifier, GetParameterPrefix）
- [ ] 4 个 Provider 实现各自的 SQL 方言
- [ ] DatabaseProviderRegistry 支持注册和按类型查找
- [ ] DatabaseAdapterConfig 包含 QueryConfig/DbFieldMapping/QueryParameter/JoinDefinition
- [ ] SqlBuilder 可从 VisualBuilder 模式生成 SQL
- [ ] DatabaseAdapterBase 实现 ReadDataAsync/ReadBatchDataAsync/ValidateConfigAsync
- [ ] DatabaseAdapterFactory 整合 Provider + AdapterBase
- [ ] MainViewModel 支持添加数据库适配器 Tab
- [ ] App.xaml.cs 注册 DatabaseAdapterFactory 和 DatabaseProviderRegistry
- [ ] DatabaseAdapterTabViewModel 包含连接/查询/参数/映射/预览命令
- [ ] DatabaseAdapterTab.xaml 四区布局
- [ ] Npgsql 包引用已添加
