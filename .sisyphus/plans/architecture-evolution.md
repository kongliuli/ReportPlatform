# 架构演进工作计划

| 字段 | 值 |
|------|-----|
| 创建日期 | 2026-05-20 |
| 基于文档 | docs/2026-05-20/Decision_Aggregation.md |
| 总阶段 | 4 Phase |
| 预估周期 | 6-9 个月 |

---

## 当前状态快照

```
ReportPlatform.sln (4 项目)
├── Contracts/                    net8.0 类库 (51 文件)
├── Editor/Core/                  net8.0 类库 (26 文件)
├── Editor/Server/                net8.0 ASP.NET Core (9 文件)
└── Generators/ReportDataMaker/   net10.0-windows WPF (81 文件)
```

依赖链: Server → Core → Contracts ← ReportDataMaker

---

## Phase 1：基础加固（1-2 周）

### 目标
安全修复 + 独立决策点实施，无破坏性变更，可逐个合并。

---

### P1-01: JWT 密钥外部化

**决策点**: 安全修复（无对应决策编号）
**前置依赖**: 无
**影响范围**: Editor/Server, Editor/Core

**当前问题**:
- JWT 密钥硬编码在 `appsettings.json` 或代码中

**执行步骤**:

1. 检查当前 JWT 密钥配置位置
   - 读取 `Editor/Server/appsettings.json` 和 `Editor/Core/` 中的 JWT 相关代码
   - 确认密钥来源（硬编码 / 配置文件 / 环境变量）

2. 创建配置结构
   - 在 `appsettings.json` 中保留占位符 `"JwtSettings:SecretKey": ""`
   - 在 `appsettings.Development.json` 中放开发用密钥
   - 生产环境通过环境变量 `JWT_SECRET_KEY` 注入

3. 修改服务注册代码
   - `Editor/Core/Extensions/ServiceCollectionExtensions.cs` 中读取配置
   - 添加启动时校验：密钥为空则抛出明确异常

4. 更新 `.gitignore`
   - 确保 `appsettings.Production.json` 不被提交（如有敏感内容）

**验证**:
```bash
dotnet build Editor/Server/Xinglin.WebReportEditor.Server.csproj
grep -rn "SecretKey\|secret\|密钥" Editor/ --include="*.cs" --include="*.json"
```

**完成标准**:
- [ ] 密钥不在源码中硬编码
- [ ] 开发环境有默认值可正常启动
- [ ] 缺少密钥时启动失败并给出明确错误信息
- [ ] 编译通过

---

### P1-02: SQL 注入防护

**决策点**: 安全修复
**前置依赖**: 无
**影响范围**: Generators/ReportDataMaker/Services/DatabaseAdapter/

**当前问题**:
- `SqlBuilder` 或数据库适配器中可能存在字符串拼接 SQL

**执行步骤**:

1. 审计 SQL 构建代码
   - 搜索所有 `string.Format`、`$"SELECT`、`+ "WHERE"` 等模式
   - 重点检查 `DatabaseAdapter/` 目录下所有文件

2. 替换为参数化查询
   - 使用 `@param` 占位符 + `SqlParameter` / `MySqlParameter` / `NpgsqlParameter`
   - 对表名/列名（不能参数化的部分）使用白名单校验

3. 添加输入校验
   - 表名/列名仅允许 `[a-zA-Z0-9_]`
   - 拒绝包含 SQL 关键字的动态标识符

**验证**:
```bash
dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj
grep -rn "string.Format\|\\$\".*SELECT\|\\$\".*INSERT\|\\$\".*UPDATE\|\\$\".*DELETE" Generators/ReportDataMaker/Services/DatabaseAdapter/
```

**完成标准**:
- [ ] 所有用户输入通过参数化传递
- [ ] 动态标识符有白名单校验
- [ ] 无字符串拼接 SQL
- [ ] 编译通过

---

### P1-03: AsyncRelayCommand 异常处理

**决策点**: 安全修复
**前置依赖**: 无
**影响范围**: Generators/ReportDataMaker/ViewModels/

**执行步骤**:

1. 审计所有 AsyncRelayCommand 使用
   - 搜索 `new AsyncRelayCommand` 或异步命令绑定
   - 检查是否有未捕获异常导致静默失败

2. 添加全局异常处理
   - 在 `App.xaml.cs` 中注册 `DispatcherUnhandledException`
   - 在 `TaskScheduler.UnobservedTaskException` 中记录日志

3. 对关键命令添加 try-catch
   - PDF 导出、数据库连接、文件读写等 I/O 操作
   - 捕获后通过 `IDialogService` 显示用户友好错误

**验证**:
```bash
dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj
grep -rn "AsyncRelayCommand\|async.*Command" Generators/ReportDataMaker/ViewModels/ --include="*.cs"
```

**完成标准**:
- [ ] 全局异常处理器已注册
- [ ] 关键异步操作有 try-catch
- [ ] 异常信息对用户可见（非静默吞掉）
- [ ] 编译通过

---

### P1-04: 输入验证注解完善

**决策点**: 安全修复
**前置依赖**: 无
**影响范围**: Contracts/Requests/, Editor/Server/Controllers/

**执行步骤**:

1. 审计 Request DTO
   - 检查 `Contracts/Requests/` 下所有请求类
   - 确认哪些字段缺少 `[Required]`、`[MaxLength]`、`[Range]` 等注解

2. 添加验证注解
   - `CreateTemplateRequest`: Name 必填 + 长度限制
   - `UpdateTemplateRequest`: 同上
   - `TemplateFilterRequest`: 分页参数范围校验
   - `RollbackRequest`: VersionId 必填

3. 确认 Server 端启用了模型验证
   - 检查 `Program.cs` 是否有 `[ApiController]` 或手动 `ModelState.IsValid`

**验证**:
```bash
dotnet build ReportPlatform.sln
grep -rn "\[Required\]\|\[MaxLength\]\|\[Range\]" Contracts/Requests/
```

**完成标准**:
- [ ] 所有 Request DTO 关键字段有验证注解
- [ ] Server 端自动返回 400 对无效请求
- [ ] 编译通过

---

### P1-05: 元素类型 Attribute + 反射注册（决策 E2）

**决策点**: E2
**前置依赖**: 无
**影响范围**: Contracts/Models/Elements/, Contracts/Registry/

**设计意图**:
新增元素类型时无需修改注册表代码，通过 Attribute 标注 + 启动时反射扫描自动注册。

**执行步骤**:

1. 定义注册 Attribute
   - 在 `Contracts/` 中创建 `[ElementType("text")]` Attribute
   ```csharp
   // Contracts/Attributes/ElementTypeAttribute.cs
   [AttributeUsage(AttributeTargets.Class, Inherited = false)]
   public sealed class ElementTypeAttribute : Attribute
   {
       public string TypeName { get; }
       public ElementTypeAttribute(string typeName) => TypeName = typeName;
   }
   ```

2. 标注所有现有元素类
   - `TextElement` → `[ElementType("text")]`
   - `DateElement` → `[ElementType("date")]`
   - `TableElement` → `[ElementType("table")]`
   - ... 对 `Contracts/Models/Elements/` 下所有 25 个元素类添加

3. 重构 ElementGroupRegistry
   - 当前 `Contracts/Registry/ElementGroupRegistry.cs` 使用手动注册
   - 改为反射扫描：
   ```csharp
   public static class ElementTypeRegistry
   {
       private static readonly Dictionary<string, Type> _map = new();

       public static void Initialize(Assembly contractsAssembly)
       {
           var types = contractsAssembly.GetTypes()
               .Where(t => t.GetCustomAttribute<ElementTypeAttribute>() != null);
           foreach (var type in types)
           {
               var attr = type.GetCustomAttribute<ElementTypeAttribute>()!;
               _map[attr.TypeName] = type;
           }
       }

       public static Type? Resolve(string typeName)
           => _map.TryGetValue(typeName, out var t) ? t : null;
   }
   ```

4. 更新 ElementJsonConverter
   - `Contracts/Converters/ElementJsonConverter.cs` 中的 `$type` 解析改为调用 `ElementTypeRegistry.Resolve()`
   - 保持向后兼容：如果 Registry 未初始化则 fallback 到硬编码 switch

5. 在各项目启动时调用初始化
   - `Editor/Server/Program.cs`: `ElementTypeRegistry.Initialize(typeof(ElementBase).Assembly);`
   - `Generators/ReportDataMaker/App.xaml.cs`: 同上

**验证**:
```bash
dotnet build ReportPlatform.sln
grep -rn "\[ElementType\(" Contracts/Models/Elements/
```

**完成标准**:
- [ ] `ElementTypeAttribute` 已定义
- [ ] 所有 25 个元素类已标注
- [ ] `ElementTypeRegistry` 通过反射扫描构建映射
- [ ] `ElementJsonConverter` 使用 Registry 解析类型
- [ ] 新增元素只需添加类 + Attribute，无需修改其他文件
- [ ] 全解决方案编译通过

---

### P1-06: 引入 CommunityToolkit.Mvvm（决策 M3）

**决策点**: M3
**前置依赖**: 无
**影响范围**: Generators/ReportDataMaker/

**设计意图**:
替换自定义 MVVM 基础设施，使用社区标准框架，获得 Source Generator 支持。

**执行步骤**:

1. 添加 NuGet 包
   ```bash
   dotnet add Generators/ReportDataMaker/ReportDataMaker.csproj package CommunityToolkit.Mvvm
   ```

2. 识别现有 MVVM 基础类
   - 搜索 `ViewModelBase`、`RelayCommand`、`ObservableObject` 等自定义实现
   - 列出所有继承自自定义基类的 ViewModel

3. 逐步迁移（按 ViewModel 逐个替换）
   - 第一批：简单 ViewModel（无复杂命令）
     - 替换基类: `ViewModelBase` → `ObservableObject`
     - 替换属性通知: 手动 `OnPropertyChanged` → `[ObservableProperty]`
     - 替换命令: 自定义 `RelayCommand` → `[RelayCommand]`
   - 第二批：复杂 ViewModel（有异步命令、CanExecute）
     - `AsyncRelayCommand` → `[RelayCommand]` + `async Task`
     - `CanExecute` → `[RelayCommand(CanExecute = nameof(CanXxx))]`

4. 删除自定义 MVVM 基础设施
   - 确认所有 ViewModel 已迁移后删除旧基类
   - 更新 `Infrastructure/` 目录

5. 验证 UI 绑定
   - 确认 XAML 中的 `{Binding}` 和 `{x:Bind}` 仍正常工作
   - Source Generator 生成的属性名与 XAML 绑定一致

**验证**:
```bash
dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj
grep -rn "CommunityToolkit.Mvvm" Generators/ReportDataMaker/ReportDataMaker.csproj
grep -rn "ViewModelBase" Generators/ReportDataMaker/ --include="*.cs"  # 应为 0 结果
```

**完成标准**:
- [ ] CommunityToolkit.Mvvm 包已安装
- [ ] 所有 ViewModel 继承自 `ObservableObject`
- [ ] 所有命令使用 `[RelayCommand]` 生成
- [ ] 自定义 MVVM 基础类已删除
- [ ] 编译通过，UI 绑定正常

---

### P1-07: SQLite 本地配置库（决策 N3）

**决策点**: N3
**前置依赖**: 无
**影响范围**: Generators/ReportDataMaker/

**设计意图**:
替换当前 JSON 文件配置存储，使用 SQLite 获得事务安全、并发友好、查询灵活的本地存储。

**执行步骤**:

1. 确认当前配置存储方式
   - 搜索 `Configs/` 目录和 JSON 序列化配置代码
   - 识别存储了哪些配置（数据库连接、适配器配置、用户偏好等）

2. 设计 SQLite Schema
   ```sql
   CREATE TABLE config (
       key TEXT PRIMARY KEY,
       value TEXT NOT NULL,
       category TEXT NOT NULL DEFAULT 'general',
       updated_at TEXT NOT NULL DEFAULT (datetime('now'))
   );

   CREATE TABLE adapter_profiles (
       id TEXT PRIMARY KEY,
       name TEXT NOT NULL,
       adapter_type TEXT NOT NULL,
       config_json TEXT NOT NULL,
       created_at TEXT NOT NULL DEFAULT (datetime('now')),
       updated_at TEXT NOT NULL DEFAULT (datetime('now'))
   );

   CREATE TABLE recent_templates (
       id TEXT PRIMARY KEY,
       path TEXT NOT NULL,
       name TEXT NOT NULL,
       last_opened TEXT NOT NULL
   );
   ```

3. 创建 ConfigStore 服务
   ```
   Generators/ReportDataMaker/Services/ConfigStore/
   ├── IConfigStore.cs
   ├── SqliteConfigStore.cs
   └── ConfigMigrator.cs    # 从旧 JSON 迁移
   ```

4. 实现 IConfigStore 接口
   ```csharp
   public interface IConfigStore
   {
       string? Get(string key, string category = "general");
       void Set(string key, string value, string category = "general");
       void Delete(string key, string category = "general");
       IReadOnlyList<AdapterProfile> GetAdapterProfiles();
       void SaveAdapterProfile(AdapterProfile profile);
       void DeleteAdapterProfile(string id);
   }
   ```

5. 注册到 DI 容器
   - 在 `App.xaml.cs` 或 DI 配置中注册 `SqliteConfigStore`
   - SQLite 文件路径: `%APPDATA%/ReportDataMaker/config.db`

6. 迁移现有配置读写代码
   - 逐步替换 `File.ReadAllText` / `JsonConvert.Deserialize` 调用
   - 首次启动时自动从旧 JSON 迁移数据

**验证**:
```bash
dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj
grep -rn "IConfigStore\|SqliteConfigStore" Generators/ReportDataMaker/ --include="*.cs"
```

**完成标准**:
- [ ] SQLite 配置库已创建
- [ ] `IConfigStore` 接口定义完整
- [ ] `SqliteConfigStore` 实现 CRUD 操作
- [ ] 旧 JSON 配置可自动迁移
- [ ] 编译通过

---

## Phase 2：核心重构（1-2 月）

### 目标
模型统一 + 核心类库提取 + 适配器注册表。这是最关键的阶段，后续所有工作依赖此阶段完成。

---

### P2-01: 消除双重模型 — R-001 修复（决策 O3）

**决策点**: O3
**前置依赖**: Phase 1 完成
**影响范围**: 全解决方案
**风险等级**: ❌ 不可逆

**当前问题**:
- ReportDataMaker 中存在影子模型（`Models/` 目录下的本地类型）
- 与 Contracts 中的模型存在重复定义和转换层
- 两套模型之间通过 Converter/Mapper 转换，增加维护成本

**执行步骤**:

1. 审计影子模型
   - 列出 `Generators/ReportDataMaker/Models/` 下所有类型
   - 对比 `Contracts/Models/` 找出重复定义
   - 标记差异点（额外字段、不同命名、不同序列化方式）

2. 制定迁移映射表
   | ReportDataMaker 本地模型 | Contracts 对应模型 | 差异 | 处理方式 |
   |---|---|---|---|
   | LocalElement | ElementBase | 额外 UI 字段 | 扩展 Contracts 或组合 |
   | ... | ... | ... | ... |

3. 扩展 Contracts 模型（如需要）
   - 对 Contracts 模型添加缺失的属性（保持向后兼容）
   - 使用 `[JsonIgnore]` 标记仅运行时使用的属性
   - 不破坏现有 API 契约

4. 逐步替换引用
   - 按文件逐个替换 `using ReportDataMaker.Models` → `using Contracts.Models`
   - 每替换一个文件立即编译验证
   - 优先替换叶子节点（被依赖最少的文件）

5. 删除转换层
   - 移除 `Converters/` 中的模型转换器（非 JSON 转换器）
   - 移除 `Models/` 目录下的影子模型文件

6. 更新序列化逻辑
   - 确认模板 JSON 加载/保存使用统一的 `TemplateSerializer`
   - 确认 `ElementJsonConverter` 能正确处理所有场景

**验证**:
```bash
dotnet build ReportPlatform.sln
# 确认 Models/ 目录下无影子模型
ls Generators/ReportDataMaker/Models/
# 确认无本地模型引用
grep -rn "using.*ReportDataMaker.Models" Generators/ReportDataMaker/ --include="*.cs"
```

**完成标准**:
- [ ] `Generators/ReportDataMaker/Models/` 目录已清空或仅含 ViewModel 专用类型
- [ ] 所有业务逻辑使用 Contracts 模型
- [ ] 模型转换层已删除
- [ ] 模板 JSON 加载/保存正常
- [ ] 全解决方案编译通过
- [ ] PDF 导出功能正常（手动验证）

---

### P2-02: ReportDataMaker.Core 类库提取（决策 H3）

**决策点**: H3
**前置依赖**: P2-01 (O3 模型统一)
**影响范围**: Generators/
**风险等级**: ❌ 不可逆

**设计意图**:
将 ReportDataMaker 的核心业务逻辑提取为独立类库，使 WPF 壳仅负责 UI，为后续多壳（Web、CLI）做准备。

**执行步骤**:

1. 创建 Core 类库项目
   ```bash
   dotnet new classlib -n ReportDataMaker.Core -o Generators/Core --framework net8.0
   dotnet sln add Generators/Core/ReportDataMaker.Core.csproj
   ```

2. 确定迁移边界
   - **迁入 Core**: Services/、Models/（ViewModel 专用除外）、业务逻辑
   - **留在 WPF**: ViewModels/、Views/、Converters/（UI 转换器）、Styles/、App.xaml

3. 迁移服务层
   ```
   Generators/Core/
   ├── Services/
   │   ├── DataBindingService.cs
   │   ├── TemplateLoaderService.cs
   │   ├── ConfigStore/
   │   │   ├── IConfigStore.cs
   │   │   └── SqliteConfigStore.cs
   │   ├── ContextAdapter/
   │   │   ├── ContextAdapterFactory.cs
   │   │   └── ContextAdapterService.cs
   │   ├── ExcelAdapter/
   │   │   ├── ExcelAdapterFactory.cs
   │   │   ├── ExcelContractReader.cs
   │   │   ├── ExcelSchemaExporter.cs
   │   │   └── TemplateFlattenService.cs
   │   ├── DatabaseAdapter/
   │   │   └── (所有数据库适配器)
   │   └── PdfExport/
   │       ├── PdfExportService.cs
   │       └── PdfElementRenderer.cs
   └── ReportDataMaker.Core.csproj
   ```

4. 更新项目引用
   - `ReportDataMaker.Core.csproj` → 引用 `Contracts`
   - `ReportDataMaker.csproj` → 引用 `ReportDataMaker.Core` + `Contracts`

5. 调整命名空间
   - `ReportDataMaker.Services.*` → `ReportDataMaker.Core.Services.*`
   - 使用全局 using 或 namespace alias 减少改动量

6. 更新 DI 注册
   - Core 提供 `IServiceCollection.AddReportDataMakerCore()` 扩展方法
   - WPF 壳在 `App.xaml.cs` 中调用

**验证**:
```bash
dotnet build ReportPlatform.sln
dotnet build Generators/Core/ReportDataMaker.Core.csproj
dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj
```

**完成标准**:
- [ ] `ReportDataMaker.Core` 项目已创建并加入解决方案
- [ ] 所有 Services 已迁移到 Core
- [ ] WPF 项目仅包含 UI 相关代码
- [ ] Core 项目无 WPF/UI 依赖（无 PresentationFramework 引用）
- [ ] 全解决方案编译通过

---

### P2-03: 适配器工厂注册表（决策 I2）

**决策点**: I2
**前置依赖**: P2-02 (H3 Core 提取)
**影响范围**: Generators/Core/Services/

**设计意图**:
新增适配器无需修改 MainViewModel 或工厂代码，通过注册表模式实现开放-封闭原则。

**执行步骤**:

1. 定义适配器注册接口
   ```csharp
   // Contracts/Models/Adapters/IDataAdapterFactory.cs
   public interface IDataAdapterFactory
   {
       AdapterType Type { get; }
       string DisplayName { get; }
       IDataAdapter Create(AdapterConfigBase config);
       AdapterConfigBase CreateDefaultConfig();
   }
   ```

2. 创建注册表
   ```csharp
   // Generators/Core/Services/AdapterRegistry.cs
   public sealed class AdapterRegistry
   {
       private readonly Dictionary<AdapterType, IDataAdapterFactory> _factories = new();

       public void Register(IDataAdapterFactory factory)
           => _factories[factory.Type] = factory;

       public IDataAdapterFactory? Get(AdapterType type)
           => _factories.TryGetValue(type, out var f) ? f : null;

       public IReadOnlyList<IDataAdapterFactory> GetAll()
           => _factories.Values.ToList();
   }
   ```

3. 重构现有适配器为工厂模式
   - `ExcelAdapterFactory` : `IDataAdapterFactory`
   - `DatabaseAdapterFactory` : `IDataAdapterFactory`
   - `ContextAdapterFactory` : `IDataAdapterFactory`

4. 更新 DI 注册
   ```csharp
   services.AddSingleton<AdapterRegistry>();
   services.AddSingleton<IDataAdapterFactory, ExcelAdapterFactory>();
   services.AddSingleton<IDataAdapterFactory, DatabaseAdapterFactory>();
   services.AddSingleton<IDataAdapterFactory, ContextAdapterFactory>();
   ```

5. 更新 ViewModel 消费方式
   - 从 `AdapterRegistry.GetAll()` 获取可用适配器列表
   - 移除 ViewModel 中的硬编码适配器类型判断

**验证**:
```bash
dotnet build ReportPlatform.sln
grep -rn "AdapterRegistry" Generators/ --include="*.cs"
```

**完成标准**:
- [ ] `IDataAdapterFactory` 接口已定义
- [ ] `AdapterRegistry` 注册表已实现
- [ ] 三个现有适配器已重构为工厂模式
- [ ] ViewModel 通过注册表获取适配器
- [ ] 新增适配器只需实现接口 + DI 注册
- [ ] 编译通过

---

### P2-04: 数据库 Provider 拆分 NuGet 包（决策 K2）

**决策点**: K2
**前置依赖**: P2-03 (I2 工厂注册表)
**影响范围**: Generators/

**设计意图**:
按需安装数据库驱动，不使用 MySQL 时不引入 MySqlConnector，减小部署体积。

**执行步骤**:

1. 创建公共数据库适配器项目
   ```bash
   dotnet new classlib -n Adapter.Database.Common -o Generators/Adapter.Database.Common
   ```

2. 定义 Provider 接口
   ```csharp
   // Generators/Adapter.Database.Common/IDatabaseProvider.cs
   public interface IDatabaseProvider
   {
       string ProviderName { get; }  // "SqlServer", "MySql", "Sqlite", "PostgreSql"
       DbConnection CreateConnection(string connectionString);
       string BuildTestQuery();
   }
   ```

3. 创建各 Provider 项目
   ```bash
   dotnet new classlib -n Adapter.Database.SqlServer -o Generators/Adapter.Database.SqlServer
   dotnet new classlib -n Adapter.Database.MySql -o Generators/Adapter.Database.MySql
   dotnet new classlib -n Adapter.Database.Sqlite -o Generators/Adapter.Database.Sqlite
   dotnet new classlib -n Adapter.Database.PostgreSql -o Generators/Adapter.Database.PostgreSql
   ```

4. 迁移现有代码
   - 从 `ReportDataMaker/Services/DatabaseAdapter/` 提取各 Provider 实现
   - 每个 Provider 项目仅引用对应的 ADO.NET 驱动

5. 配置 NuGet 包元数据
   - 在各 `.csproj` 中添加 `<PackageId>`, `<Version>`, `<Description>`
   - 使用 `Directory.Build.props` 统一版本号

6. 更新主项目引用
   - `ReportDataMaker.csproj` 引用所有 Provider（开发时全量）
   - 部署时可按需裁剪

**验证**:
```bash
dotnet build ReportPlatform.sln
dotnet pack Generators/Adapter.Database.Common/
```

**完成标准**:
- [ ] `Adapter.Database.Common` 项目已创建
- [ ] 4 个 Provider 项目已创建
- [ ] 各 Provider 仅依赖对应驱动包
- [ ] 主项目通过 ProjectReference 引用
- [ ] 全解决方案编译通过

---

### P2-05: Excel 适配器拆分 NuGet 包（决策 L2）

**决策点**: L2
**前置依赖**: P2-03 (I2 工厂注册表)
**影响范围**: Generators/

**设计意图**:
不使用 Excel 功能时可裁剪 ClosedXML 依赖（约 15MB）。

**执行步骤**:

1. 创建独立项目
   ```bash
   dotnet new classlib -n Adapter.Excel -o Generators/Adapter.Excel
   ```

2. 迁移 Excel 相关代码
   ```
   Generators/Adapter.Excel/
   ├── ExcelAdapterFactory.cs
   ├── ExcelContractReader.cs
   ├── ExcelSchemaExporter.cs
   ├── ExcelAdapterConfig.cs
   ├── TemplateFlattenService.cs
   ├── TemplateFieldSchema.cs
   └── Adapter.Excel.csproj  (引用 ClosedXML + Contracts)
   ```

3. 从主项目移除 ClosedXML 依赖
   - `ReportDataMaker.csproj` 不再直接引用 ClosedXML
   - 通过 `Adapter.Excel` 项目间接引用

4. 更新 DI 注册
   - `Adapter.Excel` 提供 `IServiceCollection.AddExcelAdapter()` 扩展方法
   - 主项目按需调用

**验证**:
```bash
dotnet build ReportPlatform.sln
grep -rn "ClosedXML" Generators/ReportDataMaker/ReportDataMaker.csproj  # 应为 0
```

**完成标准**:
- [ ] `Adapter.Excel` 项目已创建
- [ ] ClosedXML 依赖仅在 Excel 项目中
- [ ] 主项目通过 ProjectReference 引用
- [ ] 编译通过

---

### P2-06: Editor.Server 添加 Blazor 页面（决策 G3）

**决策点**: G3
**前置依赖**: 无（可与 P2-01~05 并行）
**影响范围**: Editor/Server/

**设计意图**:
为 Editor 添加内置 Web UI，非技术人员可通过浏览器操作模板，无需单独部署前端。

**执行步骤**:

1. 配置 Blazor Server
   - 在 `Editor/Server/Program.cs` 中添加:
   ```csharp
   builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();
   // ...
   app.MapRazorComponents<App>()
       .AddInteractiveServerRenderMode();
   ```

2. 创建 Blazor 基础结构
   ```
   Editor/Server/
   ├── Components/
   │   ├── App.razor
   │   ├── Routes.razor
   │   ├── Layout/
   │   │   ├── MainLayout.razor
   │   │   └── NavMenu.razor
   │   └── Pages/
   │       ├── Home.razor
   │       ├── Templates.razor
   │       └── TemplateEditor.razor
   └── wwwroot/
       └── (静态资源)
   ```

3. 实现核心页面
   - **Templates.razor**: 模板列表（调用现有 API Controller）
   - **TemplateEditor.razor**: 模板编辑器（基础版，展示 JSON 结构）
   - **Home.razor**: 仪表板/欢迎页

4. 确保 API 和 Blazor 共存
   - API 路由: `/api/*`
   - Blazor 路由: `/*`（fallback）
   - 保持现有 Swagger UI 可用

5. 添加 Blazor 专用认证
   - 复用现有 JWT 认证
   - 添加 `AuthenticationStateProvider` 适配

**验证**:
```bash
dotnet build Editor/Server/Xinglin.WebReportEditor.Server.csproj
dotnet run --project Editor/Server/Xinglin.WebReportEditor.Server.csproj &
# 访问 http://localhost:5000 确认 Blazor 页面加载
```

**完成标准**:
- [ ] Blazor Server 已配置
- [ ] 至少 3 个页面可访问
- [ ] API 端点仍正常工作
- [ ] Swagger UI 仍可用
- [ ] 编译通过

---

## Phase 3：模块整合（3-6 月）

### 目标
共享渲染层 + 插件宿主 + 多壳部署。将渲染逻辑统一、引入插件体系、实现多端部署能力。

---

### P3-01: 独立 Rendering 项目（决策 J2）

**决策点**: J2
**前置依赖**: P2-01 (O3 模型统一)
**影响范围**: 新项目 + Editor/Core + Generators/Core
**风险等级**: ❌ 不可逆

**设计意图**:
PDF 渲染逻辑当前分散在 Editor.Core 和 ReportDataMaker 中，存在两套实现。统一为独立 Rendering 项目，保证渲染一致性。

**执行步骤**:

1. 创建 Rendering 项目
   ```bash
   dotnet new classlib -n Rendering -o src/Rendering --framework net8.0
   dotnet sln add src/Rendering/Rendering.csproj
   ```

2. 定义渲染接口
   ```csharp
   // src/Rendering/ITemplateRenderer.cs
   public interface ITemplateRenderer
   {
       byte[] RenderToPdf(TemplateDefinition template, Dictionary<string, object> data);
       Stream RenderToPdfStream(TemplateDefinition template, Dictionary<string, object> data);
   }
   ```

3. 迁移 Editor.Core 中的渲染代码
   - `Editor/Core/Services/PdfRenderService.cs` → `src/Rendering/PdfTemplateRenderer.cs`
   - `Editor/Core/Services/PdfTemplateRenderer.cs` → 合并到上述文件
   - `Editor/Core/SharedInterfaces/IPdfSharpTemplateRenderer.cs` → `src/Rendering/ITemplateRenderer.cs`

4. 迁移 ReportDataMaker 中的渲染代码
   - `Generators/ReportDataMaker/Services/PdfExport/PdfExportService.cs` → 合并
   - `Generators/ReportDataMaker/Services/PdfExport/PdfElementRenderer.cs` → `src/Rendering/PdfElementRenderer.cs`

5. 实现统一渲染器
   ```
   src/Rendering/
   ├── ITemplateRenderer.cs
   ├── PdfTemplateRenderer.cs
   ├── PdfElementRenderer.cs
   ├── PdfPageLayoutEngine.cs
   ├── ReportDocumentPaginator.cs
   └── Rendering.csproj
   ```

6. 更新消费方引用
   - `Editor.Core` → 引用 `Rendering`，删除本地渲染代码
   - `ReportDataMaker.Core` → 引用 `Rendering`，删除本地渲染代码

**验证**:
```bash
dotnet build ReportPlatform.sln
grep "QuestPDF" Editor/Core/Xinglin.WebReportEditor.Core.csproj  # 应为 0
ls src/Rendering/*.cs
```

**完成标准**:
- [ ] `Rendering` 项目已创建并加入解决方案
- [ ] 所有 PDF 渲染逻辑集中在 Rendering 项目
- [ ] Editor.Core 和 ReportDataMaker.Core 通过接口调用渲染
- [ ] QuestPDF/SkiaSharp 依赖仅在 Rendering 项目中
- [ ] 渲染输出与迁移前一致
- [ ] 全解决方案编译通过

---

### P3-02: Editor.Core 渲染迁移到 Rendering（决策 F3）

**决策点**: F3
**前置依赖**: P3-01 (J2 Rendering 项目)
**影响范围**: Editor/Core/
**风险等级**: ❌ 不可逆

**执行步骤**:

1. 移除 Editor.Core 中的渲染服务
   - 删除 `Editor/Core/Services/PdfRenderService.cs`
   - 删除 `Editor/Core/Services/PdfTemplateRenderer.cs`
   - 删除 `Editor/Core/SharedInterfaces/IPdfSharpTemplateRenderer.cs`

2. 更新 Editor.Core 项目引用
   - 添加 `<ProjectReference Include="../../src/Rendering/Rendering.csproj" />`
   - 移除 QuestPDF、SkiaSharp 直接依赖

3. 更新 DI 注册
   - `ServiceCollectionExtensions.cs` 中注册 `ITemplateRenderer` → `PdfTemplateRenderer`

4. 更新 Controller 调用
   - 将 `IPdfSharpTemplateRenderer` 替换为 `ITemplateRenderer`
   - 调整方法签名适配新接口

**验证**:
```bash
dotnet build Editor/Server/Xinglin.WebReportEditor.Server.csproj
```

**完成标准**:
- [ ] Editor.Core 无渲染实现代码
- [ ] Editor.Core 无 QuestPDF/SkiaSharp 直接依赖
- [ ] PDF 导出 API 正常工作
- [ ] 编译通过

---

### P3-03: 引入 PluginHost 模块（决策 B3）

**决策点**: B3
**前置依赖**: P2-03 (I2 工厂注册表) + P3-01 (J2 Rendering)
**影响范围**: 新项目
**风险等级**: ⚠️ 接口一旦发布不可变

**设计意图**:
提供第三方扩展能力，允许外部开发者编写自定义适配器和元素类型插件。

**执行步骤**:

1. 创建 PluginHost 项目
   ```bash
   dotnet new classlib -n PluginHost -o src/PluginHost --framework net8.0
   dotnet sln add src/PluginHost/PluginHost.csproj
   ```

2. 定义插件接口
   ```csharp
   public interface IAdapterPlugin
   {
       PluginMetadata Metadata { get; }
       IDataAdapterFactory CreateFactory();
   }

   public interface IElementPlugin
   {
       PluginMetadata Metadata { get; }
       Type ElementType { get; }
       Type? RendererType { get; }
   }

   public record PluginMetadata(
       string Id, string Name, string Version, string Author, string? Description = null
   );
   ```

3. 实现插件加载器
   ```csharp
   public sealed class PluginLoader
   {
       public IReadOnlyList<IAdapterPlugin> LoadAdapterPlugins(string pluginDirectory);
       public IReadOnlyList<IElementPlugin> LoadElementPlugins(string pluginDirectory);
   }
   ```

4. 实现插件注册表
   ```csharp
   public sealed class PluginRegistry
   {
       public void RegisterAdapter(IAdapterPlugin plugin);
       public void RegisterElement(IElementPlugin plugin);
       public IReadOnlyList<IAdapterPlugin> AdapterPlugins { get; }
       public IReadOnlyList<IElementPlugin> ElementPlugins { get; }
   }
   ```

5. 集成到 AdapterRegistry
   - PluginHost 加载的适配器自动注册到 `AdapterRegistry`
   - 插件目录: `%APPDATA%/ReportDataMaker/plugins/`

**验证**:
```bash
dotnet build ReportPlatform.sln
```

**完成标准**:
- [ ] `PluginHost` 项目已创建
- [ ] `IAdapterPlugin` 和 `IElementPlugin` 接口已定义
- [ ] `PluginLoader` 可从目录加载 DLL
- [ ] `PluginRegistry` 管理已加载插件
- [ ] 编译通过

---

### P3-04: ReportDataMaker.Web — Blazor WebAssembly 壳（决策 H3 延伸）

**决策点**: H3 (多壳)
**前置依赖**: P2-02 (H3 Core 提取)
**影响范围**: 新项目

**执行步骤**:

1. 创建 Blazor WebAssembly 项目
   ```bash
   dotnet new blazorwasm -n ReportDataMaker.Web -o Generators/ReportDataMaker.Web
   dotnet sln add Generators/ReportDataMaker.Web/ReportDataMaker.Web.csproj
   ```

2. 引用 Core 类库
   - `ReportDataMaker.Web.csproj` → 引用 `ReportDataMaker.Core`

3. 实现核心页面
   ```
   Generators/ReportDataMaker.Web/Pages/
   ├── Index.razor          # 模板选择
   ├── DataEntry.razor      # 数据录入
   └── Preview.razor        # PDF 预览
   ```

4. 适配 Core 服务
   - 数据库适配器在 Web 端不可用（标记为不支持）
   - Excel 适配器通过 JS Interop 实现文件选择
   - PDF 预览通过 PDF.js 展示

**验证**:
```bash
dotnet build Generators/ReportDataMaker.Web/ReportDataMaker.Web.csproj
```

**完成标准**:
- [ ] Blazor WebAssembly 项目已创建
- [ ] 可加载模板并展示字段
- [ ] 数据录入功能可用
- [ ] 编译通过

---

### P3-05: Contracts + Rendering 发布 NuGet 包（决策 C3）

**决策点**: C3
**前置依赖**: P3-01 (J2 Rendering)
**影响范围**: Contracts/, src/Rendering/

**执行步骤**:

1. 配置包元数据
   ```xml
   <PropertyGroup>
     <PackageId>Xinglin.ReportPlatform.Contracts</PackageId>
     <Version>1.0.0</Version>
     <Authors>Xinglin</Authors>
     <Description>Report template contracts and element models</Description>
   </PropertyGroup>
   ```

2. 配置 Directory.Build.props 统一版本

3. 设置打包输出
   ```bash
   dotnet pack Contracts/ -o artifacts/
   dotnet pack src/Rendering/ -o artifacts/
   ```

4. 配置本地 NuGet 源（开发阶段）
   ```bash
   dotnet nuget add source ./artifacts -n local
   ```

**验证**:
```bash
dotnet pack Contracts/Xinglin.WebReportEditor.Contracts.csproj -o artifacts/
dotnet pack src/Rendering/Rendering.csproj -o artifacts/
ls artifacts/*.nupkg
```

**完成标准**:
- [ ] Contracts 可打包为 NuGet
- [ ] Rendering 可打包为 NuGet
- [ ] 包元数据完整
- [ ] 编译通过

---

## Phase 4：远期演进

### 目标
插件热加载、分布式能力、微服务拆分。视业务需求决定是否实施。

---

### P4-01: 插件 DLL 热加载

**前置依赖**: P3-03 (PluginHost)
**触发条件**: 第三方开发者需要开发自定义适配器

**执行步骤**:
1. 实现 `AssemblyLoadContext` 隔离加载
2. 文件监控 (`FileSystemWatcher`) 检测插件目录变化
3. 卸载旧版本 + 加载新版本
4. 插件版本兼容性检查

---

### P4-02: 分布式模板仓库

**前置依赖**: P3-05 (NuGet 发布)
**触发条件**: 多台设备需要共享模板

**执行步骤**:
1. 设计模板同步协议（基于版本号的乐观锁）
2. 实现 `ITemplateStore` 接口（本地 SQLite + 远程 REST）
3. 冲突解决策略
4. 离线队列

---

### P4-03: 微服务拆分

**前置依赖**: 所有前置阶段
**触发条件**: 用户量超过单机承载能力

**执行步骤**:
1. 拆分 Editor.Server 为独立服务
2. 引入消息队列
3. 容器化部署
4. API Gateway 统一入口

---

## 附录

### A. 解决方案结构变更对照

```
当前:                              目标:
ReportPlatform.sln                 ReportPlatform.sln
├── Contracts/                     ├── src/
├── Editor/Core/                   │   ├── Contracts/          (NuGet)
├── Editor/Server/                 │   ├── Rendering/          (NuGet) [NEW]
├── Editor/Server.Tests/           │   ├── PluginHost/         (NuGet) [NEW]
└── Generators/ReportDataMaker/    │   ├── Editor/Core/
                                   │   ├── Editor/Server/      (+Blazor)
                                   │   └── Generators/
                                   │       ├── Core/           [NEW]
                                   │       ├── Adapter.Excel/  [NEW]
                                   │       ├── Adapter.Database.Common/  [NEW]
                                   │       ├── Adapter.Database.SqlServer/ [NEW]
                                   │       ├── Adapter.Database.MySql/    [NEW]
                                   │       ├── Adapter.Database.Sqlite/   [NEW]
                                   │       ├── Adapter.Database.PostgreSql/ [NEW]
                                   │       ├── Adapter.Context/ [NEW]
                                   │       ├── ReportDataMaker.WPF/  (原 ReportDataMaker)
                                   │       └── ReportDataMaker.Web/  [NEW]
                                   └── tests/
                                       ├── Editor.Server.Tests/
                                       ├── Rendering.Tests/    [NEW]
                                       ├── Generators.Core.Tests/ [NEW]
                                       ├── Adapter.Excel.Tests/  [NEW]
                                       └── Adapter.Database.Tests/ [NEW]
```

### B. 关键路径依赖图

```
Phase 1 (全部可并行):
  P1-01 ~ P1-07 ──全部完成──▶ Phase 2 开始

Phase 2 (串行关键路径):
  P2-01 O3 ──▶ P2-02 H3 ──▶ P2-03 I2 ──┬──▶ P2-04 K2
                                          └──▶ P2-05 L2
  P2-06 G3 (独立，可并行)

Phase 3 (部分串行):
  P2-01 ──▶ P3-01 J2 ──▶ P3-02 F3 ──▶ P3-05 C3
  P2-03 + P3-01 ──▶ P3-03 B3
  P2-02 ──▶ P3-04 H3 Web
```

### C. 每阶段验收检查清单

**Phase 1 验收**:
- [ ] `dotnet build ReportPlatform.sln` 零错误
- [ ] 无硬编码密钥
- [ ] 无 SQL 拼接
- [ ] CommunityToolkit.Mvvm 已集成
- [ ] SQLite 配置库可用
- [ ] 元素类型通过 Attribute 注册

**Phase 2 验收**:
- [ ] `dotnet build ReportPlatform.sln` 零错误
- [ ] ReportDataMaker 无影子模型
- [ ] Core 类库无 UI 依赖
- [ ] 适配器通过注册表获取
- [ ] 数据库 Provider 可独立引用
- [ ] Excel 适配器可独立引用
- [ ] Blazor 页面可访问

**Phase 3 验收**:
- [ ] `dotnet build ReportPlatform.sln` 零错误
- [ ] PDF 渲染统一且输出一致
- [ ] 插件可加载和注册
- [ ] Web 壳可运行
- [ ] NuGet 包可打包

### D. 风险缓解策略

| 风险 | 概率 | 影响 | 缓解 |
|------|------|------|------|
| O3 模型统一改动量大 | 高 | 高 | 分批迁移，每次一个文件，立即编译 |
| M3 迁移破坏 UI 绑定 | 中 | 中 | 逐个 ViewModel 迁移，每次验证 |
| J2 渲染输出不一致 | 中 | 高 | 迁移前后对比 PDF 输出 |
| K2 版本管理复杂 | 低 | 中 | Directory.Build.props 统一版本 |
| G3 Blazor 增加复杂度 | 低 | 低 | Blazor Server 模式 |

### E. 分支策略建议

```
main ────────────────────────────────────────────▶
  ├── feature/phase1-security ──▶ PR ──▶ merge
  ├── feature/phase1-e2-attribute ──▶ PR ──▶ merge
  ├── feature/phase1-m3-toolkit ──▶ PR ──▶ merge
  ├── feature/phase1-n3-sqlite ──▶ PR ──▶ merge
  ├── feature/phase2-o3-model-unify ──▶ PR ──▶ merge
  ├── feature/phase2-h3-core-extract ──▶ PR ──▶ merge
  ├── feature/phase2-i2-registry ──▶ PR ──▶ merge
  ├── feature/phase2-k2-db-split ──▶ PR ──▶ merge
  ├── feature/phase2-l2-excel-split ──▶ PR ──▶ merge
  ├── feature/phase2-g3-blazor ──▶ PR ──▶ merge
  └── feature/phase3-* ──▶ ...
```

每个 Phase 内的独立任务可并行开发，串行依赖的任务按顺序合并。
