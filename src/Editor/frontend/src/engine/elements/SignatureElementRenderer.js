import { Group, Rect, Path } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class SignatureElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 60)

    const objects = []

    const bgRect = new Rect({
      left: 0,
      top: 0,
      width,
      height,
      fill: element.backgroundColor || '#FFFFFF',
      stroke: '#DCDFE6',
      strokeWidth: 1,
      rx: 4,
      ry: 4
    })
    objects.push(bgRect)

    if (element.signatureData && element.signatureData.length > 0) {
      element.signatureData.forEach(stroke => {
        const path = new Path(stroke.path, {
          stroke: element.strokeColor || '#000000',
          strokeWidth: element.strokeWidth || 2,
          fill: null
        })
        objects.push(path)
      })
    } else {
      const placeholderPath = new Path('M20 40 Q30 20 50 35 Q70 50 90 30 Q110 10 130 35', {
        left: 10,
        top: height / 2 - 10,
        stroke: '#C0C4CC',
        strokeWidth: 2,
        fill: null,
        strokeDashArray: [5, 5]
      })
      objects.push(placeholderPath)
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

  update(fabricObj, props) {
    if (props.strokeColor !== undefined) {
      fabricObj.getObjects().forEach(obj => {
        if (obj.type === 'path') obj.set('stroke', props.strokeColor)
      })
    }
    if (props.strokeWidth !== undefined) {
      fabricObj.getObjects().forEach(obj => {
        if (obj.type === 'path') obj.set('strokeWidth', props.strokeWidth)
      })
    }
    if (props.backgroundColor !== undefined) {
      const bg = fabricObj.getObjects().find(obj => obj.type === 'rect')
      if (bg) bg.set('fill', props.backgroundColor)
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
