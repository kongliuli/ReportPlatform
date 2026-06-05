import { Rect, Text, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class NumberElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const displayValue = element.value !== undefined ? String(element.value) : '0'
    const unit = element.unit || ''
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
        new Text('#', {
          left: 4,
          top: mmToPx(element.height) / 2 - 8,
          fontSize: 14
        }),
        new Text(displayValue + unit, {
          left: 24,
          top: mmToPx(element.height) / 2 - 6,
          fontSize: 11,
          fill: '#666',
          fontFamily: 'SimSun'
        })
      ],
      {
        ...this._applyCommonOptions(element, mmToPx),
        opacity: element.opacity ?? 1,
        angle: element.rotation || 0
      }
    )
    group._numberValue = displayValue
    group._numberUnit = unit
    return group
  }

  update(fabricObj, props) {
    if (props.value !== undefined || props.unit !== undefined) {
      if (props.value !== undefined) {
        fabricObj._numberValue = String(props.value)
      }
      if (props.unit !== undefined) {
        fabricObj._numberUnit = props.unit
      }
      const objects = fabricObj.getObjects()
      if (objects[2]) {
        objects[2].set('text', fabricObj._numberValue + fabricObj._numberUnit)
      }
      fabricObj.dirty = true
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
