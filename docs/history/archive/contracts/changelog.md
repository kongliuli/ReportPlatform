# 契约版本变更记录

## v2.3.0 (2026-05-09)

- 在 ElementBase 中统一添加公共字段：
  - `isVisible` - 元素是否可见（默认 true）
  - `backgroundColor` - 背景颜色
  - `borderColor` - 边框颜色
  - `borderWidth` - 边框宽度
  - `borderStyle` - 边框样式（默认 solid）
  - `cornerRadius` - 圆角半径
  - `opacity` - 透明度（默认 1.0）
  - `foregroundColor` - 前景色/文字颜色（默认 #000000）
  - `fontFamily` - 字体族
  - `fontSize` - 字体大小（默认 12）
  - `fontWeight` - 字体粗细（默认 normal）
  - `fontStyle` - 字体样式（默认 normal）
  - `textAlignment` - 文本对齐方式（默认 left）
  - `label` - 标签文本
  - `dataPath` - 数据绑定路径
  - `formatString` - 格式化字符串
- 移除 ExternalElementBase 中重复的 Label 和 DataPath 字段

## v2.2.0 (2026-05-07)

- 定义 23 种元素类型
- 双 `$type` 体系（WPF 原生格式 + Web 短格式）
- 数据绑定规范
- 序列化规则
