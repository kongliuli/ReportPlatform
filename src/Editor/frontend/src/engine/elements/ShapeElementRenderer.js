import { Rect, Circle, Triangle, Polygon } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class ShapeElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)

    let fabricObj

    const commonProps = {
      ...this._applyCommonOptions(element, mmToPx),
      fill: element.fillColor || 'transparent',
      stroke: element.strokeColor || '#000000',
      strokeWidth: element.strokeWidth || 1,
      strokeDashArray: element.strokeStyle === 'dashed' ? [5, 5] : null,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
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
    if (props.cornerRadius !== undefined) {
      fabricObj.set('rx', props.cornerRadius)
      fabricObj.set('ry', props.cornerRadius)
    }
    this._applyCommonUpdate(fabricObj, props)
    if (props.width !== undefined || props.height !== undefined) {
      const width = props.width !== undefined ? props.width * MM_TO_PX : fabricObj.width
      const height = props.height !== undefined ? props.height * MM_TO_PX : fabricObj.height
      fabricObj.set({ width, height })
    }
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
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
