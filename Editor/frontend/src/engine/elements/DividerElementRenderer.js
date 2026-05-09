import { Line } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class DividerElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)

    const fabricObj = new Line([0, 0, width, 0], {
      left,
      top,
      stroke: element.color || '#000000',
      strokeWidth: element.thickness || 1,
      strokeDashArray: element.dividerStyle === 'dashed' ? [10, 5] : 
                       element.dividerStyle === 'dotted' ? [2, 4] : null,
      opacity: element.opacity ?? 1,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false,
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
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
    if (props.width !== undefined) {
      fabricObj.set({ x2: props.width * MM_TO_PX })
    }
  }

  toModel(fabricObj) {
    return {
      x: round2((fabricObj.left - CANVAS_PADDING) / MM_TO_PX),
      y: round2((fabricObj.top - CANVAS_PADDING) / MM_TO_PX),
      width: round2(fabricObj.x2 / MM_TO_PX),
      color: fabricObj.stroke,
      thickness: fabricObj.strokeWidth,
      opacity: fabricObj.opacity
    }
  }
}
