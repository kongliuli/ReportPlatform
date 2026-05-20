# 五个 UI/逻辑问题修复

## TL;DR

> **Quick Summary**: 修复 ReportDataMaker 的五个问题：静态值下拉去重、日期/性别控件类型检测、未配置字段隔离、面板拆分、鼠标滚轮传播。
>
> **Deliverables**:
> - 上下文静态值下拉不再出现已添加字段
> - 报告日期字段显示 DatePicker，性别字段显示 ComboBox
> - 未配置字段不自动落入"已匹配"，在 UI 上分离标识
> - 适配器区/手动录入区清晰划分
> - 滚轮在任意子控件上都能滚动父级面板
>
> **Estimated Effort**: Medium
> **Parallel Execution**: YES - 5 independent tasks
> **Critical Path**: None (all tasks independent)

---

## Context

### Original Request
用户报告五个问题：
1. 上下文静态值下拉框选择一个到下方时应该取消掉，不能出现选中两次的情况
2. 报告日期和性别的控件还是只有输入框，而不是日期选择器和下拉框
3. 未配置的内容不是默认被视为被匹配的，而是被下放
4. 录入面板分为适配器适配模块和可录入模块，对被使用字段进行划分
5. 鼠标滚动时可以对当前页的滚动条起效

### Interview Summary
**Key Discussions**:
- Issue A（静态值下拉去重）: 用户选择"从选项中消失"——添加后从 UnconfiguredFields 移除
- Issue B（日期/性别控件）: 用户选择"修模板端 + 增加契约文档"——同时在 ReportDataMaker 加启发式回退
- Issue C+D（未配置分离 + 面板拆分）: 用户选择"两者都要"——既看 Group 类型也看实际占用

**Research Findings**:
- `ReportExternalElementConverter` 将 JSON `$type` 正确映射到 External*Element 类型，问题在模板 JSON 侧 `$type` 标记错误
- `FieldDataTemplateSelector` 正确路由 FieldDataType.Date/Dropdown → 对应模板
- `ContextAdapterService.FillContext` 中未配置字段走 `ResolveBuiltIn` 返回空串
- `MainTabViewModel.LoadFields` 已有三个 Section 但分类逻辑混合了 Group 和 DefaultValue
- ReportEditor 不在当前仓库中，无法直接修模板端

---

## Work Objectives

### Core Objective
修复五个 UI/逻辑问题，使 ReportDataMaker 的数据录入和上下文配置面板行为正确。

### Concrete Deliverables
- 上下文配置面板：下拉去重 + 未配置字段隔离
- 数据录入面板：日期/性别控件正确显示 + 适配器区/手动录入区清晰划分
- 全局：鼠标滚轮正常传播
- 文档：契约类型标记说明

### Definition of Done
- [ ] 从 UnconfiguredFields 下拉添加字段后，该字段从选项中消失
- [ ] 标签含"日期"的 TextElement 显示 DatePicker 控件
- [ ] 标签含"性别"或有 Options 的 TextElement 显示 ComboBox
- [x] FillContext 不再对未配置字段返回空串（标记为未配置）
- [ ] 主面板左侧滚动条在 DataGrid 等子控件上也能响应滚轮
- [ ] docs 中存在契约类型说明文档

### Must Have
- 下拉去重
- 日期/性别控件类型检测
- 未配置字段隔离
- 滚轮修复

### Must NOT Have (Guardrails)
- 不修改 Contracts 项目（类型标记问题仅记录在文档中）
- 不修改 ReportEditor（不在本仓库）
- 不添加新的外部依赖
- 不改变现有 API 契约（仅增加字段/方法）

---

## Verification Strategy

> **ZERO HUMAN INTERVENTION** - ALL verification is agent-executed.

### Test Decision
- **Infrastructure exists**: NO
- **Automated tests**: None
- **Framework**: N/A
- **Agent-Executed QA**: ALWAYS (manual verification via code review + build check)

### QA Policy
Every task MUST include agent-executed verification via code review (grep for patterns, verify logic correctness) and build check (`dotnet build`).

---

## Execution Strategy

### Parallel Execution Waves

> All 5 issues are independent — execute in ONE wave.

```
Wave 1 (All tasks start immediately — MAX PARALLEL):
├── Task A: ContextAdapterTabViewModel — 下拉去重 [quick]
├── Task B: MainTabViewModel — 日期/性别启发式回退 [quick]
├── Task C: ContextAdapterService — 未配置字段隔离 [quick]
├── Task D: MainTabViewModel — 面板拆分逻辑 [quick]
├── Task E: MainTab.xaml/.cs — 鼠标滚轮传播 [quick]
└── Task F: docs/ — 契约类型说明文档 [quick]
```

---

## TODOs

> Implementation + Test = ONE Task. Never separate.
> EVERY task MUST have: Recommended Agent Profile + Parallelization info + QA Scenarios.
> **A task WITHOUT QA Scenarios is INCOMPLETE. No exceptions.**

- [x] A. **ContextAdapterTabViewModel — 下拉去重（Issue A）**

  **What to do**:
  - 在 `ExecuteAddFromUnconfigured()` 方法末尾（line 242 之后），添加两行：
    ```csharp
    UnconfiguredFields.Remove(SelectedUnconfiguredField);
    SelectedUnconfiguredField = null;
    ```
  - 这确保字段添加到 StaticValues 后，立即从 UnconfiguredFields 下拉选项中消失
  - 现有重复检查（lines 234-240）已防止同一字段重复添加，仅需补全移除逻辑

  **Must NOT do**:
  - 不要修改 `ExecuteDetectUnconfigured`（检测逻辑正确）
  - 不要修改 `StaticValues` 的添加/删除逻辑

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单文件单方法修改，2 行代码，无新依赖
  - **Skills**: []
  - **Skills Evaluated but Omitted**: N/A

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks B, C, D, E, F)
  - **Blocks**: None
  - **Blocked By**: None

  **References**:
  - `Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs:230-244` — `ExecuteAddFromUnconfigured` 方法，添加移除逻辑的位置
  - `Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs:132-137` — `SelectedUnconfiguredField` 属性，需要设为 null
  - `Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs:352-357` — `UnconfiguredFieldInfo` 类型，确认 Remove 可以直接传入

  **Acceptance Criteria**:
  - [ ] `ExecuteAddFromUnconfigured` 末尾调用 `UnconfiguredFields.Remove(SelectedUnconfiguredField)`
  - [ ] 之后设置 `SelectedUnconfiguredField = null`
  - [ ] 编译通过：`dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

  **QA Scenarios (MANDATORY)**:

  ```
  Scenario: 添加未配置字段后该字段从下拉选项中消失
    Tool: Bash (dotnet build + grep)
    Preconditions: 代码修改完成
    Steps:
      1. grep -n "UnconfiguredFields.Remove" Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs
         → 确认移除逻辑存在
      2. grep -n "SelectedUnconfiguredField = null" Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs
         → 确认选中状态清空
      3. dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj 2>&1
         → 确认编译通过，零错误
    Expected Result: grep 找到两处修改，build 输出 "Build succeeded"
    Failure Indicators: grep 找不到对应代码行；build 有 error
    Evidence: .sisyphus/evidence/task-A-add-remove.png

  Scenario: 重复添加同一字段被阻止
    Tool: Bash (grep)
    Preconditions: 代码修改完成
    Steps:
      1. grep -A5 "已在静态值列表中" Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs
         → 确认重复检查逻辑仍存在（lines 234-240 未被删除）
    Expected Result: grep 找到重复检查代码（`_dialogService.ShowError` + "已在静态值列表中"）
    Failure Indicators: 重复检查代码被意外删除
    Evidence: .sisyphus/evidence/task-A-dup-check.png
  ```

  **Evidence to Capture**:
  - [ ] `.sisyphus/evidence/task-A-add-remove.png` — grep + build 结果截图
  - [ ] `.sisyphus/evidence/task-A-dup-check.png` — 重复检查代码确认截图

  **Commit**: YES
  - Message: `fix(ContextAdapter): remove field from unconfigured dropdown after adding to static values`
  - Files: `Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs`
  - Pre-commit: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

- [x] B. **MainTabViewModel — 日期/性别启发式回退（Issue B）**

  **What to do**:
  - 在 `CreateFieldViewModel` 方法的 `switch(element)` 块之后（line 161 之后，`return field` 之前），添加启发式回退逻辑：
    ```csharp
    // 启发式回退：模板 JSON $type 可能为 "text"，通过 Label/Options 推断正确类型
    if (field.FieldType == FieldDataType.Text)
    {
        var label = (element.Label ?? string.Empty);
        if (label.Contains("日期") || label.Contains("时间"))
        {
            field.FieldType = FieldDataType.Date;
        }
        else if (label.Contains("性别") && element.Options is { Count: > 0 })
        {
            field.FieldType = FieldDataType.Dropdown;
            field.Options = element.Options.ToList();
        }
    }
    ```
  - 仅对当前被误分类为 Text 的字段生效，不影响正确类型映射的字段

  **Must NOT do**:
  - 不修改 `switch(element)` 内部的 case 分支（正常类型映射路径不变）
  - 不修改 `FieldDataType` 枚举定义
  - 不修改模板 JSON 序列化逻辑（ReportEditor 不在本仓库）

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单方法添加 12 行启发式逻辑，纯 C# 无外部依赖
  - **Skills**: []
  - **Skills Evaluated but Omitted**: N/A

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks A, C, D, E, F)
  - **Blocks**: None
  - **Blocked By**: None

  **References**:
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:112-161` — `CreateFieldViewModel` 方法，在 switch 后添加启发式逻辑
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:148-151` — `ExternalTextElement` 的 case 分支，当前会设 FieldType=Text
  - `Generators/ReportDataMaker/ViewModels/Tabs/DataEntryTabViewModel.cs:85` — `FieldDataType` 枚举，确认 Date 和 Dropdown 值存在
  - `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml:78-88` — DateFieldTemplate，DatePicker 绑定到 `DateValue`
  - `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml:44-56` — DropdownFieldTemplate，ComboBox 绑定到 `Options`

  **Acceptance Criteria**:
  - [ ] 标签含"日期"的 Text 字段 → FieldDataType.Date
  - [ ] 标签含"性别"且有 Options 的 Text 字段 → FieldDataType.Dropdown
  - [ ] 普通 Text 字段（无关键词）不受影响，仍为 FieldDataType.Text
  - [ ] 编译通过：`dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

  **QA Scenarios (MANDATORY)**:

  ```
  Scenario: 标签含"报告日期"的 ExternalTextElement 被识别为 Date
    Tool: Bash (grep + dotnet build)
    Preconditions: 代码修改完成
    Steps:
      1. grep -B2 -A8 "label.Contains.*日期" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认启发式检测代码存在
      2. grep -n "FieldDataType.Date" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认 Date 赋值在启发式块内
      3. dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj 2>&1
    Expected Result: grep 找到启发式代码，build "Build succeeded"
    Failure Indicators: 找不到启发式逻辑；build error
    Evidence: .sisyphus/evidence/task-B-date-heuristic.png

  Scenario: 标签含"性别"且有 Options 的 ExternalTextElement 被识别为 Dropdown
    Tool: Bash (grep)
    Preconditions: 代码修改完成
    Steps:
      1. grep -A5 "label.Contains.*性别" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认性别检测及 Dropdown 赋值
      2. grep -A1 "field.FieldType = FieldDataType.Dropdown" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认 Options 赋值 `field.Options = element.Options.ToList()`
    Expected Result: 两处 grep 均找到对应代码
    Failure Indicators: 启发式块缺少 Options 赋值
    Evidence: .sisyphus/evidence/task-B-gender-heuristic.png

  Scenario: 普通标签（如"姓名"）的 ExternalTextElement 不受影响
    Tool: Bash (grep)
    Preconditions: 代码修改完成
    Steps:
      1. 阅读 grep -A15 "// 启发式回退" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认启发式仅在 `field.FieldType == FieldDataType.Text` 时才进入
         → 确认不匹配关键词时直接跳过，不修改类型
    Expected Result: 启发式逻辑有条件保护，不会误改非 Text 类型
    Failure Indicators: 启发式无条件覆盖所有类型
    Evidence: .sisyphus/evidence/task-B-guard.png
  ```

  **Evidence to Capture**:
  - [ ] `.sisyphus/evidence/task-B-date-heuristic.png` — 日期检测代码截图
  - [ ] `.sisyphus/evidence/task-B-gender-heuristic.png` — 性别检测代码截图
  - [ ] `.sisyphus/evidence/task-B-guard.png` — 条件保护确认

  **Commit**: YES
  - Message: `feat(MainTab): add heuristic fallback for date/gender field type detection from label`
  - Files: `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs`
  - Pre-commit: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

- [x] C. **ContextAdapterService — 未配置字段隔离（Issue C）**

  **What to do**:
  - 在 `FillContext` 方法中，修改 line 33 的 `data[element.DataPath] = ResolveBuiltIn(element.DataPath);`
  - 替换为：
    ```csharp
    var builtIn = ResolveBuiltIn(element.DataPath);
    if (builtIn is string s && string.IsNullOrEmpty(s))
        continue;  // 跳过未配置字段，不将其当成"已匹配"
    data[element.DataPath] = builtIn;
    ```
  - 逻辑：`ResolveBuiltIn` 对未知 dataPath 返回 `string.Empty`。之前这导致未配置字段被填入空串并传递给 `ApplyContextData`，覆盖用户手动录入的值。现在跳过这些字段，使其不出现于 AdapterResult.Data 中

  **Must NOT do**:
  - 不修改 `ResolveBuiltIn` 方法（保持内置规则映射不变）
  - 不修改 `AdapterResult` 类型定义（Contracts 项目不可改）
  - 不修改 `ApplyContextData` 方法（它已有 `data.TryGetValue` 检查，未配置字段不出现则自动不被覆盖）

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单方法 5 行逻辑改动，纯 C# 无外部依赖
  - **Skills**: []
  - **Skills Evaluated but Omitted**: N/A

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks A, B, D, E, F)
  - **Blocks**: None
  - **Blocked By**: None

  **References**:
  - `Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs:33` — 当前 `data[element.DataPath] = ResolveBuiltIn(element.DataPath);` 需修改
  - `Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs:54-66` — `ResolveBuiltIn` 方法，对未知 dataPath 返回 `string.Empty`
  - `Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs:9-37` — `FillContext` 完整方法上下文
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:197-205` — `ApplyContextData` 已有 `data.TryGetValue` 保护，确认兼容

  **Acceptance Criteria**:
  - [ ] 未配置字段的 `dataPath` 不出现在 `AdapterResult.Data` 字典中
  - [ ] 已配置字段（静态值/动态规则/内置规则）仍正常出现在结果中
  - [ ] `ResolveBuiltIn` 匹配到的内置字段（如 `Context.DateTime.Now`）仍正常返回
  - [ ] 编译通过：`dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

  **QA Scenarios (MANDATORY)**:

  ```
  Scenario: 未配置字段被 FillContext 跳过
    Tool: Bash (grep + dotnet build)
    Preconditions: 代码修改完成
    Steps:
      1. grep -B2 -A4 "is string s && string.IsNullOrEmpty" Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs
         → 确认空串检查 + continue 逻辑存在
      2. grep "data\[element.DataPath\] = builtIn" Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs
         → 确认非空时才赋值（在 continue 之后，只对有效值赋值）
      3. dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj 2>&1
    Expected Result: 修改逻辑正确，build "Build succeeded"
    Failure Indicators: continue 逻辑缺失；空串仍被赋值
    Evidence: .sisyphus/evidence/task-C-skip-unconfigured.png

  Scenario: 内置字段仍正常返回
    Tool: Bash (grep)
    Preconditions: 代码修改完成
    Steps:
      1. grep -A10 "object ResolveBuiltIn" Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs
         → 确认 ResolveBuiltIn 方法未被修改，所有 case 分支保留
      2. 确认 "Context.DateTime.Now" / "Context.DateTime.Date" 等分支仍存在
    Expected Result: ResolveBuiltIn 方法完整保留，不受影响
    Failure Indicators: ResolveBuiltIn 方法被意外修改
    Evidence: .sisyphus/evidence/task-C-resolve-unchanged.png
  ```

  **Evidence to Capture**:
  - [ ] `.sisyphus/evidence/task-C-skip-unconfigured.png` — 跳过逻辑截图
  - [ ] `.sisyphus/evidence/task-C-resolve-unchanged.png` — ResolveBuiltIn 未变确认

  **Commit**: YES
  - Message: `fix(ContextAdapter): skip unconfigured fields instead of resolving to empty string`
  - Files: `Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs`
  - Pre-commit: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

- [x] D. **MainTabViewModel — 面板拆分逻辑优化（Issue D）**

  **What to do**:
  - 在 `LoadFields` 方法（lines 95-103），修改适配器/手动录入的分类逻辑：
    - 现有逻辑：`if (element.Group == Context || DataAdapter || !string.IsNullOrEmpty(DefaultValue))` → 适配器区
    - 新逻辑：**仅当** `element.Group == Context || element.Group == DataAdapter` 才放入适配器区
    - 移除 `|| !string.IsNullOrEmpty(element.DefaultValue)` 条件
    - 将 `adapterSection` 的 Title 由 "适配器数据" 改为 "适配器控制字段"
    - 将 `manualSection` 的 Title 由 "手动录入" 改为 "可录入字段"
  - 修改后的代码（lines 95-103）：
    ```csharp
    if (element.Group == ElementGroup.Context || element.Group == ElementGroup.DataAdapter)
    {
        adapterSection.Fields.Add(field);
    }
    else
    {
        manualSection.Fields.Add(field);
    }
    ```

  **Must NOT do**:
  - 不修改三个 Section 的创建顺序和添加逻辑（lines 107-109 保持不变）
  - 不修改 Fixed 分组逻辑（lines 69-88 保持不变）
  - 不修改 `FieldViewModel` 或 `SectionViewModel` 类型

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单方法条件简化 + 标题字符串修改，纯逻辑无 UI 变更
  - **Skills**: []
  - **Skills Evaluated but Omitted**: N/A

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks A, B, C, E, F)
  - **Blocks**: None
  - **Blocked By**: None

  **References**:
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:95-103` — 分类逻辑，需修改条件并更新标题
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:59-61` — Section 创建，标题需修改
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:107-109` — Section 添加逻辑，确认不变

  **Acceptance Criteria**:
  - [ ] `adapterSection` 仅包含 `ElementGroup.Context` 或 `ElementGroup.DataAdapter` 的字段
  - [ ] 仅有 `DefaultValue` 但不在上述 Group 中的字段归入 `manualSection`
  - [ ] Section 标题分别为 "适配器控制字段" 和 "可录入字段"
  - [ ] 编译通过：`dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

  **QA Scenarios (MANDATORY)**:

  ```
  Scenario: 适配器区仅包含 Context/DataAdapter 组字段
    Tool: Bash (grep + dotnet build)
    Preconditions: 代码修改完成
    Steps:
      1. grep -A3 "element.Group == ElementGroup.Context" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认条件仅检查 Group 类型
      2. grep -n "DefaultValue" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认在适配器区分类逻辑中不再出现 DefaultValue 条件
      3. dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj 2>&1
    Expected Result: 分类逻辑仅基于 Group，build "Build succeeded"
    Failure Indicators: DefaultValue 条件仍存在；Group 检查被移除
    Evidence: .sisyphus/evidence/task-D-section-logic.png

  Scenario: 标题已更新为中文语义正确的名称
    Tool: Bash (grep)
    Preconditions: 代码修改完成
    Steps:
      1. grep '"适配器控制字段"' Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认 adapterSection 新标题
      2. grep '"可录入字段"' Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs
         → 确认 manualSection 新标题
    Expected Result: 两处新标题均存在
    Failure Indicators: 仍使用 "适配器数据" / "手动录入" 旧标题
    Evidence: .sisyphus/evidence/task-D-section-titles.png
  ```

  **Evidence to Capture**:
  - [ ] `.sisyphus/evidence/task-D-section-logic.png` — 分类逻辑截图
  - [ ] `.sisyphus/evidence/task-D-section-titles.png` — 标题修改截图

  **Commit**: YES
  - Message: `refactor(MainTab): tighten adapter section criteria to Group only, rename sections`
  - Files: `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs`
  - Pre-commit: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

- [x] E. **MainTab.xaml/.cs — 鼠标滚轮传播修复（Issue E）**

  **What to do**:
  - 在 `MainTab.xaml` 的 ScrollViewer（line 157）上添加 `PreviewMouseWheel` 事件：
    ```xml
    <ScrollViewer Grid.Column="0" VerticalScrollBarVisibility="Auto" Padding="0"
                  PreviewMouseWheel="ScrollViewer_PreviewMouseWheel">
    ```
  - 在 `MainTab.xaml.cs` 添加事件处理器：
    ```csharp
    using System.Windows.Input;  // 添加到文件顶部的 using 块

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }
    ```
  - 原理：`PreviewMouseWheel` 是隧道路由事件，在外层 ScrollViewer 先捕获，手动调整偏移后标记 `e.Handled = true` 阻止内层控件（如 DataGrid）再处理

  **Must NOT do**:
  - 不修改内层控件（DataGrid、TextBox 等）的事件处理
  - 不添加全局鼠标钩子或第三方库
  - 不修改 `MainWindow.xaml`

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: XAML 加一行属性 + code-behind 加 8 行方法，纯 WPF 标准模式
  - **Skills**: []
  - **Skills Evaluated but Omitted**: N/A

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks A, B, C, D, F)
  - **Blocks**: None
  - **Blocked By**: None

  **References**:
  - `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml:157-158` — ScrollViewer 元素，添加 PreviewMouseWheel 属性
  - `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml.cs:1-10` — using 块和类定义，添加 `using System.Windows.Input` + 事件处理器
  - `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml:98-108` — DataGrid（内层可滚动控件），滚轮在此处被拦截

  **Acceptance Criteria**:
  - [ ] ScrollViewer 有 `PreviewMouseWheel="ScrollViewer_PreviewMouseWheel"` 属性
  - [ ] MainTab.xaml.cs 包含 `ScrollViewer_PreviewMouseWheel` 方法
  - [ ] 方法内调用了 `scrollViewer.ScrollToVerticalOffset` 和 `e.Handled = true`
  - [ ] 编译通过：`dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

  **QA Scenarios (MANDATORY)**:

  ```
  Scenario: XAML 配置了 PreviewMouseWheel 事件
    Tool: Bash (grep + dotnet build)
    Preconditions: 代码修改完成
    Steps:
      1. grep -n "PreviewMouseWheel=\"ScrollViewer_PreviewMouseWheel\"" Generators/ReportDataMaker/Views/Tabs/MainTab.xaml
         → 确认 PreviewMouseWheel 属性存在于 ScrollViewer 上
      2. grep -n "ScrollViewer_PreviewMouseWheel" Generators/ReportDataMaker/Views/Tabs/MainTab.xaml.cs
         → 确认事件处理器存在
      3. dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj 2>&1
    Expected Result: XAML 属性 + code-behind 方法均存在，build "Build succeeded"
    Failure Indicators: XAML 或 code-behind 缺少对应代码
    Evidence: .sisyphus/evidence/task-E-scroll-event.png

  Scenario: 事件处理器正确实现滚轮传播
    Tool: Bash (grep)
    Preconditions: 代码修改完成
    Steps:
      1. grep -A5 "ScrollViewer_PreviewMouseWheel" Generators/ReportDataMaker/Views/Tabs/MainTab.xaml.cs
         → 确认方法内包含：类型检查 `is not ScrollViewer`、`ScrollToVerticalOffset` 调用、`e.Handled = true`
      2. grep "using System.Windows.Input" Generators/ReportDataMaker/Views/Tabs/MainTab.xaml.cs
         → 确认 MouseWheelEventArgs 的 using 已添加
    Expected Result: 实现完整，包含偏移计算和事件标记
    Failure Indicators: 缺少 `e.Handled = true`；缺少 using；偏移量计算错误（加号而非减号）
    Evidence: .sisyphus/evidence/task-E-scroll-impl.png
  ```

  **Evidence to Capture**:
  - [ ] `.sisyphus/evidence/task-E-scroll-event.png` — XAML + code-behind 截图
  - [ ] `.sisyphus/evidence/task-E-scroll-impl.png` — 实现细节确认

  **Commit**: YES
  - Message: `fix(MainTab): propagate mouse wheel events from nested controls to parent ScrollViewer`
  - Files: `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml`, `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml.cs`
  - Pre-commit: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

- [x] F. **docs/ — 契约类型标记说明文档（Issue B 文档部分）**

  **What to do**:
  - 创建 `docs/element-type-contract.md`，内容包含：
    1. **模板 JSON 序列化契约**：说明 `$type` 字段的合法值（`"text"`, `"date"`, `"dropdown"`, `"number"`, `"checkbox"`, `"radio"`, `"table"`, `"line"`, `"divider"`）
    2. **$type → CLR 类型映射表**：列出每种 `$type` 对应的 `External*Element` 类型
    3. **控件渲染规则**：说明 `FieldDataType` 如何映射到 WPF DataTemplate（Text→TextBox, Date→DatePicker, Dropdown→ComboBox）
    4. **已知问题与回退**：记录模板端 `$type` 标记错误时 ReportDataMaker 的启发式回退逻辑（标签含"日期"→Date，含"性别"+有Options→Dropdown）
    5. **最佳实践**：建议 ReportEditor 在序列化时根据元素属性正确设置 `$type`

  **Must NOT do**:
  - 不在源码中添加文档注释（仅独立文档文件）
  - 不修改 ReportEditor 或 Contracts 项目的代码

  **Recommended Agent Profile**:
  - **Category**: `writing`
    - Reason: 纯文档写作，无代码变更
  - **Skills**: []
  - **Skills Evaluated but Omitted**: N/A

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks A, B, C, D, E)
  - **Blocks**: None
  - **Blocked By**: None

  **References**:
  - `Contracts/Converters/ElementJsonConverter.cs` — JSON `$type` → CLR 类型映射，提取合法值列表
  - `Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs:112-161` — `CreateFieldViewModel` 的类型分支，提取 FieldDataType 映射
  - `Generators/ReportDataMaker/Views/Tabs/MainTab.xaml:13-120` — DataTemplate 定义，提取控件类型对应关系
  - `Generators/ReportDataMaker/Infrastructure/FieldDataTemplateSelector.cs` — FieldDataType → DataTemplate 选择器

  **Acceptance Criteria**:
  - [ ] `docs/element-type-contract.md` 文件存在
  - [ ] 包含 `$type` 合法值列表及 CLR 类型映射
  - [ ] 包含 FieldDataType → DataTemplate 映射
  - [ ] 包含启发式回退说明

  **QA Scenarios (MANDATORY)**:

  ```
  Scenario: 文档包含 $type 映射表
    Tool: Bash (grep)
    Preconditions: 文档已创建
    Steps:
      1. grep -c "ExternalDateElement\|ExternalDropdownElement\|ExternalTextElement\|ExternalNumberElement\|ExternalCheckboxElement\|ExternalTableElement" docs/element-type-contract.md
         → 确认至少 6 种 CLR 类型被提及
      2. grep -c "\$type" docs/element-type-contract.md
         → 确认 JSON 字段 $type 被说明
    Expected Result: 行数 > 0（所有关键类型被覆盖）
    Failure Indicators: 关键类型缺失；文档为空
    Evidence: .sisyphus/evidence/task-F-contract-doc.png

  Scenario: 文档包含启发式回退说明
    Tool: Bash (grep)
    Preconditions: 文档已创建
    Steps:
      1. grep -c "启发式\|heuristic\|回退\|fallback" docs/element-type-contract.md
         → 确认启发式回退被文档化
    Expected Result: 行数 >= 1
    Failure Indicators: 启发式回退未提及
    Evidence: .sisyphus/evidence/task-F-heuristic-doc.png
  ```

  **Evidence to Capture**:
  - [ ] `.sisyphus/evidence/task-F-contract-doc.png` — 类型映射表截图
  - [ ] `.sisyphus/evidence/task-F-heuristic-doc.png` — 启发式说明截图

  **Commit**: YES
  - Message: `docs: add element type contract documentation for template JSON serialization`
  - Files: `docs/element-type-contract.md`
  - Pre-commit: `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`

---

## Final Verification Wave (MANDATORY — after ALL implementation tasks)

> 4 review agents run in PARALLEL. ALL must APPROVE. Present consolidated results to user and get explicit "okay" before completing.
>
> **Do NOT auto-proceed after verification. Wait for user's explicit approval before marking work complete.**
> **Never mark F1-F4 as checked before getting user's okay.** Rejection or user feedback -> fix -> re-run -> present again -> wait for okay.

- [x] F1. **Plan Compliance Audit** — `oracle`
  Read the plan end-to-end. For each "Must Have": verify implementation exists (grep for key patterns, read modified files). For each "Must NOT Have": search codebase for forbidden patterns — reject with file:line if found. Check evidence files exist in `.sisyphus/evidence/`. Compare deliverables against plan.
  Output: `Must Have [6/6] | Must NOT Have [6/6] | Tasks [6/6] | VERDICT: APPROVE/REJECT`
  - Must Have checklist: 下拉去重代码, 日期启发式代码, 性别启发式代码, FillContext skip 逻辑, 面板拆分逻辑, PreviewMouseWheel 处理器
  - Must NOT Have checklist: Contracts 未被修改, ReportEditor 未被修改, 无新外部依赖, API 契约未变, IValueConverter 接口未破坏

- [x] F2. **Code Quality Review** — `unspecified-high`
  Run `dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj`. Review all changed files for: unused usings, empty catch blocks, commented-out code, magic strings without explanation. Check AI slop: excessive comments, over-abstraction, generic variable names.
  Output: `Build [PASS/FAIL] | Files [N clean/N issues] | VERDICT`
  - Changed files: `ContextAdapterTabViewModel.cs`, `MainTabViewModel.cs`, `ContextAdapterService.cs`, `MainTab.xaml`, `MainTab.xaml.cs`, `docs/element-type-contract.md`

- [x] F3. **Real Manual QA** — `unspecified-high`
  Start from clean state. Execute EVERY QA scenario from EVERY task — follow exact steps, capture evidence. Test cross-task integration:
  - Does Task C (skip unconfigured) work together with Task A (dropdown remove)? → StaticValues items shouldn't appear as "matched" if not configured
  - Does Task B (heuristic) work with Task D (panel split)? → Date/Dropdown fields should appear in correct section
  - Does Task E (scroll) interact with DataGrid properly? → DataGrid cell editing should still work
  Save to `.sisyphus/evidence/final-qa/`.
  Output: `Scenarios [12/12 pass] | Integration [3/3] | VERDICT`

- [x] F4. **Scope Fidelity Check** — `deep`
  For each task: read "What to do", read actual diff (git log/diff). Verify 1:1 — everything in spec was built (no missing), nothing beyond spec was built (no creep). Check "Must NOT do" compliance. Detect cross-task contamination: Task N touching Task M's files. Flag unaccounted changes.
  Output: `Tasks [6/6 compliant] | Contamination [CLEAN/N issues] | Unaccounted [CLEAN/N files] | VERDICT`
  - Expected modified files per task:
    - A: `ContextAdapterTabViewModel.cs`
    - B: `MainTabViewModel.cs`
    - C: `ContextAdapterService.cs`
    - D: `MainTabViewModel.cs` (same file as B, different method — check for contamination)
    - E: `MainTab.xaml`, `MainTab.xaml.cs`
    - F: `docs/element-type-contract.md`

---

## Commit Strategy

| Task | Message | Files |
|------|---------|-------|
| A | `fix(ContextAdapter): remove field from unconfigured dropdown after adding to static values` | `ContextAdapterTabViewModel.cs` |
| B | `feat(MainTab): add heuristic fallback for date/gender field type detection from label` | `MainTabViewModel.cs` |
| C | `fix(ContextAdapter): skip unconfigured fields instead of resolving to empty string` | `ContextAdapterService.cs` |
| D | `refactor(MainTab): tighten adapter section criteria to Group only, rename sections` | `MainTabViewModel.cs` |
| E | `fix(MainTab): propagate mouse wheel events from nested controls to parent ScrollViewer` | `MainTab.xaml`, `MainTab.xaml.cs` |
| F | `docs: add element type contract documentation for template JSON serialization` | `docs/element-type-contract.md` |

> **Note**: Tasks B and D both modify `MainTabViewModel.cs` but in different methods (`CreateFieldViewModel` vs `LoadFields`). They can be committed independently but execute in parallel. If both modify the same file simultaneously, the executor should merge or commit sequentially.

---

## Success Criteria

### Verification Commands
```bash
# 编译验证
dotnet build Generators/ReportDataMaker/ReportDataMaker.csproj

# Task A: 确认移除逻辑
grep -n "UnconfiguredFields.Remove" Generators/ReportDataMaker/ViewModels/Tabs/ContextAdapterTabViewModel.cs

# Task B: 确认启发式检测
grep -n "label.Contains" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs

# Task C: 确认跳过逻辑
grep -n "is string s && string.IsNullOrEmpty" Generators/ReportDataMaker/Services/ContextAdapter/ContextAdapterService.cs

# Task D: 确认 Group-only 条件
grep -n "ElementGroup.Context \|\| element.Group == ElementGroup.DataAdapter" Generators/ReportDataMaker/ViewModels/Tabs/MainTabViewModel.cs

# Task E: 确认滚轮处理
grep -n "ScrollViewer_PreviewMouseWheel" Generators/ReportDataMaker/Views/Tabs/MainTab.xaml.cs

# Task F: 确认文档存在
ls -la docs/element-type-contract.md
```

### Final Checklist
- [x] All "Must Have" present: 下拉去重, 日期启发式, 性别启发式, 未配置跳过, 面板拆分, 滚轮修复
- [x] All "Must NOT Have" absent: Contracts 未改, ReportEditor 未改, 无新依赖, API 未变
- [x] All 6 tasks committed with evidence in `.sisyphus/evidence/`
- [x] `dotnet build` 通过，零 error
- [x] Final Verification Wave F1-F4 all APPROVE
- [x] User gives explicit "okay" after reviewing F1-F4 results