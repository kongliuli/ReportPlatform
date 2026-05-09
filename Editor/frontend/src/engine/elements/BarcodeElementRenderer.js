import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

const BARCODE_PATTERNS = {
  CODE128: 'CODE128',
  CODE39: 'CODE39',
  EAN13: 'EAN13'
}

export class BarcodeElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
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
      left,
      top,
      width,
      height,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
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
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
  }

  toModel(fabricObj) {
    return {
      x: round2((fabricObj.left - CANVAS_PADDING) / MM_TO_PX),
      y: round2((fabricObj.top - CANVAS_PADDING) / MM_TO_PX),
      width: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      height: round2(fabricObj.getScaledHeight() / MM_TO_PX),
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
