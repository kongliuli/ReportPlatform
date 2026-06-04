import { Line } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class DividerElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)

    const fabricObj = new Line([0, 0, width, 0], {
      ...this._applyCommonOptions(element, mmToPx),
      stroke: element.color || '#000000',
      strokeWidth: element.thickness || 1,
      strokeDashArray: element.dividerStyle === 'dashed' ? [10, 5] : 
                       element.dividerStyle === 'dotted' ? [2, 4] : null,
      opacity: element.opacity ?? 1,
      hasControls: true,
      hasBorders: true
    })

    return fabricObj
  }

  update(fabricObj, props) {
    if (props.color !== undefined) fabricObj.set('stroke', props.color)
    if (props.thickness !== undefined) fabricObj.set('strokeWidth', props.thickness)
    if (props.dividerStyle !== undefined) {
      fabricObj.set('strokeDashArray', 
        props.dividerStyle === 'dashed' ? [10, 5] : 
        props.dividerStyle === 'dotted' ? [2, 4] : null
      )
    }
    this._applyCommonUpdate(fabricObj, props)
    if (props.width !== undefined) {
      fabricObj.set({ x2: props.width * MM_TO_PX })
    }
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
      width: round2(fabricObj.x2 / MM_TO_PX),
      color: fabricObj.stroke,
      thickness: fabricObj.strokeWidth,
      opacity: fabricObj.opacity
    }
  }
}
