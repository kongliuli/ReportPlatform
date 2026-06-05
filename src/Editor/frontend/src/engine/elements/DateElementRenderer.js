import { Rect, Text, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

function formatDate(value, format) {
  if (!value) return format || 'yyyy-MM-dd'
  try {
    const date = new Date(value)
    if (isNaN(date.getTime())) return value
    const replacements = {
      'yyyy': String(date.getFullYear()),
      'MM': String(date.getMonth() + 1).padStart(2, '0'),
      'dd': String(date.getDate()).padStart(2, '0'),
      'HH': String(date.getHours()).padStart(2, '0'),
      'mm': String(date.getMinutes()).padStart(2, '0'),
      'ss': String(date.getSeconds()).padStart(2, '0')
    }
    let result = format || 'yyyy-MM-dd'
    for (const [pattern, replacement] of Object.entries(replacements)) {
      result = result.replace(pattern, replacement)
    }
    return result
  } catch (_e) {
    return value
  }
}

export class DateElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const value = element.value || ''
    const format = element.format || 'yyyy-MM-dd'
    const displayText = formatDate(value, format)
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
        new Text('📅', {
          left: 4,
          top: mmToPx(element.height) / 2 - 8,
          fontSize: 14
        }),
        new Text(displayText, {
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
    group._dateValue = value
    group._dateFormat = format
    return group
  }

  update(fabricObj, props) {
    if (props.value !== undefined || props.format !== undefined) {
      if (props.value !== undefined) {
        fabricObj._dateValue = props.value
      }
      if (props.format !== undefined) {
        fabricObj._dateFormat = props.format
      }
      const displayText = formatDate(fabricObj._dateValue, fabricObj._dateFormat)
      const objects = fabricObj.getObjects()
      if (objects[2]) {
        objects[2].set('text', displayText)
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
