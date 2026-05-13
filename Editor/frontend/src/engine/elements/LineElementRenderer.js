import { Line } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class LineElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const { left: _l, top: _t, ...lineSelectionStyle } = this._applyCommonOptions(element, mmToPx)
    const fabricObj = new Line(
      [
        mmToPx(element.startX || element.x) + CANVAS_PADDING,
        mmToPx(element.startY || element.y) + CANVAS_PADDING,
        mmToPx(element.endX || (element.x + element.width)) + CANVAS_PADDING,
        mmToPx(element.endY || element.y) + CANVAS_PADDING
      ],
      {
        stroke: element.lineColor || '#000000',
        strokeWidth: element.lineWidth || 1,
        strokeDashArray: element.lineStyle === 'dashed' ? [10, 5] : element.lineStyle === 'dotted' ? [3, 3] : null,
        opacity: element.opacity ?? 1,
        ...lineSelectionStyle
      }
    )
    return fabricObj
  }

  update(fabricObj, props) {
    if (props.lineColor !== undefined) fabricObj.set('stroke', props.lineColor)
    if (props.lineWidth !== undefined) fabricObj.set('strokeWidth', props.lineWidth)
    this._applyCommonUpdate(fabricObj, props)
  }

  toModel(fabricObj) {
    return {
      startX: round2((fabricObj.x1 - CANVAS_PADDING) / MM_TO_PX),
      startY: round2((fabricObj.y1 - CANVAS_PADDING) / MM_TO_PX),
      endX: round2((fabricObj.x2 - CANVAS_PADDING) / MM_TO_PX),
      endY: round2((fabricObj.y2 - CANVAS_PADDING) / MM_TO_PX),
      lineColor: fabricObj.stroke,
      lineWidth: fabricObj.strokeWidth,
      opacity: fabricObj.opacity
    }
  }
}
