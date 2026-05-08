# Xinglin.ReportPlatform 报告平台

基于契约驱动的报告单生成平台，支持在线模板编辑、多格式数据生产和可扩展的适配器架构。

## 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                    Xinglin.ReportPlatform                   │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────┐    ┌──────────┐    ┌──────────────┐          │
│  │  Editor  │───▶│Contracts │◀───│  Generators  │          │
│  │ (编辑器) │    │ (契约层) │    │  (生产器层)  │          │
│  └──────────┘    └──────────┘    └──────────────┘          │
│         │              │                │                   │
│         ▼              ▼                ▼                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                    Adapters                           │   │
│  │     (数据适配器: 数据库/Excel/API等)                 │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## 目录结构

| 目录 | 职责 | 说明 |
|------|------|------|
| `Contracts/` | 契约定义层 | 统一的数据模型、DTO和接口契约 |
| `Editor/` | 在线编辑器 | 报告单模板设计和契约生成服务 |
| `Generators/` | 生产器层 | 基于契约的数据生产器集合 |
| `Adapters/` | 适配器层 | 数据获取组件（预留扩展） |
| `Shared/` | 共享组件 | 通用工具和公共逻辑（预留扩展） |
| `docs/` | 文档中心 | 契约文档和技术规范 |

### Contracts（契约层）

```
Contracts/
├── DTOs/           # 数据传输对象
├── Requests/       # 请求模型
├── Responses/      # 响应模型
└── Xinglin.WebReportEditor.Contracts.csproj
```

### Editor（编辑器层）

```
Editor/
├── Core/           # 编辑器核心逻辑
│   ├── Data/       # 数据实体和上下文
│   ├── Services/   # 业务服务
│   └── SharedInterfaces/  # 共享接口定义
└── Server/         # API服务端
    ├── Controllers/    # REST API控制器
    └── Middleware/     # 中间件
```

### Generators（生产器层）

```
Generators/
└── ReportDataMaker/    # WPF数据录入生产器
    ├── Models/         # 数据模型
    ├── Services/       # 业务服务
    ├── ViewModels/     # 视图模型
    ├── Views/          # 视图组件
    └── Templates/      # 内置模板
```

## 核心组件

### 1. Editor（在线报告单编辑器）

负责报告单模板的可视化设计和契约生成：
- 模板CRUD管理
- 版本控制
- 契约导出

### 2. Contracts（契约层）

作为Editor和Generators之间的桥梁：
- 定义统一的数据结构
- 规范模板元数据格式
- 确保多生产器一致性

### 3. Generators（生产器层）

可扩展的生产器集合：
- **ReportDataMaker**: WPF桌面应用，基于契约执行数据录入和报告生成
- *预留扩展*: PDF生成器、Word生成器、HTML生成器等

### 4. Adapters（适配器层）

数据获取组件的预留空间：
- 数据库适配器
- Excel导入适配器
- API数据适配器

## 工作流程

1. **设计阶段**: 通过Editor设计报告单模板，生成结构化契约
2. **导出契约**: 将模板结构导出为标准契约格式
3. **数据生产**: Generator读取契约，执行数据录入和报告生成
4. **数据适配**: 通过Adapters从多种数据源获取数据

## 扩展能力

- **新增生产器**: 在`Generators/`目录下添加新的生产器项目
- **新增适配器**: 在`Adapters/`目录下添加数据获取组件
- **共享组件**: 将通用逻辑放入`Shared/`目录

## 技术栈

| 组件 | 技术 |
|------|------|
| Editor.Server | ASP.NET Core 8 |
| Editor.Core | .NET 8 Class Library |
| Contracts | .NET 8 Class Library |
| ReportDataMaker | WPF (.NET 10) |

## 解决方案结构

建议在根目录创建 `.sln` 文件，包含以下项目引用：

- `Xinglin.ReportPlatform.Contracts`
- `Xinglin.ReportPlatform.Editor.Core`
- `Xinglin.ReportPlatform.Editor.Server`
- `Xinglin.ReportPlatform.Generators.ReportDataMaker`