# Tasks

- [x] Task 1: 提取共享常量并修复数字精度
  - [x] 1.1: 创建 `frontend/src/utils/constants.js`，导出 MM_TO_PX、PX_TO_MM、CANVAS_PADDING 和 `round2(num)` 工具函数
  - [x] 1.2: 更新 CanvasEngine.js — 导入共享常量，pxToMm/mmToPx 返回值使用 round2 截断，事件回调中坐标/尺寸使用 round2
  - [x] 1.3: 更新 GridManager.js — 导入共享常量
  - [x] 1.4: 更新所有 7 个 ElementRenderer — 导入共享常量，toModel() 返回值使用 round2
  - [x] 1.5: 更新 CanvasArea.vue — 导入共享常量，拖放坐标使用 round2
  - [x] 1.6: 更新 PropertyPanel.vue — el-input-number 添加 :precision="2"

- [x] Task 2: 实现前端PDF在线预览
  - [x] 2.1: 创建 `frontend/src/utils/pdfGenerator.js`，实现从fabric canvas导出图片和打印PDF
  - [x] 2.2: 重写 PreviewModal.vue — 图片预览模式 + 打印/PDF模式（浏览器打印对话框保存PDF）

- [x] Task 3: 实现模板JSON导出
  - [x] 3.1: 在 template store 中添加 exportTemplateJson() 方法
  - [x] 3.2: 在 Toolbar.vue 中添加"导出JSON"按钮
  - [x] 3.3: 在 EditorView.vue 中连接导出逻辑

# Task Dependencies
- Task 2 依赖 Task 1（PDF生成需要精确的坐标数据）
- Task 3 独立，可与 Task 1 并行
