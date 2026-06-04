import { Group, Rect, Circle, IText, Path } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class RadioElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const options = element.options || []
    const orientation = element.orientation || 'vertical'
    const itemHeight = 20
    const itemWidth = 100
    const radioSize = 12

    const objects = []
    let currentTop = 0
    let currentLeft = 0

    options.forEach((option, index) => {
      const isSelected = element.selectedValue === option.value

      const outerCircle = new Circle({
        left: currentLeft,
        top: currentTop + 2,
        radius: radioSize / 2,
        fill: '#FFFFFF',
        stroke: isSelected ? '#409EFF' : '#DCDFE6',
        strokeWidth: 1
      })
      objects.push(outerCircle)

      if (isSelected) {
        const innerCircle = new Circle({
          left: currentLeft + 3,
          top: currentTop + 5,
          radius: radioSize / 2 - 3,
          fill: '#409EFF',
          stroke: null
        })
        objects.push(innerCircle)
      }

      const labelText = new IText(option.label || option.value || '', {
        left: currentLeft + radioSize + 4,
        top: currentTop,
        fontSize: element.fontSize || 12,
        fontFamily: element.fontFamily || 'SimSun',
        fill: element.foregroundColor || '#000000'
      })
      objects.push(labelText)

      if (orientation === 'vertical') {
        currentTop += itemHeight
      } else {
        currentLeft += itemWidth
      }
    })

    const group = new Group(objects, {
      ...this._applyCommonOptions(element, mmToPx),
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return group
  }

  update(fabricObj, props) {
    if (props.selectedValue !== undefined || props.options !== undefined) {
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
