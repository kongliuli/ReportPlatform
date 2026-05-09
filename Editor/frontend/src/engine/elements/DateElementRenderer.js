import { Rect, Text, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

function createPlaceholderElement(element, mmToPx, label, icon) {
  const group = new Group(
    [
      new Rect({
        width: mmToPx(element.width),
        height: mmToPx(element.height),
        fill: '#fafafa',
        stroke: '#b7b7b7',
        strokeWidth: 1,
        strokeDashArray: [4, 4],
        rx: 4,
        ry: 4
      }),
      new Text(icon || '📅', {
        left: 4,
        top: mmToPx(element.height) / 2 - 8,
        fontSize: 14
      }),
      new Text(label, {
        left: 24,
        top: mmToPx(element.height) / 2 - 6,
        fontSize: 11,
        fill: '#666',
        fontFamily: 'SimSun'
      })
    ],
    {
      left: mmToPx(element.x) + CANVAS_PADDING,
      top: mmToPx(element.y) + CANVAS_PADDING,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
    }
  )
  return group
}

export class DateElementRenderer {
  create(canvas, element, mmToPx) {
    return createPlaceholderElement(element, mmToPx, element.value || element.format || 'yyyy-MM-dd', '📅')
  }
  update(fabricObj, props) {
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
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
