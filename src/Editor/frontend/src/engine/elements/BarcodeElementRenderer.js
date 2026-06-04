import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

const BARCODE_PATTERNS = {
  CODE128: 'CODE128',
  CODE39: 'CODE39',
  EAN13: 'EAN13'
}

export class BarcodeElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 50)
    const barWidth = element.barWidth || 2
    const value = element.value || '12345678'

    const objects = []
    const bars = this._generateBarcodePattern(value, width, barWidth)
    
    bars.forEach(bar => {
      const rect = new Rect({
        left: bar.x,
        top: 0,
        width: bar.width,
        height: height - (element.showText ? 16 : 0),
        fill: '#000000'
      })
      objects.push(rect)
    })

    if (element.showText) {
      const text = new IText(value, {
        left: width / 2,
        top: height - 14,
        fontSize: 10,
        fontFamily: 'monospace',
        fill: '#000000',
        textAlign: 'center',
        originX: 'center'
      })
      objects.push(text)
    }

    const group = new Group(objects, {
      ...this._applyCommonOptions(element, mmToPx),
      width,
      height,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return group
  }

  _generateBarcodePattern(value, totalWidth, barWidth) {
    const bars = []
    const pattern = this._getCode128Pattern(value)
    let x = 0
    
    pattern.forEach((bit, index) => {
      if (bit === '1') {
        bars.push({ x, width: barWidth })
      }
      x += barWidth
    })
    
    return bars
  }

  _getCode128Pattern(value) {
    const pattern = []
    pattern.push(...'11010000100')
    for (let i = 0; i < value.length; i++) {
      const charCode = value.charCodeAt(i)
      for (let j = 0; j < 11; j++) {
        pattern.push(Math.random() > 0.5 ? '1' : '0')
      }
    }
    pattern.push(...'1100011101011')
    return pattern
  }

  update(fabricObj, props) {
    if (props.value !== undefined || props.barWidth !== undefined) {
      fabricObj.dirty = true
    }
    this._applyCommonUpdate(fabricObj, props)
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
      width: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      height: round2(fabricObj.getScaledHeight() / MM_TO_PX),
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
