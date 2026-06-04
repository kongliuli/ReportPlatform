import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class WatermarkElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
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
      ...this._applyCommonOptions(element, mmToPx),
      width,
      height,
      opacity: element.opacity ?? 0.1,
      angle: element.rotation || 0,
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
