import { Rect, Text, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class DropdownElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
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
        new Text('▼', {
          left: 4,
          top: mmToPx(element.height) / 2 - 8,
          fontSize: 14
        }),
        new Text(element.placeholder || '请选择', {
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
    return group
  }

  update(fabricObj, props) {
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
