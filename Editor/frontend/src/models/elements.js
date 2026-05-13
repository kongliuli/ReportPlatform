let _idCounter = 0

function generateId() {
  return `el_${Date.now()}_${++_idCounter}`
}

export class ElementBase {
  constructor(props = {}) {
    if (typeof props !== 'object' || props === null) {
      throw new Error('Props must be an object')
    }
    this.id = props.id || generateId()
    this.x = props.x ?? 0
    this.y = props.y ?? 0
    this.width = props.width ?? 100
    this.height = props.height ?? 30
    this.isVisible = props.isVisible ?? true
    this.rotation = props.rotation ?? 0
    this.zIndex = props.zIndex ?? 0

    this.backgroundColor = props.backgroundColor ?? 'transparent'
    this.borderColor = props.borderColor ?? '#000000'
    this.borderWidth = props.borderWidth ?? 0
    this.borderStyle = props.borderStyle ?? 'solid'
    this.cornerRadius = props.cornerRadius ?? 0
    this.opacity = props.opacity ?? 1
    this.shadow = props.shadow ?? null

    this.fontFamily = props.fontFamily ?? 'SimSun'
    this.fontSize = props.fontSize ?? 12
    this.fontWeight = props.fontWeight ?? 'normal'
    this.fontStyle = props.fontStyle ?? 'normal'
    this.foregroundColor = props.foregroundColor ?? '#000000'
    this.textAlignment = props.textAlignment ?? 'left'

    this.label = props.label ?? ''
    this.labelWidth = props.labelWidth ?? 0
    this.defaultValue = props.defaultValue ?? ''
    this.isRequired = props.isRequired ?? false
    this.dataPath = props.dataPath ?? ''
    this.formatString = props.formatString ?? ''
    this.isDataBound = props.isDataBound ?? false

    this.$type = props.$type || ''
  }

  getElementType() {
    return 'ElementBase'
  }

  getCategory() {
    return 'base'
  }

  clone() {
    const json = JSON.parse(JSON.stringify(this))
    json.id = generateId()
    return this.constructor.fromJSON(json)
  }

  static fromJSON(json) {
    const instance = new this(json)
    return instance
  }
}

export class TextElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.text = props.text ?? ''
    this.richText = props.richText ?? null
    this.isRichText = props.isRichText ?? false
    this.textDecoration = props.textDecoration ?? 'none'
    this.lineHeight = props.lineHeight ?? 1.5
    this.letterSpacing = props.letterSpacing ?? 0
    this.$type = props.$type || 'template.element.text'
  }

  getElementType() { return 'TextElement' }
  getCategory() { return 'basic' }
}

export class ImageElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.imagePath = props.imagePath ?? ''
    this.imageData = props.imageData ?? ''
    this.stretch = props.stretch ?? 'Uniform'
    this.maintainAspectRatio = props.maintainAspectRatio ?? true
    this.altText = props.altText ?? ''
    this.$type = props.$type || 'template.element.image'
  }

  getElementType() { return 'ImageElement' }
  getCategory() { return 'basic' }
}

export class LineElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.lineColor = props.lineColor ?? '#000000'
    this.lineWidth = props.lineWidth ?? 1
    this.lineStyle = props.lineStyle ?? 'solid'
    this.startX = props.startX ?? 0
    this.startY = props.startY ?? 0
    this.endX = props.endX ?? 100
    this.endY = props.endY ?? 0
    this.$type = props.$type || 'template.element.line'
  }

  getElementType() { return 'LineElement' }
  getCategory() { return 'basic' }
}

export class ShapeElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.shapeType = props.shapeType ?? 'rectangle'
    this.fillColor = props.fillColor ?? 'transparent'
    this.strokeColor = props.strokeColor ?? '#000000'
    this.strokeWidth = props.strokeWidth ?? 1
    this.strokeStyle = props.strokeStyle ?? 'solid'
    this.$type = props.$type || 'template.element.shape'
  }

  getElementType() { return 'ShapeElement' }
  getCategory() { return 'basic' }
}

export class DividerElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.dividerStyle = props.dividerStyle ?? 'solid'
    this.thickness = props.thickness ?? 1
    this.color = props.color ?? '#000000'
    this.marginTop = props.marginTop ?? 5
    this.marginBottom = props.marginBottom ?? 5
    this.$type = props.$type || 'template.element.divider'
  }

  getElementType() { return 'DividerElement' }
  getCategory() { return 'basic' }
}

export class CheckboxElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.checked = props.checked ?? false
    this.checkboxLabel = props.checkboxLabel ?? ''
    this.checkboxPosition = props.checkboxPosition ?? 'left'
    this.$type = props.$type || 'template.element.checkbox'
  }

  getElementType() { return 'CheckboxElement' }
  getCategory() { return 'input' }
}

export class RadioElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.options = props.options ?? []
    this.selectedValue = props.selectedValue ?? ''
    this.orientation = props.orientation ?? 'vertical'
    this.$type = props.$type || 'template.element.radio'
  }

  getElementType() { return 'RadioElement' }
  getCategory() { return 'input' }
}

export class DropdownElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.value = props.value ?? ''
    this.options = props.options ?? []
    this.placeholder = props.placeholder ?? '请选择'
    this.allowCustom = props.allowCustom ?? false
    this.$type = props.$type || 'template.element.dropdown'
  }

  getElementType() { return 'DropdownElement' }
  getCategory() { return 'input' }
}

export class NumberElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.value = props.value ?? 0
    this.format = props.format ?? ''
    this.minValue = props.minValue ?? null
    this.maxValue = props.maxValue ?? null
    this.unit = props.unit ?? ''
    this.decimalPlaces = props.decimalPlaces ?? 2
    this.showThousandsSeparator = props.showThousandsSeparator ?? false
    this.$type = props.$type || 'template.element.number'
  }

  getElementType() { return 'NumberElement' }
  getCategory() { return 'input' }
}

export class DateElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.value = props.value ?? ''
    this.format = props.format ?? 'yyyy-MM-dd'
    this.minDate = props.minDate ?? ''
    this.maxDate = props.maxDate ?? ''
    this.showTime = props.showTime ?? false
    this.$type = props.$type || 'template.element.date'
  }

  getElementType() { return 'DateElement' }
  getCategory() { return 'input' }
}

export class TableElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.rows = props.rows ?? 3
    this.columns = props.columns ?? 3
    this.cellData = props.cellData ?? []
    this.cellPadding = props.cellPadding ?? 4
    this.tableBorder = props.tableBorder ?? 1
    this.hasHeader = props.hasHeader ?? true
    this.headerStyle = props.headerStyle ?? {}
    this.alternateRowColors = props.alternateRowColors ?? false
    this.$type = props.$type || 'template.element.table'
  }

  getElementType() { return 'TableElement' }
  getCategory() { return 'data' }
}

export class BarcodeElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.value = props.value ?? ''
    this.barcodeFormat = props.barcodeFormat ?? 'CODE128'
    this.barWidth = props.barWidth ?? 2
    this.height = props.height ?? 50
    this.showText = props.showText ?? true
    this.$type = props.$type || 'template.element.barcode'
  }

  getElementType() { return 'BarcodeElement' }
  getCategory() { return 'data' }
}

export class QrCodeElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.value = props.value ?? ''
    this.size = props.size ?? 100
    this.errorLevel = props.errorLevel ?? 'M'
    this.foregroundColor = props.foregroundColor ?? '#000000'
    this.backgroundColor = props.backgroundColor ?? '#FFFFFF'
    this.$type = props.$type || 'template.element.qrcode'
  }

  getElementType() { return 'QrCodeElement' }
  getCategory() { return 'data' }
}

export class ChartElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.chartType = props.chartType ?? 'bar'
    this.dataSource = props.dataSource ?? ''
    this.series = props.series ?? []
    this.xAxis = props.xAxis ?? {}
    this.yAxis = props.yAxis ?? {}
    this.legend = props.legend ?? { show: true, position: 'bottom' }
    this.colors = props.colors ?? ['#409EFF', '#67C23A', '#E6A23C', '#F56C6C']
    this.$type = props.$type || 'template.element.chart'
  }

  getElementType() { return 'ChartElement' }
  getCategory() { return 'data' }
}

export class ContainerElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.children = props.children ?? []
    this.layout = props.layout ?? 'absolute'
    this.padding = props.padding ?? 0
    this.gap = props.gap ?? 0
    this.$type = props.$type || 'template.element.container'
  }

  getElementType() { return 'ContainerElement' }
  getCategory() { return 'layout' }
}

export class RepeatElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.dataSource = props.dataSource ?? ''
    this.template = props.template ?? null
    this.maxItems = props.maxItems ?? 0
    this.direction = props.direction ?? 'vertical'
    this.gap = props.gap ?? 10
    this.$type = props.$type || 'template.element.repeat'
  }

  getElementType() { return 'RepeatElement' }
  getCategory() { return 'layout' }
}

export class HeaderElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.content = props.content ?? ''
    this.showOnAllPages = props.showOnAllPages ?? true
    this.height = props.height ?? 30
    this.$type = props.$type || 'template.element.header'
  }

  getElementType() { return 'HeaderElement' }
  getCategory() { return 'layout' }
}

export class FooterElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.content = props.content ?? ''
    this.showOnAllPages = props.showOnAllPages ?? true
    this.showPageNumber = props.showPageNumber ?? false
    this.height = props.height ?? 30
    this.$type = props.$type || 'template.element.footer'
  }

  getElementType() { return 'FooterElement' }
  getCategory() { return 'layout' }
}

export class PageNumberElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.format = props.format ?? '{page} / {total}'
    this.startFrom = props.startFrom ?? 1
    this.position = props.position ?? 'center'
    this.$type = props.$type || 'template.element.pagenumber'
  }

  getElementType() { return 'PageNumberElement' }
  getCategory() { return 'layout' }
}

export class SignatureElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.strokeWidth = props.strokeWidth ?? 2
    this.strokeColor = props.strokeColor ?? '#000000'
    this.backgroundColor = props.backgroundColor ?? '#FFFFFF'
    this.signatureData = props.signatureData ?? null
    this.$type = props.$type || 'template.element.signature'
  }

  getElementType() { return 'SignatureElement' }
  getCategory() { return 'special' }
}

export class WatermarkElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.text = props.text ?? ''
    this.angle = props.angle ?? -45
    this.fontSize = props.fontSize ?? 48
    this.opacity = props.opacity ?? 0.1
    this.repeat = props.repeat ?? true
    this.$type = props.$type || 'template.element.watermark'
  }

  getElementType() { return 'WatermarkElement' }
  getCategory() { return 'special' }
}

export class IconElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.iconName = props.iconName ?? ''
    this.iconSet = props.iconSet ?? 'element-plus'
    this.iconSize = props.iconSize ?? 24
    this.iconColor = props.iconColor ?? '#000000'
    this.$type = props.$type || 'template.element.icon'
  }

  getElementType() { return 'IconElement' }
  getCategory() { return 'special' }
}

export class HyperlinkElement extends ElementBase {
  constructor(props = {}) {
    super(props)
    this.url = props.url ?? ''
    this.text = props.text ?? ''
    this.target = props.target ?? '_blank'
    this.underline = props.underline ?? true
    this.linkColor = props.linkColor ?? '#409EFF'
    this.$type = props.$type || 'template.element.hyperlink'
  }

  getElementType() { return 'HyperlinkElement' }
  getCategory() { return 'special' }
}

const ELEMENT_TYPE_MAP = {
  'template.element.text': TextElement,
  'template.element.image': ImageElement,
  'template.element.line': LineElement,
  'template.element.shape': ShapeElement,
  'template.element.divider': DividerElement,
  'template.element.checkbox': CheckboxElement,
  'template.element.radio': RadioElement,
  'template.element.dropdown': DropdownElement,
  'template.element.number': NumberElement,
  'template.element.date': DateElement,
  'template.element.table': TableElement,
  'template.element.barcode': BarcodeElement,
  'template.element.qrcode': QrCodeElement,
  'template.element.chart': ChartElement,
  'template.element.container': ContainerElement,
  'template.element.repeat': RepeatElement,
  'template.element.header': HeaderElement,
  'template.element.footer': FooterElement,
  'template.element.pagenumber': PageNumberElement,
  'template.element.signature': SignatureElement,
  'template.element.watermark': WatermarkElement,
  'template.element.icon': IconElement,
  'template.element.hyperlink': HyperlinkElement,
  'Xinglin.Core.Elements.TextElement, Xinglin.Core': TextElement,
  'Xinglin.Core.Elements.ImageElement, Xinglin.Core': ImageElement,
  'Xinglin.Core.Elements.LineElement, Xinglin.Core': LineElement,
  'Xinglin.Core.Elements.TableElement, Xinglin.Core': TableElement,
  'Xinglin.Core.Elements.DateElement, Xinglin.Core': DateElement,
  'Xinglin.Core.Elements.DropdownElement, Xinglin.Core': DropdownElement,
  'Xinglin.Core.Elements.NumberElement, Xinglin.Core': NumberElement
}

export function getElementClassByType($type) {
  return ELEMENT_TYPE_MAP[$type] || null
}

export function createElementByType(type, props = {}) {
  const typeMap = {
    TextElement: TextElement,
    ImageElement: ImageElement,
    LineElement: LineElement,
    ShapeElement: ShapeElement,
    DividerElement: DividerElement,
    CheckboxElement: CheckboxElement,
    RadioElement: RadioElement,
    DropdownElement: DropdownElement,
    NumberElement: NumberElement,
    DateElement: DateElement,
    TableElement: TableElement,
    BarcodeElement: BarcodeElement,
    QrCodeElement: QrCodeElement,
    ChartElement: ChartElement,
    ContainerElement: ContainerElement,
    RepeatElement: RepeatElement,
    HeaderElement: HeaderElement,
    FooterElement: FooterElement,
    PageNumberElement: PageNumberElement,
    SignatureElement: SignatureElement,
    WatermarkElement: WatermarkElement,
    IconElement: IconElement,
    HyperlinkElement: HyperlinkElement
  }
  const Cls = typeMap[type]
  if (!Cls) throw new Error(`Unknown element type: ${type}`)
  return new Cls(props)
}

export const ELEMENT_CATEGORIES = {
  basic: {
    label: '基础元素',
    description: '文本、图片、线条等基础元素',
    icon: 'Document',
    order: 1
  },
  input: {
    label: '输入元素',
    description: '表单输入控件',
    icon: 'Edit',
    order: 2
  },
  data: {
    label: '数据元素',
    description: '表格、图表、条码等数据展示',
    icon: 'DataLine',
    order: 3
  },
  layout: {
    label: '布局元素',
    description: '容器、页眉页脚等布局组件',
    icon: 'Grid',
    order: 4
  },
  special: {
    label: '特殊元素',
    description: '签名、水印等特殊功能',
    icon: 'Star',
    order: 5
  }
}

export const ELEMENT_TYPES = [
  { key: 'TextElement', label: '文本', icon: 'Document', category: 'basic' },
  { key: 'ImageElement', label: '图片', icon: 'Picture', category: 'basic' },
  { key: 'LineElement', label: '线条', icon: 'Minus', category: 'basic' },
  { key: 'ShapeElement', label: '形状', icon: 'CircleCheck', category: 'basic' },
  { key: 'DividerElement', label: '分隔线', icon: 'SemiSelect', category: 'basic' },
  
  { key: 'CheckboxElement', label: '复选框', icon: 'Select', category: 'input' },
  { key: 'RadioElement', label: '单选框', icon: 'CircleCheck', category: 'input' },
  { key: 'DropdownElement', label: '下拉框', icon: 'ArrowDown', category: 'input' },
  { key: 'NumberElement', label: '数字', icon: 'Histogram', category: 'input' },
  { key: 'DateElement', label: '日期', icon: 'Calendar', category: 'input' },
  
  { key: 'TableElement', label: '表格', icon: 'Grid', category: 'data' },
  { key: 'BarcodeElement', label: '条形码', icon: 'Tickets', category: 'data' },
  { key: 'QrCodeElement', label: '二维码', icon: 'Grid', category: 'data' },
  { key: 'ChartElement', label: '图表', icon: 'TrendCharts', category: 'data' },
  
  { key: 'ContainerElement', label: '容器', icon: 'Files', category: 'layout' },
  { key: 'RepeatElement', label: '重复区域', icon: 'CopyDocument', category: 'layout' },
  { key: 'HeaderElement', label: '页眉', icon: 'Top', category: 'layout' },
  { key: 'FooterElement', label: '页脚', icon: 'Bottom', category: 'layout' },
  { key: 'PageNumberElement', label: '页码', icon: 'Collection', category: 'layout' },
  
  { key: 'SignatureElement', label: '签名', icon: 'EditPen', category: 'special' },
  { key: 'WatermarkElement', label: '水印', icon: 'Stamp', category: 'special' },
  { key: 'IconElement', label: '图标', icon: 'Star', category: 'special' },
  { key: 'HyperlinkElement', label: '超链接', icon: 'Link', category: 'special' }
]

export function getElementsByCategory(category) {
  return ELEMENT_TYPES.filter(e => e.category === category)
}

export function getGroupedElements() {
  const groups = {}
  Object.keys(ELEMENT_CATEGORIES).forEach(cat => {
    groups[cat] = {
      ...ELEMENT_CATEGORIES[cat],
      elements: getElementsByCategory(cat)
    }
  })
  return Object.values(groups).sort((a, b) => a.order - b.order)
}
