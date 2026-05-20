# 风险报告 R-003：PDF 渲染双重实现

| 字段 | 值 |
|------|-----|
| 风险编号 | R-003 |
| 风险等级 | 🟡 中 |
| 影响模块 | Editor.Core + Generators |
| 发现日期 | 2026-05-20 |
| 状态 | 待修复 |

---

## 1. 风险描述

Editor.Core 和 Generators 各自独立实现了基于 QuestPDF + SkiaSharp 的 PDF 渲染逻辑，两套实现功能重复、输入模型不同、维护成本翻倍。当新增元素类型时，需要在两处添加渲染逻辑，且两套实现的渲染结果可能不一致。

---

## 2. 涉及代码

### 2.1 Editor.Core 实现

| 文件 | 行数 | 职责 |
|------|------|------|
| [PdfTemplateRenderer.cs](../../Editor/Core/Services/PdfTemplateRenderer.cs) | ~427 行 | 完整的 PDF 模板渲染器 |
| [PdfRenderService.cs](../../Editor/Core/Services/PdfRenderService.cs) | ~51 行 | 服务包装，调用 PdfTemplateRenderer |

**输入模型**：`TemplateDefinition`（Contracts 层）

**特点**：
- 用于编辑器预览（图片 + PDF）
- 实现了 `IPdfSharpTemplateRenderer` 接口
- 注册为 Scoped 生命周期

### 2.2 Generators 实现

| 文件 | 行数 | 职责 |
|------|------|------|
| [PdfExportService.cs](../../Generators/ReportDataMaker/Services/PdfExport/PdfExportService.cs) | ~99 行 | PDF 导出服务 |
| [PdfElementRenderer.cs](../../Generators/ReportDataMaker/Services/PdfExport/PdfElementRenderer.cs) | — | 元素渲染器 |
| [PdfPageLayoutEngine.cs](../../Generators/ReportDataMaker/Services/PdfExport/PdfPageLayoutEngine.cs) | — | 页面布局引擎 |
| [BatchExportService.cs](../../Generators/ReportDataMaker/Services/PdfExport/BatchExportService.cs) | ~108 行 | 批量导出服务 |
| [ExportHistoryStore.cs](../../Generators/ReportDataMaker/Services/PdfExport/ExportHistoryStore.cs) | ~74 行 | 导出历史 |

**输入模型**：`ExternalTemplateDefinition`（Generators 层）

**特点**：
- 用于数据生产端的 PDF 导出
- 支持批量导出和导出历史
- 注册为 Singleton 生命周期

### 2.3 共同依赖

| 依赖 | 版本 | 用途 |
|------|------|------|
| QuestPDF | 2025.1.0 | PDF 文档结构生成 |
| SkiaSharp | 3.116.1 | 图形渲染 |

两处均设置了 `QuestPDF.Settings.License = LicenseType.Community`：
- [Editor/Server/Program.cs](../../Editor/Server/Program.cs) 第 13 行
- [Generators/ReportDataMaker/App.xaml.cs](../../Generators/ReportDataMaker/App.xaml.cs) 第 26 行

---

## 3. 风险场景

### 3.1 渲染结果不一致

两套实现使用不同的输入模型和不同的渲染逻辑：

| 维度 | Editor.Core | Generators |
|------|------------|------------|
| 输入模型 | `TemplateDefinition`（嵌套 PageSettings） | `ExternalTemplateDefinition`（扁平化页面属性） |
| 元素类型 | 22 种 Contracts 元素 | 23 种 Generators 元素 |
| 默认值处理 | nullable 属性使用 C# 默认值 | non-nullable 属性使用自定义默认值 |
| 页面布局 | 自定义实现 | `PdfPageLayoutEngine` |

**结果**：同一模板在编辑器预览和数据生产端导出的 PDF 可能存在视觉差异。

### 3.2 新增元素类型双重维护

新增元素类型时，需要同时修改：
1. Editor.Core 的 `PdfTemplateRenderer` — 添加元素渲染逻辑
2. Generators 的 `PdfElementRenderer` — 添加元素渲染逻辑

如果遗漏任何一处，该元素在对应端的 PDF 输出中将缺失。

### 3.3 QuestPDF 许可证重复设置

两处入口都设置了 `QuestPDF.Settings.License`，如果共享渲染层，只需设置一次。

### 3.4 Bug 修复不同步

修复一端的渲染 Bug 时，另一端可能遗漏相同的问题。

---

## 4. 影响范围

| 影响维度 | 评估 |
|----------|------|
| 维护成本 | 🔴 高 — 双重实现，Bug 修复需同步 |
| 渲染一致性 | 🟡 中 — 两端渲染结果可能不同 |
| 新元素开发效率 | 🟡 中 — 需要两处添加渲染逻辑 |
| 包体积 | 🟢 低 — 依赖相同，无额外开销 |

---

## 5. 修复方案

### 方案 A：提取共享渲染层（推荐）

新建 `Rendering` 项目（或放入 Contracts 层），将 PDF 渲染逻辑统一：

```
Contracts/ (或 Rendering/)
├── Models/
│   └── RenderContext.cs        # 统一渲染上下文
├── Services/
│   ├── ITemplateRenderer.cs    # 渲染接口
│   ├── PdfTemplateRenderer.cs  # 统一渲染实现
│   ├── PdfElementRenderer.cs   # 统一元素渲染
│   └── PdfPageLayoutEngine.cs  # 统一页面布局
```

Editor 和 Generators 均引用该共享层，传入统一的渲染上下文。

**优点**：彻底消除重复，渲染一致性有保障
**缺点**：需要统一输入模型（与 R-001 关联），改动量大

### 方案 B：Generators 引用 Editor.Core 的渲染器

让 Generators 直接引用 Editor.Core 的 `PdfTemplateRenderer`。

**优点**：改动最小
**缺点**：引入不必要的依赖（Generators 依赖 Editor.Core 的 EF Core 等），架构不清晰

### 方案 C：保持现状，添加一致性测试

不合并代码，但添加对比测试，确保两端渲染结果一致。

**优点**：零改动
**缺点**：维护成本仍然翻倍

---

## 6. 关联风险

本风险与 R-001（双重模型体系）强关联。统一 PDF 渲染的前提是统一输入模型，因此建议在 R-001 修复后再处理本风险。

---

## 7. 建议优先级

**P2 — 长期修复（3-6 月）**

建议采用方案 A，在 R-001 修复后提取共享渲染层。中期可采用方案 C 添加一致性测试作为过渡措施。
