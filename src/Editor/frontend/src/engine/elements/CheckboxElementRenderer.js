import { Group, Rect, IText, Path } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class CheckboxElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const boxSize = Math.min(mmToPx(element.height), 16)
    const labelOffset = element.checkboxPosition === 'left' ? 0 : boxSize + 4

    const checkboxRect = new Rect({
      left: element.checkboxPosition === 'left' ? 0 : mmToPx(element.width) - boxSize,
      top: 0,
      width: boxSize,
      height: boxSize,
      fill: '#FFFFFF',
      stroke: '#DCDFE6',
      strokeWidth: 1,
      rx: 2,
      ry: 2
    })

    const checkPath = element.checked ? new Path('M2 6 L5 9 L10 2', {
      left: element.checkboxPosition === 'left' ? 1 : mmToPx(element.width) - boxSize + 1,
      top: 3,
      stroke: '#409EFF',
      strokeWidth: 2,
      fill: null
    }) : null

    const labelText = element.checkboxLabel ? new IText(element.checkboxLabel, {
      left: element.checkboxPosition === 'left' ? boxSize + 4 : 0,
      top: 2,
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fill: element.foregroundColor || '#000000'
    }) : null

    const objects = [checkboxRect]
    if (checkPath) objects.push(checkPath)
    if (labelText) objects.push(labelText)

    const group = new Group(objects, {
      ...this._applyCommonOptions(element, mmToPx),
      width: mmToPx(element.width),
      height: mmToPx(element.height),
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return group
  }

  update(fabricObj, props) {
    if (props.checked !== undefined) {
      const checkPath = fabricObj.getObjects().find(obj => obj.type === 'path')
      if (checkPath) {
        checkPath.visible = props.checked
      }
    }
    if (props.checkboxLabel !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('text', props.checkboxLabel)
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
