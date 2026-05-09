import { ElementBase } from './elements'
import { TemplateDataBinding } from './databinding'

export class ReportTemplateDefinition {
  constructor(props = {}) {
    this.id = props.id || crypto.randomUUID()
    this.name = props.name ?? ''
    this.version = props.version ?? 1
    this.type = props.type ?? ''
    this.hospitalId = props.hospitalId ?? ''
    this.isDefault = props.isDefault ?? false
    this.isForceUpdate = props.isForceUpdate ?? false

    this.pageWidth = props.pageWidth ?? 210
    this.pageHeight = props.pageHeight ?? 297
    this.marginLeft = props.marginLeft ?? 15
    this.marginRight = props.marginRight ?? 15
    this.marginTop = props.marginTop ?? 15
    this.marginBottom = props.marginBottom ?? 15
    this.orientation = props.orientation ?? 'Portrait'
    this.backgroundColor = props.backgroundColor ?? '#FFFFFF'

    this.globalFontSize = props.globalFontSize ?? 12
    this.enableGlobalFontSize = props.enableGlobalFontSize ?? false

    this.createTime = props.createTime ?? new Date().toISOString()
    this.updateTime = props.updateTime ?? new Date().toISOString()

    this.elements = props.elements ?? []
    this.dataBindings = (props.dataBindings ?? []).map(
      b => b instanceof TemplateDataBinding ? b : new TemplateDataBinding(b)
    )

    this._selectedElementId = null
  }

  addElement(element) {
    this.elements.push(element)
  }

  removeElement(id) {
    this.elements = this.elements.filter(e => e.id !== id)
  }

  getElementById(id) {
    return this.elements.find(e => e.id === id) || null
  }

  getSortedElements() {
    return [...this.elements].sort((a, b) => a.zIndex - b.zIndex)
  }

  get printableArea() {
    return {
      x: this.marginLeft,
      y: this.marginTop,
      width: this.pageWidth - this.marginLeft - this.marginRight,
      height: this.pageHeight - this.marginTop - this.marginBottom
    }
  }
}
