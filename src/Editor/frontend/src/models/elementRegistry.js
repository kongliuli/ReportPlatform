/**
 * 杏林Web报告编辑器 — 模板结构契约 (Template Schema Contract)
 * ==============================================================
 * 契约版本: 2.2.0
 * 生效日期: 2026-05-07
 *
 * 【定位】
 * 本文件是「模板编辑器（生产者）」与「模板消费系统（消费者）」之间的数据交换契约。
 * - 编辑器输出的 JSON 严格遵循本契约定义
 * - 下游消费项目（模板渲染/打印/归档系统）依据本契约解析 JSON
 *
 * 【版本策略】
 * - 主版本号递增：破坏性变更（字段删除/重命名/类型变更）
 * - 次版本号递增：新增字段/新增元素类型（向后兼容）
 * - 修订号递增：文档修正/描述更新（无实际变更）
 *
 * 【契约保证】
 * - JSON 中所有数值最多保留 2 位小数（编辑器侧 round2 保证）
 * - 每个元素必须包含 $type 字段以标识其具体类型
 * - elements 数组中的元素顺序即图层顺序（zIndex 大的在前）
 * - 坐标原点 (0,0) = 页面上边距与左边距的交叉点
 * - 所有坐标单位为 mm（毫米）
 */

export const CONTRACT_VERSION = '2.2.0'

/**
 * ═══════════════════════════════════════════════════════════
 * 第1章：版本历史
 * ═══════════════════════════════════════════════════════════
 *
 * v2.2.0 (2026-05-07) — 破坏性变更
 *   统一 $type 为 template.element.*；label 强制中文；isVisible 必须显式 true；剥离 10 个 WPF 未用字段
 *   移除 dataBindings 数组，改为内联绑定（label + dataPath）
 *   废弃 LabelInputBox 类型
 *
 * v2.1.0 (2026-05-07)
 *   WPF 端 SerializationBinder 集成指南
 *
 * v2.0.0 (2026-05-07)
 *   新增 16 种 Web 扩展元素类型；引入双 $type 体系
 *
 * v1.0.0 (2026-04-27) — 初始契约
 *   支持的纸张: A4竖版/A4横版/A5竖版/A5横版
 *   支持的元素: 7种（见第3章元素注册表）
 *   序列化格式: JSON with $type discriminator
 *   精度策略: 坐标/尺寸 round2，最多保留2位小数
 *   坐标单位: mm
 *   像素转换: 1mm = 3.7795275591px (96 DPI)
 *   画布留白: 20px (CANVAS_PADDING)
 *   网格间距: 5mm
 *   纸张预设:
 *     A4竖版 210×297  Portrait
 *     A4横版 297×210  Landscape
 *     A5竖版 148×210  Portrait
 *     A5横版 210×148  Landscape
 *   页面背景色: #FFFFFF
 *   默认全局字号: 12
 *   默认边距: 15mm 四边
 */

/**
 * ═══════════════════════════════════════════════════════════
 * 第2章：模板顶层结构 (ReportTemplateDefinition)
 * ═══════════════════════════════════════════════════════════
 *
 * JSON Schema:
 * {
 *   "name"          : string,        // 模板名称，≤100字符
 *   "type"          : string,        // 模板分类：门诊/住院/检验/处方/影像/护理
 *   "version"       : integer,       // 模板版本号，从1开始递增
 *   "elements"      : Element[],     // 元素数组
 *
 *   // 页面设置
 *   "pageWidth"     : number,        // 默认 210 (A4宽度mm)
 *   "pageHeight"    : number,        // 默认 297 (A4高度mm)
 *   "orientation"   : "Portrait"|"Landscape", // 默认 "Portrait"
 *   "marginLeft"    : number,        // 默认 15
 *   "marginRight"   : number,        // 默认 15
 *   "marginTop"     : number,        // 默认 15
 *   "marginBottom"  : number,        // 默认 15
 *   "backgroundColor": string,       // 默认 "#FFFFFF"
 *
 *   // 全局样式
 *   "globalFontSize": number,        // 默认 12
 * }
 *
 * v2.2.0 移除：dataBindings 数组（改为内联绑定），enableGlobalFontSize
 *
 * 可打印区域计算:
 *   printableArea = {
 *     x: marginLeft,
 *     y: marginTop,
 *     width: pageWidth - marginLeft - marginRight,
 *     height: pageHeight - marginTop - marginBottom
 *   }
 */

/**
 * ═══════════════════════════════════════════════════════════
 * 第3章：元素注册表
 * ═══════════════════════════════════════════════════════════
 *
 * 每个元素继承 ElementBase 公共字段，再加上自身的特有字段。
 * 消费者通过 $type 字段区分元素具体类型。
 *
 * v2.2.0 起统一使用 template.element.xxx 格式，覆盖全部 23 种元素类型。
 * 已废弃：Xinglin.Core.Elements.{Type}Element, Xinglin.Core 旧格式。
 */

export const ELEMENT_BASE = {
  version: '2.2.0',
  description: '所有23种元素的公共基类字段',
  fields: {
    $type:           { type: 'string',   default: '',              nullable: false, description: '类型标识，格式 template.element.xxx' },
    id:              { type: 'string',   default: 'el_<ts>_<seq>', nullable: false, description: '元素唯一标识符' },
    x:               { type: 'number',   default: 0,               nullable: false, description: '左上角X坐标(mm)' },
    y:               { type: 'number',   default: 0,               nullable: false, description: '左上角Y坐标(mm)' },
    width:           { type: 'number',   default: 100,             nullable: false, description: '元素宽度(mm)' },
    height:          { type: 'number',   default: 30,              nullable: false, description: '元素高度(mm)' },
    isVisible:       { type: 'boolean',  default: true,            nullable: false, description: '是否可见（必须显式设为 true，C# bool 默认 false）' },
    zIndex:          { type: 'number',   default: 0,               nullable: false, description: '图层层级，值越大越靠前' },
    backgroundColor: { type: 'string',   default: 'transparent',   nullable: false, description: '背景颜色' },
    borderColor:     { type: 'string',   default: '#000000',       nullable: false, description: '边框颜色' },
    borderWidth:     { type: 'number',   default: 0,               nullable: false, description: '边框宽度' },
    borderStyle:     { type: 'string',   default: 'solid',         nullable: false, description: '边框样式：solid/dashed/dotted' },
    cornerRadius:    { type: 'number',   default: 0,               nullable: false, description: '圆角半径' },
    opacity:         { type: 'number',   default: 1,               nullable: false, description: '透明度(0~1)' },
    fontFamily:      { type: 'string',   default: 'SimSun',        nullable: false, description: '字体名称' },
    fontSize:        { type: 'number',   default: 12,              nullable: false, description: '字号大小' },
    fontWeight:      { type: 'string',   default: 'normal',        nullable: false, description: '字体粗细：normal/bold' },
    fontStyle:       { type: 'string',   default: 'normal',        nullable: false, description: '字体样式：normal/italic' },
    foregroundColor: { type: 'string',   default: '#000000',       nullable: false, description: '前景色/文字颜色' },
    textAlignment:   { type: 'string',   default: 'left',          nullable: false, description: '文本对齐：left/center/right' },
    label:           { type: 'string',   default: '',              nullable: false, description: '中文标签（可编辑元素必填，WPF 输入面板显示用）' },
    dataPath:        { type: 'string',   default: '',              nullable: false, description: '数据绑定路径（驼峰格式）' },
    formatString:    { type: 'string',   default: '',              nullable: false, description: '格式化字符串' }
  }
}

export const ELEMENT_REGISTRY = {

  // ═══════════════ 基础元素 (basic) ═══════════════

  TextElement: {
    label: '文本',
    icon: 'Document',
    version: '2.2.0',
    since: '1.0.0',
    description: '用于显示和编辑文本内容。最常用的报告元素，可用于标题、正文、标签、数据绑定字段等。',
    $type: 'template.element.text',
    props: {
      text:           { type: 'string',  default: '',      description: '静态文本内容' },
      lineHeight:     { type: 'number',  default: 1.5,     description: '行高倍数' },
      letterSpacing:  { type: 'number',  default: 0,       description: '字间距（px）' },
      textDecoration: { type: 'string',  default: 'none',  description: '文字装饰：none/underline/overline/line-through' }
    }
  },

  ImageElement: {
    label: '图片',
    icon: 'Picture',
    version: '2.2.0',
    since: '1.0.0',
    description: '用于嵌入图片或占位图片区域。支持本地路径和Base64编码图片。',
    $type: 'template.element.image',
    props: {
      imagePath:           { type: 'string', default: '',        description: '图片文件路径' },
      imageData:           { type: 'string', default: '',        description: 'Base64编码的图片数据' },
      stretch:             { type: 'string', default: 'Uniform', description: '拉伸模式：None/Fill/Uniform/UniformToFill' },
      maintainAspectRatio: { type: 'boolean', default: true,     description: '保持宽高比' }
    }
  },

  LineElement: {
    label: '线条',
    icon: 'Minus',
    version: '2.2.0',
    since: '1.0.0',
    description: '绘制直线，用于分隔页面区域或装饰。支持实线/虚线/点线。',
    $type: 'template.element.line',
    props: {
      lineColor: { type: 'string', default: '#000000', description: '线条颜色' },
      lineWidth: { type: 'number', default: 1,         description: '线条宽度' },
      lineStyle: { type: 'string', default: 'solid',   description: '线条样式：solid/dashed/dotted' },
      startX:    { type: 'number', default: 0,         description: '起点X坐标(mm)' },
      startY:    { type: 'number', default: 0,         description: '起点Y坐标(mm)' },
      endX:      { type: 'number', default: 100,       description: '终点X坐标(mm)' },
      endY:      { type: 'number', default: 0,         description: '终点Y坐标(mm)' }
    }
  },

  ShapeElement: {
    label: '形状',
    icon: 'CircleCheck',
    version: '2.2.0',
    since: '2.0.0',
    description: '绘制几何形状，支持矩形、圆形、三角形、椭圆。',
    $type: 'template.element.shape',
    props: {
      shapeType:   { type: 'string', default: 'rectangle',  description: '形状类型：rectangle/circle/triangle/ellipse' },
      fillColor:   { type: 'string', default: 'transparent', description: '填充颜色' },
      strokeColor: { type: 'string', default: '#000000',     description: '描边颜色' },
      strokeWidth: { type: 'number', default: 1,             description: '描边宽度' },
      strokeStyle: { type: 'string', default: 'solid',       description: '描边样式：solid/dashed/dotted' }
    }
  },

  DividerElement: {
    label: '分隔线',
    icon: 'SemiSelect',
    version: '2.2.0',
    since: '2.0.0',
    description: '水平分隔线，用于分隔内容区域。',
    $type: 'template.element.divider',
    props: {
      dividerStyle: { type: 'string', default: 'solid',    description: '分隔线样式：solid/dashed/dotted' },
      thickness:    { type: 'number', default: 1,          description: '线条粗细' },
      color:        { type: 'string', default: '#CCCCCC',  description: '线条颜色' },
      marginTop:    { type: 'number', default: 5,          description: '上边距(mm)' },
      marginBottom: { type: 'number', default: 5,          description: '下边距(mm)' }
    }
  },

  // ═══════════════ 输入元素 (input) ═══════════════

  CheckboxElement: {
    label: '复选框',
    icon: 'Select',
    version: '2.2.0',
    since: '2.0.0',
    description: '复选框，用于多选场景。',
    $type: 'template.element.checkbox',
    props: {
      checked:          { type: 'boolean', default: false, description: '是否选中' },
      checkboxLabel:    { type: 'string',  default: '',    description: '复选框标签文本' },
      checkboxPosition: { type: 'string',  default: 'left', description: '复选框位置：left/right' }
    }
  },

  RadioElement: {
    label: '单选框',
    icon: 'CircleCheck',
    version: '2.2.0',
    since: '2.0.0',
    description: '单选框，用于单选场景。',
    $type: 'template.element.radio',
    props: {
      options:       { type: 'string[]', default: [],        description: '选项列表' },
      selectedValue: { type: 'string',   default: '',        description: '当前选中的值' },
      orientation:   { type: 'string',   default: 'vertical', description: '排列方向：vertical/horizontal' }
    }
  },

  DropdownElement: {
    label: '下拉框',
    icon: 'ArrowDown',
    version: '2.2.0',
    since: '1.0.0',
    description: '下拉选择框，提供预定义选项供用户选择。',
    $type: 'template.element.dropdown',
    props: {
      value:       { type: 'string',   default: '',       description: '当前选中的值' },
      options:     { type: 'string[]', default: [],       description: '下拉选项列表' },
      placeholder: { type: 'string',   default: '请选择',  description: '占位提示文字' },
      allowCustom: { type: 'boolean',  default: false,    description: '是否允许自定义输入' }
    }
  },

  NumberElement: {
    label: '数字',
    icon: 'Histogram',
    version: '2.2.0',
    since: '1.0.0',
    description: '数字输入控件，支持单位显示、小数位设置。',
    $type: 'template.element.number',
    props: {
      value:                 { type: 'number',  default: 0,     description: '当前数值' },
      format:                { type: 'string',  default: '',     description: '数字格式化字符串' },
      unit:                  { type: 'string',  default: '',     description: '单位后缀，如：岁、次/分、mmHg、℃' },
      decimalPlaces:         { type: 'number',  default: 2,     description: '小数位数' },
      showThousandsSeparator: { type: 'boolean', default: false, description: '是否显示千分位分隔符' }
    }
  },

  DateElement: {
    label: '日期',
    icon: 'Calendar',
    version: '2.2.0',
    since: '1.0.0',
    description: '日期选择器，支持格式化显示。',
    $type: 'template.element.date',
    props: {
      value:    { type: 'string',  default: '',           description: '当前日期值' },
      format:   { type: 'string',  default: 'yyyy-MM-dd', description: '日期显示格式' },
      showTime: { type: 'boolean', default: false,        description: '是否显示时间' }
    }
  },

  // ═══════════════ 数据元素 (data) ═══════════════

  TableElement: {
    label: '表格',
    icon: 'Grid',
    version: '2.2.0',
    since: '1.0.0',
    description: '结构化表格，支持表头和单元格数据。',
    $type: 'template.element.table',
    props: {
      rows:               { type: 'number',     default: 3,    description: '行数' },
      columns:            { type: 'number',     default: 4,    description: '列数' },
      cellData:           { type: 'string[][]', default: [],    description: '二维数组，cellData[row][col]为单元格内容' },
      cellPadding:        { type: 'number',     default: 2,    description: '单元格内边距' },
      tableBorder:        { type: 'number',     default: 1,    description: '表格边框线宽度' },
      hasHeader:          { type: 'boolean',    default: true, description: '是否有表头行' },
      headerStyle:        { type: 'object',     default: {},    description: '表头样式配置' },
      alternateRowColors: { type: 'boolean',    default: false, description: '是否交替行颜色' }
    }
  },

  BarcodeElement: {
    label: '条形码',
    icon: 'Tickets',
    version: '2.2.0',
    since: '2.0.0',
    description: '条形码，支持 CODE128、CODE39、EAN13、EAN8 格式。',
    $type: 'template.element.barcode',
    props: {
      value:         { type: 'string',  default: '',        description: '条码值' },
      barcodeFormat: { type: 'string',  default: 'CODE128', description: '条码格式：CODE128/CODE39/EAN13/EAN8' },
      barWidth:      { type: 'number',  default: 2,         description: '条码线宽' },
      showText:      { type: 'boolean', default: true,      description: '是否显示文本' }
    }
  },

  QrCodeElement: {
    label: '二维码',
    icon: 'Grid',
    version: '2.2.0',
    since: '2.0.0',
    description: '二维码，支持错误纠正级别设置。',
    $type: 'template.element.qrcode',
    props: {
      value:           { type: 'string', default: '',        description: '二维码内容' },
      size:            { type: 'number', default: 100,       description: '二维码尺寸' },
      errorLevel:      { type: 'string', default: 'M',       description: '错误纠正级别：L/M/Q/H' },
      foregroundColor: { type: 'string', default: '#000000', description: '前景色' },
      backgroundColor: { type: 'string', default: '#FFFFFF', description: '背景色' }
    }
  },

  ChartElement: {
    label: '图表',
    icon: 'TrendCharts',
    version: '2.2.0',
    since: '2.0.0',
    description: '图表元素，支持 line、bar、pie 等类型。',
    $type: 'template.element.chart',
    props: {
      chartType:  { type: 'string',   default: 'line',                                                 description: '图表类型：line/bar/pie/area/scatter' },
      dataSource: { type: 'string',   default: '',                                                     description: '数据源路径' },
      series:     { type: 'array',    default: [],                                                     description: '数据系列' },
      xAxis:      { type: 'object',   default: {},                                                     description: 'X轴配置' },
      yAxis:      { type: 'object',   default: {},                                                     description: 'Y轴配置' },
      legend:     { type: 'object',   default: { show: true, position: 'bottom' },                      description: '图例配置' },
      colors:     { type: 'string[]', default: ['#409EFF', '#67C23A', '#E6A23C', '#F56C6C'],          description: '颜色方案' }
    }
  },

  // ═══════════════ 布局元素 (layout) ═══════════════

  ContainerElement: {
    label: '容器',
    icon: 'Files',
    version: '2.2.0',
    since: '2.0.0',
    description: '容器元素，用于元素分组和布局。',
    $type: 'template.element.container',
    props: {
      children: { type: 'Element[]', default: [],         description: '子元素数组' },
      layout:   { type: 'string',    default: 'absolute', description: '布局方式：absolute/flow/stack' },
      padding:  { type: 'number',    default: 0,          description: '内边距(mm)' },
      gap:      { type: 'number',    default: 0,          description: '子元素间距(mm)' }
    }
  },

  RepeatElement: {
    label: '重复区域',
    icon: 'CopyDocument',
    version: '2.2.0',
    since: '2.0.0',
    description: '重复区域，用于数据列表的动态渲染。',
    $type: 'template.element.repeat',
    props: {
      dataSource: { type: 'string',  default: '',         description: '数据源路径' },
      template:   { type: 'object',  default: null,        description: '重复项模板' },
      maxItems:   { type: 'number',  default: 0,          description: '最大重复数量，0=不限制' },
      direction:  { type: 'string',  default: 'vertical', description: '排列方向：vertical/horizontal' },
      gap:        { type: 'number',  default: 10,         description: '项间距(mm)' }
    }
  },

  HeaderElement: {
    label: '页眉',
    icon: 'Top',
    version: '2.2.0',
    since: '2.0.0',
    description: '页眉元素，显示在每页顶部。',
    $type: 'template.element.header',
    props: {
      content:        { type: 'string',  default: '',   description: '页眉内容（支持 {{占位符}}）' },
      showOnAllPages: { type: 'boolean', default: true, description: '是否所有页都显示' }
    }
  },

  FooterElement: {
    label: '页脚',
    icon: 'Bottom',
    version: '2.2.0',
    since: '2.0.0',
    description: '页脚元素，显示在每页底部。',
    $type: 'template.element.footer',
    props: {
      content:         { type: 'string',  default: '',   description: '页脚内容（支持 {{占位符}}）' },
      showOnAllPages:  { type: 'boolean', default: true, description: '是否所有页都显示' },
      showPageNumber:  { type: 'boolean', default: false, description: '是否显示页码' }
    }
  },

  PageNumberElement: {
    label: '页码',
    icon: 'Collection',
    version: '2.2.0',
    since: '2.0.0',
    description: '页码元素，显示当前页/总页数。',
    $type: 'template.element.pagenumber',
    props: {
      format:    { type: 'string', default: '{page} / {total}', description: '页码格式' },
      startFrom: { type: 'number', default: 1,                  description: '起始页码' },
      position:  { type: 'string', default: 'center',           description: '位置：left/center/right' }
    }
  },

  // ═══════════════ 特殊元素 (special) ═══════════════

  SignatureElement: {
    label: '签名',
    icon: 'EditPen',
    version: '2.2.0',
    since: '2.0.0',
    description: '签名区域，用于手写签名或电子签章。',
    $type: 'template.element.signature',
    props: {
      strokeWidth:    { type: 'number', default: 2,           description: '签名笔触宽度' },
      strokeColor:    { type: 'string', default: '#000000',   description: '签名笔触颜色' },
      backgroundColor: { type: 'string', default: '#FFFFFF',  description: '签名区域背景色' },
      signatureData:  { type: 'string|null', default: null,   description: '签名数据（Base64 图像或坐标数组）' }
    }
  },

  WatermarkElement: {
    label: '水印',
    icon: 'Stamp',
    version: '2.2.0',
    since: '2.0.0',
    description: '水印元素，用于页面背景水印。',
    $type: 'template.element.watermark',
    props: {
      text:     { type: 'string',  default: '机密', description: '水印文本' },
      angle:    { type: 'number',  default: -45,    description: '旋转角度（度）' },
      fontSize: { type: 'number',  default: 48,     description: '水印字号' },
      opacity:  { type: 'number',  default: 0.1,    description: '透明度(0~1)' },
      repeat:   { type: 'boolean', default: true,   description: '是否平铺重复' }
    }
  },

  IconElement: {
    label: '图标',
    icon: 'Star',
    version: '2.2.0',
    since: '2.0.0',
    description: '图标元素，用于显示图标。',
    $type: 'template.element.icon',
    props: {
      iconName:  { type: 'string', default: '',             description: '图标名称' },
      iconSet:   { type: 'string', default: 'element-plus', description: '图标库名称' },
      iconSize:  { type: 'number', default: 24,             description: '图标尺寸' },
      iconColor: { type: 'string', default: '#000000',      description: '图标颜色' }
    }
  },

  HyperlinkElement: {
    label: '超链接',
    icon: 'Link',
    version: '2.2.0',
    since: '2.0.0',
    description: '超链接元素，用于添加可点击链接。',
    $type: 'template.element.hyperlink',
    props: {
      url:       { type: 'string',  default: '',        description: '链接地址' },
      text:      { type: 'string',  default: '点击查看', description: '显示文本' },
      target:    { type: 'string',  default: '_blank',  description: '打开方式：_blank/_self' },
      underline: { type: 'boolean', default: true,      description: '是否显示下划线' },
      linkColor: { type: 'string',  default: '#409EFF', description: '链接颜色' }
    }
  }
}

/**
 * ═══════════════════════════════════════════════════════════
 * 第4章：数据绑定契约
 * ═══════════════════════════════════════════════════════════
 *
 * v2.2.0：数据绑定改为内联方式（label + dataPath），不再使用独立的 dataBindings 数组。
 *
 * 可编辑元素通过 label + dataPath 内联绑定：
 * {
 *   "$type": "template.element.text",
 *   "label": "姓名",
 *   "dataPath": "PatientName",
 *   "isVisible": true
 * }
 *
 * 常用数据路径（dataPath）参考（驼峰格式）：
 *   患者信息: PatientName / Gender / Age / Birthday / PatientId
 *   就诊信息: Department / DoctorName / ChiefComplaint / Diagnosis
 *   住院信息: AdmissionNo / AdmissionDate / DischargeDate / BedNo
 *   检验信息: SampleType / ReportDate / Technician / Reviewer
 *   处方信息: PrescriptionDate / Pharmacist
 *   影像信息: BodyPart / Modality / Findings / Impression / ReportingDoctor
 *   护理信息: RecordDate / NurseName
 *
 * DataTransformRule（数据转换规则）:
 *   类型: Format(格式化) / Case(大小写) / Math(数学计算) / DateFormat(日期格式化) / Custom(自定义)
 *
 * DataValidationRule（数据校验规则）:
 *   类型: Required(必填) / Range(范围) / Regex(正则) / Length(长度) / Custom(自定义)
 */

/**
 * ═══════════════════════════════════════════════════════════
 * 第5章：序列化契约
 * ═══════════════════════════════════════════════════════════
 *
 * 【$type 字段映射表（v2.2.0 统一格式）】
 * 消费者通过 $type 值确定元素类型，选择对应的反序列化逻辑：
 *
 *   $type 值                              → 元素类型
 *   ──────────────────────────────────────────────
 *   template.element.text                 → TextElement
 *   template.element.image                → ImageElement
 *   template.element.line                 → LineElement
 *   template.element.shape                → ShapeElement
 *   template.element.divider              → DividerElement
 *   template.element.checkbox             → CheckboxElement
 *   template.element.radio                → RadioElement
 *   template.element.dropdown             → DropdownElement
 *   template.element.number               → NumberElement
 *   template.element.date                 → DateElement
 *   template.element.table                → TableElement
 *   template.element.barcode              → BarcodeElement
 *   template.element.qrcode               → QrCodeElement
 *   template.element.chart                → ChartElement
 *   template.element.container            → ContainerElement
 *   template.element.repeat               → RepeatElement
 *   template.element.header               → HeaderElement
 *   template.element.footer               → FooterElement
 *   template.element.pagenumber           → PageNumberElement
 *   template.element.signature            → SignatureElement
 *   template.element.watermark             → WatermarkElement
 *   template.element.icon                 → IconElement
 *   template.element.hyperlink            → HyperlinkElement
 *
 * v2.2.0 序列化时剥离的字段（已确认 WPF 渲染器不使用）：
 *   rotation, shadow, labelWidth, isDataBound,
 *   richText, isRichText, minValue, maxValue, minDate, maxDate
 *
 * 【序列化规则】
 * - 过滤 _ 前缀的属性（编辑器内部状态）
 * - 过滤 function 类型的属性
 * - 保留 $type 字段作为类型判别符
 * - 数值精度：坐标/尺寸最多保留到小数点后2位
 * - JSON 格式化：2空格缩进
 * - 剥离 WPF 未使用的 10 个字段
 *
 * 【反序列化规则（消费端参考实现）】
 * 1. JSON.parse 得到原始对象
 * 2. 遍历 elements 数组中每个元素：
 *    a. 读取 element.$type
 *    b. 查表确定元素子类
 *    c. 用子类构造函数或工厂函数创建实例
 *    d. 将 element 的所有字段赋值给实例
 */

/**
 * ═══════════════════════════════════════════════════════════
 * 第6章：渲染契约（消费端参考）
 * ═══════════════════════════════════════════════════════════
 *
 * 坐标转换公式：
 *   px = mm × 3.7795275591
 *   mm = round2(px ÷ 3.7795275591)
 *
 * 画布计算（编辑器侧）：
 *   画布宽度  = pageWidth × 3.7795 + 40   (40 = CANVAS_PADDING × 2)
 *   画布高度  = pageHeight × 3.7795 + 40
 *   元素在画布上的像素坐标: (x × 3.7795 + 20, y × 3.7795 + 20)
 *
 * 渲染器映射（编辑器实际渲染 → 消费端可参考）：
 *   TextElement      → fabric.IText             → 消费端渲染为文本
 *   ImageElement     → fabric.Group/FabricImage → 消费端渲染为图片或占位框
 *   LineElement      → fabric.Line              → 消费端根据端点画线
 *   ShapeElement     → fabric.Rect/Circle等     → 消费端渲染为形状
 *   DividerElement   → fabric.Line              → 消费端渲染为分隔线
 *   TableElement     → fabric.Group(Rect+Text)  → 消费端渲染为表格
 *   CheckboxElement  → fabric.Group(占位符)      → 消费端渲染为复选框
 *   RadioElement     → fabric.Group(占位符)      → 消费端渲染为单选框
 *   DateElement      → fabric.Group(占位符)      → 消费端渲染为日期选择器
 *   DropdownElement  → fabric.Group(占位符)      → 消费端渲染为下拉框
 *   NumberElement    → fabric.Group(占位符)      → 消费端渲染为数字输入框
 *   BarcodeElement   → fabric.Group(占位符)      → 消费端渲染为条码
 *   QrCodeElement    → fabric.Group(占位符)      → 消费端渲染为二维码
 *   ChartElement     → fabric.Group(占位符)      → 消费端渲染为图表
 *   ContainerElement → fabric.Group             → 消费端渲染为容器
 *   RepeatElement    → fabric.Group(占位符)      → 消费端渲染为重复区域
 *   HeaderElement    → fabric.Group             → 消费端渲染为页眉
 *   FooterElement    → fabric.Group             → 消费端渲染为页脚
 *   PageNumberElement → fabric.Text             → 消费端渲染为页码
 *   SignatureElement → fabric.Group             → 消费端渲染为签名区
 *   WatermarkElement → fabric.Text(倾斜)         → 消费端渲染为水印
 *   IconElement      → fabric.Group             → 消费端渲染为图标
 *   HyperlinkElement → fabric.Text(下划线)       → 消费端渲染为超链接
 *
 * 消费端渲染注意事项：
 * - 输入控件类元素(Checkbox/Radio/Date/Dropdown/Number)在编辑器中渲染为占位符
 *   消费端应根据各自平台UI库渲染为实际的交互控件
 * - ImageElement 如果 imageData 为空，渲染为占位框
 * - TableElement 的 hasHeader 决定第一行是否加背景色
 * - 所有坐标需从mm转换为目标渲染系统的单位
 */

/**
 * ═══════════════════════════════════════════════════════════
 * 第7章：示例模板 JSON 参考（v2.2.0 格式）
 * ═══════════════════════════════════════════════════════════
 *
 * 以下是一个完整的、符合 v2.2.0 契约的模板 JSON 示例：
 */
export const SAMPLE_TEMPLATE_JSON = {
  name: '门诊病历（示例）',
  type: '门诊',
  version: 1,
  pageWidth: 210,
  pageHeight: 297,
  orientation: 'Portrait',
  marginLeft: 15,
  marginRight: 15,
  marginTop: 15,
  marginBottom: 15,
  backgroundColor: '#FFFFFF',
  globalFontSize: 12,
  elements: [
    {
      $type: 'template.element.text',
      id: 'header_1',
      x: 15, y: 15, width: 180, height: 14,
      text: '杏林医院 门诊病历',
      fontSize: 18, fontWeight: 'bold', textAlignment: 'center',
      fontFamily: 'SimHei', foregroundColor: '#000000',
      isVisible: true
    },
    {
      $type: 'template.element.line',
      id: 'line_1',
      startX: 15, startY: 32, endX: 195, endY: 32,
      lineColor: '#000000', lineWidth: 1.5, lineStyle: 'solid',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'label_name',
      x: 15, y: 38, width: 25, height: 9,
      text: '姓名：', fontSize: 10, foregroundColor: '#333333',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'field_name',
      x: 40, y: 38, width: 40, height: 9,
      text: '', fontSize: 10,
      label: '姓名', dataPath: 'PatientName',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'label_gender',
      x: 85, y: 38, width: 20, height: 9,
      text: '性别：', fontSize: 10, foregroundColor: '#333333',
      isVisible: true
    },
    {
      $type: 'template.element.dropdown',
      id: 'field_gender',
      x: 105, y: 38, width: 25, height: 9,
      label: '性别', dataPath: 'Gender',
      options: ['男', '女'],
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'label_age',
      x: 140, y: 38, width: 20, height: 9,
      text: '年龄：', fontSize: 10, foregroundColor: '#333333',
      isVisible: true
    },
    {
      $type: 'template.element.number',
      id: 'field_age',
      x: 160, y: 38, width: 25, height: 9,
      label: '年龄', dataPath: 'Age', unit: '岁',
      isVisible: true
    },
    {
      $type: 'template.element.line',
      id: 'line_divider',
      startX: 15, startY: 50, endX: 195, endY: 50,
      lineColor: '#999999', lineWidth: 0.5, lineStyle: 'dashed',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'section_complaint',
      x: 15, y: 56, width: 40, height: 9,
      text: '主诉：', fontSize: 10, fontWeight: 'bold',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'field_complaint',
      x: 15, y: 68, width: 180, height: 25,
      text: '', fontSize: 10,
      label: '主诉', dataPath: 'ChiefComplaint',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'section_diagnosis',
      x: 15, y: 100, width: 40, height: 9,
      text: '诊断：', fontSize: 10, fontWeight: 'bold',
      isVisible: true
    },
    {
      $type: 'template.element.text',
      id: 'field_diagnosis',
      x: 15, y: 112, width: 180, height: 25,
      text: '', fontSize: 10,
      label: '诊断', dataPath: 'Diagnosis',
      isVisible: true
    }
  ]
}

/**
 * ═══════════════════════════════════════════════════════════
 * 第8章：跨端兼容检查清单
 * ═══════════════════════════════════════════════════════════
 *
 * - [ ] 所有元素 $type 使用 template.element.xxx 格式
 * - [ ] 可编辑元素包含非空 label 中文标签
 * - [ ] 所有元素显式 "isVisible": true
 * - [ ] dataPath 使用驼峰格式（如 PatientName）
 * - [ ] 坐标单位为 mm，范围 0~300
 * - [ ] 字体使用中文字体名
 * - [ ] 颜色格式 #RRGGBB
 */

export default ELEMENT_REGISTRY
