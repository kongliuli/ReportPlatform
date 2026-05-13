import { Rect } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class ContainerElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)

    const fabricObj = new Rect({
      ...this._applyCommonOptions(element, mmToPx),
      width,
      height,
      fill: element.backgroundColor || 'transparent',
      stroke: element.borderColor || '#DCDFE6',
      strokeWidth: element.borderWidth ?? 1,
      strokeDashArray: [5, 5],
      rx: element.cornerRadius || 0,
      ry: element.cornerRadius || 0,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return fabricObj
  }

  update(fabricObj, props) {
    if (props.backgroundColor !== undefined) fabricObj.set('fill', props.backgroundColor)
    if (props.borderColor !== undefined) fabricObj.set('stroke', props.borderColor)
    if (props.borderWidth !== undefined) fabricObj.set('strokeWidth', props.borderWidth)
    if (props.cornerRadius !== undefined) {
      fabricObj.set('rx', props.cornerRadius)
      fabricObj.set('ry', props.cornerRadius)
    }
    this._applyCommonUpdate(fabricObj, props)
    if (props.width !== undefined || props.height !== undefined) {
      fabricObj.set({
        width: props.width !== undefined ? props.width * MM_TO_PX : fabricObj.width,
        height: props.height !== undefined ? props.height * MM_TO_PX : fabricObj.height
      })
    }
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
