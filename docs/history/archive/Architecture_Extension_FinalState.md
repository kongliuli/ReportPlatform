# 架构扩展方向与最终态评估报告

| 字段 | 值 |
|------|-----|
| 评估日期 | 2026-05-20 |
| 评估范围 | 全部 3 个项目、4 个模块、100+ 个类型 |
| 评估标准 | 见下方定义 |

---

## 评估标准定义

| 标记 | 含义 | 判定条件 |
|------|------|----------|
| 🔒 **最终态** | 无需迁移和强化，可定性为不许改变 | 接口/契约已稳定；实现完整且无已知缺陷；修改将破坏向后兼容性 |
| 🟢 **可增强** | 当前实现正确，但可按需添加新功能 | 核心逻辑稳定，仅需扩展方法或新增重载 |
| 🟡 **待强化** | 存在已知缺陷或安全风险，需在原位修补 | 不涉及结构变更，仅需修复或加固 |
| 🔴 **待迁移** | 存在结构性问题，需重构或迁移到新位置 | 涉及继承关系、职责划分或跨模块依赖的变更 |

---

## 一、Contracts 契约层

### 1.1 枚举

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ElementGroup` | 🔒 最终态 | 运行时分组逻辑完备（Fixed/Context/Editable/DataAdapter），修改将破坏 ClassifyElement 语义 |
| `ElementAdaptationGroup` | 🟡 待强化 | Advanced 组过大（11种元素），建议拆分为 Media/Security/Layout 三个子组 |
| `AdapterType` | 🟢 可增强 | Api 类型已预留但未实现，待新增 API 适配器时扩展 |

### 1.2 元素模型

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ElementBase` | 🟡 待强化 | 属性膨胀（25+属性），部分属性对特定元素无意义；建议将属性分组为值对象但不改变继承关系 |
| `ExternalElementBase` | 🔴 待迁移 | 与 ElementBase 形成不必要的中间层，应合并或重新定义职责边界 |
| `TextElement` | 🔒 最终态 | 最基础的元素类型，属性集完整，无修改必要 |
| `NumberElement` | 🔒 最终态 | 数字输入元素，属性集完整 |
| `DateElement` | 🔒 最终态 | 日期元素，属性集完整 |
| `LineElement` | 🔒 最终态 | 线条元素，无额外属性需求 |
| `TableElement` | 🟢 可增强 | 表格单元格数据绑定模式可能需要扩展（如合并单元格） |
| `ShapeElement` | 🔒 最终态 | 形状元素，属性集完整 |
| `DividerElement` | 🔒 最终态 | 分割线元素，无扩展需求 |
| `BarcodeElement` | 🔒 最终态 | 条形码元素，属性集完整 |
| `QrCodeElement` | 🔒 最终态 | 二维码元素，属性集完整 |
| `SignatureElement` | 🔒 最终态 | 签名元素，属性集完整 |
| `ContainerElement` | 🟢 可增强 | 容器嵌套规则可能需要约束（如最大嵌套深度） |
| `RepeatElement` | 🟢 可增强 | 重复元素的数据源绑定模式可能需要扩展 |
| `ChartElement` | 🟢 可增强 | 图表类型和数据映射可能需要扩展 |
| `CheckboxElement` | 🔒 最终态 | 复选框元素，属性集完整 |
| `RadioElement` | 🔒 最终态 | 单选按钮元素，属性集完整 |
| `DropdownElement` | 🔒 最终态 | 下拉列表元素，属性集完整 |
| `ImageElement` | 🔒 最终态 | 图片元素，属性集完整 |
| `IconElement` | 🔒 最终态 | 图标元素，属性集完整 |
| `HyperlinkElement` | 🔒 最终态 | 超链接元素，属性集完整 |
| `HeaderElement` | 🔒 最终态 | 页眉元素，属性集完整 |
| `FooterElement` | 🔒 最终态 | 页脚元素，属性集完整 |
| `PageNumberElement` | 🔒 最终态 | 页码元素，属性集完整 |
| `WatermarkElement` | 🔒 最终态 | 水印元素，属性集完整 |

### 1.3 模板模型

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `TemplateDefinition` | 🟡 待强化 | 缺少模板版本标识字段（用于序列化兼容），Elements 类型应改为不可变列表 |
| `PageSettings` | 🔒 最终态 | 页面设置属性完备，默认值合理 |
| `DataBindingDefinition` | 🟢 可增强 | Transform 字段目前为字符串，可扩展为结构化转换规则 |

### 1.4 适配器模型

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `IDataAdapter` | 🔴 待迁移 | 接口已定义但三种适配器均未实现，需统一适配器实现 |
| `AdapterConfigBase` | 🟡 待强化 | 缺少 `Validate()` 方法，校验逻辑分散在各工厂类中 |
| `AdapterResult` | 🟡 待强化 | 缺少 `Timestamp` 和 `Duration` 元数据，不利于性能监控 |
| `FieldSchema` | 🔒 最终态 | 字段模式定义完整，数据类型和约束完备 |
| `TableDataValue` | 🔒 最终态 | 表格数据值结构清晰 |
| `ValidationResult` | 🔒 最终态 | 校验结果结构完整 |

### 1.5 序列化与工具

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ElementJsonConverter` | 🔴 待迁移 | 静态字典 WebShortTypeMap 不可扩展，需改为动态注册机制；未知类型应抛异常而非降级 |
| `TemplateSerializer` | 🟡 待强化 | 静态 JsonSerializerSettings 不支持运行时配置注入；缺少版本标识写入 |
| `IdGenerator` | 🔒 最终态 | Nanoid 21位ID生成逻辑稳定，无需修改 |
| `ElementGroupRegistry` | 🔴 待迁移 | 静态构造函数不可扩展，需改为动态注册或 Attribute 扫描 |

### 1.6 DTO / Request / Response

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ApiResponse<T>` | 🔒 最终态 | 统一响应封装，Ok/Fail 工厂方法完备 |
| `PagedResponse<T>` | 🔒 最终态 | 分页元数据完整（TotalPages/HasPrevious/HasNext） |
| `TemplateDto` | 🔒 最终态 | 列表项 DTO，字段精简 |
| `TemplateDetailDto` | 🔒 最终态 | 详情 DTO，含 ContentJson |
| `TemplateVersionDto` | 🔒 最终态 | 版本列表项 DTO |
| `TemplateVersionDetailDto` | 🔒 最终态 | 版本详情 DTO |
| `AuthDtos` (LoginRequest/Response 等) | 🟡 待强化 | 缺少输入验证注解（[Required]、[StringLength]） |
| `CreateTemplateRequest` | 🟡 待强化 | 缺少输入验证注解 |
| `UpdateTemplateRequest` | 🟡 待强化 | 缺少输入验证注解 |
| `TemplateFilterRequest` | 🟡 待强化 | PageSize 缺少上限验证注解 |
| `RollbackRequest` | 🔒 最终态 | 字段简洁，无扩展需求 |

### 1.7 Contracts 层统计

| 状态 | 数量 | 占比 |
|------|------|------|
| 🔒 最终态 | 28 | 66.7% |
| 🟢 可增强 | 5 | 11.9% |
| 🟡 待强化 | 8 | 19.0% |
| 🔴 待迁移 | 4 | 9.5% |

---

## 二、Editor.Core 模块

### 2.1 数据层

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `TemplateDbContext` | 🟡 待强化 | 缺少 TemplateEntity → TemplateVersionEntity 的级联删除配置；缺少查询拦截器（如软删除） |
| `TemplateEntity` | 🔒 最终态 | 实体属性完备，与数据库表一一对应 |
| `TemplateVersionEntity` | 🔒 最终态 | 版本实体属性完备 |
| `UserEntity` | 🔒 最终态 | 用户实体属性完备 |
| `RefreshTokenEntity` | 🔒 最终态 | 令牌实体属性完备 |
| `TemplateSeedData` | 🔴 待迁移 | 明文密码硬编码，应迁移到配置文件或环境变量；种子逻辑应支持幂等检查 |

### 2.2 服务层

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ITemplateService` | 🔒 最终态 | 接口方法集完整（CRUD），签名稳定 |
| `TemplateService` | 🟡 待强化 | 缺少显式事务边界；UpdateAsync 的版本自动创建逻辑需事务保障 |
| `IAuthService` | 🔒 最终态 | 接口方法集完整（Login/Refresh/Revoke） |
| `AuthService` | 🔴 待迁移 | JWT 密钥硬编码需外部化；AccessToken 无法撤销需引入黑名单 |
| `IVersionService` | 🔒 最终态 | 接口方法集完整 |
| `VersionService` | 🟡 待强化 | DiffAsync 缺少属性级对比；元素类型提取依赖字符串操作，脆弱 |
| `ContextService` | 🟡 待强化 | 缺少接口抽象（无 IContextService）；内置上下文路径硬编码 |
| `PdfRenderService` | 🔴 待迁移 | 与 Generators 层 PdfExportService 功能重复，应迁移到共享渲染层 |
| `PdfTemplateRenderer` | 🔴 待迁移 | 同上，427 行渲染逻辑应提取到共享层 |
| `JsonTemplateSerializer` | 🔒 最终态 | 序列化/反序列化实现正确，与 Contracts 层 TemplateSerializer 配置一致 |
| `DataBindingEngine` | 🟢 可增强 | 当前仅支持样本数据注入，可扩展为支持真实数据源绑定 |

### 2.3 共享接口

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `IDataBindingEngine` | 🔒 最终态 | 接口方法集精简且稳定 |
| `IPdfSharpTemplateRenderer` | 🔴 待迁移 | 接口签名与 Editor 绑定（返回 byte[]/string），应迁移到共享渲染层并重新定义 |
| `IJsonTemplateSerializer` | 🔒 最终态 | 接口方法集精简且稳定 |
| `IPdfRenderService` | 🔴 待迁移 | 同 IPdfSharpTemplateRenderer，应合并到共享渲染接口 |

### 2.4 扩展与迁移

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ServiceCollectionExtensions` | 🟡 待强化 | ContextService 未通过接口注册；PdfRenderService 注册方式需随渲染层迁移调整 |
| `InitialCreate` (Migration) | 🔒 最终态 | EF Core 迁移文件，不可修改 |
| `AddAuthEntities` (Migration) | 🔒 最终态 | EF Core 迁移文件，不可修改 |
| `TemplateDbContextModelSnapshot` | 🔒 最终态 | EF Core 快照文件，不可修改 |

### 2.5 Editor.Core 统计

| 状态 | 数量 | 占比 |
|------|------|------|
| 🔒 最终态 | 12 | 52.2% |
| 🟢 可增强 | 1 | 4.3% |
| 🟡 待强化 | 5 | 21.7% |
| 🔴 待迁移 | 5 | 21.7% |

---

## 三、Editor.Server 模块

### 3.1 控制器

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `AuthController` | 🟡 待强化 | 缺少速率限制（防暴力破解）；Logout 缺少 AccessToken 黑名单处理 |
| `TemplatesController` | 🟡 待强化 | 缺少输入验证注解；分页查询缺少上限保护 |
| `VersionsController` | 🟡 待强化 | Diff 端点参数缺少非空验证；缺少属性级 Diff 输出 |
| `PreviewController` | 🔒 最终态 | 图片/PDF 预览端点设计合理 |
| `ContextController` | 🔒 最终态 | 上下文值查询端点设计合理 |
| `HealthController` | 🔒 最终态 | 健康检查端点简洁稳定 |

### 3.2 中间件

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `GlobalExceptionMiddleware` | 🟡 待强化 | 缺少 TimeoutException、DbUpdateException、JsonException 处理 |
| `ApiLoggingMiddleware` | 🟢 可增强 | 可添加请求体/响应体选择性记录（调试模式） |

### 3.3 启动配置

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `Program` | 🟡 待强化 | QuestPDF License 设置应移到 Core 层；缺少速率限制中间件注册；缺少 HTTPS 重定向 |

### 3.4 测试

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `AuthControllerTests` | 🟢 可增强 | 覆盖登录/刷新/登出，可增加边界用例 |
| `TemplatesControllerTests` | 🟢 可增强 | 覆盖 CRUD，可增加并发修改测试 |
| `PreviewControllerTests` | 🟢 可增强 | 覆盖预览，可增加大模板性能测试 |
| `HealthControllerTests` | 🔒 最终态 | 健康检查测试完备 |
| `VersionsControllerTests` | 🟢 可增强 | 新增测试文件，可继续扩展 |

### 3.5 Editor.Server 统计

| 状态 | 数量 | 占比 |
|------|------|------|
| 🔒 最终态 | 3 | 23.1% |
| 🟢 可增强 | 5 | 38.5% |
| 🟡 待强化 | 5 | 38.5% |
| 🔴 待迁移 | 0 | 0% |

---

## 四、Generators/ReportDataMaker 模块

### 4.1 基础设施层

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ViewModelBase` | 🔒 最终态 | INotifyPropertyChanged 实现 + SetProperty<T> 泛型方法，MVVM 基类稳定 |
| `RelayCommand` | 🔒 最终态 | ICommand 委托封装，CommandManager 集成正确 |
| `AsyncRelayCommand` | 🟡 待强化 | async void 异常未捕获，需添加 try-catch 路由到错误处理 |
| `IDialogService` | 🔒 最终态 | 对话框服务接口定义完整 |
| `DialogService` | 🔒 最终态 | WPF 对话框实现完整 |
| `FileLogger` | 🔴 待迁移 | 应迁移到 Microsoft.Extensions.Logging + Serilog；当前无日志级别、无结构化 |
| `FieldDataTemplateSelector` | 🔒 最终态 | WPF DataTemplate 选择逻辑稳定 |
| `ReportExternalElementConverter` | 🔴 待迁移 | 双重模型转换器，随 R-001 修复需重写为组合模式映射器 |

### 4.2 模型层

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ExternalTemplateDefinition` | 🔴 待迁移 | 与 Contracts 层 TemplateDefinition 重复，应迁移为直接使用 Contracts 模型 + 扩展属性 |
| `ReportExternalElementBase` | 🔴 待迁移 | `new` 隐藏基类 14 个属性，应迁移为组合模式 |
| `ExternalTextElement` ~ `ExternalWatermarkElement` (23种) | 🔴 待迁移 | 全部随 ReportExternalElementBase 迁移，消除 `new` 隐藏 |

### 4.3 服务接口

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ITemplateLoaderService` | 🔒 最终态 | 模板加载接口稳定 |
| `ITemplatePreviewService` | 🔒 最终态 | 模板预览接口稳定 |
| `IDataBindingService` | 🔒 最终态 | 数据绑定接口稳定 |
| `IPdfExportService` | 🟡 待强化 | 缺少进度回调接口（批量导出时需要） |

### 4.4 服务聚合

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `TemplateServices` | 🟡 待强化 | 聚合类隐藏了真实依赖关系，建议改为直接注入 |
| `AdapterServices` | 🟡 待强化 | 同上 |
| `ExportServices` | 🟡 待强化 | 同上 |

### 4.5 核心服务

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `TemplateLoaderService` | 🟡 待强化 | 缺少模板版本兼容性检查；异常处理不够细致 |
| `TemplatePreviewService` | 🔒 最终态 | 预览逻辑稳定 |
| `DataBindingService` | 🟡 待强化 | 表格数据处理策略复杂，缺少单元测试保障 |
| `CanvasRenderer` | 🟡 待强化 | 与 PdfElementRenderer 功能部分重叠，可统一 |
| `AdapterConfigStore` | 🟡 待强化 | 空 catch 块吞没异常；JSON 并发写入不安全 |
| `ConfigProtector` | 🟡 待强化 | DPAPI 加密无法跨机器迁移；ENC: 前缀检测不够健壮 |

### 4.6 Excel 适配器

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ExcelAdapterFactory` | 🟡 待强化 | 应实现 Contracts 层 IDataAdapter 接口；同步方法需包装为异步 |
| `ExcelAdapterConfig` | 🔒 最终态 | Excel 适配器配置模型完整 |
| `TemplateFlattenService` | 🔒 最终态 | 模板扁平化逻辑正确且稳定 |
| `TemplateFieldSchema` | 🔒 最终态 | 字段模式定义完整 |
| `ExcelSchemaExporter` | 🔒 最终态 | 契约行导出逻辑稳定 |
| `ExcelContractReader` | 🟡 待强化 | 缺少契约行完整性校验（用户可能误删隐藏行） |
| `ExcelDataValidator` | 🔒 最终态 | 数据校验规则完整 |

### 4.7 数据库适配器

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `IDatabaseProvider` | 🔒 最终态 | 数据库提供者接口设计合理，方法集完整 |
| `DatabaseProviderRegistry` | 🔒 最终态 | 注册表模式正确，CreateDefault 覆盖 4 种数据库 |
| `SqlServerProvider` | 🔒 最终态 | SQL Server 提供者实现完整 |
| `MySqlProvider` | 🔒 最终态 | MySQL 提供者实现完整 |
| `SqliteProvider` | 🔒 最终态 | SQLite 提供者实现完整 |
| `PostgreSqlProvider` | 🔒 最终态 | PostgreSQL 提供者实现完整 |
| `DatabaseAdapterBase` | 🟡 待强化 | ExecuteReadAsync 中 DbCommand/DbDataReader 需确保异常时释放 |
| `DatabaseAdapterFactory` | 🟡 待强化 | 应实现 Contracts 层 IDataAdapter 接口 |
| `DatabaseAdapterConfig` | 🔒 最终态 | 数据库适配器配置模型完整 |
| `SqlBuilder` | 🔴 待迁移 | RawSQL 模式存在 SQL 注入风险，需迁移到参数化构建 + 只读限制 |
| `ConnectionPoolManager` | 🟡 待强化 | 需添加连接泄漏检测和超时回收机制 |
| `TableInfo` | 🔒 最终态 | 表信息模型完整 |

### 4.8 上下文适配器

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `ContextAdapterService` | 🟡 待强化 | 应实现 Contracts 层 IDataAdapter 接口；缺少校验方法 |
| `ContextAdapterFactory` | 🟡 待强化 | 同上 |
| `ContextAdapterConfig` | 🔒 最终态 | 上下文适配器配置模型完整 |
| `ContextProfileStore` | 🟡 待强化 | 文件并发写入不安全 |
| `ContextDataSource` | 🔒 最终态 | 上下文数据源枚举完整 |

### 4.9 PDF 导出

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `PdfExportService` | 🔴 待迁移 | 与 Editor.Core PdfTemplateRenderer 重复，应迁移到共享渲染层 |
| `PdfElementRenderer` | 🔴 待迁移 | 同上 |
| `PdfPageLayoutEngine` | 🔴 待迁移 | 同上，应作为共享渲染层的一部分 |
| `ReportDocumentPaginator` | 🔴 待迁移 | 同上 |
| `BatchExportService` | 🟡 待强化 | 缺少内存限制和背压机制；并行度控制需优化 |
| `ExportHistoryStore` | 🟡 待强化 | 文件并发写入不安全；MaxRecords 限制仅在 Add 时检查 |

### 4.10 ViewModel 层

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `MainViewModel` | 🔴 待迁移 | God Object，6 种职责需拆分为 TemplateManager/AdapterManager/ExportManager/TabManager |
| `SidePanelViewModel` | 🔒 最终态 | 侧边栏展开/折叠逻辑简单稳定 |
| `TemplateLoadViewModel` | 🔒 最终态 | 模板加载逻辑稳定 |
| `TabViewModelBase` | 🟡 待强化 | 事件订阅需确保取消订阅，防止内存泄漏 |
| `MainTabViewModel` | 🔒 最终态 | 主标签页（数据录入+预览）逻辑稳定 |
| `DataEntryTabViewModel` | 🟡 待强化 | 数据录入字段动态生成逻辑复杂，缺少输入验证 |
| `PreviewTabViewModel` | 🔒 最终态 | 预览缩放逻辑稳定 |
| `ExcelAdapterTabViewModel` | 🟡 待强化 | 随 ExcelAdapterFactory 实现 IDataAdapter 需调整 |
| `DatabaseAdapterTabViewModel` | 🟡 待强化 | 随 DatabaseAdapterFactory 实现 IDataAdapter 需调整 |
| `ContextAdapterTabViewModel` | 🟡 待强化 | 随 ContextAdapterFactory 实现 IDataAdapter 需调整 |
| `ExportTabViewModel` | 🔒 最终态 | 导出标签页逻辑稳定 |
| `AddAdapterDialogViewModel` | 🔒 最终态 | 添加适配器对话框逻辑稳定 |

### 4.11 转换器

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `TableDataConverter` | 🔒 最终态 | 表格数据 WPF 转换器稳定 |
| `RadioGroupConverter` | 🔒 最终态 | 单选组转换器稳定 |
| `FieldTemplateSelector` | 🔒 最终态 | 字段模板选择器稳定 |
| `DoubleConverter` | 🔒 最终态 | 双精度转换器稳定 |
| `BooleanConverters` | 🔒 最终态 | 布尔转换器稳定 |
| `BoolToVisibilityConverter` | 🔒 最终态 | 可见性转换器稳定 |

### 4.12 应用入口

| 类型 | 状态 | 判定理由 |
|------|------|----------|
| `App` (App.xaml.cs) | 🟡 待强化 | TestTemplateLoading 应条件编译；DI 注册需随 MainViewModel 拆分调整 |
| `AssemblyInfo` | 🔒 最终态 | 程序集元数据，不需修改 |

### 4.13 Generators 统计

| 状态 | 数量 | 占比 |
|------|------|------|
| 🔒 最终态 | 31 | 50.8% |
| 🟢 可增强 | 0 | 0% |
| 🟡 待强化 | 19 | 31.1% |
| 🔴 待迁移 | 11 | 18.0% |

---

## 五、全局统计

| 状态 | Contracts | Editor.Core | Editor.Server | Generators | 合计 | 占比 |
|------|-----------|-------------|---------------|------------|------|------|
| 🔒 最终态 | 28 | 12 | 3 | 31 | **74** | 55.2% |
| 🟢 可增强 | 5 | 1 | 5 | 0 | **11** | 8.2% |
| 🟡 待强化 | 8 | 5 | 5 | 19 | **37** | 27.6% |
| 🔴 待迁移 | 4 | 5 | 0 | 11 | **20** | 14.9% |
| **合计** | **45** | **23** | **13** | **61** | **142** | 100% |

---

## 六、架构级扩展方向

### 6.1 Contracts 层扩展方向

| 方向 | 描述 | 涉及类型 | 优先级 |
|------|------|----------|--------|
| **适配器契约完善** | 定义 IDataAdapter 接口，统一三种适配器实现 | `IDataAdapter`, `AdapterConfigBase`, `AdapterResult` | P1 |
| **元素类型动态注册** | 将 ElementJsonConverter 和 ElementGroupRegistry 改为可动态注册 | `ElementJsonConverter`, `ElementGroupRegistry` | P2 |
| **模板版本化** | 为 TemplateDefinition 添加 SchemaVersion 字段，支持向后兼容 | `TemplateDefinition`, `TemplateSerializer` | P2 |
| **适配器元数据扩展** | AdapterResult 添加 Timestamp/Duration 元数据 | `AdapterResult` | P3 |

### 6.2 Editor 层扩展方向

| 方向 | 描述 | 涉及类型 | 优先级 |
|------|------|----------|--------|
| **安全加固** | JWT 密钥外部化、AccessToken 黑名单、速率限制 | `AuthService`, `AuthController`, `Program` | P0 |
| **渲染层共享** | 提取 PdfTemplateRenderer 到共享项目 | `PdfTemplateRenderer`, `PdfRenderService`, `IPdfSharpTemplateRenderer`, `IPdfRenderService` | P2 |
| **版本 Diff 增强** | 属性级 Diff + JSON Patch 输出 | `VersionService` | P2 |
| **输入验证完善** | 为所有 Request 添加验证注解 | `CreateTemplateRequest`, `UpdateTemplateRequest`, `AuthDtos` | P1 |
| **审计日志** | 添加操作审计中间件 | 新增 `AuditLogMiddleware` | P3 |

### 6.3 Generators 层扩展方向

| 方向 | 描述 | 涉及类型 | 优先级 |
|------|------|----------|--------|
| **模型统一** | 消除双重模型，使用组合模式替代继承 | `ReportExternalElementBase`, 23 种 External 元素, `ExternalTemplateDefinition` | P1 |
| **MainViewModel 拆分** | 拆分为 4 个管理器类 | `MainViewModel` | P1 |
| **适配器插件化** | 定义 IAdapterPlugin 接口，支持动态注册 | `MainViewModel`, `App`, 新增 `IAdapterPlugin` | P2 |
| **SQL 注入防护** | RawSQL 只读限制 + WHERE 参数化 | `SqlBuilder` | P0 |
| **日志框架升级** | 替换 FileLogger 为 Microsoft.Extensions.Logging | `FileLogger`, 所有使用 FileLogger 的类 | P1 |
| **渲染层共享** | 与 Editor 端合并 PDF 渲染到共享项目 | `PdfExportService`, `PdfElementRenderer`, `PdfPageLayoutEngine`, `ReportDocumentPaginator` | P2 |
| **批量导出优化** | 流式生成 + 内存限制 + 背压 | `BatchExportService`, `PdfExportService` | P3 |

---

## 七、最终态冻结建议

以下类型建议标记为**最终态冻结**，在代码中添加 `[Obsolete("此类型已冻结，不可修改")]` 或通过代码审查规则强制保护：

### 7.1 Contracts 层冻结清单（28 个）

```
ElementGroup, IdGenerator, ApiResponse<T>, PagedResponse<T>,
TemplateDto, TemplateDetailDto, TemplateVersionDto, TemplateVersionDetailDto,
RollbackRequest, PageSettings, FieldSchema, TableDataValue, ValidationResult,
TextElement, NumberElement, DateElement, LineElement, ShapeElement,
DividerElement, BarcodeElement, QrCodeElement, SignatureElement,
CheckboxElement, RadioElement, DropdownElement, ImageElement,
IconElement, HyperlinkElement, HeaderElement, FooterElement,
PageNumberElement, WatermarkElement
```

### 7.2 Editor 层冻结清单（15 个）

```
TemplateEntity, TemplateVersionEntity, UserEntity, RefreshTokenEntity,
ITemplateService, IAuthService, IVersionService, IDataBindingEngine,
IJsonTemplateSerializer, JsonTemplateSerializer, PreviewController,
ContextController, HealthController, InitialCreate, AddAuthEntities
```

### 7.3 Generators 层冻结清单（31 个）

```
ViewModelBase, RelayCommand, IDialogService, DialogService,
FieldDataTemplateSelector, ITemplateLoaderService, ITemplatePreviewService,
IDataBindingService, ExcelAdapterConfig, TemplateFlattenService,
TemplateFieldSchema, ExcelSchemaExporter, ExcelDataValidator,
IDatabaseProvider, DatabaseProviderRegistry, SqlServerProvider,
MySqlProvider, SqliteProvider, PostgreSqlProvider, DatabaseAdapterConfig,
TableInfo, ContextAdapterConfig, ContextDataSource, SidePanelViewModel,
TemplateLoadViewModel, MainTabViewModel, PreviewTabViewModel,
ExportTabViewModel, AddAdapterDialogViewModel, TableDataConverter,
RadioGroupConverter, FieldTemplateSelector, DoubleConverter,
BooleanConverters, BoolToVisibilityConverter, AssemblyInfo
```

---

## 八、迁移路线图

```
Phase 1 (P0, 1-2周)          Phase 2 (P1, 1-2月)           Phase 3 (P2, 3-6月)
┌─────────────────────┐    ┌──────────────────────┐    ┌──────────────────────────┐
│ • SqlBuilder 注入防护 │    │ • IDataAdapter 接口   │    │ • 共享渲染层提取          │
│ • JWT 密钥外部化     │    │ • 模型统一(组合模式)   │    │ • 适配器插件化            │
│ • AsyncRelayCommand  │    │ • MainViewModel 拆分  │    │ • 元素类型动态注册        │
│   异常处理           │    │ • FileLogger → Serilog│    │ • 模板版本化              │
│ • 输入验证注解       │    │ • 输入验证完善        │    │ • 审计日志                │
└─────────────────────┘    └──────────────────────┘    └──────────────────────────┘
     安全优先                    架构治理                     生态扩展
```

**关键约束**：Phase 1 和 Phase 2 期间，🔒 最终态类型**不可修改**。所有变更通过新增类型或扩展方法实现，确保向后兼容。
