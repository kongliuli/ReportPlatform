import { Rect, Circle, Triangle, Polygon } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class ShapeElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)

    let fabricObj

    const commonProps = {
      left,
      top,
      fill: element.fillColor || 'transparent',
      stroke: element.strokeColor || '#000000',
      strokeWidth: element.strokeWidth || 1,
      strokeDashArray: element.strokeStyle === 'dashed' ? [5, 5] : null,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
    }

    switch (element.shapeType) {
      case 'circle':
        fabricObj = new Circle({
          ...commonProps,
          radius: Math.min(width, height) / 2,
          originX: 'center',
          originY: 'center'
        })
        break
      case 'triangle':
        fabricObj = new Triangle({
          ...commonProps,
          width,
          height
        })
        break
      case 'diamond':
        fabricObj = new Polygon([
          { x: width / 2, y: 0 },
          { x: width, y: height / 2 },
          { x: width / 2, y: height },
          { x: 0, y: height / 2 }
        ], {
          ...commonProps,
          left,
          top
        })
        break
      default:
        fabricObj = new Rect({
          ...commonProps,
          width,
          height,
          rx: element.cornerRadius || 0,
          ry: element.cornerRadius || 0
        })
    }

    return fabricObj
  }

  update(fabricObj, props) {
    if (props.fillColor !== undefined) fabricObj.set('fill', props.fillColor)
    if (props.strokeColor !== undefined) fabricObj.set('stroke', props.strokeColor)
    if (props.strokeWidth !== undefined) fabricObj.set('strokeWidth', props.strokeWidth)
    if (props.strokeStyle !== undefined) {
      fabricObj.set('strokeDashArray', props.strokeStyle === 'dashed' ? [5, 5] : null)
    }
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
    if (props.cornerRadius !== undefined) {
      fabricObj.set('rx', props.cornerRadius)
      fabricObj.set('ry', props.cornerRadius)
    }
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
    if (props.width !== undefined || props.height !== undefined) {
      const width = props.width !== undefined ? props.width * MM_TO_PX : fabricObj.width
      const height = props.height !== undefined ? props.height * MM_TO_PX : fabricObj.height
      fabricObj.set({ width, height })
    }
  }

  toModel(fabricObj) {
    return {
      x: round2((fabricObj.left - CANVAS_PADDING) / MM_TO_PX),
      y: round2((fabricObj.top - CANVAS_PADDING) / MM_TO_PX),
      width: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      height: round2(fabricObj.getScaledHeight() / MM_TO_PX),
      fillColor: fabricObj.fill,
      strokeColor: fabricObj.stroke,
      strokeWidth: fabricObj.strokeWidth,
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
