# v0.6.0 任务清单

## Phase 1: Provider 层
- [ ] IDatabaseProvider.cs + TableInfo.cs
- [ ] SqlServerProvider.cs
- [ ] MySqlProvider.cs
- [ ] SqliteProvider.cs
- [ ] PostgreSqlProvider.cs
- [ ] DatabaseProviderRegistry.cs

## Phase 2: 适配器核心
- [ ] DatabaseAdapterConfig.cs
- [ ] SqlBuilder.cs
- [ ] DatabaseAdapterBase.cs

## Phase 3: 工厂与集成
- [ ] DatabaseAdapterFactory.cs
- [ ] MainViewModel.cs 修改
- [ ] App.xaml.cs DI 注册

## Phase 4: ViewModel
- [ ] DatabaseAdapterTabViewModel.cs

## Phase 5: View
- [ ] DatabaseAdapterTab.xaml + .cs

## Phase 6: 验证
- [ ] 编译验证 (⚠️ 待 .NET SDK)
- [ ] 文档更新
