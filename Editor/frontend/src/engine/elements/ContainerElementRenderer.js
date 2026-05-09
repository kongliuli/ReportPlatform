import { Rect } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class ContainerElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)

    const fabricObj = new Rect({
      left,
      top,
      width,
      height,
      fill: element.backgroundColor || 'transparent',
      stroke: element.borderColor || '#DCDFE6',
      strokeWidth: element.borderWidth ?? 1,
      strokeDashArray: [5, 5],
      rx: element.cornerRadius || 0,
      ry: element.cornerRadius || 0,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
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
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
    if (props.width !== undefined || props.height !== undefined) {
      fabricObj.set({
        width: props.width !== undefined ? props.width * MM_TO_PX : fabricObj.width,
        height: props.height !== undefined ? props.height * MM_TO_PX : fabricObj.height
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
