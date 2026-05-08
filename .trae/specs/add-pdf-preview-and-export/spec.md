# PDF在线预览、模板JSON导出与数字精度修复 Spec

## Why
当前PDF预览功能依赖后端空实现的PdfSharpTemplateRendererStub，无法生成实际PDF内容；缺少模板JSON导出功能；拖拽/缩放元素后坐标和尺寸出现超长小数（如0.2645833333333333），影响显示和数据质量。

## What Changes
- 实现前端纯浏览器端PDF生成（使用jsPDF + html2canvas），替代后端空实现，实现真正的在线PDF预览
- 新增模板JSON导出功能，支持下载当前模板的完整JSON定义文件
- 修复所有数字精度问题，统一限制为最多2位小数
- 提取MM_TO_PX常量为共享模块，消除10处重复定义

## Impact
- Affected specs: PDF预览、模板导出、画布坐标系统
- Affected code:
  - `frontend/src/components/preview/PreviewModal.vue` — 重写为前端PDF生成
  - `frontend/src/engine/CanvasEngine.js` — pxToMm精度截断
  - `frontend/src/engine/elements/*.js` — 所有Renderer的toModel精度截断
  - `frontend/src/components/editor/CanvasArea.vue` — 拖放坐标精度截断
  - `frontend/src/components/editor/Toolbar.vue` — 新增导出按钮
  - `frontend/src/stores/template.js` — 新增exportTemplate方法
  - `frontend/src/utils/constants.js` — 新建共享常量文件

## ADDED Requirements

### Requirement: 前端PDF在线预览
系统应提供基于浏览器的PDF在线预览功能，无需依赖后端渲染服务。

#### Scenario: 用户点击预览按钮
- **WHEN** 用户在编辑器工具栏点击"预览"按钮
- **THEN** 弹出预览对话框，默认显示PDF在线预览（使用浏览器内置PDF查看器）
- **AND** PDF内容应准确反映当前画布上的元素布局和样式

#### Scenario: 用户下载PDF
- **WHEN** 用户在预览对话框中点击"下载PDF"按钮
- **THEN** 系统生成PDF文件并触发浏览器下载
- **AND** 文件名格式为`{模板名称}.pdf`

### Requirement: 模板JSON导出
系统应支持将当前模板导出为JSON文件。

#### Scenario: 用户导出模板JSON
- **WHEN** 用户在工具栏点击"导出JSON"按钮
- **THEN** 系统将当前模板的完整定义（含页面设置、所有元素、数据绑定）序列化为JSON
- **AND** 触发浏览器下载，文件名格式为`{模板名称}.json`
- **AND** JSON格式化输出，便于人工阅读和编辑

### Requirement: 数字精度限制
系统中所有显示和存储的数值属性最多保留2位小数。

#### Scenario: 拖拽元素后坐标精度
- **WHEN** 用户拖拽元素到新位置
- **THEN** 元素的x、y坐标值四舍五入到最多2位小数
- **AND** 属性面板中显示的坐标值不超过2位小数

#### Scenario: 缩放元素后尺寸精度
- **WHEN** 用户调整元素大小
- **THEN** 元素的width、height值四舍五入到最多2位小数

#### Scenario: 拖放新元素坐标精度
- **WHEN** 用户从工具箱拖放新元素到画布
- **THEN** 放置坐标四舍五入到最多2位小数

## MODIFIED Requirements

### Requirement: 共享常量
MM_TO_PX和CANVAS_PADDING常量应从共享模块导入，而非在各文件中重复定义。

#### Scenario: 常量引用
- **WHEN** 任何模块需要MM_TO_PX或CANVAS_PADDING常量
- **THEN** 从`@/utils/constants`统一导入
- **AND** 不允许在导入模块外重新定义这些常量
