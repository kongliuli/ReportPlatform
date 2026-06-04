import { Canvas, Rect, Shadow, util } from 'fabric'
import { createElementByType } from '@/models/elements'
import { SelectionManager } from './SelectionManager'
import { GridManager } from './GridManager'
import { CommandManager } from './CommandManager'
import { TextElementRenderer } from './elements/TextElementRenderer'
import { ImageElementRenderer } from './elements/ImageElementRenderer'
import { LineElementRenderer } from './elements/LineElementRenderer'
import { TableElementRenderer } from './elements/TableElementRenderer'
import { DateElementRenderer } from './elements/DateElementRenderer'
import { DropdownElementRenderer } from './elements/DropdownElementRenderer'
import { NumberElementRenderer } from './elements/NumberElementRenderer'
import { ShapeElementRenderer } from './elements/ShapeElementRenderer'
import { DividerElementRenderer } from './elements/DividerElementRenderer'
import { CheckboxElementRenderer } from './elements/CheckboxElementRenderer'
import { RadioElementRenderer } from './elements/RadioElementRenderer'
import { BarcodeElementRenderer } from './elements/BarcodeElementRenderer'
import { QrCodeElementRenderer } from './elements/QrCodeElementRenderer'
import { ChartElementRenderer } from './elements/ChartElementRenderer'
import { ContainerElementRenderer } from './elements/ContainerElementRenderer'
import { RepeatElementRenderer } from './elements/RepeatElementRenderer'
import { HeaderElementRenderer } from './elements/HeaderElementRenderer'
import { FooterElementRenderer } from './elements/FooterElementRenderer'
import { PageNumberElementRenderer } from './elements/PageNumberElementRenderer'
import { SignatureElementRenderer } from './elements/SignatureElementRenderer'
import { WatermarkElementRenderer } from './elements/WatermarkElementRenderer'
import { IconElementRenderer } from './elements/IconElementRenderer'
import { HyperlinkElementRenderer } from './elements/HyperlinkElementRenderer'
import { MM_TO_PX, PX_TO_MM, CANVAS_PADDING, round2 } from '@/utils/constants'

const BATCH_RENDER_THRESHOLD = 20
const MAX_CACHED_OBJECTS = 500

export class CanvasEngine {
  constructor(canvasEl, template, callbacks = {}) {
    this.canvasEl = canvasEl
    this.template = template
    this.callbacks = callbacks
    this.canvas = null
    this.fabricObjects = new Map()
    this.selectionManager = null
    this.gridManager = null
    this.commandManager = new CommandManager()
    this.renderers = {
      TextElement: new TextElementRenderer(),
      ImageElement: new ImageElementRenderer(),
      LineElement: new LineElementRenderer(),
      TableElement: new TableElementRenderer(),
      DateElement: new DateElementRenderer(),
      DropdownElement: new DropdownElementRenderer(),
      NumberElement: new NumberElementRenderer(),
      ShapeElement: new ShapeElementRenderer(),
      DividerElement: new DividerElementRenderer(),
      CheckboxElement: new CheckboxElementRenderer(),
      RadioElement: new RadioElementRenderer(),
      BarcodeElement: new BarcodeElementRenderer(),
      QrCodeElement: new QrCodeElementRenderer(),
      ChartElement: new ChartElementRenderer(),
      ContainerElement: new ContainerElementRenderer(),
      RepeatElement: new RepeatElementRenderer(),
      HeaderElement: new HeaderElementRenderer(),
      FooterElement: new FooterElementRenderer(),
      PageNumberElement: new PageNumberElementRenderer(),
      SignatureElement: new SignatureElementRenderer(),
      WatermarkElement: new WatermarkElementRenderer(),
      IconElement: new IconElementRenderer(),
      HyperlinkElement: new HyperlinkElementRenderer()
    }
    this._isBatchAdding = false
    this._pendingUpdates = []
    this._animationFrameId = null
    this._lastMoveTime = 0
    this._throttleInterval = 16
  }

  init() {
    const pageWidthPx = this.mmToPx(this.template.pageWidth)
    const pageHeightPx = this.mmToPx(this.template.pageHeight)

    this.canvas = new Canvas(this.canvasEl, {
      width: pageWidthPx + CANVAS_PADDING * 2,
      height: pageHeightPx + CANVAS_PADDING * 2,
      backgroundColor: '#e0e0e0',
      selection: true
    })

    const pageBg = new Rect({
      left: CANVAS_PADDING,
      top: CANVAS_PADDING,
      width: pageWidthPx,
      height: pageHeightPx,
      fill: '#ffffff',
      selectable: false,
      evented: false,
      shadow: new Shadow({ color: 'rgba(0,0,0,0.15)', blur: 10, offsetX: 2, offsetY: 2 })
    })
    this.canvas.add(pageBg)
    this._pageBg = pageBg

    this.selectionManager = new SelectionManager(this.canvas, {
      onSelected: (id) => this.callbacks.onElementSelected?.(id),
      onSelectionCleared: () => this.callbacks.onSelectionCleared?.()
    })

    this.gridManager = new GridManager(this.canvas, pageWidthPx, pageHeightPx, CANVAS_PADDING)

    if (this.template.elements?.length) {
      this.batchAddElements(this.template.elements)
    }

    this._bindEvents()
  }

  destroy() {
    if (this.canvas) {
      this.canvas.dispose()
      this.canvas = null
    }
    this.fabricObjects.clear()
    if (this._animationFrameId) {
      cancelAnimationFrame(this._animationFrameId)
      this._animationFrameId = null
    }
  }

  addElement(type, props = {}) {
    const defaults = this._getDefaultProps(type)
    const element = createElementByType(type, { ...defaults, ...props })
    this.template.addElement(element)
    this._addFabricObject(element)
    this.canvas.renderAll()
    return element.id
  }

  batchAddElements(elements) {
    if (!elements || elements.length === 0) return
    
    const useBatch = elements.length >= BATCH_RENDER_THRESHOLD
    
    if (useBatch) {
      this.canvas.renderOnAddRemove = false
    }
    
    elements.forEach(el => this._addFabricObject(el))
    
    if (useBatch) {
      this.canvas.renderOnAddRemove = true
      this.canvas.renderAll()
    } else {
      this.canvas.renderAll()
    }
  }

  scheduleUpdate(id, props) {
    const existingIndex = this._pendingUpdates.findIndex(u => u.id === id)
    if (existingIndex !== -1) {
      Object.assign(this._pendingUpdates[existingIndex].props, props)
    } else {
      this._pendingUpdates.push({ id, props })
    }
    
    if (!this._animationFrameId) {
      this._animationFrameId = requestAnimationFrame(() => this._processPendingUpdates())
    }
  }

  _processPendingUpdates() {
    if (this._pendingUpdates.length === 0) {
      this._animationFrameId = null
      return
    }
    
    this._pendingUpdates.forEach(update => {
      this.updateElementImmediate(update.id, update.props)
    })
    
    this._pendingUpdates = []
    this._animationFrameId = null
  }

  updateElementImmediate(id, props) {
    const fabricObj = this.fabricObjects.get(id)
    const element = this.template.getElementById(id)
    if (!fabricObj || !element) return

    const elementType = element.getElementType?.() || 'TextElement'
    const renderer = this.renderers[elementType]
    if (renderer) {
      renderer.update(fabricObj, props)
    }

    Object.assign(element, props)
  }

  removeElement(id) {
    const fabricObj = this.fabricObjects.get(id)
    if (fabricObj) {
      this.canvas.remove(fabricObj)
      this.fabricObjects.delete(id)
    }
    this.template.removeElement(id)
    this.canvas.renderAll()
  }

  updateElement(id, props) {
    const fabricObj = this.fabricObjects.get(id)
    const element = this.template.getElementById(id)
    if (!fabricObj || !element) return

    const elementType = element.getElementType?.() || 'TextElement'
    const renderer = this.renderers[elementType]
    if (renderer) {
      renderer.update(fabricObj, props)
    }

    Object.assign(element, props)
    this.canvas.renderAll()
  }

  reorderElement(id, zIndex) {
    const fabricObj = this.fabricObjects.get(id)
    if (fabricObj) {
      fabricObj.moveTo(zIndex + 1)
    }
    const element = this.template.getElementById(id)
    if (element) element.zIndex = zIndex
    this.canvas.renderAll()
  }

  setZoom(level) {
    this.canvas.setZoom(Math.max(0.1, Math.min(5.0, level)))
    this.canvas.renderAll()
  }

  fitToScreen() {
    const containerWidth = this.canvasEl.parentElement?.clientWidth || 800
    const containerHeight = this.canvasEl.parentElement?.clientHeight || 600
    const pageWidthPx = this.mmToPx(this.template.pageWidth)
    const pageHeightPx = this.mmToPx(this.template.pageHeight)
    const scaleX = (containerWidth - 60) / pageWidthPx
    const scaleY = (containerHeight - 60) / pageHeightPx
    const zoom = Math.min(scaleX, scaleY, 1.5)
    this.canvas.setZoom(zoom)
    this.canvas.renderAll()
  }

  toggleGrid(show) {
    if (show === undefined) {
      show = !this.gridManager.isVisible
    }
    if (show) {
      this.gridManager.show()
    } else {
      this.gridManager.hide()
    }
  }

  renderAll() {
    this.canvas?.renderAll()
  }

  mmToPx(mm) {
    return mm * MM_TO_PX
  }

  pxToMm(px) {
    return round2(px * PX_TO_MM)
  }

  _addFabricObject(element) {
    const elementType = element.getElementType?.() || 'TextElement'
    const renderer = this.renderers[elementType]
    if (!renderer) return

    const fabricObj = renderer.create(this.canvas, element, this.mmToPx.bind(this))
    if (!fabricObj) return

    fabricObj._elementId = element.id
    fabricObj._elementType = elementType
    this.fabricObjects.set(element.id, fabricObj)
    this.canvas.add(fabricObj)
  }

  _getDefaultProps(type) {
    const pageArea = this.template.printableArea
    const centerX = pageArea.x + pageArea.width / 2
    const centerY = pageArea.y + pageArea.height / 2

    const defaults = {
      TextElement: { x: round2(centerX - 50), y: round2(centerY - 15), width: 100, height: 30, text: '文本' },
      ImageElement: { x: round2(centerX - 50), y: round2(centerY - 50), width: 100, height: 100 },
      LineElement: { x: pageArea.x, y: round2(centerY), endX: round2(pageArea.x + pageArea.width), endY: round2(centerY) },
      ShapeElement: { x: round2(centerX - 40), y: round2(centerY - 40), width: 80, height: 80, shapeType: 'rectangle' },
      DividerElement: { x: pageArea.x, y: round2(centerY), width: round2(pageArea.width), height: 10 },
      CheckboxElement: { x: round2(centerX - 50), y: round2(centerY - 15), width: 100, height: 24, checkboxLabel: '选项' },
      RadioElement: { x: round2(centerX - 50), y: round2(centerY - 30), width: 100, height: 60, options: [{ label: '选项1', value: '1' }, { label: '选项2', value: '2' }] },
      DropdownElement: { x: round2(centerX - 50), y: round2(centerY - 15), width: 100, height: 30 },
      NumberElement: { x: round2(centerX - 50), y: round2(centerY - 15), width: 100, height: 30, value: 0 },
      DateElement: { x: round2(centerX - 50), y: round2(centerY - 15), width: 100, height: 30, value: new Date().toISOString().split('T')[0] },
      TableElement: { x: round2(centerX - 100), y: round2(centerY - 50), width: 200, height: 100, rows: 3, columns: 3 },
      BarcodeElement: { x: round2(centerX - 60), y: round2(centerY - 25), width: 120, height: 50, value: '12345678' },
      QrCodeElement: { x: round2(centerX - 50), y: round2(centerY - 50), width: 100, height: 100, size: 100, value: 'https://example.com' },
      ChartElement: { x: round2(centerX - 100), y: round2(centerY - 75), width: 200, height: 150, chartType: 'bar', series: [{ data: [30, 50, 40, 60, 45] }] },
      ContainerElement: { x: round2(centerX - 75), y: round2(centerY - 50), width: 150, height: 100 },
      RepeatElement: { x: round2(centerX - 75), y: round2(centerY - 50), width: 150, height: 100 },
      HeaderElement: { x: pageArea.x, y: 0, width: round2(pageArea.width), height: 30, content: '页眉内容' },
      FooterElement: { x: pageArea.x, y: round2(this.template.pageHeight - 30), width: round2(pageArea.width), height: 30, content: '页脚内容' },
      PageNumberElement: { x: round2(centerX - 30), y: round2(this.template.pageHeight - 20), width: 60, height: 20 },
      SignatureElement: { x: round2(centerX - 75), y: round2(centerY - 30), width: 150, height: 60 },
      WatermarkElement: { x: pageArea.x, y: pageArea.y, width: round2(pageArea.width), height: round2(pageArea.height), text: '水印' },
      IconElement: { x: round2(centerX - 12), y: round2(centerY - 12), width: 24, height: 24, iconName: 'star' },
      HyperlinkElement: { x: round2(centerX - 50), y: round2(centerY - 10), width: 100, height: 20, text: '链接文本', url: 'https://example.com' }
    }
    return defaults[type] || {}
  }

  _bindEvents() {
    this.canvas.on('object:moving', (e) => {
      const now = Date.now()
      if (now - this._lastMoveTime < this._throttleInterval) return
      this._lastMoveTime = now

      const obj = e.target
      if (obj._elementId) {
        const x = this.pxToMm(obj.left - CANVAS_PADDING)
        const y = this.pxToMm(obj.top - CANVAS_PADDING)
        this.callbacks.onElementMoving?.(obj._elementId, x, y)
      }
    })

    this.canvas.on('object:modified', (e) => {
      const obj = e.target
      if (obj._elementId) {
        const props = {
          x: this.pxToMm(obj.left - CANVAS_PADDING),
          y: this.pxToMm(obj.top - CANVAS_PADDING),
          width: this.pxToMm(obj.getScaledWidth()),
          height: this.pxToMm(obj.getScaledHeight()),
          rotation: round2(obj.angle || 0)
        }
        this.callbacks.onElementModified?.(obj._elementId, props)
      }
    })
  }

  optimizePerformance() {
    if (this.canvas) {
      this.canvas.selection = false
      this.canvas.skipTargetFind = true
    }
  }

  restorePerformance() {
    if (this.canvas) {
      this.canvas.selection = true
      this.canvas.skipTargetFind = false
    }
  }
}
