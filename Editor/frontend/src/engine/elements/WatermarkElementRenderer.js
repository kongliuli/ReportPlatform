import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class WatermarkElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)

    const objects = []

    const bgRect = new Rect({
      left: 0,
      top: 0,
      width,
      height,
      fill: 'transparent',
      stroke: '#E4E7ED',
      strokeWidth: 1,
      strokeDashArray: [4, 4]
    })
    objects.push(bgRect)

    const watermarkText = new IText(element.text || '水印', {
      left: width / 2,
      top: height / 2,
      fontSize: element.fontSize || 48,
      fontFamily: element.fontFamily || 'SimHei',
      fontWeight: 'bold',
      fill: element.foregroundColor || '#C0C4CC',
      textAlign: 'center',
      originX: 'center',
      originY: 'center',
      angle: element.angle || -45
    })
    objects.push(watermarkText)

    const group = new Group(objects, {
      left,
      top,
      width,
      height,
      opacity: element.opacity ?? 0.1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false,
      selectable: true
    })

    return group
  }

  update(fabricObj, props) {
    if (props.text !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('text', props.text)
    }
    if (props.fontSize !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('fontSize', props.fontSize)
    }
    if (props.angle !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('angle', props.angle)
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
